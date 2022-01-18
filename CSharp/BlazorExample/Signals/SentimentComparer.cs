using QuantGate.API.Signals.Events;

namespace BlazorExample.Signals
{
    public class SentimentComparer : IComparer<SentimentEventArgs>
    {
        public int Compare(SentimentEventArgs? x, SentimentEventArgs? y)
        {
            if (x is null || y is null)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                return (x is null) ? 1 : -1;
            }

            if (x.Symbol != y.Symbol)
                return x.Symbol.CompareTo(y.Symbol);

            if (x.Lengths.Count != y.Lengths.Count)
                return x.Lengths.Count.CompareTo(y.Lengths.Count);

            if (x.Colors.Count != y.Colors.Count)
                return x.Colors.Count.CompareTo(y.Colors.Count);            

            for (int index = 0; index < x.Lengths.Count; index++)
                if (x.Lengths[index] != y.Lengths[index])
                    return x.Lengths[index].CompareTo(y.Lengths[index]);

            for (int index = 0; index < x.Colors.Count; index++)
                if (x.Colors[index] != y.Colors[index])
                    return x.Colors[index].CompareTo(y.Colors[index]);

            return 0;
        }
    }
}
