using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using System.Diagnostics;

namespace QuantGate.API.Signals.ProtoStomp
{
    /// <summary>
    /// Represents the base class for a client's STOMP subscription.
    /// </summary>
    internal class ProtoStompSubscription : IReceiptable, IObserver<ByteString>
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "PSSubs";

        /// <summary>
        /// The STOMP client that will handle this subscription.
        /// </summary>
        internal APIClient Client { get; private set; }

        /// <summary>
        /// The subscription request underlying this subscription.
        /// </summary>
        private readonly SubscribeRequest _request;

        /// <summary>
        /// Creates a new StompSubscription instance.
        /// </summary>
        /// <param name="client">The STOMP client that will handle this subscription.</param>
        /// <param name="destination">The destination to subscribe to.</param>
        /// <param name="receipt">Should we request a subscription receipt?</param>
        /// <param name="throttleRate">The initial throttle rate for this subscription.</param>
        public ProtoStompSubscription(APIClient client, string destination,
                                      bool receipt = false, uint throttleRate = 0)
        {
            Client = client;

            _request = new SubscribeRequest();

            if (!destination.StartsWith("/"))
                _request.Destination = '/' + destination;
            else
                _request.Destination = destination;

            _request.ThrottleRate = throttleRate;
            _request.SubscriptionId = client.IDGenerator.NextID;

            if (receipt)
                _request.ReceiptId = client.IDGenerator.NextID;
        }

        /// <summary>
        /// The destination to subscribe to.
        /// </summary>
        internal string Destination => _request.Destination;

        /// <summary>
        /// The id used to identify this subscription instance.
        /// </summary>
        internal ulong SubscriptionID
        {
            get => _request.SubscriptionId;
            set => _request.SubscriptionId = value;
        }

        public ulong ReceiptID
        {
            get => _request.ReceiptId;
            set => _request.ReceiptId = value;
        }

        /// <summary>
        /// The internal request object.
        /// </summary>
        internal SubscribeRequest Request => _request;

        /// <summary>
        /// Sets the current throttle rate for this subscription.
        /// </summary>
        /// <remarks>The default throttle rate is zero (no throttling).</remarks>
        internal uint ThrottleRate
        {
            get
            {
                return _request.ThrottleRate;
            }
            set
            {
                try
                {
                    bool changed = false;

                    // Check if the throttle rate changed.
                    changed = _request.ThrottleRate != value;

                    if (changed)
                    {
                        // If throttle value changed, update it and set.
                        _request.ThrottleRate = value;
                        Client?.Throttle(this, value);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(_moduleID + ":sThr - Exception" + ex.Message);
                }
            }
        }

        /// <summary>
        /// Unsubscribes from this subscription on the client and disconnects the client.
        /// </summary>
        public void Unsubscribe()
        {
            Client?.Enqueue(() => Client.Unsubscribe(this));
        }

        /// <summary>
        /// Used to handle the next subscription response.
        /// </summary>
        /// <param name="subscription">The subscription the response is for.</param>
        /// <param name="message">The response message to handle.</param>
        void IObserver<ByteString>.OnNext(ByteString value) => OnNext(this, value);

        /// <summary>
        /// Used to handle the next subscription response.
        /// </summary>
        /// <param name="subscription">The subscription the response is for.</param>
        /// <param name="message">The response message to handle.</param>
        public event Action<ProtoStompSubscription, ByteString> OnNext = delegate { };

        void IObserver<ByteString>.OnError(Exception error) => OnError(this, error);
        /// <summary>
        /// Used to handle error messages received on the subscription.
        /// </summary>
        public event Action<ProtoStompSubscription, Exception> OnError = delegate { };

        void IObserver<ByteString>.OnCompleted() => OnCompleted(this);

        public event Action<ProtoStompSubscription> OnCompleted = delegate { };

        void IReceiptable.OnReceipt()
        {
            OnReceipt(this);
            _request.ReceiptId = 0;
        }

        public event Action<IReceiptable> OnReceipt = delegate { };

        void IReceiptable.OnInvalidate() => Invalidated(this);

        public event Action<IReceiptable> Invalidated = delegate { };
    }
}
