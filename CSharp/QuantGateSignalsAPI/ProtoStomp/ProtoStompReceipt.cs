namespace QuantGate.API.Signals.ProtoStomp
{
    internal class ProtoStompReceipt(ulong id) : IReceiptable
    {
        public event Action Receipted = delegate { };
        public event Action Invalidated = delegate { };

        public ulong ReceiptID { get; private set; } = id;

        public void OnInvalidate()
        {
            Invalidated();
        }

        public void OnReceipt()
        {
            Receipted();
            ReceiptID = 0;
        }
    }
}
