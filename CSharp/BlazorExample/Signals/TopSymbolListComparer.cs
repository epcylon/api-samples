using QuantGate.API.Signals.Events;

namespace BlazorExample.Signals
{
    public class TopSymbolListComparer : IComparer<TopSymbol[]>
    {
        public int Compare(TopSymbol[]? x, TopSymbol[]? y)
        {
            if (x is null || y is null)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                return (x is null) ? 1: -1;
            }
        
            if (x.Length != y.Length)
                return x.Length.CompareTo(y.Length);

            TopSymbol X, Y;

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

            return 0;
        }
    }
}
