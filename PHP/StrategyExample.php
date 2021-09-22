<?php
  
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\StrategyUpdate;
    use \QuantGate\API\Signals\Events\PerceptionUpdate;

    // Strategy ID constant.
    $strategyId = "Crb7.6";
    // Symbols we will be subscribing to.
    $symbols = array("USD.CAD", "AUD.CAD", "AUD.USD", "CAD.JPY", "CHF.JPY", "EUR.AUD", "EUR.GBP", "EUR.NOK",
                     "EUR.USD", "GBP.CAD", "GBP.JPY", "NZD.USD", "USD.CAD", "USD.JPY", "USD.SEK", "CCM X1-B3",
                     "DOL V1-B3", "IND V1-B3", "BGI V1-B3", "WDO V1-B3", "WIN V1-B3");

    // Create an event loop to run with.
    $loop = \React\EventLoop\Factory::create();
    
    // Create the client with the event loop reference, and pointing to the test server.
    $client = new APIClient($loop, "wss://test.stealthtrader.com");

    // Close after 60 seconds (remove this line and the cancellation to keep going indefinitely)
    $timer = $loop->addTimer(60, function() use ($client) { $client->close(); });

    // Add event handler to signal when connected/disconnected.
    $client->on('connected', function() { echo "Connected!\n"; });
    $client->on('disconnected', function() use ($loop, $timer)
    {
        // Log disconnected, and cancel the closure timer.
        echo "Disonnected!\n";
        $loop->cancelTimer($timer);
    });

    // Set up the callback to handle strategy updates.
    $client->on('strategyUpdated', function (StrategyUpdate $update)
    {        
        echo "Strategy Update: ".$update->getSymbol().", ".($update->getEntryProgress() * 100.0)."%, ".$update->getSignal()."\n";
    });

    // Set up the callback to handle perception updates.
    $client->on('perceptionUpdated', function (PerceptionUpdate $update)
    {        
        echo "Perception Update: ".$update->getSymbol().", ".($update->getValue() * 100.0)."%\n";
    });

    // Connect with a JWT token.
    $client->connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.".
                     "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw".
                     "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9.".
                     "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");

    // Go through the symbols and subscribe to each.
    foreach ($symbols as $value)
        $client->subscribeStrategy($strategyId, $value, 100);

    // Subscribe to Perception stream to test.
    $client->subscribePerception("USD.CAD", 100);
    // Throttle the "USD.CAD" strategy to 10 seconds.
    $client->throttleStrategy($strategyId, "USD.CAD", 10000);
    // Unsubscribe from "CAD.JPY".
    $client->unsubscribeStrategy($strategyId, "CAD.JPY");    

    // Continue running until there are no more events to handle.
    $loop->run();

    // Indicate when finished running.
    echo "Done!\n";
?>