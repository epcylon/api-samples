using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using QuantGate.API.Signals.Events;
using System;
using System.Diagnostics;

namespace BridgeRock.MauiExample.Controls
{
    public partial class SentimentViewer : Grid
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SVwr";

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

        #endregion

        #region Static Brush Setup

        /// <summary>
        /// Creates the static brushes when the first item is created.
        /// </summary>
        static SentimentViewer()
        {
            _hLineBrush = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            _baselineFill = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x00));
            _peakStroke = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));
            _topPeakFill = new SolidColorBrush(Colors.Red);
            _bottomPeakFill = new SolidColorBrush(Colors.Blue);
        }

        #endregion

        #region Spectrum Variables

        /// <summary>
        /// The spectrum bars to display.
        /// </summary>
        private readonly SentimentBar[] _lines = new SentimentBar[SentimentEventArgs.TotalBars];

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

        private double _lastWidth = double.NaN;

        //private Storyboard _story = default;
        //private Rectangle _rectLoading;
        //private Viewbox _vbLoading;

        /// <summary>
        /// Is the (loading) storyboard currently running?
        /// </summary>
       //private bool _storyboardRunning = false;

        #endregion

        #region Initialization / Finalization

        /// <summary>
        /// Creates a new SentimentViewer instance.
        /// </summary>
        public SentimentViewer()
        {
            InitializeComponent();

            // Set up event handlers.
            //Loaded += HandleSpectrumSizeChange;
            //IsVisibleChanged += Spectrum_IsVisibleChanged;

            // Create the storyboards.
            //CreateStoryBoards();
        }

        #endregion

        #region Object Setup

        ///// <summary>
        ///// Handles visibility changes.
        ///// </summary>
        //private void Spectrum_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    // If the spectrum is now visible, refresh it.
        //    Refresh(true);
        //}

        protected override void OnClear()
        {
            Refresh(true);
            base.OnClear();
        }

        protected override Size ArrangeOverride(Microsoft.Maui.Graphics.Rectangle bounds)
        {
            if (bounds.Width > 0 && bounds.Width != _lastWidth)
            {
                SetupSpectrum(bounds);
                _lastWidth = bounds.Width;
            }
            return base.ArrangeOverride(bounds);
        }

        ///// <summary>
        ///// Handles spectrum size changes.
        ///// </summary>
        //private void HandleSpectrumSizeChange(object sender, RoutedEventArgs e)
        //{
        //    SetupSpectrum();
        //}

        /// <summary>
        /// Set up the spectrum as necessary.
        /// </summary>
        private void SetupSpectrum(Microsoft.Maui.Graphics.Rectangle bounds)
        {
            int effectiveBars;

            try
            {
                // If the bounds changed, rearrange everything.
                sBars.Children.Clear();

                // Add the column definitions.
                //gBars.ColumnSpacing = 0;
                //gBars.ColumnDefinitions.Clear();
                effectiveBars = (SentimentEventArgs.TotalBars - 1) / SkipBars + 1;
                for (int index = 0; index < effectiveBars; index++)
                    sBars.Children.Add(new SentimentBar { WidthRequest = (long)bounds.Width / effectiveBars });

                // Add the vertical lines.
                Array.Clear(_lines, 0, SentimentEventArgs.TotalBars);
                for (int i = 0; i < SentimentEventArgs.TotalBars; i += SkipBars)
                    AddVerticalLine(i);

                // Add the center line.
                AddCenterLine();

                // Add and update the peaks.
                //_topPeak = CreatePeak(_topPeakFill);
                //cvMain.Children.Add(_topPeak);

                //_bottomPeak = CreatePeak(_bottomPeakFill);
                //Canvas.SetTop(_bottomPeak, ActualHeight - _topPeak.Height);
                //cvMain.Children.Add(_bottomPeak);

                //UpdatePeaks();

                // Arrange the storyboards.
                //ArrangeStoryBoards();
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
            //Line hLine;

            try
            {
                //    // Create the line for this index.
                //    hLine = new Line
                //    {
                //        Stroke = _hLineBrush,
                //        StrokeThickness = 1,
                //        X1 = 0,
                //        X2 = ActualWidth,
                //        Y1 = ActualHeight * (index / 20.0)
                //    };
                //    hLine.Y2 = hLine.Y1;

                //    // Add to the canvas.
                //    cvMain.Children.Add(hLine);
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
            //Rectangle centerLine;
            //double height;

            //try
            //{
            //    height = Math.Max(Math.Min(ActualHeight / 40.0, 3), 1);

            //    // Create the center line object.
            //    centerLine = new Rectangle()
            //    {
            //        Stroke = Brushes.Transparent,
            //        StrokeThickness = 0,
            //        StrokeLineJoin = PenLineJoin.Round,
            //        Height = height,
            //        RadiusY = height / 2,
            //        RadiusX = height / 2,
            //        Fill = _baselineFill,
            //        Width = ActualWidth
            //    };

            //    // Set the location on the canvas and add to it.
            //    Canvas.SetTop(centerLine, (ActualHeight - centerLine.Height) / 2);
            //    cvMain.Children.Add(centerLine);
            //}
            //catch (Exception ex)
            //{
            //    Trace.TraceError(_moduleID + ":ACL - " + ex.Message);
            //}

        }

        /// <summary>
        /// Adds a new vertical line for the column supplied.
        /// </summary>
        /// <param name="index">The array index to tie to.</param>
        private void AddVerticalLine(int index)
        {
            SentimentBar line;
            int columnIndex;
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

                line = (SentimentBar)sBars.Children[columnIndex];
                _lines[index] = line;

                // Update the line.
                UpdateLine(line, index);
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
                    IsVisible = false,
                    Stroke = _peakStroke,
                    StrokeThickness = 1
                };

                // Return the peak.
                return peak;
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CP - " + ex.Message);
                return null;
            }
        }

        ///// <summary>
        ///// Arranges the storyboards within the Spectrum.
        ///// </summary>
        //private void ArrangeStoryBoards()
        //{
        //    try
        //    {
        //        // Arrange the loading rectangle.
        //        _rectLoading.Height = ActualHeight * 0.15;
        //        _rectLoading.Width = ActualWidth * 0.75;
        //        _rectLoading.RadiusX = ActualWidth * 0.03;
        //        _rectLoading.RadiusY = ActualWidth * 0.03;
        //        Canvas.SetLeft(_rectLoading, (ActualWidth - _rectLoading.Width) / 2.0);
        //        Canvas.SetTop(_rectLoading, (ActualHeight - _rectLoading.Height) / 2.0);

        //        // Arrange the loading text.
        //        _vbLoading.Height = ActualHeight * 0.08;
        //        _vbLoading.Width = ActualWidth * 0.30;
        //        Canvas.SetLeft(_vbLoading, (ActualWidth - _vbLoading.Width) / 2.0);
        //        Canvas.SetTop(_vbLoading, (ActualHeight - _vbLoading.Height) / 2.0);

        //        // Add to the canvas.
        //        cvMain.Children.Add(_rectLoading);
        //        cvMain.Children.Add(_vbLoading);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceError(_moduleID + ":ASBs - " + ex.Message);
        //    }
        //}

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
                BatchBegin();

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
                BatchCommit();
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
                        _topPeak.IsVisible = true;
                        _bottomPeak.IsVisible = false;
                        break;
                    case -1:
                        //If bottom peaking, show bottom peak.
                        _topPeak.IsVisible = false;
                        _bottomPeak.IsVisible = true;
                        break;
                    default:
                        //If not peaking, hide peaks.
                        _topPeak.IsVisible = false;
                        _bottomPeak.IsVisible = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":UdPs - " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the bar at the index given.
        /// </summary>
        /// <param name="bar">The spectrum bar to update.</param>
        /// <param name="barIndex">The bar index of the line to update.</param>
        private void UpdateLine(SentimentBar bar, int barIndex)
        {
            try
            {
                // If no line to update, just return.
                if (bar is null)
                    return;

                bar.Length = _lengths[barIndex];

                if (_isDirty)
                    bar.Color = double.NaN;
                else
                    bar.Color = _colors[barIndex];
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":UdLn - " + ex.Message);
            }
        }

        #endregion

        #region Properties

        int _skipBars = 1;

        /// <summary>
        /// The number of bars to skip to get to the next bar in the display.
        /// </summary>
        public int SkipBars
        {
            get => _skipBars;
            set
            {
                if (_skipBars != value)
                {
                    _skipBars = value;
                    SetupSpectrum(this.Bounds);
                }
            }
        }

        /// <summary>
        /// The sensitivity to changes for length bars (must be positive and less than 0.1).
        /// </summary>
        public double LengthSensitivity { get; set; } = 0.005;

        /// <summary>
        /// The sensitivity to changes for color bars (must be positive and less than 0.1).
        /// </summary>
        public double ColorSensitivity { get; set; } = 0.005;

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

                        //if (_story is object)
                        //{
                        //    if (_dataRetrieved)                                 // If data not loading...
                        //    {
                        //        if (_storyboardRunning)                         // If we have storyboard.
                        //        {
                        //            _story.Stop(this);                          // Stop storyboard.                                    
                        //            _storyboardRunning = false;                 // No longer running.
                        //        }

                        //        _vbLoading.Visibility = Visibility.Hidden;      // Hide Loading label.
                        //        _rectLoading.Visibility = Visibility.Hidden;    // Hide rectangle.
                        //    }
                        //    else
                        //    {
                        //        _vbLoading.Visibility = Visibility.Visible;     // Show Loading label.
                        //        _rectLoading.Visibility = Visibility.Visible;   // Show rectangle.

                        //        if (!_storyboardRunning)                        // If we have storyboard.
                        //        {
                        //            _storyboardRunning = true;                  // Storyboard is running.                                    
                        //            _story.Begin(this, true);                   // Begin storyboard.
                        //        }
                        //    }
                        //}

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