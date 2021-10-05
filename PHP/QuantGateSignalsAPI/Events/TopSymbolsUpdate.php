<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds top symbols update information from a top symbols stream.
     */
    class TopSymbolsUpdate
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
         * Holds the top symbols results as an array.
         * @var array
         */
        private array $symbols;
        /**
         * Holds error information, if a subscription error occured.
         */
        private ?SubscriptionError $error;

        /**
         * Creates a new TopSymbolsUpdate instance.
         * @param  string             $broker          The broker to get the Top Symbols for. Must match a valid broker type string.
         * @param  string             $instrumentType  The type of instrument to include in the results.
         * @param  array              $symbols         The top symbols results as an array.
         * @param  SubscriptionError  $error           Holds error information, if a subscription error occured.
         */
        function __construct(string $broker, string $instrumentType, array $symbols, ?SubscriptionError $error)
        {
            $this->broker = $broker;
            $this->instrumentType = $instrumentType;
            $this->symbols = $symbols;
            $this->error = $error;
        }

        /**
         * Returns the identifier of the broker that the Top Symbols are for.
         * @return string
         */
        public function getBroker() : string
        {
            return $this->broker;
        }

        /**
         * Returns type of instrument included in the results.
         * @return string
         */
        public function getInstrumentType() : string
        {
            return $this->instrumentType;
        }

        /**
         * Returns the top symbols results as an array.
         * @return array
         */
        public function getSymbols() : array
        {
            return $this->symbols;
        }

        /**
         * Returns error information, if a subscription error occured.
         * return null|SubscriptionError
         */
        public function getError() : ?SubscriptionError
        {
            return $this->error;
        }
    }

?>