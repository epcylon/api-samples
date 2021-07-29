using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using System;

namespace BridgeRock.CSharpExample.ProtoStomp
{
    public class ProtoStompSend : IReceiptable
    {
        public string Destination { get { return Request.Destination; } }
        
        public ByteString Body { get { return Request.Body; } }

        public ulong ReceiptID { get { return Request.ReceiptId; } }

        /// <summary>
        /// The internal request object.
        /// </summary>
        internal SendRequest Request { get; private set; }

        public ProtoStompSend(ProtoStompClient client, string destination, ByteString body = null, bool receipt = false)
        {
            Request = new SendRequest();

            if (!destination.StartsWith("/"))
                Request.Destination = '/' + destination;
            else
                Request.Destination = destination;

            if (!ReferenceEquals(body, null))
                Request.Body = body;

            if (receipt)
                Request.ReceiptId = IDGenerator.NextID;
            else
                Request.ReceiptId = 0;
        }

        void IReceiptable.OnReceipt() => OnReceipt(this);

        public event Action<IReceiptable> OnReceipt = delegate { };

        void IReceiptable.OnInvalidate() => Invalidated(this);

        public event Action<IReceiptable> Invalidated = delegate { };
    }
}

