namespace QuantGate.API.Events;

public class ErrorEventArgs(string message)
{
    public string Message { get; } = message;
}
