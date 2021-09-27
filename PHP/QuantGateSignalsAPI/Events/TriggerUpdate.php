<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Trigger update information from a Trigger stream.
     */
    class TriggerUpdate
    {
        /**
         * The time of this update.
         * @var DateTime
         */
        private \DateTime $updateTime;        
        /**
         * The symbol being subscribed to for this Strategy.
         * @var string
         */
        private string $symbol;
        /**
         * The stream from which the update is being received (realtime/delay/demo).
         * @var string
         */
        private string $stream;
        /**
         * Bias value (as determined by mid and long-term sentiment).
         * @var float
         */
        private float $bias;
        /**
         * The Perception level.
         * @var float
         */
        private float $perception;
        /**
         * The Commitment level.
         * @var float
         */
        private float $commitment;
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
         * Sentiment length value at point 0 (center).
         * @var float
         */
        private float $sentiment;
        /**
         * Whether the data used to generate this gauge value is potentially dirty (values are missing) 
         * or stale (not the most recent data).
         * @var bool
         */
        private bool $isDirty;

        /**
         * Creates a new TriggerUpdate instance.
         * @param   DateTime    $updateTime         The time of this update.
         * @param   string      $symbol             The symbol being subscribed to for this Trigger.
         * @param   string      $stream             The stream from which the update is being received (realtime/delay/demo).
         * @param   float       $bias               Bias value (as determined by mid and long-term sentiment).
         * @param   float       $perception         The Perception level.
         * @param   float       $commitment         The Commitment level.
         * @param   float       $equilibriumPrice   The Equilibrium Price (fair market value at time of calculation).
         * @param   float       $gapSize            Gap size of each equilibrium deviation.
         * @param   float       $lastPrice          Last traded price at the time of calculation.
         * @param   float       $sentiment          Sentiment length value at point 0 (center).
         * @param   bool        $isDirty            Whether the data used to generate this gauge value is potentially dirty 
         *                                          (values are missing) or stale (not the most recent data).
         */
        function __construct(\DateTime $updateTime, string $symbol, string $stream, float $bias, 
                             float $perception, float $commitment, float $equilibriumPrice,
                             float $gapSize, float $lastPrice, float $sentiment, bool $isDirty)
        {
            $this->updateTime = $updateTime;
            $this->symbol = $symbol;
            $this->stream = $stream;            
            $this->bias = $bias;
            $this->perception = $perception;
            $this->commitment = $commitment;
            $this->equilibriumPrice = $equilibriumPrice;
            $this->gapSize = $gapSize;
            $this->lastPrice = $lastPrice;
            $this->sentiment = $sentiment;
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
         * Returns the symbol being subscribed to for this Trigger stream.
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
         * Returns the equilibrium bias (as determined by the slope).
         * @return float
         */
        public function getBias() : float
        {
            return $this->bias;
        }

        /**
         * Returns the Perception level.
         * @var float
         */
        public function getPerception() : float
        {
            return $this->perceptionLevel;
        }

        /**
         * Returns the Commitment level.
         * @var float
         */
        public function getCommitment() : float
        {
            return $this->commitment;
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
         * Returns the Sentiment length value at point 0 (center).
         * @var float
         */
        public function getSentiment() : float
        {
            return $this->sentiment;
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
    }

?>