<?php

    namespace QuantGate\API\Signals;

    require_once __DIR__ . '/vendor/autoload.php';    
    require_once __DIR__ . '/Utilities.php';    
    require_once __DIR__ . '/Proto/GPBMetadata/StealthApiV20.php';    
    require_once __DIR__ . '/Proto/GPBMetadata/StompV01.php'; 
    require_once __DIR__ . '/Proto/Stomp/RequestFrame.php';
    require_once __DIR__ . '/Proto/Stomp/ResponseFrame.php';
    require_once __DIR__ . '/Proto/Stomp/Heartbeat.php';
    require_once __DIR__ . '/Proto/Stomp/ConnectRequest.php';
    require_once __DIR__ . '/Proto/Stomp/SubscribeRequest.php';
    require_once __DIR__ . '/Proto/Stomp/ConnectedResponse.php';
    require_once __DIR__ . '/Proto/Stomp/MessageResponse.php';
    require_once __DIR__ . '/Proto/Stomp/MessageResponses.php';
    require_once __DIR__ . '/Proto/Stomp/SubscriptionErrorResponse.php';
    require_once __DIR__ . '/Proto/Stomp/ServerErrorResponse.php';    
    require_once __DIR__ . '/Proto/Stealth/StrategyUpdate.php';
    require_once __DIR__ . '/Subscriptions/SubscriptionBase.php';
    require_once __DIR__ . '/Subscriptions/StrategySubscription.php';
    require_once __DIR__ . '/Events/StrategyUpdate.php';

    use \Stomp\RequestFrame;
    use \Stomp\ConnectRequest;
    use \Stomp\SubscribeRequest;
    use \Stomp\ResponseFrame;
    use \Stomp\ConnectedResponse;
    use \Stomp\MessageResponse;
    use \Stomp\MessageResponses;
    use \Stomp\Heartbeat;    
    use \Ratchet\Client;
    use \Ratchet\RFC6455\Messaging\Frame;
    use \React\EventLoop\LoopInterface;
    use \React\EventLoop\Timer\Timer;
    use \Ratchet\Client\WebSocket;
    use \Ratchet\RFC6455\Messaging\Message;
    use \QuantGate\API\Signals\Subscriptions\SubscriptionBase;
    use \QuantGate\API\Signals\Subscriptions\StrategySubscription;
    use \QuantGate\API\Signals\Events\StrategyUpdate;

    /**
     * Simple QuantGate signals API client.
     */
    class APIClient
    {
        /**
         * Represents a real-time data stream.
         * @var string
         */
        public const REALTIME_STREAM = "realtime";        
        /**
         * Represents a delayed data stream.
         * @var string
         */
        public const DELAY_STREAM = "delay";
        /**
         * Represents a stream coming from the 24/7 demo servers (simulated data).
         * @var string
         */
        public const DEMO_STREAM = "demo";
        
        /**
         * The message loop that all events will be run within.
         * @var LoopInterface
         */
        private LoopInterface $loop;

        /**
         * The web address to connect to.
         * @var string
         */
        private string $host;
        /**
         * The port to connect to.
         * @var int
         */
        private int $port;
        /**
         * The base datastream to connect to (default = realtime).
         * @var string
         */
        private string $stream;
        /**
         * The JWT token to log in with.
         * @var string
         */
        private string $jwtToken;
        
        /**
         * WebSocket connection instance.
         * @var WebSocket
         */
        private WebSocket $webSocket;        
        /**
         * Are we currently connected to the websocket.
         * @var bool
         */
        private bool $isConnected = false;
        /**
         * Used to generate ids for each new subscription.
         * @var int
         */
        private int $nextID = 1;
        
        /**
         * Holds a list of all current subscriptions by subscription id.
         * @var array
         */
        private array $subscriptionsById = [];        
        /**
         * Holds a list of all current subscriptions by destination.
         * @var array
         */
        private array $subscriptionsByDest = [];
         
        /**
         * Holds all the callbacks attached to the strategy update 'event'.
         * @var array
         */
        private array $strategyUpdateCallbacks = array();

        /**
         * Holds a reference to the timer used to send heartbeat messages.
         * @var Timer
         */
        private Timer $heartbeatTimer;

        /**
         * Creates a new APIClient instance.
         * @param   LoopInterface   $loop   The message loop that all events will be run within.
         * @param   string          $host   The web address to connect to.
         * @param   int             $port   The port to connect to.
         * @param   string          $stream The base datastream to connect to (default = realtime).
         */
        function __construct(LoopInterface $loop, string $host, int $port = 443, string $stream = self::REALTIME_STREAM)
        {
            // Set the internal values.
            $this->loop = $loop;
            $this->host = $host;
            $this->port = $port;
            $this->stream = $stream;
            
            // Add the timer to use for heartbeat checking.
            $this->heartbeatTimer = $this->loop->addPeriodicTimer(5, array($this, 'checkHeartbeats'));
        }

        /**
         * Connects to the server on the specified address.
         * @param   string  $jwtToken   JWT Token to connect with.
         * @return  void
         */
        function connect(string $jwtToken)
        {            
            // Remember the JWT token.
            $this->jwtToken = $jwtToken;
            
            // Connect to the client at the appropriate host, with the given message loop.
            Client\connect($this->host.':'.$this->port, [], [], $this->loop)->then(
                function(WebSocket $connection)
                {
                    // Keep a reference to this WebSocket connection.
                    $this->webSocket = $connection;

                    // Log as connected.
                    echo "Connected!\n";

                    // Add a handler to handle the messages received.
                    $connection->on('message', array($this, 'handleMessage'));
                                        
                    // Create the connection request frame.
                    $connectReq = new ConnectRequest();
                    $connectReq->setAcceptVersion("1.0");
                    $connectReq->setLogin(Utilities::getUserFromJWT($this->jwtToken));
                    $connectReq->setPasscode($this->jwtToken);                    
                    $request = new RequestFrame();
                    $request->setConnect($connectReq);

                    // Send the connection request to the server.
                    $this->sendFrame($request);
                }, 
                function ($e) 
                {
                    // Handle any errors.
                    echo "Could not connect: {$e->getMessage()}\n";
                });
        }

        /**
         * Used to close the APIClient connection.
         */
        function close()
        {
            // No longer connected.
            $this->isConnected = false;
            // Close the websocket.
            $this->webSocket->close();
            // Cancel the heartbeat timer.
            $this->loop->cancelTimer($this->heartbeatTimer);
        }

        /**
         * Sends a RequestFrame to the server.
         * @param   RequestFrame    $frame  The frame to send to the server.
         * @return  void
         */
        public function sendFrame(RequestFrame $frame)
        {
            // Serialize the request frame.
            $data = $frame->serializeToString();
            // Convert the frame to a binary frame.
            $binary = new Frame($data, true, Frame::OP_BINARY);
            // Send the binary data through the WebSocket.
            $this->webSocket->send($binary);            
        }

        /**
         * Subscribes the given SubscriptionBase object and updates arrays.
         * @param   SubscriptionBase    $subscription   The subscription to subscribe to.
         * @return  void
         */
        function subscribe(SubscriptionBase $subscription)
        {
            // If already subscribed, no need to subscribe again.
            if (array_key_exists($subscription->getDestination(), $this->subscriptionsByDest))
                return null;            

            // Add the subscription to the arrays.
            $this->subscriptionsById[$subscription->getID()] = $subscription;
            $this->subscriptionsByDest[$subscription->getDestination()] = $subscription;

            if ($this->isConnected)
            {
                // If we're connected, create the subscription request frame.
                $subscribeReq = new SubscribeRequest();
                $subscribeReq->setSubscriptionId($subscription->getID());
                $subscribeReq->setDestination($subscription->getDestination());
                $subscribeReq->setThrottleRate($subscription->getThrottleRate());                
                $request = new RequestFrame();
                $request->setSubscribe($subscribeReq);

                // Request the subscription from the server.
                $this->sendFrame($request);
            }
        }

        /**
         * Resubscribes all current subscriptions to the back-end (after disconnect/initial 
         * connection - i.e. when not present in current connection).
         * @return void
         */
        function resubscribeAll()
        {
            // Reset the subscription ID.
            $this->nextID = 0;
            // Get a copy of the subscriptions array.
            $toSubscribe = $this->subscriptionsById;            

            // Clear the subscriptions arrays.
            $this->subscriptionsById = [];
            $this->subscriptionsByDest = [];

            // Go through all subscriptions and subscribe.
            foreach ($toSubscribe as $subscription)
                $this->subscribe($subscription);
        }





        /**
         * Runs heartbeat checks on a 5-second interval.
         */
        function checkHeartbeats()
        {
            // Create a new heartbeat request frame.
            $request = new RequestFrame();
            $request->setHeartbeat(new Heartbeat());
            // Send the request to the server.
            $this->sendFrame($request);
        }

        /**
         * Subscribes to a Strategy update data stream for a specific strategy and symbol.
         * @param   string  $strategyID     Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @param   string  $symbol         Symbol to get the Strategy update data for.
         * @param   int     $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @return  void
         */
        public function subscribeStrategy(string $strategyId, string $symbol, int $throttleRate = 0)        
        {
            // Subscribe within the loop.
            $this->loop->futureTick(function() use ($strategyId, $symbol, $throttleRate)
            {
                // Create a new strategy subscription.
                $subscription = new StrategySubscription($this->nextID, $strategyId, $symbol, $this->stream,
                                                         $throttleRate, array($this, 'sendStrategyUpdate'));
                // Update the next ID and subscribe.
                $this->nextID++;
                $this->subscribe($subscription);
            });
        }




            

        /**
         * Adds a callback subscriber for the StrategyUpdate event. 
         * @param   Closure $callback   The callback used to handle StrategyUpdate events.
         */
        function addStrategyUpdateCallback(\Closure $callback)
        {
            $this->strategyUpdateCallbacks[] = $callback;
        }

        /**
         * Called from the StrategySubscription to send a strategy update to all subscribed callbacks.
         * @param   StrategyUpdate  $update The updated strategy details to send.
         */
        function sendStrategyUpdate(StrategyUpdate $update)
        {
            // Go through callback handlers and send.
            foreach ($this->strategyUpdateCallbacks as $callback) 
                \call_user_func_array($callback, array($update));
        }



        function handleMessage(Message $message)
        {
            $response = new ResponseFrame();
            $response->mergeFromString($message->getPayload());

            switch ($response->getResponse())
            {
                case 'connected':
                    $this->isConnected = true;
                    $this->resubscribeAll();
                    break;
                
                case 'heartbeat':
                    echo "Received: Heartbeat\n";
                    break;

                case 'single_message':
                    $this->handleMessageResponse($response->getSingleMessage());
                    break;

                default:
                    echo "Received: Unknown ".$response->getResponse()."\n";
                    break;
            }            
        }

        function handleMessageResponses(MessageResponses $responses)
        {
            //$responses->
        }

        function handleMessageResponse(MessageResponse $message)
        {
            // Try to get the subscription tied to this message.
            $subscription = $this->subscriptionsById[$message->getSubscriptionId()];

            // If the subscription is found, handle the stream message.
            if (isset($subscription))
                $subscription->handleMessage($message->getBody());
        }
    }

?>