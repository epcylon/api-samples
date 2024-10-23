using Epcylon.Common.Logging;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using System.Globalization;

namespace QuantGate.API.Signals.Subscriptions;

internal class FuturesListSubscription(APIClient client, EventHandler<FuturesListEventArgs> handler,
                                       string streamID, string underlying, string currency,
                                       bool receipt = false, uint throttleRate = 0, object reference = null) :
    SubscriptionBase<SingleDefinitionUpdate, FuturesListEventArgs>(client, SingleDefinitionUpdate.Parser, handler,
        new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnFutures,
                              streamID, underlying, securityType: "FUT", currency: currency).Destination,
                              receipt, throttleRate, reference)
{
    /// <summary>
    /// Module-level Identifier.
    /// </summary>
    private const string _moduleID = "FLSub";

    private readonly string _underlying = underlying;
    private readonly string _currency = currency;
    private readonly DataStream _stream = APIClient.ToStream(streamID);

    #region Enumerations

    /// <summary>
    /// The comma-separated fields used in the option definition.
    /// </summary>
    private enum BaseFields
    {
        Underlying,
        Currency,
        MonthOffset,
        TradingClass,
        Multiplier,
        Margin,
        TradeExchange,
        ListExchange,
        CompanyName,
        Length
    }

    #endregion

    #region Internal Character Arrays and Maps

    /// <summary>
    /// Delta encoding characters.
    /// </summary>
    private static readonly char[] _deltaChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /// <summary>
    /// Future month codes.
    /// </summary>
    private static readonly char[] _futureMonthChars = "FGHJKMNQUVXZ".ToCharArray();

    /// <summary>
    /// Conversions between raw (futures) exchange codes and MICs.
    /// </summary>
    private static readonly Dictionary<string, string> _exchangeMics = new()
    {
        ["BMF"] = "BMVF",
        ["CFE"] = "XCBF",
        ["CFECRYPTO"] = "XCBC",
        ["CME"] = "XCME",
        ["CMECRYPTO"] = "XCMC",
        ["EEX"] = "XEEE",
        ["FTA"] = "ALXA",
        ["ICEEU"] = "IFEU",
        ["MONEP"] = "XPAR",
        ["SGX"] = "XSES",
        ["SMFE"] = "SMFE",
        ["SOFFEX"] = "XSFX",
        ["CDE"] = "XMOD",
        ["DTB"] = "XEUR",
        ["ECBOT"] = "XCBT",
        ["GLOBEX"] = "GLBX",
        ["NYBOT"] = "IFUS",
        ["NYMEX"] = "XNYM",
        ["NYSELIFFE"] = "XNLI",
    };

    #endregion

    #region Encoding Constants

    /// <summary>
    /// Base symbol encoding field delimiter.
    /// </summary>
    private const char _baseDelimiter = ',';
    /// <summary>
    /// Character used when the list exchange matches the trade exchange.
    /// </summary>
    private const string _exchangeMatchString = "=";
    /// <summary>
    /// Symbol used to split a trading class from its multiplier.
    /// </summary>
    private const char _tradingClassDelimiter = ':';
    /// <summary>
    /// Symbol used to split a symbol's root from its exchange postfix.
    /// </summary>
    private const char _tradingClassSplit = '-';
    /// <summary>
    /// Array version of symbol used to split root from exchange postfix.
    /// </summary>
    private static readonly char[] _tradingClassSplitArray = [_tradingClassSplit];

    /// <summary>
    /// Character used to indicate that the security type is a future with an offset expiry pattern.
    /// </summary>
    private const string _futureOptionOffsetDelimiter = "O,";
    /// <summary>
    /// String used as a replacement in a company name for the trading class character.
    /// </summary>
    private const string _baseDelimiterReplacement = ";;";

    /// <summary>
    /// The default currency used for an option.
    /// </summary>
    private const string _defaultCurrency = "USD";
    /// <summary>
    /// Base date/time for expiries encoding.
    /// </summary>
    private static readonly DateTime _1800 = new(1800, 1, 1);

    #endregion

    protected override FuturesListEventArgs HandleUpdate(SingleDefinitionUpdate update, object processed)
    {
        try
        {
            return new FuturesListEventArgs(_underlying, _currency, _stream, Decode(update.Definition));
        }
        catch (Exception ex)
        {
            SharedLogger.LogException(_moduleID + ":HUd", ex);
            return new FuturesListEventArgs(_underlying, _currency, _stream,
                        new SubscriptionError("Internal error handling update.", ex.Message));
        }
        finally
        {
            Unsubscribe();
        }
    }

    /// <summary>
    /// Decodes a list of futures security details from an encoded string.
    /// </summary>
    /// <param name="toDecode">The encoded list of futures as a string.</param>
    /// <returns>The decoded futures security details.</returns>
    public static List<InstrumentBase> Decode(string toDecode)
    {
        List<InstrumentBase> toReturn = [];
        string[] fields;
        string baseString, detailString, codedExpiry;
        DateTime fullExpiry = _1800;
        string postfix = string.Empty;
        int lastSplitPoint = 0;
        int nextSplitPoint;

        try
        {
            if (string.IsNullOrEmpty(toDecode))
                return toReturn;

            fields = toDecode.Split([_tradingClassDelimiter], 2);
            baseString = fields[0];
            detailString = fields[1];

            DecodeBaseFuture(baseString, out InstrumentBase baseFuture, out int futureMonthOffset);

            if (baseFuture.Underlying.Contains(_tradingClassSplit))
                postfix = _tradingClassSplit + baseFuture.Underlying.Split(_tradingClassSplitArray)[1];

            nextSplitPoint = detailString.IndexOfAny(_deltaChars, 1);
            while (nextSplitPoint != -1)
            {
                codedExpiry = detailString[lastSplitPoint..nextSplitPoint];
                toReturn.Add(DecodeFuture(baseFuture, ref fullExpiry, codedExpiry, futureMonthOffset, postfix));

                lastSplitPoint = nextSplitPoint;
                nextSplitPoint = detailString.IndexOfAny(_deltaChars, lastSplitPoint + 1);
            }

            codedExpiry = detailString[lastSplitPoint..];
            toReturn.Add(DecodeFuture(baseFuture, ref fullExpiry, codedExpiry, futureMonthOffset, postfix));
        }
        catch (Exception ex)
        {
            //log the exception.
            SharedLogger.LogException(_moduleID + ":Dec", ex);
        }

        return toReturn;
    }

    private static InstrumentBase DecodeFuture(InstrumentBase baseFuture, ref DateTime fullExpiry,
                                               string expiryCode, int futureMonthOffset, string postfix)
    {
        string symbol;
        int high = 0;
        int daysOffset;
        DateTime adjustedExpiry;

        try
        {
            //If there is a high portion, parse it.
            if (expiryCode.Length > 1)
                _ = int.TryParse(expiryCode.AsSpan(1), out high);

            //Calculate the full offset..
            daysOffset = high * _deltaChars.Length + Array.IndexOf(_deltaChars, expiryCode[0]);
            // Update the expiry by the offset.
            fullExpiry = fullExpiry.AddDays(daysOffset);

            // Get the expiry adjusted for the future month offset.
            adjustedExpiry = fullExpiry.AddMonths(futureMonthOffset);
            // Calculate the symbol.
            symbol = baseFuture.Symbol + " " + _futureMonthChars[adjustedExpiry.Month - 1] +
                     adjustedExpiry.ToString("yyyy")[3] + postfix;

            // Return the full instrument base information for the future.
            return new InstrumentBase(symbol, baseFuture.Underlying, baseFuture.Currency,
                                      baseFuture.Exchange, baseFuture.InstrumentType,
                                      baseFuture.PutOrCall, baseFuture.Strike, fullExpiry.AddDays(0),
                                      baseFuture.Multiplier, baseFuture.DisplayName);
        }
        catch (Exception ex)
        {
            SharedLogger.LogException(_moduleID + ":DCF", ex);
            return null;
        }
    }

    private static void DecodeBaseFuture(string toDecrypt, out InstrumentBase baseDefinition, out int futureMonthOffset)
    {
        string[] fields;
        string field;
        string tradingClass;
        string currency = _defaultCurrency;
        string tradeExchange = "GLOBEX";
        string listExchange;
        string companyName;

        try
        {
            fields = toDecrypt.Split(_baseDelimiter);

            string underlying = fields[(int)BaseFields.Underlying];

            if (!string.IsNullOrEmpty(fields[(int)BaseFields.Currency]))
                currency = fields[(int)BaseFields.Currency];

            //Try to get the future month offset.
            field = fields[(int)BaseFields.MonthOffset];
            if (!string.IsNullOrEmpty(field))
            {
                if (_futureOptionOffsetDelimiter.StartsWith(field))
                    futureMonthOffset = 1;
                else
                    _ = int.TryParse(fields[(int)BaseFields.MonthOffset], out futureMonthOffset);
            }
            else
            {
                futureMonthOffset = 0;
            }

            //If the trading class is not specified, it matches the underlying.
            if (string.IsNullOrEmpty(fields[(int)BaseFields.TradingClass]))
                tradingClass = underlying.Split(_tradingClassSplitArray)[0];
            else
                tradingClass = fields[(int)BaseFields.TradingClass];

            double.TryParse(fields[(int)BaseFields.Multiplier], NumberStyles.Any,
                            CultureInfo.InvariantCulture, out double multiplier);
            double.TryParse(fields[(int)BaseFields.Margin], NumberStyles.Any,
                            CultureInfo.InvariantCulture, out double margin);

            if (!string.IsNullOrEmpty(fields[(int)BaseFields.TradeExchange]))
                tradeExchange = fields[(int)BaseFields.TradeExchange];
            tradeExchange = GetMIC(tradeExchange);

            if (string.IsNullOrEmpty(fields[(int)BaseFields.ListExchange]) ||
                _exchangeMatchString.Equals(fields[(int)BaseFields.ListExchange]))
            {
                listExchange = tradeExchange;
            }
            else
            {
                listExchange = fields[(int)BaseFields.ListExchange];
                listExchange = GetMIC(listExchange);
            }

            companyName = fields[(int)BaseFields.CompanyName].
                            Replace(_baseDelimiterReplacement, _tradingClassDelimiter.ToString());

            baseDefinition = new InstrumentBase(tradingClass, underlying, currency, listExchange,
                                                InstrumentType.Future, PutOrCall.NoPutCall, 0.0,
                                                default, multiplier, companyName);
        }
        catch (Exception ex)
        {
            //log the exception.
            SharedLogger.LogException(_moduleID + ":DBF", ex);
            futureMonthOffset = 0;
            baseDefinition = null;
        }
    }

    /// <summary>
    /// Returns the MIC for the given exchange.
    /// </summary>
    /// <param name="exchange">The exchange to get the MIC for.</param>
    /// <returns>The MIC for the given exchange.</returns>
    private static string GetMIC(string exchange)
        => _exchangeMics.TryGetValue(exchange, out string mic) ? mic : exchange;

    protected override FuturesListEventArgs WrapError(SubscriptionError error)
        => new(_underlying, _currency, _stream, error);
}
