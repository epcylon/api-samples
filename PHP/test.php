<?php
  
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\StrategyUpdate;

    $loop = \React\EventLoop\Factory::create();
    
    $client = new APIClient($loop, "wss://test.stealthtrader.com");
    $client->connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.".
                     "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw".
                     "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9.".
                     "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");    

    $client->addStrategyUpdateCallback(function (StrategyUpdate $update)
    {        
        echo $update->getSymbol().", ".($update->getEntryProgress() * 100.0)."%, ".$update->getSignal()."\n";
    });

    $client->subscribeStrategy("Crb7.6", "NQ Z1", 0);
    $client->subscribeStrategy("Crb7.6", "AUD.CAD", 0);

    $loop->addTimer(30, function() use (&$client) 
    {
        $client->close();
    });

    $loop->run();

    echo "Done!\n";
?>