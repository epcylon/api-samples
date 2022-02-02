﻿using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Top Symbol values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class TopSymbolsEventArgs : EventArgs
    {
        /// <summary>
        /// Top symbol results.
        /// </summary>
        public IReadOnlyList<TopSymbol> Symbols { get; }
        /// <summary>
        /// Holds error information, if a subscription error occured.
        /// </summary>
        public SubscriptionError Error { get; }

        /// <summary>
        /// The broker to get the Top Symbols for. Must match a valid broker type string.
        /// </summary>
        public string Broker { get; }
        /// <summary>
        /// The type of instrument to include in the results.
        /// </summary>
        public InstrumentType InstrumentType { get; }
        /// <summary>
        /// Reference object tied to the initial request.
        /// </summary>
        public object Reference { get; }

        /// <summary>
        /// Creates a new TopSymbolsEventArgs instance.
        /// </summary>
        /// <param name="broker">The broker to get the Top Symbols for. Must match a valid broker type string.</param>
        /// <param name="instrumentType">The type of instrument to include in the results.</param>
        /// <param name="reference">Reference object tied to the initial request.</param>
        /// <param name="symbols">Top symbol results.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        public TopSymbolsEventArgs(string broker, InstrumentType instrumentType, List<TopSymbol> symbols, 
                                   object reference = null, SubscriptionError error = null)
        {
            Broker = broker;
            InstrumentType = instrumentType;
            Symbols = symbols;
            Reference = reference;
            Error = error;
        }
    }
}
