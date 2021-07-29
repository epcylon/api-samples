using QuantGate.API.Proto.Stealth;
using QuantGateAPI.API.Subscriptions;
using System;

namespace BridgeRock.CSharpExample.API.Values
{
    public class StrategyValues : ValueBase
    {
        private DateTime _timestamp;
        private double _entryProgress;
        private double _exitProgress;
        private StrategySignal _signal;
        private double _perceptionLevel;
        private GaugeSignal _perceptionSignal;
        private double _commitmentLevel;
        private GaugeSignal _commitmentSignal;
        private double _equilibriumLevel;
        private GaugeSignal _equilibriumSignal;
        private double _sentimentLevel;
        private GaugeSignal _sentimentSignal;

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

        public double PerceptionLevel
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

        public double CommitmentLevel
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

        public double EquilibriumLevel
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

        public double SentimentLevel
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
