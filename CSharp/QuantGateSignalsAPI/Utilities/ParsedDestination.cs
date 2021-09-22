using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QuantGate.API.Signals.Utilities
{
    /// <summary>
    /// Type of subscription requested by the client.
    /// </summary>
    internal enum SubscriptionType : long
    {
        /// <summary>
        /// Not requesting or invalid.
        /// </summary>
        None = 0x0000000000,
        /// <summary>
        /// Requesting calculation results for a gauge.
        /// </summary>
        Gauge = 0x0000000FFF,
        /// <summary>
        /// Requesting an instrument definition/search.
        /// </summary>
        Definition = 0x000000F000,
        /// <summary>
        /// Requesting a strategy stream.
        /// </summary>
        Strategy = 0xF000000000,
    }

    /// <summary>
    /// The main subscription path of the subscription requested by the client.
    /// </summary>
    internal enum SubscriptionPath : long
    {
        /// <summary>
        /// Not subscribed to anything.
        /// </summary>
        None = 0x0000000000,

        /// <summary>
        /// Sentiment gauge subscription.
        /// </summary>
        GaugeSentiment = 0x0000000001,
        /// <summary>
        /// Equilibrium gauge subscription.
        /// </summary>
        GaugeEquilibrium = 0x0000000002,
        /// <summary>
        /// Multi-time-frame gauge subscription.
        /// </summary>
        GaugeMultiframeEquilibrium = 0x0000000003,        
        /// <summary>
        /// Commitment gauge subscription.
        /// </summary>
        GaugeCommitment = 0x0000000005,
        /// <summary>
        /// Headroom gauge subscription.
        /// </summary>
        GaugeHeadroom = 0x0000000006,
        /// <summary>
        /// Book pressure gauge subscription.
        /// </summary>
        GaugeBookPressure = 0x0000000007,
        /// <summary>
        /// Trigger value subscription.
        /// </summary>
        GaugeTrigger = 0x0000000020,
        /// <summary>
        /// Perception2 gauge subscription.
        /// </summary>
        GaugePerception = 0x0000000040,
        
        /// <summary>
        /// Long instrument definition subscription.
        /// </summary>
        DefnInstrument = 0x0000002000,        
        /// <summary>
        /// Symbol search subscription.
        /// </summary>
        DefnSymbolSearch = 0x0000005000,
        /// <summary>
        /// Top symbol subscription.
        /// </summary>
        DefnTopSymbols = 0x0000006000,
    }

    /// <summary>
    /// Holds the parsed information from a subscription destination string.
    /// </summary>
    internal class ParsedDestination
    {
        /// <summary>
        /// Class-level identifier.
        /// </summary>
        private const string _moduleID = "PDfn";

        /// <summary>
        /// Used to separate destination path fields.
        /// </summary>
        private const char _separator = '/';

        /// <summary>
        /// Default stream ID (real-time).
        /// </summary>
        public const string RealtimeStreamID = "realtime";
        /// <summary>
        /// Stream ID for delayed stream.
        /// </summary>
        public const string DelayStreamID = "delay";
        /// <summary>
        /// Stream ID for 24-hour demo stream.
        /// </summary>
        public const string DemoStreamID = "demo";

        /// <summary>
        /// Holds a list of all valid subscription types.
        /// </summary>
        private static Dictionary<string, SubscriptionType> _subscriptionTypes;

        /// <summary>
        /// Holds a list of all subscription strings by subscription type.
        /// </summary>
        private static Dictionary<SubscriptionType, string> _subscriptionStringsByType;

        private static Dictionary<string, SubscriptionPath> _subscriptionPaths;

        /// <summary>
        /// Holds a list of all subscription path strings by subscription path.
        /// </summary>
        private static Dictionary<SubscriptionPath, string> _subscriptionStringsByPath;

        /// <summary>
        /// The full destination path that was parsed.
        /// </summary>
        public readonly string Destination;
        /// <summary>
        /// The type of subscription requested.
        /// </summary>
        public readonly SubscriptionType SubscriptionType;
        /// <summary>
        /// The path of the subscription requested.
        /// </summary>
        public readonly SubscriptionPath SubscriptionPath = SubscriptionPath.None;


        /// <summary>
        /// The stream identifier of the destination.
        /// </summary>
        public readonly string StreamID;
        /// <summary>
        /// The underlying security.
        /// </summary>
        public readonly string Symbol;
        /// <summary>
        /// The gauge compression requested.
        /// </summary>
        public readonly string Compression = string.Empty;
        public readonly string Broker = string.Empty;
        public readonly string SecurityType = string.Empty;
        public readonly string Term = string.Empty;
        /// <summary>
        /// The identifier of the strategy associated with strategy requests.
        /// </summary>
        public readonly string StrategyID;

        static ParsedDestination()
        {
            _subscriptionTypes = new Dictionary<string, SubscriptionType>()
            {
                ["none"] = SubscriptionType.None,
                ["gauge"] = SubscriptionType.Gauge,
                ["defn"] = SubscriptionType.Definition,
                ["strategy"] = SubscriptionType.Strategy,
            };

            _subscriptionStringsByType = new Dictionary<SubscriptionType, string>();
            foreach (KeyValuePair<string, SubscriptionType> pair in _subscriptionTypes)
                _subscriptionStringsByType.Add(pair.Value, pair.Key);

            _subscriptionPaths = new Dictionary<string, SubscriptionPath>()
            {
                ["none"] = SubscriptionPath.None,

                ["sent"] = SubscriptionPath.GaugeSentiment,
                ["eq"] = SubscriptionPath.GaugeEquilibrium,
                ["meq"] = SubscriptionPath.GaugeMultiframeEquilibrium,
                ["per2"] = SubscriptionPath.GaugePerception,
                ["comm"] = SubscriptionPath.GaugeCommitment,
                ["head"] = SubscriptionPath.GaugeHeadroom,
                ["bp"] = SubscriptionPath.GaugeBookPressure,
                ["tr"] = SubscriptionPath.GaugeTrigger,

                ["instrument"] = SubscriptionPath.DefnInstrument,
                ["search"] = SubscriptionPath.DefnSymbolSearch,
                ["top"] = SubscriptionPath.DefnTopSymbols,
            };

            _subscriptionStringsByPath = new Dictionary<SubscriptionPath, string>();
            foreach (KeyValuePair<string, SubscriptionPath> pair in _subscriptionPaths)
                _subscriptionStringsByPath.Add(pair.Value, pair.Key);
        }

        /// <summary>
        /// Creates a new parsed definition.
        /// </summary>
        /// <param name="destination">The definition to parse.</param>
        public ParsedDestination(string destination)
        {
            string[] fields;

            if (string.IsNullOrEmpty(destination) || destination[0] != _separator)
                destination = _separator + destination;
            
            Destination = destination;
            SubscriptionType = SubscriptionType.None;

            try
            {
                // Split on the path delimiter and remove any empty entries.
                fields = destination.Substring(1).Split(new char[] { _separator });

                // Try to parse the subscription type.
                if (!_subscriptionTypes.TryGetValue(fields[0], out SubscriptionType))
                    SubscriptionType = SubscriptionType.None;

                if ((fields.Length >= 2) && (SubscriptionType != SubscriptionType.Strategy))
                {
                    // If there is a path, 
                    if (!_subscriptionPaths.TryGetValue(fields[1], out SubscriptionPath))
                        SubscriptionPath = SubscriptionPath.None;

                    // If the subscription path does not match the type, set the type to none.
                    if (((long)SubscriptionPath & (long)SubscriptionType) != (long)SubscriptionPath)
                        SubscriptionType = SubscriptionType.None;
                }

                switch (SubscriptionType)
                {
                    case SubscriptionType.Gauge:
                        // For gauge values, get the stream and symbol info.
                        StreamID = fields[2].ToLower();
                        Symbol = fields[3];

                        // If there is a compression, get it.
                        if (fields.Length > 4)
                            Compression = fields[4];

                        break;

                    case SubscriptionType.Strategy:
                        // For strategy requests, the ID is the second value and the regular security info is after.
                        StrategyID = fields[1];
                        StreamID = fields[2].ToLower();
                        Symbol = fields[3];
                        break;

                    case SubscriptionType.Definition:
                        // Parse the definition values.
                        switch (SubscriptionPath)
                        {
                            case SubscriptionPath.DefnInstrument:
                                // Long definition always uses old format, get security from fields.
                                StreamID = fields[2].ToLower();
                                Symbol = fields[3];
                                break;

                            case SubscriptionPath.DefnSymbolSearch:
                                // If this is a symbol search, try to get the symbol ID.
                                StreamID = fields[2].ToLower();

                                if (fields.Length > 3)
                                    Term = fields[3];
                                if (fields.Length > 4)
                                    Broker = fields[4];
                                break;

                            case SubscriptionPath.DefnTopSymbols:
                                // Get the top symbols subscription details.
                                StreamID = null;
                                Broker = fields[2];

                                if (fields.Length > 3)
                                    SecurityType = fields[3];
                                return;
                        }

                        // If no StreamID found, return no subscription type.
                        if (string.IsNullOrEmpty(StreamID))
                            SubscriptionType = SubscriptionType.None;

                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":GSI = " + ex.Message);
                SubscriptionType = SubscriptionType.None;
            }
        }

        /// <summary>
        /// Creates a new parsed definition with the values supplied (for Gauge/Chart subscriptions).
        /// </summary>
        /// <param name="subscriptionType">The type of subscription requested.</param>
        /// <param name="subscriptionPath">The path of the subscription requested.</param>
        /// <param name="streamID">The stream identifier of the destination.</param>
        /// <param name="security">The underlying security.</param>
        /// <param name="compression">The gauge compression requested.</param>
        /// <param name="settings">Additional gauge settings requested.</param>
        public ParsedDestination(SubscriptionType subscriptionType, SubscriptionPath subscriptionPath,
                                 string streamID = null, string symbol = null, string compression = null,
                                 string broker = null, string securityType = null, string term = null, string strategyID = null)
        {
            StringBuilder builder = new StringBuilder(_separator);

            // Set the values.
            SubscriptionType = subscriptionType;
            SubscriptionPath = subscriptionPath;
            StreamID = streamID ?? string.Empty;
            Symbol = symbol ?? string.Empty;
            Compression = compression ?? string.Empty;
            Broker = broker ?? string.Empty;
            SecurityType = securityType ?? string.Empty;
            Term = term ?? string.Empty;
            StrategyID = strategyID ?? string.Empty;

            // Generate the destination.
            switch (SubscriptionType)
            {
                case SubscriptionType.Gauge:
                    // If this is a gauge or chart, set up the values.
                    builder.Append(_subscriptionStringsByType[SubscriptionType]);
                    builder.Append(_separator);
                    builder.Append(_subscriptionStringsByPath[SubscriptionPath]);
                    builder.Append(_separator);
                    builder.Append(StreamID);
                    builder.Append(_separator);
                    builder.Append(Symbol);

                    if (!string.IsNullOrEmpty(Compression))
                    {
                        builder.Append(_separator);
                        builder.Append(Compression);
                    }
                    break;

                case SubscriptionType.Strategy:
                    // If this is a strategy, set up the values.
                    builder.Append(_subscriptionStringsByType[SubscriptionType]);
                    builder.Append(_separator);
                    builder.Append(StrategyID);
                    builder.Append(_separator);
                    builder.Append(StreamID);
                    builder.Append(_separator);
                    builder.Append(Symbol);
                    break;

                case SubscriptionType.Definition:
                    builder.Append(_subscriptionStringsByType[SubscriptionType]);
                    builder.Append(_separator);
                    builder.Append(_subscriptionStringsByPath[SubscriptionPath]);

                    switch (SubscriptionPath)
                    {
                        case SubscriptionPath.DefnInstrument:

                            builder.Append(_separator);
                            builder.Append(StreamID);
                            builder.Append(_separator);
                            builder.Append(Symbol);
                            break;

                        case SubscriptionPath.DefnSymbolSearch:
                            builder.Append(_separator);
                            builder.Append(StreamID);

                            if (!string.IsNullOrEmpty(Term) || !string.IsNullOrEmpty(Broker))
                            {
                                builder.Append(_separator);
                                builder.Append(Term);

                                if (!string.IsNullOrEmpty(Broker))
                                {
                                    builder.Append(_separator);
                                    builder.Append(Broker);
                                }
                            }

                            break;

                        case SubscriptionPath.DefnTopSymbols:
                            builder.Append(_separator);
                            builder.Append(Broker);

                            if (!string.IsNullOrEmpty(SecurityType))
                            {
                                builder.Append(_separator);
                                builder.Append(SecurityType);
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }

            // Set the destination.
            Destination = builder.ToString();
        }

        public static ParsedDestination CreateGaugeDestination(SubscriptionPath subscriptionPath, string streamID, string symbol, string compression = null)
        {
            return new ParsedDestination(SubscriptionType.Gauge, subscriptionPath, streamID, symbol, compression);
        }

        public static ParsedDestination CreateStrategyDestination(string strategyID, string streamID, string symbol)
        {
            return new ParsedDestination(SubscriptionType.Strategy, SubscriptionPath.None, streamID, symbol, strategyID: strategyID);
        }

        public static ParsedDestination CreateSearchDestination(string streamID, string term = null, string broker = null)
        {
            return new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnSymbolSearch, streamID, term: term, broker: broker);
        }

        public static ParsedDestination CreateTopSymbolsDestination(string broker, string securityType = null)
        {
            return new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnTopSymbols, broker: broker, securityType: securityType);
        }

        public static ParsedDestination CreateInstrumentDestination(string streamID, string symbol)
        {
            return new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnInstrument, streamID, symbol);
        }

        /// <summary>
        /// Returns the proper stream ID for the given security type.
        /// </summary>
        /// <param name="streamID">The base stream ID in use by the broker.</param>
        /// <param name="symbol">The symbol to return the stream ID for.</param>
        /// <returns>The proper stream ID for the given security.</returns>
        public static string StreamIDForSymbol(string streamID, string symbol)
        {
            // If delayed for currency pair, just use realtime. Otherwise, Set the stream ID.
            if (streamID == DelayStreamID && (symbol.Contains('.') || symbol.Contains(':')))
                return RealtimeStreamID;
            else
                return streamID;
        }
    }
}
