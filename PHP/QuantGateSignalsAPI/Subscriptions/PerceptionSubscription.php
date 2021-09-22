<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\PerceptionUpdate;    
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a perception gauge stream subscription and convert the received messages.
     */
    class PerceptionSubscription extends SubscriptionBase
    {
        /**
         * Symbol to get the Perception gauge updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;
        /**
         * Holds a reference to the parent APIClient instance to send updates to.
         * @var callback
         */        
        private APIClient $client;

        /** 
         * Creates a new PerceptionSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Perception gauge updates data for.
         * @param string   $stream         Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param int      $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param          $client         Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $symbol, string $stream, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->client = $client;

            // Create the target destination.
            $destination = $this->createDestination($symbol, $stream);

            // Initialize values in the parent class.
            parent::__construct($destination, $id, $throttleRate);
        }

        /**
         * Handles new messages from the stream and converts to subscription updates.
         * @param   $body   The raw message received from the stream.
         * @return void
         */
        public function handleMessage($body)
        {
            // Convert the message to a (Protobuf) strategy update object.
            $update = new \Stealth\SingleValueUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            $value = $update->getValue() / 1000.0;
            $isDirty = $update->getIsDirty();         

            // Create the update object.
            $result = new PerceptionUpdate($updateTime, $this->symbol, $this->stream, $value, $isDirty);

            // Send the results back to the APIClient class.
            $this->client->emit('perceptionUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this strategy stream.
         * @param   string  $strategyID Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @param   string  $symbol     Symbol to get the Strategy update data for.
         * @param   string  $stream     Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream) : string
        {            
            return "/gauge/per2/".Utilities::streamIdForSymbol($stream, $symbol)."/".$symbol;
        }
    }

?>
    