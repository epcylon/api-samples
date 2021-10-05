<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\CommitmentUpdate;
    use \QuantGate\API\Signals\Events\SubscriptionError;
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a commitment gauge stream subscription and convert the received messages.
     */
    class CommitmentSubscription extends SubscriptionBase
    {
        /**
         * Symbol to get the Commitment gauge updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;

        /** 
         * Creates a new CommitmentSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Commitment gauge updates data for.
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
            $result = new CommitmentUpdate($updateTime, $this->symbol, $this->stream, $value, $isDirty, null);

            // Send the results back to the APIClient class.
            $this->client->emit('commitmentUpdated', [$result]);
        }

        /**
         * Called to send a subscription error to the subscribers.
         * @param   $error  The error information to send.
         * @return  void
         */
        public function sendError(SubscriptionError $error)
        {
            // Create the update object.
            $result = new CommitmentUpdate(new \DateTime, $this->symbol, $this->stream, 0.0, true, $error);

            // Send the results back to the APIClient class.
            $this->client->emit('commitmentUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this Commitment gauge stream.
         * @param   string  $symbol   Symbol to get the Commitment update data for.
         * @param   string  $stream   Stream ID associated with the stream the client is connected to (realtime, delay, demo).         
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream) : string
        {            
            return "/gauge/comm/".Utilities::streamIdForSymbol($stream, $symbol)."/".$symbol."/1m";
        }
    }

?>
