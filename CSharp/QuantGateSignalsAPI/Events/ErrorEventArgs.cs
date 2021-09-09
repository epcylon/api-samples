namespace QuantGate.API.Events
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
