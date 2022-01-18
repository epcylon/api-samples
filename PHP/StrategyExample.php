<?php
  
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\ErrorDetails;
    use \QuantGate\API\Signals\Events\StrategyUpdate;
    use \QuantGate\API\Signals\Events\TopSymbolsUpdate;

    // Strategy ID constant.
    $strategyId = "Crb7.6";
    // Symbols we will be subscribing to.
    $symbols = array("USD.CAD", "AUD.CAD", "AUD.USD", "CAD.JPY", "CHF.JPY", "EUR.AUD", "EUR.GBP", "EUR.NOK",
                     "EUR.USD", "GBP.CAD", "GBP.JPY", "NZD.USD", "USD.CAD", "USD.JPY", "USD.SEK",
                     "CCM X1-B3", "DOL X1-B3", "IND V1-B3", "BGI V1-B3", "WDO X1-B3", "WIN V1-B3", "INVALID");

    // Create an event loop to run with.
    $loop = \React\EventLoop\Factory::create();
    
    // Create the client with the event loop reference, and pointing to the test server.
    $client = new APIClient($loop, "wss://feed.stealthtrader.com");

    // Close after 40 seconds (remove this line and the cancellation to keep going indefinitely)
    $timer = $loop->addTimer(40, function() use ($client) { $client->close(); });

    // Add event handler to signal when connected/disconnected.
    $client->on('connected', function() { echo "Connected!\n"; });
    $client->on('disconnected', function() use ($loop, $timer)
    {
        // Log disconnected, and cancel the closure timer.
        echo "Disonnected!\n";
        $loop->cancelTimer($timer);
    });

    // Log any error events.
    $client->on('error', function(ErrorDetails $details)
    {
        echo "Error received: ".$details->getMessage()."\n"; 
    });
    
    // Set up the callback to handle strategy updates.
    $client->on('strategyUpdated', function (StrategyUpdate $update)
    {
        if (null !== $update->getError())
        {
            echo "Strategy Subscription Error: ".$update->getSymbol().", ".$update->getError()->getMessage()."\n";
        }
        else
        {                        
            echo "Strategy Update: ".$update->getSymbol().", ".
                 ($update->getEntryProgress() * 100.0)."%, ".$update->getSignal()."\n";
        }
    });

    // Set up the callback to handle trigger updates.
    $client->on('topSymbolsUpdated', function (TopSymbolsUpdate $update)
    {
        if (null !== $update->getError())
        {
            echo "Top Symbols Subscription Error: ".$update->getBroker().", ".
                 $update->getInstrumentType().", ".$update->getError()->getMessage()."\n";
        }
        else
        {  
            echo "Top Symbols Update: ".$update->getBroker().", ".$update->getInstrumentType()."\n";
        }
    });

    // Connect with a JWT token.
    $client->connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                     "eyJzdWIiOiJUZXN0ZXIiLCJpYXQiOjE2Mzk3NjYzMjQsImV4cCI6MTY0MDk5N"+
                     "TIwMCwiYXVkIjoiMldVamVvYlJYUlc5cHNOREVjeGUxTUQ5d3RkZmRoMUMifQ." +
                     "i1UwC0tuCMw7KVxlN0ieCyyUwWNTmzQ4ZjikLZfj0eM");

    // Go through the symbols and subscribe to each.
    foreach ($symbols as $value)
        $client->subscribeStrategy($strategyId, $value, 100);

    // Subscribe to Top Symbols stream to test.
    $client->subscribeTopSymbols("ib");
    // Throttle the "USD.CAD" strategy to 10 seconds.
    $client->throttleStrategy($strategyId, "USD.CAD", 10000);
    // Unsubscribe from "CAD.JPY".
    $client->unsubscribeStrategy($strategyId, "CAD.JPY");

    // Continue running until there are no more events to handle.
    $loop->run();

    // Indicate when finished running.
    echo "Done!\n";
?>