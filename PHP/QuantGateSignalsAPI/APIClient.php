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
    use \Ratchet\Client\WebSocket;
    use \Ratchet\RFC6455\Messaging\Message;
    use \QuantGate\API\Signals\Subscriptions\SubscriptionBase;
    use \QuantGate\API\Signals\Subscriptions\StrategySubscription;

    class APIClient
    {
        public const REALTIME_STREAM = "realtime";
        public const DELAY_STREAM = "delay";
        public const DEMO_STREAM = "demo";

        private LoopInterface $loop;

        public string $host;
        public string $port;
        public string $stream;
        public WebSocket $webSocket;        
        public bool $isConnected = false;
        public int $nextID = 1;

        public string $jwtToken;

        public array $subscriptionsById = [];
        public array $subscriptionsByDest = [];
 
        function __construct(LoopInterface $loop, string $host, int $port = 443, string $stream = self::REALTIME_STREAM)
        {
            $this->loop = $loop;
            $this->host = $host;
            $this->port = $port;
            $this->stream = $stream;
            
            $this->loop->addPeriodicTimer(5, array($this, 'checkHeartbeats'));
        }

        function checkHeartbeats()
        {
            $request = new RequestFrame();
            $request->setHeartbeat(new Heartbeat());
            
            $this->sendFrame($request);
            echo "Heartbeat check\n";
        }

        public function sendFrame(RequestFrame $frame)
        {
            echo "Sending\n";
            $data = $frame->serializeToString();            
            $binary = new Frame($data, true, Frame::OP_BINARY);
            $this->webSocket->send($binary);
            echo "Sent\n";
        }

        public function subscribeStrategy(string $strategyId, string $symbol, int $throttleRate = 0)        
        {
            $this->loop->futureTick(function() use ($strategyId, $symbol, $throttleRate)
            {
                $subscription = new StrategySubscription($this->nextID, $strategyId, $symbol, $this->stream, $throttleRate);
                $this->nextID++;
                $this->subscribe($subscription);
            });
        }

        function subscribe(SubscriptionBase $subscription)
        {
            $this->subscriptionsById[$subscription->getID()] = $subscription;
            $this->subscriptionsByDest[$subscription->getDestination()] = $subscription;

            if ($this->isConnected)
            {
                $subscribeReq = new SubscribeRequest();
                $subscribeReq->setSubscriptionId($subscription->getID());
                $subscribeReq->setDestination($subscription->getDestination());
                $subscribeReq->setThrottleRate($subscription->getThrottleRate());
                
                $request = new RequestFrame();
                $request->setSubscribe($subscribeReq);

                $this->sendFrame($request);
            }
        }

        function connect($jwtToken)
        {            
            $this->jwtToken = $jwtToken;

            //\Ratchet\Client\connect('wss://test.stealthtrader.com:443')->then(function($conn)
            Client\connect($this->host.':'.$this->port, [], [], $this->loop)->then(
                function($conn)
                {
                    $this->webSocket = $conn;

                    $conn->on('message', array($this, 'handleMessage'));        
                    
                    echo "Connected!\n";
                    echo "Creating\n";

                    $connectReq = new ConnectRequest();
                    $connectReq->setAcceptVersion("1.0");
                    $connectReq->setLogin("JohnH");
                    $connectReq->setPasscode($this->jwtToken);
                    
                    $request = new RequestFrame();
                    $request->setConnect($connectReq);

                    $this->sendFrame($request);
                }, 
                function ($e) 
                {
                    echo "Could not connect: {$e->getMessage()}\n";
                });
        }

        function resubscribeAll()
        {
            $toSubscribe = $this->subscriptionsById;            

            $this->subscriptionsById = [];
            $this->subscriptionsByDest = [];

            foreach ($toSubscribe as $subscription)
            {
                $this->subscribe($subscription);
            }
        }

        function handleMessage(Message $message)
        {
            $response = new ResponseFrame();
            $response->mergeFromString($message->getPayload());

            switch ($response->getResponse())
            {
                case 'connected':
                    echo "Received: {$response->getConnected()->getVersion()}\n";
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
            $subscription = $this->subscriptionsById[$message->getSubscriptionId()];

            if (isset($subscription))
            {
                echo "Got the subscription!\n";
                $subscription->handleMessage($message->getBody());
            }
        }

        function close()
        {
            $this->isConnected = false;
            $this->webSocket->close();
            $this->loop->stop();
        }
    }

?>