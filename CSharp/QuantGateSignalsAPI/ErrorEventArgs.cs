namespace QuantGate.API.Signals
{
    public class ErrorEventArgs
    {
        public string Message { get; }

        public ErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
