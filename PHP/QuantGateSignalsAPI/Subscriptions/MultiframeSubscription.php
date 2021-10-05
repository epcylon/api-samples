<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\MultiframeUpdate;
    use \QuantGate\API\Signals\Events\SubscriptionError;
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a Multiframe Equilibrium gauge stream subscription and convert the received messages.
     */
    class MultiframeSubscription extends SubscriptionBase
    {
        /**
         * Symbol to get the Multiframe Equilibrium gauge updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;

        /** 
         * Creates a new MultiframeSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Multiframe Equilibrium gauge updates data for.
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
            $update = new \Stealth\MultiframeUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            $min5 = $update->getMin5() / 1000;
            $min10 = $update->getMin10() / 1000;
            $min15 = $update->getMin15() / 1000;
            $min30 = $update->getMin30() / 1000;
            $min45 = $update->getMin45() / 1000;
            $min60 = $update->getMin60() / 1000;
            $min120 = $update->getMin120() / 1000;
            $min180 = $update->getMin180() / 1000;
            $min240 = $update->getMin240() / 1000;
            $day1 = $update->getDay1() / 1000;
            $isDirty = $update->getIsDirty();

            // Create the update object.
            $result = new MultiframeUpdate($updateTime, $this->symbol, $this->stream, $min5, $min10, $min15, $min30,
                                           $min45, $min60, $min120, $min180, $min240, $day1, $isDirty, null);

            // Send the results back to the APIClient class.
            $this->client->emit('multiframeUpdated', [$result]);
        }

        /**
         * Called to send a subscription error to the subscribers.
         * @param   $error  The error information to send.
         * @return  void
         */
        public function sendError(SubscriptionError $error)
        {
            // Create the update object.
            $result = new MultiframeUpdate(new \DateTime, $this->symbol, $this->stream, 0.0, 0.0, 
                                           0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, true, $error);

            // Send the results back to the APIClient class.
            $this->client->emit('multiframeUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this Multiframe Equilibrium gauge stream.
         * @param   string  $symbol   Symbol to get the Multiframe Equilibrium update data for.
         * @param   string  $stream   Stream ID associated with the stream the client is connected to (realtime, delay, demo).         
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream) : string
        {            
            return "/gauge/meq/".Utilities::streamIdForSymbol($stream, $symbol)."/".$symbol;
        }
    }

?>
