namespace QuantGate.API.Signals.ProtoStomp;

internal interface IReceiptable
{
    ulong ReceiptID { get; }

    void OnReceipt();

    void OnInvalidate();
}
