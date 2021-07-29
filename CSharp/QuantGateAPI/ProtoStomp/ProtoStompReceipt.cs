using System;

namespace BridgeRock.CSharpExample.ProtoStomp
{
    internal class ProtoStompReceipt : IReceiptable
    {
        private readonly ulong _receiptID;

        public event Action Receipted = delegate { };
        public event Action Invalidated = delegate { };

        public ProtoStompReceipt()
        {
            _receiptID = IDGenerator.NextID;
        }

        public ulong ReceiptID { get { return _receiptID; } }

        public void OnInvalidate() { Invalidated(); }

        public void OnReceipt() { Receipted(); }
    }
}
