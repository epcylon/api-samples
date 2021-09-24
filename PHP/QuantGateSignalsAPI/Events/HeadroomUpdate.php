<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Headroom update information from a Headroom gauge stream.
     */
    class HeadroomUpdate
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
         * Creates a new HeadroomUpdate instance.
         * @param DateTime  $updateTime The time of this update.
         * @param string    $symbol     The symbol being subscribed to for this gauge.
         * @param string    $stream     The stream from which the update is being received (realtime/delay/demo).
         * @param float     $value      The gauge value at the last update.
         * @param bool      $isDirty    Whether the data used to generate this gauge value is potentially dirty 
         *                              (values are missing) or stale (not the most recent data).
         */
        function __construct(\DateTime $updateTime, string $symbol, string $stream, float $value, bool $isDirty)
        {
            $this->updateTime = $updateTime;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->value = $value;
            $this->isDirty = $isDirty;
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
    }

?>