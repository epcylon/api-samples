<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Commitment update information from a Commitment gauge stream.
     */
    class CommitmentUpdate
    {
        /**
         * The time of this update.
         * @var DateTime
         */
        private \DateTime $updateTime;
        /**
         * The symbol being subscribed to for this gauge.
         * @var string
         */
        private string $symbol;
        /**
         * The stream from which the update is being received (realtime/delay/demo).
         * @var string
         */
        private string $stream;
        /**
         * The gauge value at the last update.
         * @var float
         */
        private float $value;
        /**
         * Whether the data used to generate this gauge value is potentially dirty (values are missing) 
         * or stale (not the most recent data).
         * @var bool
         */
        private bool $isDirty;
        /**
         * Holds error information, if a subscription error occured.
         */
        private ?SubscriptionError $error;

        /**
         * Creates a new CommitmentUpdate instance.
         * @param  DateTime           $updateTime  The time of this update.
         * @param  string             $symbol      The symbol being subscribed to for this gauge.
         * @param  string             $stream      The stream from which the update is being received (realtime/delay/demo).
         * @param  float              $value       The gauge value at the last update.
         * @param  bool               $isDirty     Whether the data used to generate this gauge value is potentially dirty 
         *                                         (values are missing) or stale (not the most recent data).
         * @param  SubscriptionError  $error       Holds error information, if a subscription error occured.
         */
        function __construct(\DateTime $updateTime, string $symbol, string $stream, 
                             float $value, bool $isDirty, ?SubscriptionError $error)
        {
            $this->updateTime = $updateTime;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->value = $value;
            $this->isDirty = $isDirty;
            $this->error = $error;
        }

        /**
         * Returns the time of this update.
         * @return DateTime
         */
        public function getUpdateTime() : \DateTime
        {
            return $this->updateTime;
        } 

        /**
         * Returns the symbol being subscribed to for this gauge.
         * @return string
         */
        public function getSymbol() : string
        {
            return $this->symbol;            
        }

        /**
         * Returns the stream from which the update is being received (realtime/delay/demo).
         * @return string
         */
        public function getStream() : string
        {
            return $this->stream;
        }

        /**
         * Returns the gauge value at the last update.
         * @return float
         */
        public function getValue() : float
        {
            return $this->value;
        }
        
        /**
         * Returns true if the data used to generate this gauge value is potentially dirty
         * (values are missing) or stale (not the most recent data), false if clean.
         * @return bool
         */
        public function getIsDirty() : bool
        {
            return $this->isDirty;
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