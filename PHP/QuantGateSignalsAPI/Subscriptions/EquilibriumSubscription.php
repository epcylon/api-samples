<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\EquilibriumUpdate;
    use \QuantGate\API\Signals\Events\SubscriptionError;
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a equilibrium gauge stream subscription and convert the received messages.
     */
    class EquilibriumSubscription extends SubscriptionBase
    {
        /**
         * The total number of bars in the Equilibrium gauge.
         * @var int
         */
        public const TOTAL_BARS = 55;

        /**
         * Symbol to get the Equilibrium gauge updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;
        /**
         * Compression timeframe to apply to the gauge. Default value is 300s.
         * @var string
         */
        private string $compression;

        /** 
         * Creates a new EquilibriumSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Equilibrium gauge updates data for.
         * @param string   $stream         Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param string   $compression    Compression timeframe to apply to the gauge. Default value is 300s.
         * @param int      $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param          $client         Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $symbol, string $stream, string $compression, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->compression = Utilities::cleanCompression($compression);

            // Create the target destination.
            $destination = $this->createDestination($symbol, $stream, $compression);

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
            $update = new \Stealth\EquilibriumUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            $equilibriumPrice = Utilities::decodePrice($update->getEquilibriumPrice());
            $gapSize = Utilities::decodePrice($update->getGapSize());
            $lastPrice = Utilities::decodePrice($update->getLastPrice());
            $high = $update->getHigh() / 1000.0;
            $low = $update->getLow() / 1000.0;
            $projected = $update->getProjected() / 1000.0;
            $bias = $update->getBias() / 1000.0;
            $isDirty = $update->getIsDirty();

            // Create the update object.
            $result = new EquilibriumUpdate($updateTime, $this->symbol, $this->stream, $this->compression, 
                                            $equilibriumPrice, $gapSize, $lastPrice, $high, $low, 
                                            $projected, $bias, $isDirty, null);

            // Send the results back to the APIClient class.
            $this->client->emit('equilibriumUpdated', [$result]);
        }

        /**
         * Called to send a subscription error to the subscribers.
         * @param   $error  The error information to send.
         * @return  void
         */
        public function sendError(SubscriptionError $error)
        {
            // Create the update object.
            $result = new EquilibriumUpdate(new \DateTime(), $this->symbol, $this->stream, $this->compression,
                                            0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, true, $error);

            // Send the results back to the APIClient class.
            $this->client->emit('equilibriumUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this Equilibrium gauge stream.
         * @param   string  $symbol       Symbol to get the Equilibrium update data for.
         * @param   string  $stream       Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param   string  $compression  Compression timeframe to apply to the gauge. Default value is 300s.
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream, string $compression) : string
        {            
            return "/gauge/eq/".Utilities::streamIdForSymbol($stream, $symbol).
                    "/".$symbol."/".Utilities::cleanCompression($compression);
        }
    }

?>
