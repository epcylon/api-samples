using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Instrument details. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient after values are received 
    /// to end the subscription.
    /// </summary>
    public class Instrument : ValueBase
    {
        /// <summary>
        /// Symbol as listed by the QuantGate servers.
        /// </summary>
        private string _symbol;
        /// <summary>
        /// Underlying symbol.
        /// </summary>
        private string _underlying;
        /// <summary>
        /// Currency the instrument is traded in.
        /// </summary>
        private string _currency;
        /// <summary>
        /// Primary exchange (ISO 10383 MIC) the instrument is traded on.
        /// </summary>
        private string _exchange;
        /// <summary>
        /// Display name of the instrument.
        /// </summary>
        private string _displayName;
        /// <summary>
        /// Type of instrument. 
        /// </summary>
        private InstrumentType _instrumentType;
        /// <summary>
        /// Right of an option, if an option (will be empty otherwise).
        /// </summary>
        private PutOrCall _putOrCall;
        /// <summary>
        /// Strike price of an option, if an option (will be zero otherwise).
        /// </summary>
        private double _strike;
        /// <summary>
        /// Expiry date of the instrument, if applicable
        /// </summary>
        private DateTime _expiryDate;
        /// <summary>
        /// Display name of the instrument.
        /// </summary>
        private double _multiplier;
        /// <summary>
        /// Time zone of the primary exchange the instrument is traded on.
        /// </summary>
        private TimeZoneInfo _timeZone;
        /// <summary>
        /// Tick ranges used to determine price levels.
        /// </summary>
        private List<TickRange> _tickRanges = new List<TickRange>();
        /// <summary>
        /// Trading session end times and lengths for each day Sunday-Saturday, specified in the exchange time_zone.
        /// </summary>
        private List<TradingSession> _tradingSessions = new List<TradingSession>();
        /// <summary>
        /// Map of broker symbols according to broker (ib, cqg, dtniq, etc.).
        /// </summary>
        private Dictionary<string, string> _brokerSymbols = new Dictionary<string, string>();

        /// <summary>
        /// Symbol as listed by the QuantGate servers.
        /// </summary>
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

        /// <summary>
        /// Underlying symbol.
        /// </summary>
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

        /// <summary>
        /// Currency the instrument is traded in.
        /// </summary>
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

        /// <summary>
        /// Primary exchange (ISO 10383 MIC) the instrument is traded on.
        /// </summary>
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

        /// <summary>
        /// Display name of the instrument.
        /// </summary>
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

        /// <summary>
        /// Type of instrument. 
        /// </summary>
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

        /// <summary>
        /// Right of an option, if an option (will be empty otherwise).
        /// </summary>
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

        /// <summary>
        /// Strike price of an option, if an option (will be zero otherwise).
        /// </summary>
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

        /// <summary>
        /// Expiry date of the instrument, if applicable
        /// </summary>
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

        /// <summary>
        /// Display name of the instrument.
        /// </summary>
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

        /// <summary>
        /// Time zone of the primary exchange the instrument is traded on.
        /// </summary>
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

        /// <summary>
        /// Tick ranges used to determine price levels.
        /// </summary>
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

        /// <summary>
        /// Trading session end times and lengths for each day Sunday-Saturday, specified in the exchange time_zone.
        /// </summary>
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

        /// <summary>
        /// Map of broker symbols according to broker (ib, cqg, dtniq, etc.).
        /// </summary>
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
