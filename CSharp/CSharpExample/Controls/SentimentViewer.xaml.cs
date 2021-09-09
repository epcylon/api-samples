using QuantGate.API.Signals;
using QuantGate.API.Signals.Values;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BridgeRock.CSharpExample.Controls
{
    /// <summary>
    /// Interaction logic for SentimentViewer.xaml
    /// </summary>
    public partial class SentimentViewer : UserControl
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "Sent";

        #region Spectrum Constants

        /// <summary>
        /// The basic horizontal line brush to use.
        /// </summary>
        public static readonly SolidColorBrush _hLineBrush;
        /// <summary>
        /// The base line brush to use for the fill.
        /// </summary>
        public static readonly SolidColorBrush _baselineFill;

        /// <summary>
        /// The outline stroke for any of the peaks.
        /// </summary>
        public static readonly SolidColorBrush _peakStroke;
        /// <summary>
        /// The fill for top peak.
        /// </summary>
        public static readonly SolidColorBrush _topPeakFill;
        /// <summary>
        /// The fill for bottom peak.
        /// </summary>
        public static readonly SolidColorBrush _bottomPeakFill;

        /// <summary>
        /// The inactive color for a spectrum line.
        /// </summary>
        private static readonly SolidColorBrush _inactiveColor;
        /// <summary>
        /// Holds a list of frozen brushes to use.
        /// </summary>
        private static readonly SolidColorBrush[] _brushes = new SolidColorBrush[401];

        #endregion

        #region Spectrum Variables

        /// <summary>
        /// The spectrum lines to display.
        /// </summary>
        private readonly Rectangle[] _lines = new Rectangle[SentimentEventArgs.TotalBars];

        /// <summary>
        /// Spectrum colors.
        /// </summary>
        private readonly double[] _colors = new double[SentimentEventArgs.TotalBars];
        /// <summary>
        /// Spectrum lengths.
        /// </summary>
        private readonly double[] _lengths = new double[SentimentEventArgs.TotalBars];

        /// <summary>
        /// Is the data dirty?
        /// </summary>
        private bool _isDirty = false;

        /// <summary>
        /// The top peak to display.
        /// </summary>
        private Ellipse _topPeak;
        /// <summary>
        /// The bottom peak to display.
        /// </summary>
        private Ellipse _bottomPeak;

        /// <summary>
        /// Has the data been retrieved?
        /// </summary>
        private bool _dataRetrieved = false;

        private Storyboard _story = default;
        private Rectangle _rectLoading;
        private Viewbox _vbLoading;

        /// <summary>
        /// Is the (loading) storyboard currently running?
        /// </summary>
        private bool _storyboardRunning = false;

        #endregion

        #region Static Brush Setup

        /// <summary>
        /// Creates the static brushes when the first item is created.
        /// </summary>
        static SentimentViewer()
        {
            _hLineBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0));
            _baselineFill = new SolidColorBrush(Color.FromArgb(0xFF, 0x40, 0x40, 0x00));
            _peakStroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x1A, 0x1A));
            _topPeakFill = new SolidColorBrush(Colors.Red);
            _bottomPeakFill = new SolidColorBrush(Colors.Blue);
            _inactiveColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x27, 0x27, 0x26));
            
            _hLineBrush.Freeze();
            _baselineFill.Freeze();
            _peakStroke.Freeze();
            _topPeakFill.Freeze();
            _bottomPeakFill.Freeze();
            _inactiveColor.Freeze();

            for (int index = 0; index <= 400; index++)
            {
                _brushes[index] = new SolidColorBrush(RedBlueColor(index / 400.0));
                _brushes[index].Freeze();
            }
        }

        /// <summary>
        /// This method is used to calculate an RGB value as an integer from a double.
        /// </summary>
        /// <param name="forValue">The value to calculate for.</param>
        /// <returns>Returns an color representing the RGB value for the provided value.</returns>        
        private static Color RedBlueColor(double forValue)
        {
            //The final color is the combination of the RGB elements.
            return Color.FromArgb(255, CalculateRBElement(forValue, 0.7125),
                                  CalculateRBElement(forValue, 0.5), CalculateRBElement(forValue, 0.2875));
        }

        /// <summary>
        /// SlidePct is the mid-point where the max value of that color is place, in percentage points.
        /// </summary>
        /// <param name="forValue">The value to use for the calculation.</param>
        /// <param name="slidePercent">The percentage to slide.</param>
        /// <returns>Returns a value from 0 to 255.</returns>        
        private static byte CalculateRBElement(double forValue, double slidePercent)
        {
            byte result;
            double adjustedValue;                                               // The main adjusted value.

            try
            {
                adjustedValue = Math.Abs(forValue - slidePercent) * 1166 - 146; // Calculate the value.

                if (adjustedValue > 255.0)                                      // If we're too high.
                    result = 255;                                               // Use the max.
                else if (adjustedValue < 0.0)                                   // If we're too low.
                    result = 0;                                                 // Use the min.
                else                                                            // Otherwise...
                    result = Convert.ToByte(adjustedValue);                     // Use the adjusted value.                
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CRBE - " + ex.Message);
                //Use the adjusted value.
                result = Convert.ToByte(0);
            }

            return result;
        }

        #endregion

        #region Initialization / Finalization

        /// <summary>
        /// Creates a new Spectrum instance.
        /// </summary>
        public SentimentViewer()
        {
            InitializeComponent();

            // Set up event handlers.
            Unloaded += HandleUnloaded;
            Loaded += HandleSpectrumSizeChange;
            SizeChanged += HandleSpectrumSizeChange;
            IsVisibleChanged += Spectrum_IsVisibleChanged;

            // Create the storyboards.
            CreateStoryBoards();
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Unloaded -= HandleUnloaded;
                Loaded -= HandleSpectrumSizeChange;
                SizeChanged -= HandleSpectrumSizeChange;
                IsVisibleChanged -= Spectrum_IsVisibleChanged; 
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HUl - " + ex.Message);
            }
        }

        /// <summary>
        /// Creates the StoryBoard elements to display the "Loading" message.
        /// </summary>
        private void CreateStoryBoards()
        {
            GradientStop stop1;
            GradientStop stop2;
            GradientStop stop3;

            try
            {
                // Create a NameScope for the page so that
                // Storyboards can be used.
                NameScope.SetNameScope(this, new NameScope());

                LinearGradientBrush rectBrush = new LinearGradientBrush();
                rectBrush.StartPoint = new Point(-0.25, 0.5);
                rectBrush.EndPoint = new Point(1.25, 0.5);
                stop1 = new GradientStop(Color.FromArgb(0x9F, 0x77, 0x77, 0x77), -1);
                stop2 = new GradientStop(Color.FromArgb(0x9F, 0xAA, 0xAA, 0xAA), -0.5);
                stop3 = new GradientStop(Color.FromArgb(0x9F, 0x77, 0x77, 0x77), 0);
                RegisterName("stop1", stop1);
                RegisterName("stop2", stop2);
                RegisterName("stop3", stop3);
                rectBrush.GradientStops.Add(stop1);
                rectBrush.GradientStops.Add(stop2);
                rectBrush.GradientStops.Add(stop3);

                _rectLoading = new Rectangle()
                {
                    Visibility = Visibility.Hidden,
                    Stroke = new SolidColorBrush(Color.FromArgb(0x9F, 0x00, 0x00, 0x00)),
                    Fill = rectBrush
                };

                TextBlock vbText = new TextBlock()
                {
                    Foreground = new SolidColorBrush(Color.FromArgb(0xD8, 0x00, 0x00, 0x00)),
                    Text = "Loading...",
                    FontFamily = new FontFamily("Arial"),
                    Padding = new Thickness(0)
                };

                _vbLoading = new Viewbox()
                {
                    Visibility = Visibility.Hidden,
                    Child = vbText
                };

                _story = new Storyboard();
                _story.RepeatBehavior = RepeatBehavior.Forever;
                Timeline.SetDesiredFrameRate(_story, 15);
                
                DoubleAnimation animation;
                animation = new DoubleAnimation()
                {
                    From = -1,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(2),
                    BeginTime = TimeSpan.FromSeconds(0)
                };

                Storyboard.SetTargetName(animation, "stop1");
                Storyboard.SetTargetProperty(animation, new PropertyPath(GradientStop.OffsetProperty));
                _story.Children.Add(animation);

                animation = new DoubleAnimation()
                {
                    From = -0.5,
                    To = 1.5,
                    Duration = TimeSpan.FromSeconds(2),
                    BeginTime = TimeSpan.FromSeconds(0)
                };

                Storyboard.SetTargetName(animation, "stop2");
                Storyboard.SetTargetProperty(animation, new PropertyPath(GradientStop.OffsetProperty));
                _story.Children.Add(animation);

                animation = new DoubleAnimation()
                {
                    From = 0,
                    To = 2,
                    Duration = TimeSpan.FromSeconds(2),
                    BeginTime = TimeSpan.FromSeconds(0)
                };

                Storyboard.SetTargetName(animation, "stop3");
                Storyboard.SetTargetProperty(animation, new PropertyPath(GradientStop.OffsetProperty));
                _story.Children.Add(animation);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CSBs - " + ex.Message);
            }
        }

        #endregion

        #region Object Setup

        /// <summary>
        /// Handles visibility changes.
        /// </summary>
        private void Spectrum_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the spectrum is now visible, refresh it.
            Refresh(true);
        }

        /// <summary>
        /// Handles spectrum size changes.
        /// </summary>
        private void HandleSpectrumSizeChange(object sender, RoutedEventArgs e)
        {
            SetupSpectrum();
        }

        /// <summary>
        /// Set up the spectrum as necessary.
        /// </summary>
        private void SetupSpectrum()
        {
            try
            {
                // If the bounds changed, rearrange everything.
                cvMain.Children.Clear();

                // Add the horizontal lines.
                for (int index = 1; index < 10; index++)
                {
                    AddHorizontalLine(index);
                    AddHorizontalLine(index + 10);
                }

                // Add the vertical lines.
                Array.Clear(_lines, 0, SentimentEventArgs.TotalBars);
                for (int i = 0; i < SentimentEventArgs.TotalBars; i += SkipBars)
                    AddVerticalLine(i);

                // Add the center line.
                AddCenterLine();

                // Add and update the peaks.
                _topPeak = CreatePeak(_topPeakFill);
                cvMain.Children.Add(_topPeak);

                _bottomPeak = CreatePeak(_bottomPeakFill);
                Canvas.SetTop(_bottomPeak, ActualHeight - _topPeak.Height);
                cvMain.Children.Add(_bottomPeak);

                UpdatePeaks();

                // Arrange the storyboards.
                ArrangeStoryBoards();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":SuS - " + ex.Message);
            }
        }

        /// <summary>
        /// Adds a horizontal line at the index supplied.
        /// </summary>
        /// <param name="index">The index to add at.</param>
        private void AddHorizontalLine(int index)
        {
            Line hLine;

            try
            {
                // Create the line for this index.
                hLine = new Line();
                hLine.Stroke = _hLineBrush;
                hLine.StrokeThickness = 1;
                hLine.X1 = 0;
                hLine.X2 = ActualWidth;
                hLine.Y1 = ActualHeight * (index / 20.0);
                hLine.Y2 = hLine.Y1;

                // Add to the canvas.
                cvMain.Children.Add(hLine);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":AHL - " + ex.Message);
            }
        }

        /// <summary>
        /// Creates the center line.
        /// </summary>
        private void AddCenterLine()
        {
            Rectangle centerLine;
            double height;

            try
            {
                height = Math.Max(Math.Min(ActualHeight / 40.0, 3), 1);

                // Create the center line object.
                centerLine = new Rectangle()
                {
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0,
                    StrokeLineJoin = PenLineJoin.Round,
                    Height = height,
                    RadiusY = height / 2,
                    RadiusX = height / 2,
                    Fill = _baselineFill,
                    Width = ActualWidth
                };

                // Set the location on the canvas and add to it.
                Canvas.SetTop(centerLine, (ActualHeight - centerLine.Height) / 2);
                cvMain.Children.Add(centerLine);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":ACL - " + ex.Message);
            }

        }

        /// <summary>
        /// Adds a new vertical line for the column supplied.
        /// </summary>
        /// <param name="index">The array index to tie to.</param>
        private void AddVerticalLine(int index)
        {
            Rectangle line;
            double columnIndex;
            double scaledWidth;
            int effectiveBars;
            int center;

            try
            {
                // Calculate the number of bars displayed.
                effectiveBars = (SentimentEventArgs.TotalBars - 1) / SkipBars + 1;
                center = effectiveBars / 2;

                // Calculate the column index for this array index.
                if ((index / SkipBars) % 2 == 0)
                    columnIndex = center + (index / SkipBars) / 2;
                else
                    columnIndex = center - ((index / SkipBars) + 1) / 2;

                // Scaled width of the bar to use.
                scaledWidth = ActualWidth / effectiveBars;

                // Creates a new rectangle to display the bar with.
                line = new Rectangle()
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.04 * scaledWidth,
                    RadiusX = scaledWidth / 2.0,
                    RadiusY = scaledWidth / 2.0,
                    Fill = Brushes.Lime,
                    Width = scaledWidth
                };

                // Set the left location of the line.
                Canvas.SetLeft(line, columnIndex * scaledWidth);

                // Add the line to the internal reference list.
                _lines[index] = line;

                UpdateLine(line, index);                                        // Update the line.
                cvMain.Children.Add(line);                                      // Add the line to the display.
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":AVL - " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a new peak with the brush supplied.
        /// </summary>
        /// <param name="brush">The brush to create the peak for.</param>
        /// <returns>The new peak.</returns>
        private Ellipse CreatePeak(SolidColorBrush brush)
        {
            Ellipse peak;

            try
            {
                // Create the peak with the given brush.
                peak = new Ellipse()
                {
                    Fill = brush,
                    Visibility = Visibility.Hidden,
                    Stroke = _peakStroke,
                    StrokeThickness = ActualWidth * 0.032 * 0.075,
                    Height = ActualWidth * 0.032,
                    Width = ActualWidth * 0.032
                };

                // Set the location of the peak.
                Canvas.SetLeft(peak, (ActualWidth - peak.Width) / 2.0);

                // Return the peak.
                return peak;
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CP - " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Arranges the storyboards within the Spectrum.
        /// </summary>
        private void ArrangeStoryBoards()
        {
            try
            {
                // Arrange the loading rectangle.
                _rectLoading.Height = ActualHeight * 0.15;
                _rectLoading.Width = ActualWidth * 0.75;
                _rectLoading.RadiusX = ActualWidth * 0.03;
                _rectLoading.RadiusY = ActualWidth * 0.03;
                Canvas.SetLeft(_rectLoading, (ActualWidth - _rectLoading.Width) / 2.0);
                Canvas.SetTop(_rectLoading, (ActualHeight - _rectLoading.Height) / 2.0);

                // Arrange the loading text.
                _vbLoading.Height = ActualHeight * 0.08;
                _vbLoading.Width = ActualWidth * 0.30;
                Canvas.SetLeft(_vbLoading, (ActualWidth - _vbLoading.Width) / 2.0);
                Canvas.SetTop(_vbLoading, (ActualHeight - _vbLoading.Height) / 2.0);

                // Add to the canvas.
                cvMain.Children.Add(_rectLoading);
                cvMain.Children.Add(_vbLoading);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":ASBs - " + ex.Message);
            }
        }

        #endregion

        #region Spectrum Functions

        /// <summary>
        /// This method is used to update the current spectrum from the provided values.
        /// </summary>
        /// <param name="sentiment">The sentiment values to update from.</param>      
        public void UpdateSpectrum(SentimentEventArgs sentiment)
        {
            bool changed;                                                       // Have the values changed?
            int peaking = 0;                                                    // Is the spectrum peaking?

            try
            {
                if (sentiment is null)
                {
                    // If no sentiment values, clear the spectrum and return.
                    ClearSpectrum();
                    return;
                }

                // Changed if the "Dirty" value changed.
                changed = _isDirty != sentiment.IsDirty;

                // Set the dirty flag.
                _isDirty = sentiment.IsDirty;

                // Check if peaking.
                if (sentiment.Lengths[0] > 0.95)
                    peaking = 1;
                else if (sentiment.Lengths[0] < -0.95)
                    peaking = -1;

                // If we have new values, set the peaking state.
                if (Peaking != peaking)
                {
                    changed = true;
                    Peaking = peaking;
                }                

                for (int index = 0; index <= SentimentEventArgs.TotalBars - 1; index++)
                {
                    //Go through the bars, update the values.
                    if (Math.Abs(_lengths[index] - sentiment.Lengths[index]) > LengthSensitivity)
                    {
                        //If values changed, something has changed.
                        changed = true;
                        //Set the new values.
                        _lengths[index] = sentiment.Lengths[index];
                    }
                    if (Math.Abs(_colors[index] - sentiment.Colors[index]) > ColorSensitivity)
                    {
                        //If values changed, something has changed.
                        changed = true;
                        //Set the new values.
                        _colors[index] = sentiment.Colors[index];
                    }
                }

                // If a value changed, refresh self.
                if (changed)
                    Refresh(true);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":USpc - " + ex.Message);
            }
        }

        /// <summary>
        /// This method is used to clear the current spectrum.
        /// </summary>
        public void ClearSpectrum()
        {
            try
            {
                Array.Clear(_lengths, 0, _lengths.Length);                      // Clear the lengths.
                Array.Clear(_colors, 0, _colors.Length);                        // Clear the colors.
                Peaking = 0;                                                    // Not peaking.
                Refresh(true);                                                  // Refresh the control.
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CSpc - " + ex.Message);
            }
        }

        /// <summary>
        /// This method is used to refresh the spectrum.
        /// </summary>
        /// <param name="peaks">Should the peaks be refreshed?</param>
        public void Refresh(bool peaks = false)
        {
            // If not visible, Don't do anything.
            if (!IsVisible)
                return;

            try
            {
                // Begin initialization.
                BeginInit();

                // If updating peaks, update the peaks.
                if (peaks)
                    UpdatePeaks();

                // Go through the lines and, set the new values.
                for (int index = 0; index < SentimentEventArgs.TotalBars; index++)
                    UpdateLine(_lines[index], index);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":R - " + ex.Message);
            }
            finally
            {
                //End initialization.
                EndInit();
            }
        }

        /// <summary>
        /// This property is used to determine if this spectrum is peaking.
        /// </summary>
        public int Peaking { get; private set; } = 0;

        /// <summary>
        /// Updates the peak displays.
        /// </summary>
        private void UpdatePeaks()
        {
            try
            {
                // If no peaks, just return.
                if (_topPeak is null)
                    return;
                                
                switch (Peaking)
                {
                    case 1:
                        //If top peaking, show top peak.
                        _topPeak.Visibility = Visibility.Visible;
                        _bottomPeak.Visibility = Visibility.Hidden;
                        break;
                    case -1:
                        //If bottom peaking, show bottom peak.
                        _topPeak.Visibility = Visibility.Hidden;
                        _bottomPeak.Visibility = Visibility.Visible;
                        break;
                    default:
                        //If not peaking, hide peaks.
                        _topPeak.Visibility = Visibility.Hidden;
                        _bottomPeak.Visibility = Visibility.Hidden;
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":UdPs - " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the line at the index given.
        /// </summary>
        /// <param name="line">The spectrum line to update.</param>
        /// <param name="barIndex">The bar index of the line to update.</param>
        private void UpdateLine(Rectangle line, int barIndex)
        {
            try
            {
                // If no line to update, just return.
                if (line is null)
                    return;

                if (_lengths[barIndex] < 0)
                {
                    // If bar is below, set the height and top from center.
                    line.Height = -0.5 * ActualHeight * _lengths[barIndex];
                    Canvas.SetTop(line, ActualHeight / 2.0);
                }
                else
                {
                    // If the bar is above, set the height and top offset.
                    line.Height = 0.5 * ActualHeight * _lengths[barIndex];
                    Canvas.SetTop(line, ActualHeight / 2.0 - line.Height);
                }

                if (!_isDirty)
                {
                    //If this control is active, calculate color, calculate the new color.                            
                    line.Fill = _brushes[(int)((_colors[barIndex] + 1) * 200)];
                }
                else
                {
                    //Otherwise, use the inactive color.
                    line.Fill = _inactiveColor;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":UdLn - " + ex.Message);
            }
        }       

        private void HandleSentimentUpdated(object sender, SentimentEventArgs sentiment)
        {
            try
            {
                // Update the spectrum.              
                UpdateSpectrum(sentiment);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HSPCh - " + ex.Message);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of bars to skip to get to the next bar in the display.
        /// </summary>
        public int SkipBars
        {
            get => (int)GetValue(SkipBarsProperty);
            set => SetValue(SkipBarsProperty, value);
        }

        /// <summary>         
        /// Using a DependencyProperty as the backing store for SkipBars. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SkipBarsProperty =
            DependencyProperty.Register("SkipBars", typeof(int), typeof(SentimentViewer),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange, OnSkipBarsChanged));

        /// <summary>
        /// Handles updates related to SkipBars value changes.
        /// </summary>
        /// <param name="d">The dependency object that triggered the update.</param>
        /// <param name="e">The change event arguments.</param>
        public static void OnSkipBarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SentimentViewer)d).SetupSpectrum();
        }

        /// <summary>
        /// The sensitivity to changes for length bars (must be positive and less than 0.1).
        /// </summary>
        public double LengthSensitivity
        {
            get => (double)GetValue(LengthSensitivityProperty);
            set => SetValue(LengthSensitivityProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for LengthSensitivity.  This enables animation, styling, binding, etc...         
        /// </summary>
        public static readonly DependencyProperty LengthSensitivityProperty =
            DependencyProperty.Register("LengthSensitivity", typeof(double), typeof(SentimentViewer), new PropertyMetadata(0.005));

        /// <summary>
        /// The sensitivity to changes for color bars (must be positive and less than 0.1).
        /// </summary>
        public double ColorSensitivity
        {
            get => (double)GetValue(ColorSensitivityProperty);
            set => SetValue(ColorSensitivityProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ColorSensitivity.  This enables animation, styling, binding, etc...         
        /// </summary>
        public static readonly DependencyProperty ColorSensitivityProperty =
            DependencyProperty.Register("ColorSensitivity", typeof(double), typeof(SentimentViewer), new PropertyMetadata(0.005));

        /// <summary>
        /// The sentiment values to update.
        /// </summary>
        public Subscription<SentimentEventArgs> Values
        {
            get { return (Subscription<SentimentEventArgs>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Values.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(Subscription<SentimentEventArgs>), typeof(SentimentViewer), 
                                        new PropertyMetadata(null, HandleSentimentChange));

        /// <summary>
        /// Called whenever the Sentiment property changes.
        /// </summary>
        /// <param name="obj">The object that the property changed on.</param>
        /// <param name="args">The change arguments (old and new values).</param>
        private static void HandleSentimentChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!(obj is SentimentViewer viewer))
                return;

            if (args.OldValue is Subscription<SentimentEventArgs> oldSentiment)
                oldSentiment.Updated -= viewer.HandleSentimentUpdated;

            if (args.NewValue is Subscription<SentimentEventArgs> newSentiment)
                newSentiment.Updated += viewer.HandleSentimentUpdated;
        }        

        /// <summary>
        /// Has the data been retrieved?
        /// </summary>
        public bool DataRetrieved
        {
            get => _dataRetrieved;
            set
            {
                try
                {
                    if (_dataRetrieved != value)                                // If it's a new value.
                    {
                        _dataRetrieved = value;                                 // Set the new value.                        

                        if (_story is object)
                        {
                            if (_dataRetrieved)                                 // If data not loading...
                            {
                                if (_storyboardRunning)                         // If we have storyboard.
                                {
                                    _story.Stop(this);                          // Stop storyboard.                                    
                                    _storyboardRunning = false;                 // No longer running.
                                }

                                _vbLoading.Visibility = Visibility.Hidden;      // Hide Loading label.
                                _rectLoading.Visibility = Visibility.Hidden;    // Hide rectangle.
                            }
                            else
                            {
                                _vbLoading.Visibility = Visibility.Visible;     // Show Loading label.
                                _rectLoading.Visibility = Visibility.Visible;   // Show rectangle.

                                if (!_storyboardRunning)                        // If we have storyboard.
                                {
                                    _storyboardRunning = true;                  // Storyboard is running.                                    
                                    _story.Begin(this, true);                   // Begin storyboard.
                                }
                            }
                        }

                        Refresh();                                              // Update the display.
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":IDLs - " + ex.Message);
                }
            }
        }

        #endregion
    }
}
