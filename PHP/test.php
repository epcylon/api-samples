<?php

    require __DIR__ . '/QuantGateSignalsAPI/vendor/autoload.php';
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/RequestFrame.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ResponseFrame.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ConnectRequest.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ConnectedResponse.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/GPBMetadata/StompV01.php';

    use \Stomp\ConnectRequest;
    use \Stomp\RequestFrame;
    use \Stomp\ResponseFrame;
    use \Stomp\ConnectedResponse;
    use \Ratchet\Client;
    use \Ratchet\RFC6455\Messaging\Frame;

    $client = new APIClient("this", 12);    

    //\Ratchet\Client\connect('wss://test.stealthtrader.com:443')->then(function($conn)
    Client\connect('ws://localhost:2432')->then(function($conn)
     {
        $conn->on('message', function($msg) use ($conn) {
            $response = new ResponseFrame();
            $response->mergeFromString($msg->getPayload());
            echo "Received: {$response->getConnected()->getVersion()}\n";
            $conn->close();
        });        
        
        echo "Connected!\n";
        echo "Creating\n";

        $connectReq = new ConnectRequest();
        $connectReq->setAcceptVersion("1.0");
        $connectReq->setLogin("JohnH");
        $connectReq->setPasscode("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.".
                                 "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw".
                                 "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9.".
                                 "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");
        
        $request = new RequestFrame();
        $request->setConnect($connectReq);
        $data = $request->serializeToString();

        echo "Sending\n";

        $binary = new Frame($data, true, Frame::OP_BINARY);
        $conn->send($binary);

        echo "Sent\n";

    }, function ($e) 
    {
        echo "Could not connect: {$e->getMessage()}\n";
    });

?>