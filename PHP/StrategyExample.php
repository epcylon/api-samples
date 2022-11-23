<?php
  
    require __DIR__ . '/QuantGateSignalsAPI/APIClient.php';

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\ErrorDetails;
    use \QuantGate\API\Signals\Events\StrategyUpdate;
    use \QuantGate\API\Signals\Events\TopSymbolsUpdate;

    // Strategy ID constant.
    $strategyId = "Crb7.6";
    $updated = false;
    // Symbols we will be subscribing to.
    //$symbols = array("USD.CAD", "AUD.CAD", "AUD.USD", "CAD.JPY", "CHF.JPY", "EUR.AUD", "EUR.GBP", "EUR.NOK",
    //                 "EUR.USD", "GBP.CAD", "GBP.JPY", "NZD.USD", "USD.CAD", "USD.JPY", "USD.SEK",
    //                 "CCM X1-B3", "DOL X1-B3", "IND V1-B3", "BGI V1-B3", "WDO X1-B3", "WIN V1-B3", "INVALID");

    $symbols = array("ETHUSD:BITF", "ADAUSD:BITF", "ATOMUSD:CBPR", "BCHUSD:BITF", "BTCEUR:BITF", "DOGEUSD:BITF",
                     "DOTUSD:BITF", "ETHEUR:BITF", "LINKUSD:BITF", "LTCUSD:BITF", "SOLUSD:BITF", "TRXUSD:BITF", 
                     "XLMUSD:BITF", "XMRUSD:BITF", "BNBUSD:CXIO", "MANAUSD:BITF", "ALGOUSD:FMFW", "UNIUSD:CBPR",
                     "MATICUSD:FMFW", "AVAXUSD:CBPR", "LUNAUSD:BITF", "XRPUSD:BITF");

    // Create an event loop to run with.
    $loop = \React\EventLoop\Factory::create();
    
    // Create the client with the event loop reference, and pointing to the test server.
    $client = new APIClient($loop, "wss://feed.stealthtrader.com");

    // Close after 30 seconds, if no updates (remove this line and the cancellation to keep going indefinitely)
    $timer = $loop->addTimer(30, function() use ($client, &$updated) 
    {
        if ($updated == true)
        {
            $updated = false;
        }
        else
        {
            echo "No updates in the last 30 seconds! Stopping...\n";
            $client->close(); 
        }
    });

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
    $client->on('strategyUpdated', function (StrategyUpdate $update) use (&$updated)
    {
        if (null !== $update->getError())
        {
            echo "Strategy Subscription Error: ".$update->getSymbol().", ".$update->getError()->getMessage()."\n";
        }
        else
        {            
            $updated = true;
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
    $client->connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.".
                     "eyJzdWIiOiJKb2huVGVzdCIsImlhdCI6MTY1MDU2ODk1MiwiZXhwIjoxOTI0OT".
                     "A1NjAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9.".
                     "EHaFHW_y1ZCuHv6rmN8Lr3tTQ0U3Ci6O832Pv7w_NQA");

    // Go through the symbols and subscribe to each.
    foreach ($symbols as $value)
        $client->subscribeStrategy($strategyId, $value, 100);

    // Subscribe to Top Symbols stream to test.
   // $client->subscribeTopSymbols("ib");
    // Throttle the "USD.CAD" strategy to 10 seconds.
   // $client->throttleStrategy($strategyId, "USD.CAD", 10000);
    // Unsubscribe from "CAD.JPY".
   // $client->unsubscribeStrategy($strategyId, "CAD.JPY");

    // Continue running until there are no more events to handle.
    $loop->run();

    // Indicate when finished running.
    echo "Done!\n";
?>