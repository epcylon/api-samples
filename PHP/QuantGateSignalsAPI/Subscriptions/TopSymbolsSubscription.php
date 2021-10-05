<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\TopSymbolsUpdate;
    use \QuantGate\API\Signals\Events\TopSymbolItem;    
    use \QuantGate\API\Signals\Events\InstrumentType;
    use \QuantGate\API\Signals\Events\SignalType;
    use \QuantGate\API\Signals\Events\SubscriptionError;
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a Top Symbols stream subscription and convert the received messages.
     */
    class TopSymbolsSubscription extends SubscriptionBase
    {
        /**
         * The broker to get the Top Symbols for. Must match a valid broker type string.
         * @var string
         */
        private string $broker;
        /**
         * The type of instrument to include in the results.
         * @var string
         */
        private string $instrumentType;

        /** 
         * Creates a new TopSymbolsSubscription instance.          
         * @param  int      $id              The (integer) identifier to associate with this subscription on the server's end.
         * @param  string   $broker          The broker to get the Top Symbols for. Must match a valid broker type string.
         * @param  string   $instrumentType  The type of instrument to include in the results.
         * @param  int      $throttleRate    Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param           $client          Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $broker, string $instrumentType, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->broker = $broker;
            $this->instrumentType = $instrumentType;

            // Create the target destination.
            $destination = $this->createDestination($broker, $instrumentType);

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
            $update = new \Stealth\TopSymbolsUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $symbols = [];

            foreach ($update->getSymbols() as $rawSymbol)
            {
                // Convert the values from the raw symbol.
                $updateTime = Utilities::timestampSecondsToDate($rawSymbol->getTimestamp());
                $symbol = $rawSymbol->getSymbol();
                $underlying = $rawSymbol->getUnderlying();
                $currency = $rawSymbol->getCurrency();
                $instrumentType = InstrumentType::getTypeFromInt($rawSymbol->getInstrumentType());
                $exchange = $rawSymbol->getExchange();
                $displayName = $rawSymbol->getDisplayName();
                $entryProgress = $rawSymbol->getEntryProgress() / 1000.0;
                $signal = SignalType::getFromStrategySignal($rawSymbol->getSignal());
                $perceptionSignal = SignalType::getFromGaugeSignal($rawSymbol->getPerceptionSignal());
                $commitmentSignal = SignalType::getFromGaugeSignal($rawSymbol->getCommitmentSignal());
                $equilibriumSignal = SignalType::getFromGaugeSignal($rawSymbol->getEquilibriumSignal());
                $sentimentSignal = SignalType::getFromGaugeSignal($rawSymbol->getSentimentSignal());

                // Create and add the next item.
                $symbols[] = new TopSymbolItem($updateTime, $symbol, $underlying, $currency, $instrumentType,
                                               $exchange, $displayName, $entryProgress, $signal, $perceptionSignal,
                                               $commitmentSignal, $equilibriumSignal, $sentimentSignal);
            }            

            // Create the update object.
            $result = new TopSymbolsUpdate($this->broker, $this->instrumentType, $symbols, null);

            // Send the results back to the APIClient class.
            $this->client->emit('topSymbolsUpdated', [$result]);
        }

        /**
         * Called to send a subscription error to the subscribers.
         * @param   $error  The error information to send.
         * @return  void
         */
        public function sendError(SubscriptionError $error)
        {
            // Create the update object.
            $result = new TopSymbolsUpdate($this->broker, $this->instrumentType, array(), $error);

            // Send the results back to the APIClient class.
            $this->client->emit('topSymbolsUpdated', [$result]);
        }

        /**
         * Creates the destination string that identifies this top symbols stream.
         * @param   string  $broker          
         * @param   string  $instrumentType
         * @return  string
         */
        public static function createDestination(string $broker, string $instrumentType = "") : string
        {
            $baseString = "/defn/top/".$broker;
            
            if (empty($instrumentType) || isnull($instrumentType))
                return $baseString;

            switch ($instrumentType)
            {
                case InstrumentType::COMMON_STOCK: return $baseString."/CS";
                case InstrumentType::FUTURE: return $baseString."/FUT";
                case InstrumentType::FOREX: return $baseString."/FX";
                case InstrumentType::CRYPTO_CURRENCY: return $baseString."/CRY";
                case InstrumentType::PERPETUAL_CRYPTO: return $baseString."/CYP";
                case InstrumentType::INDEX: return $baseString."/IDX";
                default: return $baseString;
            }
        }
    }

?>
    