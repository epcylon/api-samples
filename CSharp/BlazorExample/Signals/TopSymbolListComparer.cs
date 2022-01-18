using QuantGate.API.Signals.Events;

namespace BlazorExample.Signals
{
    /// <summary>
    /// Used to compare two TopSymbol arrays.
    /// </summary>
    public class TopSymbolListComparer : IComparer<TopSymbol[]>
    {
        /// <summary>
        /// Compares two instances of TopSymbol arrays.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns>
        /// Zero if equal, 
        /// Greater than zero if the first instance is greater than the second,
        /// Less than zero if the first instance is less than the second.
        /// </returns>
        public int Compare(TopSymbol[]? x, TopSymbol[]? y)
        {
            // Compare on null references first.
            if (x is null || y is null)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                return (x is null) ? 1: -1;
            }
        
            // Compare the length of the array next.
            if (x.Length != y.Length)
                return x.Length.CompareTo(y.Length);

            TopSymbol X, Y;

            // Go through each array and compare TopSymbol instances.
            for (int index = 0; index < x.Length; index++)
            {
                X = x[index];
                Y = y[index];

                if (X.Symbol != Y.Symbol)
                    return X.Symbol.CompareTo(Y.Symbol);

                if (X.Signal != Y.Signal)
                    return X.Signal.CompareTo(Y.Signal);

                if (X.PerceptionSignal != Y.PerceptionSignal)
                    return X.PerceptionSignal.CompareTo(Y.PerceptionSignal);

                if (X.CommitmentSignal != Y.CommitmentSignal)
                    return X.CommitmentSignal.CompareTo(Y.CommitmentSignal);

                if (X.EquilibriumSignal != Y.EquilibriumSignal)
                    return X.EquilibriumSignal.CompareTo(Y.EquilibriumSignal);

                if (X.SentimentSignal != Y.SentimentSignal)
                    return X.SentimentSignal.CompareTo(Y.SentimentSignal);
            }

            // If got here, the value are equal.
            return 0;
        }
    }
}
