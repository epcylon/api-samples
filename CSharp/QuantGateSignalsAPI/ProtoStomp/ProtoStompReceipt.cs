using System;

namespace QuantGate.API.Signals.ProtoStomp
{
    internal class ProtoStompReceipt : IReceiptable
    {
        private readonly ulong _receiptID;

        public event Action Receipted = delegate { };
        public event Action Invalidated = delegate { };

        public ProtoStompReceipt(ulong id)
        {
            _receiptID = id;
        }

        public ulong ReceiptID { get { return _receiptID; } }

        public void OnInvalidate() { Invalidated(); }

        public void OnReceipt() { Receipted(); }
    }
}
