<?php

    require __DIR__ . '/vendor/autoload.php';    
    require __DIR__ . '/Proto/Stomp/RequestFrame.php';
    require __DIR__ . '/Proto/Stomp/ResponseFrame.php';
    require __DIR__ . '/Proto/Stomp/Heartbeat.php';
    require __DIR__ . '/Proto/Stomp/ConnectRequest.php';
    require __DIR__ . '/Proto/Stomp/ConnectedResponse.php';
    require __DIR__ . '/Proto/GPBMetadata/StompV01.php';

    use \Stomp\ConnectRequest;
    use \Stomp\RequestFrame;
    use \Stomp\ResponseFrame;
    use \Stomp\ConnectedResponse;
    use \Stomp\Heartbeat;
    use \Ratchet\Client;
    use \Ratchet\RFC6455\Messaging\Frame;
    use \React\EventLoop\LoopInterface;
    use \Ratchet\Client\WebSocket;

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

        public string $jwtToken;
 
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

        function handleMessage(Ratchet\RFC6455\Messaging\Message $message)
        {
            $response = new ResponseFrame();
            $response->mergeFromString($message->getPayload());

            switch ($response->getResponse())
            {
                case 'connected':
                    echo "Received: {$response->getConnected()->getVersion()}\n";
                    break;
                
                case 'heartbeat':
                    echo "Received: Heartbeat\n";
                    break;

                default:
                    echo "Received: Unknown ".$response->getResponse()."\n";
                    break;
            }            
        }

        function close()
        {
            $this->webSocket->close();
            $this->loop->stop();
        }
    }

?>