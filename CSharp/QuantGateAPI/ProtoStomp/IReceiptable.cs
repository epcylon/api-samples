namespace QuantGate.API.ProtoStomp
{
    public interface IReceiptable
    {
        ulong ReceiptID { get; }

        void OnReceipt();

        void OnInvalidate();
    }
}
