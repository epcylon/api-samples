<?php

    namespace QuantGate\API\Signals;

    require_once __DIR__ . '/vendor/autoload.php';
    require_once __DIR__ . '/Events/PerceptionUpdate.php';
    require_once __DIR__ . '/Events/StrategyUpdate.php';
    require_once __DIR__ . '/Proto/GPBMetadata/StealthApiV20.php';    
    require_once __DIR__ . '/Proto/GPBMetadata/StompV01.php'; 
    require_once __DIR__ . '/Proto/Stealth/SingleValueUpdate.php';
    require_once __DIR__ . '/Proto/Stealth/StrategyUpdate.php';
    require_once __DIR__ . '/Proto/Stomp/ConnectRequest.php';
    require_once __DIR__ . '/Proto/Stomp/DisconnectRequest.php';
    require_once __DIR__ . '/Proto/Stomp/ConnectedResponse.php';
    require_once __DIR__ . '/Proto/Stomp/Heartbeat.php';
    require_once __DIR__ . '/Proto/Stomp/MessageResponse.php';
    require_once __DIR__ . '/Proto/Stomp/MessageResponses.php';
    require_once __DIR__ . '/Proto/Stomp/RequestFrame.php';
    require_once __DIR__ . '/Proto/Stomp/ResponseFrame.php';
    require_once __DIR__ . '/Proto/Stomp/ServerErrorResponse.php';    
    require_once __DIR__ . '/Proto/Stomp/SubscribeRequest.php';
    require_once __DIR__ . '/Proto/Stomp/SubscriptionErrorResponse.php';
    require_once __DIR__ . '/Proto/Stomp/ThrottleRequest.php';
    require_once __DIR__ . '/Proto/Stomp/UnsubscribeRequest.php';
    require_once __DIR__ . '/Subscriptions/SubscriptionBase.php';
    require_once __DIR__ . '/Subscriptions/PerceptionSubscription.php';
    require_once __DIR__ . '/Subscriptions/StrategySubscription.php';
    require_once __DIR__ . '/Utilities.php';

    use \Evenement\EventEmitterInterface;
    use \Evenement\EventEmitterTrait;
    use \QuantGate\API\Signals\Events\PerceptionUpdate;
    use \QuantGate\API\Signals\Events\StrategyUpdate;
    use \QuantGate\API\Signals\Subscriptions\PerceptionSubscription;
    use \QuantGate\API\Signals\Subscriptions\StrategySubscription;
    use \QuantGate\API\Signals\Subscriptions\SubscriptionBase;
    use \Ratchet\Client;
    use \Ratchet\Client\WebSocket;
    use \Ratchet\RFC6455\Messaging\Frame;
    use \Ratchet\RFC6455\Messaging\Message;
    use \React\EventLoop\LoopInterface;
    use \React\EventLoop\Timer\Timer;
    use \Stomp\ConnectRequest;
    use \Stomp\DisconnectRequest;
    use \Stomp\ConnectedResponse;
    use \Stomp\Heartbeat;    
    use \Stomp\MessageResponse;
    use \Stomp\MessageResponses;
    use \Stomp\RequestFrame;
    use \Stomp\ResponseFrame;
    use \Stomp\SubscribeRequest;
    use \Stomp\ThrottleRequest;
    use \Stomp\UnsubscribeRequest;

    /**
     * Simple QuantGate signals API client.
     */
    class APIClient implements EventEmitterInterface
    {
        // Set up event emitter capabilities.
        use EventEmitterTrait;

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
         * The time (in seconds) to wait after a connection attempt before killing the attempt.
         * @var float
         */
        const CONNECT_KILL = 20.0;
        /**
         * The minimum time (in seconds) to wait to reconnect.
         * @var float
         */
        const MIN_RECONNECT = 5.0;
        /**
         * The maximum reconnection attempt count to use to adjust reconnection attempts.
         * @var int
         */
        const MAX_RECONNECT = 10;
        /**
         * The maximum time to wait (in seconds) before receiveing a message.
         * @var float
         */
        const MAX_HEARTBEAT_WAIT = 60.0;
        /**
         * Time to wait (in seconds) after not receiving a message before sending a heartbeat request.
         * @var float
         */
        const HEARTBEAT_CHECK_WAIT = 10.0;

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
         * Are we currently disconnecting the client?
         * @var bool
         */
        private bool $isDisconnecting = false;
        /**
         * Used to generate ids for each new subscription.
         * @var int
         */
        private int $nextId = 1;
        
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
         * Holds a reference to the timer used to send heartbeat messages.
         * @var Timer
         */
        private Timer $timer;
        /**
         * Holds the last time that a message was received from the server.
         * @var float
         */
        private float $lastMessageTime = 0.0;
        /**
         * The next time to attempt a reconnection.
         * @var float
         */
        private float $reconnectTime = 0.0;
        /**
         * The time to kill a reconnection attempt.
         * @var float
         */
        private float $killTime = 0.0;
        /**
         * The number of times a reconnection has been attempted.
         * @var int
         */
        private int $reconnectCount = 0;

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
            // Add the timer to use for heartbeat checking.
            $this->timer = $this->loop->addPeriodicTimer(5, array($this, 'handleTimer'));
            // Connect to the websocket.
            $this->connectWebsocket();
        }

        /**
         * Creates a websocket connection and connects.
         * @return  void
         */
        function connectWebsocket()
        {
            // Set last message time to now.
            $this->lastMessageTime = microtime(true);

            // Set up the connection timers.
            $this->isDisconnecting = false;
            $this->reconnectTime = 0.0;
            $this->killTime = $this->lastMessageTime + APIClient::CONNECT_KILL;

            // Connect to the client at the appropriate host, with the given message loop.
            Client\connect($this->host.':'.$this->port, [], [], $this->loop)->then(
                function(WebSocket $connection)
                {
                    // Keep a reference to this WebSocket connection.
                    $this->webSocket = $connection;

                    // Add handlers to handle the events received.
                    $connection->on('message', array($this, 'handleMessage'));
                    $connection->on('close', array($this, 'handleClosed'));
                    
                    // Handle as connected.
                    $this->handleConnected();
                }, 
                function ($e) 
                {
                    // Handle any errors.
                    echo "Could not connect: {$e->getMessage()}\n";
                    $this->handleClosed();
                });
        }

        /**
         * Logs in when the Websocket establishes a connection.
         * @return  void
         */
        function handleConnected()
        {
            // Create the connection request frame.
            $connectReq = new ConnectRequest();
            $connectReq->setAcceptVersion("1.0");
            $connectReq->setLogin(Utilities::getUserFromJWT($this->jwtToken));
            $connectReq->setPasscode($this->jwtToken);
            $request = new RequestFrame();
            $request->setConnect($connectReq);

            // Send the connection request to the server.
            $this->sendFrame($request);
        }

        /**
         *  Called when connected received after login.
         * @return void
         */
        function handleLoggedIn()
        {
            // If connected, clear the reconnect properties.
            $this->reconnectCount = 0.0;
            $this->reconnectTime = 0.0;
            $this->killTime = 0.0;

            // Set the connected flag.
            $this->isConnected = true;
            // Send the connected event.
            $this->emit('connected', []);
            // Resubscribe all subscriptions.
            $this->resubscribeAll();
        }

        /**
         * Handles WebSocket close events.
         * @return  void
         */
        function handleClosed()
        {
            if ($this->isDisconnecting)
            {
                $this->clearSubscriptions();
                $this->reconnectTime = 0.0;
            }
            else
            {
                $this->reconnectCount++;
                if ($this->reconnectCount > APIClient::MAX_RECONNECT)
                    $this->reconnectCount = APIClient::MAX_RECONNECT;
                
                $this->reconnectTime = microtime(true) + APIClient::MIN_RECONNECT * $this->reconnectCount;
            }

            // Clear the websocket reference.
            unset($this->webSocket);
            // No longer connected
            $this->isConnected = false;
            // Send out the disconnected event.
            $this->emit('disconnected', []);

            if ($this->isDisconnecting)
            {
                if (isset($this->timer))
                {
                    // If there is a heartbeat timer, cancel it.
                    $this->loop->cancelTimer($this->timer);
                    unset($this->timer);
                }
            }
        }

        /**
         * Used to close the APIClient connection.
         * @return  void
         */
        function close()
        {
            // No need to close twice.
            if ($this->isDisconnecting)
                return;

            // Set to disconnecting/not connected.
            $this->isDisconnecting = true;
            // Disconnect the websocket.
            $this->disconnect();
        }

        /**
         * Disconnects from the open websocket (if open).
         * @return  void
         */
        private function disconnect()
        {
            if (isset($this->webSocket))
            {
                 // Create the connection request frame.
                $request = new RequestFrame();
                $request->setDisconnect(new DisconnectRequest());

                // Send the connection request to the server.
                $this->sendFrame($request);

                // Close the websocket.
                $this->webSocket->close();
            }
            else
            {
                // Handle as closed.
                $this->handleClosed();
            }
        }

        /**
         * Sends a RequestFrame to the server.
         * @param   RequestFrame    $frame  The frame to send to the server.
         * @return  void
         */
        public function sendFrame(RequestFrame $frame)
        {
            if (isset($this->webSocket))
            {
                // Serialize the request frame.
                $data = $frame->serializeToString();
                // Convert the frame to a binary frame.
                $binary = new Frame($data, true, Frame::OP_BINARY);
                // Send the binary data through the WebSocket.
                $this->webSocket->send($binary);
            }
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

            // Increment the next ID.
            $this->nextId++;

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
         * Throttles the stream with the given destination.
         * @param   string  $destination    The destination of the stream to stop getting data for.
         * @param   int     $throttleRate   The new throttle rate to set to (in ms). Enter 0 for no throttling.
         * @return  void
         */
        function throttle(string $destination, int $throttleRate)
        {
            // Get the subscription from the array.
            $subscription = $this->subscriptionsByDest[$destination];

            if (isset($subscription))
            {
                // If the subscription exists, set the throttle rate.
                $subscription->setThrottleRate($throttleRate);

                if ($this->isConnected)
                {
                    // If connected, send the throttle request.
                    $throttleReq = new ThrottleRequest();
                    $throttleReq->setSubscriptionId($subscription->getId());
                    $throttleReq->setThrottleRate($throttleRate);
                    $request = new RequestFrame();
                    $request->setUnsubscribe($throttleReq);

                    // Request the unsubscription from the server.
                    $this->sendFrame($request);
                }
            }
        }

        /**
         * Unsubscribes the specified destination.
         * @param   string  $destination    The destination of the stream to stop getting data for.
         * @return  void
         */
        function unsubscribe(string $destination)
        {            
            // Get the subscription from the array.
            $subscription = $this->subscriptionsByDest[$destination];

            if (isset($subscription))
            {
                // If the subscription exists,
                unset($this->subscriptionsById[$subscription->getId()]);
                unset($this->subscriptionsByDest[$subscription->getDestination()]);

                if ($this->isConnected)
                {
                    // If connected, send the unsubscription request.
                    $unsubscribeReq = new UnsubscribeRequest();
                    $unsubscribeReq->setSubscriptionId($subscription->getId());
                    $request = new RequestFrame();
                    $request->setUnsubscribe($unsubscribeReq);

                    // Request the unsubscription from the server.
                    $this->sendFrame($request);
                }
            }
        }

        /**
         * Resubscribes all current subscriptions to the back-end (after disconnect/initial 
         * connection - i.e. when not present in current connection).
         * @return  void
         */
        function resubscribeAll()
        {
            // Reset the subscription ID.
            $this->nextId = 0;
            // Get a copy of the subscriptions array.
            $toSubscribe = $this->subscriptionsById;            

            // Clear the subscriptions arrays.
            $this->clearSubscriptions();
            
            foreach ($toSubscribe as $subscription)
            {
                // Go through all subscriptions and subscribe (with new ids).
                $subscription->setId($this->nextId);
                $this->subscribe($subscription);
            }
        }

        /**
         * Clears the subscription references.
         * @return  void
         */
        function clearSubscriptions()
        {
            // Clear the subscriptions arrays.
            $this->subscriptionsById = [];
            $this->subscriptionsByDest = [];
        }

        /**
         * Handles a Protostomp response message.
         * @param   Message $message    The Protostomp response message to handle.
         * @return  void
         */
        function handleMessage(Message $message)
        {
            // Convert the raw binary message to a response frame.
            $response = new ResponseFrame();
            $response->mergeFromString($message->getPayload());

            switch ($response->getResponse())
            {
                case 'connected':
                    // If this is a connected response, set connected flag.
                    $this->handleLoggedIn();
                    break;
                
                case 'heartbeat':
                    // Log heartbeat (for now).
                    echo "Received: Heartbeat\n";
                    // Mark as the last time a message was received.
                    $this->lastMessageTime = microtime(true);
                    break;

                case 'single_message':
                    // If a single update message was recieved, handle it.
                    $this->handleMessageResponse($response->getSingleMessage());
                    // Set last message time to now.
                    $this->lastMessageTime = microtime(true);
                    break;

                case 'batch_messages':
                    // If a batch update message was recieved, handle it.
                    $this->handleMessageResponses($response->getBatchMessages());
                    // Set last message time to now.
                    $this->lastMessageTime = microtime(true);
                    break;

                default:
                    // Log any unknown message types.
                    echo "Received: Unknown ".$response->getResponse()."\n";
                    break;
            }            
        }

        /**
         * Handles a MessageResponses message that includes multiple MessageResponse messages.
         * @param   MessageResponses    $responses  Holds a group of MessageResponse messages.
         * @return  void
         */
        function handleMessageResponses(MessageResponses $responses)
        {            
            // Go through messages and handle each.
            foreach ($responses->getMessage() as $message) 
                $this->handleMessageResponse($message);
        }

        /**
         * Handles a single MessageResponse message (i.e. stream message). Will find the
         * appropriate subscription and have it handle the message accordingly.
         * @param   MessageResponse $message    The single message to handle.
         */
        function handleMessageResponse(MessageResponse $message)
        {
            // Try to get the subscription tied to this message.
            $subscription = $this->subscriptionsById[$message->getSubscriptionId()];

            // If the subscription is found, handle the stream message.
            if (isset($subscription))
                $subscription->handleMessage($message->getBody());
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
                $subscription = new StrategySubscription($this->nextId, $strategyId, $symbol, 
                                                         $this->stream, $throttleRate, $this);
                // Subscribe.
                $this->subscribe($subscription);
            });
        }

        /**
         * Changes the maximum rate at which the back-end sends Strategy updates for the given strategy and symbol.
         * @param   string  $strategyID     The identifier of the strategy to throttle.
         * @param   string  $symbol         The symbol to change the Strategy throttle rate for.
         * @param   int     $throttleRate   The new throttle rate to set to (in ms). Enter 0 for no throttling.
         * @return  void
         */
        public function throttleStrategy(string $strategyId, string $symbol, int $throttleRate = 0)        
        {
            // Throttle within the loop.
            $this->loop->futureTick(function() use ($strategyId, $symbol, $throttleRate)
            {
                // Create strategy destination and throttle.
                $this->throttle(StrategySubscription::createDestination(
                                    $strategyId, $symbol, $this->stream), $throttleRate);
            });
        }

        /**
         * Unsubscribes from Stategy data for the given strategy and symbol.
         * @param   string  $strategyID The identifier of the strategy to stop running.
         * @param   string  $symbol     The symbol to stop getting Strategy data for.
         * @return  void
         */
        public function unsubscribeStrategy(string $strategyId, string $symbol)
        {            
            // Unsubscribe within the loop.
            $this->loop->futureTick(function() use ($strategyId, $symbol)
            {
                // Create strategy destination and unsubscribe.
                $this->unsubscribe(StrategySubscription::createDestination($strategyId, $symbol, $this->stream));
            });
        }

        /**
         * Subscribes to Perception gauge update data stream for a specific symbol.
         * @param   string  $symbol         Symbol to get the Perception gauge update data for.
         * @param   int     $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @return  void
         */
        public function subscribePerception(string $symbol, int $throttleRate = 0)        
        {
            // Subscribe within the loop.
            $this->loop->futureTick(function() use ($symbol, $throttleRate)
            {
                // Create a new Perception subscription.
                $subscription = new PerceptionSubscription($this->nextId, $symbol, 
                                                           $this->stream, $throttleRate, $this);
                // Subscribe.
                $this->subscribe($subscription);
            });
        }

        /**
         * Changes the maximum rate at which the back-end sends Perception gauge updates for the given symbol.         
         * @param   string  $symbol         The symbol to change the Perception gauge throttle rate for.
         * @param   int     $throttleRate   The new throttle rate to set to (in ms). Enter 0 for no throttling.
         * @return  void
         */
        public function throttlePerception(string $symbol, int $throttleRate = 0)        
        {
            // Throttle within the loop.
            $this->loop->futureTick(function() use ($symbol, $throttleRate)
            {
                // Create Perception destination and throttle.
                $this->throttle(PerceptionSubscription::createDestination($symbol, $this->stream), $throttleRate);
            });
        }

        /**
         * Unsubscribes from Perception gauge data for the given symbol.
         * @param   string  $symbol     The symbol to stop getting Perception data for.
         * @return  void
         */
        public function unsubscribePerception(string $symbol)
        {            
            // Unsubscribe within the loop.
            $this->loop->futureTick(function() use ($strategyId, $symbol)
            {
                // Create Perception destination and unsubscribe.
                $this->unsubscribe(PerceptionSubscription::createDestination($symbol, $this->stream));
            });
        }

        /**
         * Handles the timer on a 5-second interval.
         * @return  void
         */
        function handleTimer()
        {
            // Get the current time.
            $time = microtime(true);

            if (!$this->isConnected && !$this->isDisconnecting)
            {
                // If not connected, check if we need to reconnect.
                if ($time > $this->reconnectTime && $this->reconnectTime !== 0.0)
                {
                    // If we need to connect, reconnect.
                    $this->connectWebsocket();
                    $this->reconnectTime = 0.0;
                    $this->killTime = $time + APIClient::CONNECT_KILL;
                }
                else if ($time > $this->killTime && $this->killTime !== 0.0)
                {
                    // If we need to kill the connection, kill it.
                    $this->disconnect();
                    $this->killTime = 0;
                }
            }
            else if ($this->isConnected && !$this->isDisconnecting)
            {
                // If connected and not in the process of disconnecting.
                if ($time > $this->lastMessageTime + APIClient::MAX_HEARTBEAT_WAIT)
                {
                    // If it's been too long before receiving a message, disconnect (to reconnect).
                    $this->disconnect();
                }
                else if ($time > $this->lastMessageTime + APIClient::HEARTBEAT_CHECK_WAIT)
                {
                    // If past the last heartbeat checks, create a new heartbeat request frame.
                    $request = new RequestFrame();
                    $request->setHeartbeat(new Heartbeat());
                    // Send the request to the server.
                    $this->sendFrame($request);
                }
            }
        }
    }

?>