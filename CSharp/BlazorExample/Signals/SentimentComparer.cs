using QuantGate.API.Signals.Events;

namespace BlazorExample.Signals
{
    /// <summary>
    /// Used to compare two SentimentEventArgs objects.
    /// </summary>
    public class SentimentComparer : IComparer<SentimentEventArgs>
    {
        /// <summary>
        /// Compares two instances of the SentimentEventArgs class.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns>
        /// Zero if equal, 
        /// Greater than zero if the first instance is greater than the second,
        /// Less than zero if the first instance is less than the second.
        /// </returns>
        public int Compare(SentimentEventArgs? x, SentimentEventArgs? y)
        {
            // Compare on null references first.
            if (x is null || y is null)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                return (x is null) ? 1 : -1;
            }

            // Symbol next.
            if (x.Symbol != y.Symbol)
                return x.Symbol.CompareTo(y.Symbol);

            // Compare counts of lengths and colors.
            if (x.Lengths.Count != y.Lengths.Count)
                return x.Lengths.Count.CompareTo(y.Lengths.Count);

            if (x.Colors.Count != y.Colors.Count)
                return x.Colors.Count.CompareTo(y.Colors.Count);            

            // Compare values of lengths and colors.
            for (int index = 0; index < x.Lengths.Count; index++)
                if (x.Lengths[index] != y.Lengths[index])
                    return x.Lengths[index].CompareTo(y.Lengths[index]);

            for (int index = 0; index < x.Colors.Count; index++)
                if (x.Colors[index] != y.Colors[index])
                    return x.Colors[index].CompareTo(y.Colors[index]);

            // If got here, the value are equal.
            return 0;
        }
    }
}
