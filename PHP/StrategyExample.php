<?php
  
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\StrategyUpdate;
    use \QuantGate\API\Signals\Events\TriggerUpdate;

    // Strategy ID constant.
    $strategyId = "Crb7.6";
    // Symbols we will be subscribing to.
    $symbols = array("USD.CAD", "AUD.CAD", "AUD.USD", "CAD.JPY", "CHF.JPY", "EUR.AUD", "EUR.GBP", "EUR.NOK",
                     "EUR.USD", "GBP.CAD", "GBP.JPY", "NZD.USD", "USD.CAD", "USD.JPY", "USD.SEK",
                     "CCM X1-B3", "DOL V1-B3", "IND V1-B3", "BGI V1-B3", "WDO V1-B3", "WIN V1-B3");

    // Create an event loop to run with.
    $loop = \React\EventLoop\Factory::create();
    
    // Create the client with the event loop reference, and pointing to the test server.
    $client = new APIClient($loop, "wss://test.stealthtrader.com");

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
        echo "Strategy Update: ".$update->getSymbol().", ".($update->getEntryProgress() * 100.0)."%, ".$update->getSignal()."\n";
    });

    // Set up the callback to handle trigger updates.
    $client->on('triggerUpdated', function (TriggerUpdate $update)
    {        
        echo "Trigger Update: ".$update->getSymbol().", ".$update->getEquilibriumStd()."\n";
    });

    // Connect with a JWT token.
    $client->connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.".
                     "eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz".
                     "A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0.".
                     "xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8");

    // Go through the symbols and subscribe to each.
    foreach ($symbols as $value)
        $client->subscribeStrategy($strategyId, $value, 100);

    // Subscribe to Commitment stream to test.
    $client->subscribeTrigger("EUR.USD");
    // Throttle the "USD.CAD" strategy to 10 seconds.
    $client->throttleStrategy($strategyId, "USD.CAD", 10000);
    // Unsubscribe from "CAD.JPY".
    $client->unsubscribeStrategy($strategyId, "CAD.JPY");    

    // Continue running until there are no more events to handle.
    $loop->run();

    // Indicate when finished running.
    echo "Done!\n";
?>