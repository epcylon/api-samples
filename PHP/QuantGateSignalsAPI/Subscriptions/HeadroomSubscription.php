<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\HeadroomUpdate;
    use \QuantGate\API\Signals\Events\SubscriptionError;
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a headroom gauge stream subscription and convert the received messages.
     */
    class HeadroomSubscription extends SubscriptionBase
    {
        /**
         * Symbol to get the Headroom gauge updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;

        /** 
         * Creates a new HeadroomSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Headroom gauge updates data for.
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
            $update = new \Stealth\SingleValueUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            $value = $update->getValue() / 1000.0;
            $isDirty = $update->getIsDirty();         

            // Create the update object.
            $result = new HeadroomUpdate($updateTime, $this->symbol, $this->stream, $value, $isDirty, null);

            // Send the results back to the APIClient class.
            $this->client->emit('headroomUpdated', [$result]);
        }

        /**
         * Called to send a subscription error to the subscribers.
         * @param   $error  The error information to send.
         * @return  void
         */
        public function sendError(SubscriptionError $error)
        {
            // Create the update object.
            $result = new HeadroomUpdate(new \DateTime(), $this->symbol, $this->stream, 0.0, true, $error);

            // Send the results back to the APIClient class.
            $this->client->emit('headroomUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this Headroom gauge stream.
         * @param   string  $symbol   Symbol to get the Headroom update data for.
         * @param   string  $stream   Stream ID associated with the stream the client is connected to (realtime, delay, demo).         
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream) : string
        {            
            return "/gauge/head/".Utilities::streamIdForSymbol($stream, $symbol)."/".$symbol."/5m";
        }
    }

?>
