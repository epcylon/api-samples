using System;
using System.Collections.Generic;

namespace QuantGate.API.Values
{
    public class Instrument : ValueBase
    {
        private string _symbol;
        private string _underlying;
        private string _currency;
        private string _displayName;
        private string _exchange;
        private InstrumentType _instrumentType;
        private PutOrCall _putOrCall;
        private double _strike;
        private DateTime _expiryDate;
        private double _multiplier;
        private TimeZoneInfo _timeZone;
        private List<TickRange> _tickRanges = new List<TickRange>();
        private List<TradingSession> _tradingSessions = new List<TradingSession>();
        private Dictionary<string, string> _brokerSymbols = new Dictionary<string, string>();

        public string Symbol
        {
            get => _symbol;
            internal set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Underlying
        {
            get => _underlying;
            internal set
            {
                if (_underlying != value)
                {
                    _underlying = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Currency 
        {
            get => _currency;
            internal set
            {
                if (_currency != value)
                {
                    _currency = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string DisplayName
        {
            get => _displayName;
            internal set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Exchange 
        {
            get => _exchange;
            internal set
            {
                if (_exchange != value)
                {
                    _exchange = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public InstrumentType InstrumentType 
        {
            get => _instrumentType;
            internal set
            {
                if (_instrumentType != value)
                {
                    _instrumentType = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public PutOrCall PutOrCall 
        {
            get => _putOrCall;
            internal set
            {
                if (_putOrCall != value)
                {
                    _putOrCall = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Strike 
        {
            get => _strike;
            internal set
            {
                if (_strike != value)
                {
                    _strike = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public DateTime ExpiryDate 
        {
            get => _expiryDate;
            internal set
            {
                if (_expiryDate != value)
                {
                    _expiryDate = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Multiplier 
        {
            get => _multiplier;
            internal set
            {
                if (_multiplier != value)
                {
                    _multiplier = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TimeZoneInfo TimeZone 
        {
            get => _timeZone;
            internal set
            {
                if (_timeZone != value)
                {
                    _timeZone = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public List<TickRange> TickRanges 
        {
            get => _tickRanges;
            internal set
            {
                _tickRanges.Clear();
                _tickRanges.AddRange(value);
                NotifyPropertyChanged();
            }
        }
        public List<TradingSession> TradingSessions 
        {
            get => _tradingSessions;
            internal set
            {                
                _tradingSessions.Clear();
                _tradingSessions.AddRange(value);
                NotifyPropertyChanged();                
            }
        }
        public Dictionary<string, string> BrokerSymbols 
        {
            get => _brokerSymbols;
            internal set
            {                
                _brokerSymbols.Clear();

                foreach (KeyValuePair<string, string> pair in value)
                    _brokerSymbols.Add(pair.Key, pair.Value);

                NotifyPropertyChanged();                
            }
        }
    }
}
