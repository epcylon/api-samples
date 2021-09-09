using System;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Strategy values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class StrategyValues : ValueBase
    {
        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        private DateTime _timestamp;
        /// <summary>
        /// Entry progress value.
        /// </summary>
        private double _entryProgress;
        /// <summary>
        /// Exit progress value.
        /// </summary>
        private double _exitProgress;
        /// <summary>
        /// Entry signal for the strategy.
        /// </summary>
        private StrategySignal _signal;
        /// <summary>
        /// Perception level. 0 represents an unset value.
        /// </summary>
        private double? _perceptionLevel;
        /// <summary>
        /// Signal tied to the perception level.
        /// </summary>
        private GaugeSignal _perceptionSignal;
        /// <summary>
        /// Commitment level. 0 represents an unset value.
        /// </summary>
        private double? _commitmentLevel;
        /// <summary>
        /// Signal tied to the commitment level.
        /// </summary>
        private GaugeSignal _commitmentSignal;
        /// <summary>
        /// Equilibrium level. 0 represents an unset value.
        /// </summary>
        private double? _equilibriumLevel;
        /// <summary>
        /// Signal tied to the equilibrium level.
        /// </summary>
        private GaugeSignal _equilibriumSignal;
        /// <summary>
        /// Sentiment level. 0 represents an unset value.
        /// </summary>
        private double? _sentimentLevel;
        /// <summary>
        /// Signal tied to the 50t sentiment indication.
        /// </summary>
        private GaugeSignal _sentimentSignal;

        /// <summary>
        /// Symbol to get the Strategy update data for.
        /// </summary>
        public string Symbol { get; internal set; }
        /// <summary>
        /// Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0,  Crb.8.4.
        /// </summary>
        public string StrategyID { get; internal set; }

        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime TimeStamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp.Ticks != value.Ticks)
                {
                    _timestamp = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Entry progress value.
        /// </summary>
        public double EntryProgress
        {
            get => _entryProgress;
            set
            {
                if (_entryProgress != value)
                {
                    _entryProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Exit progress value.
        /// </summary>
        public double ExitProgress
        {
            get => _exitProgress;
            set
            {
                if (_exitProgress != value)
                {
                    _exitProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Entry signal for the strategy.
        /// </summary>
        public StrategySignal Signal
        {
            get => _signal;
            set
            {
                if (_signal != value)
                {
                    _signal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Perception level. 0 represents an unset value.
        /// </summary>
        public double? PerceptionLevel
        {
            get => _perceptionLevel;
            set
            {
                if (_perceptionLevel != value)
                {
                    _perceptionLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Signal tied to the perception level.
        /// </summary>
        public GaugeSignal PerceptionSignal
        {
            get => _perceptionSignal;
            set
            {
                if (_perceptionSignal != value)
                {
                    _perceptionSignal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Commitment level. 0 represents an unset value.
        /// </summary>
        public double? CommitmentLevel
        {
            get => _commitmentLevel;
            set
            {
                if (_commitmentLevel != value)
                {
                    _commitmentLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Signal tied to the commitment level.
        /// </summary>
        public GaugeSignal CommitmentSignal
        {
            get => _commitmentSignal;
            set
            {
                if (_commitmentSignal != value)
                {
                    _commitmentSignal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Equilibrium level. 0 represents an unset value.
        /// </summary>
        public double? EquilibriumLevel
        {
            get => _equilibriumLevel;
            set
            {
                if (_equilibriumLevel != value)
                {
                    _equilibriumLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Signal tied to the equilibrium level.
        /// </summary>
        public GaugeSignal EquilibriumSignal
        {
            get => _equilibriumSignal;
            set
            {
                if (_equilibriumSignal != value)
                {
                    _equilibriumSignal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sentiment level. 0 represents an unset value.
        /// </summary>
        public double? SentimentLevel
        {
            get => _sentimentLevel;
            set
            {
                if (_sentimentLevel != value)
                {
                    _sentimentLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Signal tied to the 50t sentiment indication.
        /// </summary>
        public GaugeSignal SentimentSignal
        {
            get => _sentimentSignal;
            set
            {
                if (_sentimentSignal != value)
                {
                    _sentimentSignal = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
