namespace QuantGate.API.Signals.Events;

public class ErrorEventArgs(string message)
{
    public string Message { get; } = message;
}
