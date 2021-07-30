namespace QuantGate.API.ProtoStomp
{
    internal interface IReceiptable
    {
        ulong ReceiptID { get; }

        void OnReceipt();

        void OnInvalidate();
    }
}
