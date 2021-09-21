<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\StrategyUpdate;    
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
         * Holds a reference to the parent APIClient instance to send updates to.
         * @var callback
         */        
        private APIClient $client;

        /** 
         * Creates a new StrategySubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $strategyId     The strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @param string   $symbol         Symbol to get the Strategy update data for.
         * @param string   $stream         Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param int      $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param          $client         Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $strategyId, string $symbol, string $stream, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->strategyId = $strategyId;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->client = $client;

            // Create the target destination.
            $destination = $this->createDestination($strategyId, $symbol, $stream);

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
            $perceptionSignal = $this->convertGaugeSignal($update->getPerceptionSignal());
            $commitmentSignal = $this->convertGaugeSignal($update->getCommitmentSignal());
            $equilibriumSignal = $this->convertGaugeSignal($update->getEquilibriumSignal());
            $sentimentSignal = $this->convertGaugeSignal($update->getSentimentSignal());
            $signal = $this->convertStrategySignal($update->getSignal());            

            // Create the update object.
            $result = new StrategyUpdate($updateTime, $this->strategyId, $this->symbol, $this->stream, 
                                         $entryProgress, $exitProgress, $perceptionSignal, $perceptionLevel, 
                                         $commitmentSignal, $commitmentLevel, $equilibriumSignal,
                                         $equilibriumLevel, $sentimentSignal, $sentimentLevel, $signal);

            // Send the results back to the APIClient class.
            $this->client->sendStrategyUpdate($result);
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
         * Converts a strategy signal value from an integer to a (readable) constant string value.
         * @param   int $value  The strategy signal value to convert.
         * @return  string
         */
        private function convertStrategySignal(int $value) : string
        {
            switch ($value)
            {
                case 0: return StrategyUpdate::SIGNAL_FLAT;
                case 1: return StrategyUpdate::SIGNAL_LONG;
                case 2: return StrategyUpdate::SIGNAL_SHORT;
                default: return StrategyUpdate::SIGNAL_UNKNOWN;                    
            }
        }

        /** 
         * Converts a strategy gauge signal value from an integer to a (readable) constant string value.
         * @param   string  $value  The strategy gauge signal value to convert.
         * @return  string
         */
        private function convertGaugeSignal(int $value) : string
        {
            switch ($value)
            {
                case 0: return StrategyUpdate::SIGNAL_UNKNOWN;
                case 1: return StrategyUpdate::SIGNAL_SHORT;
                case 2: return StrategyUpdate::SIGNAL_FLAT;
                case 3: return StrategyUpdate::SIGNAL_LONG;
                case 4: return StrategyUpdate::SIGNAL_DUAL;
                default: return StrategyUpdate::SIGNAL_UNKNOWN;                    
            }
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
            return "/strategy/".$strategyID."/".$stream."/".$symbol;
        }
    }

?>
    