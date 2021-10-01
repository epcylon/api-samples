<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\StrategyUpdate;
    use \QuantGate\API\Signals\Events\SignalType; 
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a strategy stream subscription and convert the received messages.
     */
    class StrategySubscription extends SubscriptionBase
    {
        /**
         * The strategy subscribed to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @var string
         */
        private string $strategyId;
        /**
         * Symbol to get the Strategy update data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;

        /** 
         * Creates a new StrategySubscription instance.          
         * @param  int        $id            The (integer) identifier to associate with this subscription on the server's end.
         * @param  string     $strategyId    The strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @param  string     $symbol        Symbol to get the Strategy update data for.
         * @param  string     $stream        Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param  int        $throttleRate  Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param  APIClient  $client        Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $strategyId, string $symbol, string $stream, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->strategyId = $strategyId;
            $this->symbol = $symbol;
            $this->stream = $stream;

            // Create the target destination.
            $destination = $this->createDestination($strategyId, $symbol, $stream);

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
            $update = new \Stealth\StrategyUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            $entryProgress = $update->getEntryProgress() / 1000.0;
            $exitProgress = $update->getExitProgress() / 1000.0;
            $perceptionLevel = $this->convertGaugeLevel($update->getPerceptionLevel());
            $commitmentLevel = $this->convertGaugeLevel($update->getCommitmentLevel());
            $equilibriumLevel = $this->convertGaugeLevel($update->getEquilibriumLevel());
            $sentimentLevel = $this->convertGaugeLevel($update->getSentimentLevel());
            $perceptionSignal = SignalType::getFromGaugeSignal($update->getPerceptionSignal());
            $commitmentSignal = SignalType::getFromGaugeSignal($update->getCommitmentSignal());
            $equilibriumSignal = SignalType::getFromGaugeSignal($update->getEquilibriumSignal());
            $sentimentSignal = SignalType::getFromGaugeSignal($update->getSentimentSignal());
            $signal = SignalType::getFromStrategySignal($update->getSignal());            

            // Create the update object.
            $result = new StrategyUpdate($updateTime, $this->strategyId, $this->symbol, $this->stream, 
                                         $entryProgress, $exitProgress, $perceptionSignal, $perceptionLevel, 
                                         $commitmentSignal, $commitmentLevel, $equilibriumSignal,
                                         $equilibriumLevel, $sentimentSignal, $sentimentLevel, $signal);

            // Send the results back to the APIClient class.
            $this->client->emit('strategyUpdated', [$result]);
        }

        /**
         * Converts a raw gauge level value from an integer to a nullable float.
         * @param   int $level  The raw gauge level to convert.
         * @return  null|float
         */
        private function convertGaugeLevel(int $level) : ?float
        {
            // If zero, the result should be null.
            if ($level === 0)
                return null;

            // Convert non-zero values to a level.
            return ($level - 1001) / 1000.0;
        }

        /**
         * Creates the destination string that identifies this strategy stream.
         * @param   string  $strategyID Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @param   string  $symbol     Symbol to get the Strategy update data for.
         * @param   string  $stream     Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @return  string
         */
        public static function createDestination(string $strategyID, string $symbol, string $stream) : string
        {            
            return "/strategy/".$strategyID."/".Utilities::streamIdForSymbol($stream, $symbol)."/".$symbol;
        }
    }

?>
    