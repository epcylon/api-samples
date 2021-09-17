<?php

  //  require __DIR__ . '/QuantGateSignalsAPI/vendor/autoload.php';
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';
  //  require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/RequestFrame.php';
  //  require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ResponseFrame.php';
  //  require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ConnectRequest.php';
  //  require __DIR__ . '/QuantGateSignalsAPI/Proto/Stomp/ConnectedResponse.php';
  //  require __DIR__ . '/QuantGateSignalsAPI/Proto/GPBMetadata/StompV01.php';

    use \Stomp\ConnectRequest;
    use \Stomp\RequestFrame;
    use \Stomp\ResponseFrame;
    use \Stomp\ConnectedResponse;
    use \Ratchet\Client;
    use \Ratchet\RFC6455\Messaging\Frame;

    $loop = \React\EventLoop\Factory::create();

    $client = new APIClient($loop, "ws://localhost", 2432);
    $client->connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.".
                     "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw".
                     "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9.".
                     "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");

    $loop->addTimer(30, function() use (&$client) 
    {
        $client->close();
    });

    $loop->run();

    echo "Done!\n";
?>