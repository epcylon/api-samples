using System;

namespace QuantGate.API.Signals.ProtoStomp
{
    internal class ProtoStompReceipt : IReceiptable
    {
        public event Action Receipted = delegate { };
        public event Action Invalidated = delegate { };

        public ulong ReceiptID { get; private set; }

        public ProtoStompReceipt(ulong id)
        {
            ReceiptID = id;
        }

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
