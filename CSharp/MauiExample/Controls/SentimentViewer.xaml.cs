using QuantGate.API.Signals.Events;
using System.Diagnostics;

namespace BridgeRock.MauiExample.Controls
{
    public partial class SentimentViewer : Grid
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SVwr";

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
        /// Has the data been retrieved?
        /// </summary>
        private bool _dataRetrieved = false;

        private double _lastWidth = double.NaN;       

        #endregion

        #region Initialization / Finalization

        /// <summary>
        /// Creates a new SentimentViewer instance.
        /// </summary>
        public SentimentViewer()
        {
            InitializeComponent();
        }

        #endregion

        #region Object Setup

        protected override void OnClear()
        {
            Refresh(true);
            base.OnClear();
        }

        protected override Size ArrangeOverride(Rect bounds)
        {
            if (bounds.Width > 0 && bounds.Width != _lastWidth)
            {
                SetupSpectrum(bounds);
                _lastWidth = bounds.Width;
            }
            return base.ArrangeOverride(bounds);
        }

        /// <summary>
        /// Set up the spectrum as necessary.
        /// </summary>
        private void SetupSpectrum(Rect bounds)
        {
            int effectiveBars;

            try
            {
                // If the bounds changed, rearrange everything.
                sBars.Children.Clear();

                // Add the column definitions.
                effectiveBars = (SentimentEventArgs.TotalBars - 1) / SkipBars + 1;
                for (int index = 0; index < effectiveBars; index++)
                    sBars.Children.Add(new SentimentBar { WidthRequest = (long)(bounds.Width / effectiveBars + 0.1) });

                // Add the vertical lines.
                Array.Clear(_lines, 0, SentimentEventArgs.TotalBars);
                for (int i = 0; i < SentimentEventArgs.TotalBars; i += SkipBars)
                    AddVerticalLine(i);
                
                // Update the peaks.
                UpdatePeaks();                
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":SuS - " + ex.Message);
            }
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
                if (sentiment?.Lengths is null)
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
                switch (Peaking)
                {
                    case 1:
                        //If top peaking, show top peak.
                        peakTop.IsVisible = true;
                        peakBottom.IsVisible = false;
                        break;
                    case -1:
                        //If bottom peaking, show bottom peak.
                        peakTop.IsVisible = false;
                        peakBottom.IsVisible = true;
                        break;
                    default:
                        //If not peaking, hide peaks.
                        peakTop.IsVisible = false;
                        peakBottom.IsVisible = false;
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
                    if (_dataRetrieved != value)
                    {
                        // If it's a new value, set and refresh.
                        _dataRetrieved = value;
                        Refresh();
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