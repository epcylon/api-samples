<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Equilibrium update information from a Equilibrium gauge stream.
     */
    class EquilibriumUpdate
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
         * The Equilibrium Price (fair market value at time of calculation).
         * @var float
         */
        private float $equilibriumPrice;
        /**
         * Gap size of each equilibrium deviation.
         * @var float
         */
        private float $gapSize;
        /**
         * Last traded price at the time of calculation.
         * @var float
         */
        private float $lastPrice;
        /**
         * Position of the high value.
         * @var float
         */
        private float $high;
        /**
         * Position of the low value.
         * @var float
         */
        private float $low;
        /**
         * Position of the projected value.
         * @var float
         */
        private float $projected;
        /**
         * Equilibrium bias (as determined by the slope).
         * @var float
         */
        private float $bias;
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
         * Creates a new EquilibriumUpdate instance.
         * @param  DateTime           $updateTime        The time of this update.
         * @param  string             $symbol            The symbol being subscribed to for this gauge.
         * @param  string             $stream            The stream from which the update is being received (realtime/delay/demo).
         * @param  string             $compression       Compression timeframe applied to the gauge.
         * @param  float              $equilibriumPrice  The Equilibrium Price (fair market value at time of calculation).
         * @param  float              $gapSize           Gap size of each equilibrium deviation.
         * @param  float              $lastPrice         Last traded price at the time of calculation.
         * @param  float              $high              Position of the high value.
         * @param  float              $low               Position of the low value.
         * @param  float              $projected         Position of the projected value.
         * @param  float              $bias              Equilibrium bias (as determined by the slope).
         * @param  bool               $isDirty           Whether the data used to generate this gauge value is potentially dirty 
         *                                               (values are missing) or stale (not the most recent data).
         * @param  SubscriptionError  $error             Holds error information, if a subscription error occured.
         */
        function __construct(\DateTime $updateTime, string $symbol, string $stream, string $compression, 
                             float $equilibriumPrice, float $gapSize, float $lastPrice, float $high,
                             float $low, float $projected, float $bias, bool $isDirty, ?SubscriptionError $error)
        {
            $this->updateTime = $updateTime;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->compression = $compression;
            $this->equilibriumPrice = $equilibriumPrice;
            $this->gapSize = $gapSize;
            $this->lastPrice = $lastPrice;
            $this->high = $high;
            $this->low = $low;
            $this->projected = $projected;
            $this->bias = $bias;
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
         * Returns the compression timeframe applied to the gauge.
         * @return string
         */
        public function getCompression() : string
        {
            return $this->compression;
        }

        /**
         * Returns the Equilibrium Price (fair market value at time of calculation).
         * @return float
         */
        public function getEquilibriumPrice() : float
        {
            return $this->equilibriumPrice;
        }

        /**
         * Returns the gap size of each equilibrium deviation.
         * @return float
         */
        public function getGapSize() : float
        {
            return $this->gapSize;
        }
        
        /**
         * Returns the last traded price at the time of calculation.
         * @return float
         */
        public function getLastPrice() : float
        {
            return $this->lastPrice;
        }

        /**
         * Returns the position of the high value.
         * @return float
         */
        public function getHigh() : float
        {
            return $this->high;
        }

        /**
         * Returns the position of the low value.
         * @return float
         */
        public function getLow() : float
        {
            return $this->low;
        }

        /**
         * Returns the position of the projected value.
         * @return float
         */
        public function getProjected() : float
        {
            return $this->projected;
        }

        /**
         * Returns the equilibrium bias (as determined by the slope).
         * @return float
         */
        public function getBias() : float
        {
            return $this->bias;
        }

        /**
         * Returns the current equilibrium gauge level in standard deviations from the equilibrium price.
         * @return float
         */
        public function getEquilibriumStd() : float
        {
            if ($this->lastPrice === 0.0 || $this->equilibriumPrice === 0.0 || $this->gapSize === 0.0)
                return 0;

            return ($this->lastPrice - $this->equilibriumPrice) / $this->gapSize;
        }

        /**
         * Returns the equilibrium band price at the given level of standard deviations.
         * @param   float $level  The level of standard deviations above or below the equilibrium 
         *                        price to calculate.
         * @return  float
         */
        public function EquilibriumBand(float $level) : float
        {
            return $this->equilibriumPrice + $this->gapSize * $level;
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