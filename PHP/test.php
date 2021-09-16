<?php

    require __DIR__ . '/QuantGateSignalsAPI/vendor/autoload.php';
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/RequestFrame.php';
    require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ConnectRequest.php';

    $client = new APIClient("this", 12);

    \Ratchet\Client\connect('wss://test.stealthtrader.com:443')->then(function($conn)
     {
        $conn->on('message', function($msg) use ($conn) {
            echo "Received: {$msg}\n";
            $conn->close();
        });

        echo "Connected!\n";

        $connectReq = new ConnectRequest();
        $request = new RequestFrame();
        $request->setConnect($connectReq);


        $conn->send('Hello World!');
    }, function ($e) 
    {
        echo "Could not connect: {$e->getMessage()}\n";
    });

?>