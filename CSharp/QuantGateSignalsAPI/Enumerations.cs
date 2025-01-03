namespace QuantGate.API.Signals;

/*     
 *   NOTE: SYNCHRONIZE THE BELOW ENUMS WITH THE ENUMERATIONS GENERATED FROM THE PROTO FILE.
 */

/// <summary>
/// Specifies the type of streaming data source to get guages and signals for.
/// </summary>
public enum DataStream : int
{
    /// <summary>
    /// Invalid data stream (will use default).
    /// </summary>
    Invalid = -1,
    /// <summary>
    /// Data received is realtime data.
    /// </summary>
    Realtime = 0,
    /// <summary>
    /// Data received is delayed data (generally 10 or 15 min).
    /// </summary>
    Delayed = 1,
    /// <summary>
    /// Data received is simulated demo data (24/7).
    /// </summary>
    Demo = 2,
}

/// <summary>
/// The type of instrument being traded. 
/// </summary>
public enum InstrumentType
{
    /// <summary>
    /// Not a valid instrument.
    /// </summary>
    NoInstrument = 0,
    /// <summary>
    /// Common stock.
    /// </summary>
    CommonStock = 1,
    /// <summary>
    /// Stock option.
    /// </summary>
    StockOption = 2,
    /// <summary>
    /// Future.
    /// </summary>
    Future = 3,
    /// <summary>
    /// Index.
    /// </summary>
    Index = 4,
    /// <summary>
    /// Foreign Exchange (Currency).
    /// </summary>
    ForexContract = 5,
    /// <summary>
    /// Future option.
    /// </summary>
    FutureOption = 6,
    /// <summary>
    /// Future instrument (underlying).
    /// </summary>
    FutureInstrument = 7,
    /// <summary>
    /// Combo pairing.
    /// </summary>
    Combo = 8,
    /// <summary>
    /// Combo underlying.
    /// </summary>
    ComboInstrument = 9,
    /// <summary>
    /// Crypto Currency (Bitcoin, etc.)
    /// </summary>
    CryptoCurrency = 10,
}

/// <summary>
/// The Put/Call side ("right") of an option. 
/// </summary>
public enum PutOrCall
{
    /// <summary>
    /// Instrument is not an option.
    /// </summary>
    NoPutCall = 0,
    /// <summary>
    /// Put Option (option to sell at strike).
    /// </summary>
    Put = 1,
    /// <summary>
    /// Call Option (option to buy at strike).
    /// </summary>
    Call = 2,
}

/// <summary>
/// Entry/Exit signal state for strategies. 
/// </summary>
public enum StrategySignal
{
    /// <summary>
    /// No current signal.
    /// </summary>
    None = 0,
    /// <summary>
    /// Signal to enter a long position.
    /// </summary>
    LongSignal = 1,
    /// <summary>
    /// Signal to enter a short position.
    /// </summary>
    ShortSignal = 2,
}

/// <summary>
/// Gauge signal state for strategies (PCES lights). 
/// </summary>
public enum GaugeSignal
{
    /// <summary>
    /// Unknown signal (gauge signal not set - use default).
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Signal to enter a short position.
    /// </summary>
    Short = 1,
    /// <summary>
    /// No current signal.
    /// </summary>
    Flat = 2,
    /// <summary>
    /// Signal to enter a long position.
    /// </summary>
    Long = 3,
    /// <summary>
    /// Dual signal (long or short - no specific direction).
    /// </summary>
    Dual = 4,
}

/// <summary>
/// Types of tick formats available to display prices in.
/// </summary>
public enum TickFormat
{
    /// <summary>
    /// Decimal display (regular 0.000, etc.) 
    /// </summary>
    Decimal = 0,
    /// <summary>
    /// Fractional format, such as 34 1/4.
    /// In this case, the denominator that will be used for non-integer portions of the
    /// number will be that supplied with the format. Note, that the fraction is generally 
    /// displayed in its simplified format (divide numerator and denominator by GCD). 
    /// </summary>
    Fraction = 1,
    /// <summary>
    /// Tick format, such as 34'120.
    /// In this case, the value after the tick is the non-integer portion of the number,
    /// multiplied by the denominator value supplied, zero padded to the left to fit the
    /// number of digits required to display the denominator value as a full integer. 
    /// </summary>
    Tick = 2,
}
