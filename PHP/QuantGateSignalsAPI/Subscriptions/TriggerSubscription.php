<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\TriggerUpdate;    
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a trigger stream subscription and convert the received messages.
     */
    class TriggerSubscription extends SubscriptionBase
    {
        /**
         * Symbol to get the Trigger updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;

        /** 
         * Creates a new TriggerSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Trigger updates data for.
         * @param string   $stream         Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param int      $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param          $client         Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $symbol, string $stream, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->symbol = $symbol;
            $this->stream = $stream;

            // Create the target destination.
            $destination = $this->createDestination($symbol, $stream);

            // Initialize values in the parent class.
            parent::__construct($destination, $id, $throttleRate, $client);
        }

        /**
         * Handles new messages from the stream and converts to subscription updates.
         * @param   $body   The raw message received from the stream.
         * @return void
         */
        public function handleMessage($body)
        {
            // Convert the message to a (Protobuf) strategy update object.
            $update = new \Stealth\TriggerUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            
            $bias = $update->getBias() / 1000.0;
            $perception = $update->getPerception() / 1000.0;
            $commitment = $update->getCommitment() / 1000.0;
            $equilibriumPrice = Utilities::decodePrice($update->getEquilibriumPrice());
            $gapSize = Utilities::decodePrice($update->getGapSize());
            $lastPrice = Utilities::decodePrice($update->getLastPrice());
            $sentiment = $update->getSentiment() / 1000.0;
            $isDirty = $update->getIsDirty();

            // Create the update object.
            $result = new TriggerUpdate($updateTime, $this->symbol, $this->stream, $bias, $perception, $commitment,
                                        $equilibriumPrice, $gapSize, $lastPrice, $sentiment, $isDirty);

            // Send the results back to the APIClient class.
            $this->client->emit('triggerUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this Trigger stream.
         * @param   string  $symbol   Symbol to get the Trigger update data for.
         * @param   string  $stream   Stream ID associated with the stream the client is connected to (realtime, delay, demo).         
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream) : string
        {            
            return "/gauge/tr/".Utilities::streamIdForSymbol($stream, $symbol)."/".$symbol;
        }
    }

?>
