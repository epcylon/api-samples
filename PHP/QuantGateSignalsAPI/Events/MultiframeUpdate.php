<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Multiframe Equilibrium update information from a Multiframe Equilibrium gauge stream.
     */
    class MultiframeUpdate
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
         * The 5-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min5;
        /**
         * The 10-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min10;
        /**
         * The 15-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min15;
        /**
         * The 30-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min30;
        /**
         * The 45-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min45;
        /**
         * The 60-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min60;
        /**
         * The 120-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min120;
        /**
         * The 180-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min180;
        /**
         * The 240-minute equilibrium deviations value at the last update.
         * @var float
         */
        private float $min240;
        /**
         * The 1-day equilibrium deviations value at the last update.
         * @var float
         */
        private float $day1;
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
         * Creates a new MultiframeUpdate instance.
         * @param  DateTime           $updateTime  The time of this update.
         * @param  string             $symbol      The symbol being subscribed to for this gauge.
         * @param  string             $stream      The stream from which the update is being received (realtime/delay/demo).
         * @param  float              $min5        The 5-minute equilibrium deviations value at the last update.
         * @param  float              $min10       The 10-minute equilibrium deviations value at the last update.
         * @param  float              $min15       The 15-minute equilibrium deviations value at the last update.
         * @param  float              $min30       The 30-minute equilibrium deviations value at the last update.
         * @param  float              $min45       The 45-minute equilibrium deviations value at the last update.
         * @param  float              $min60       The 60-minute equilibrium deviations value at the last update.
         * @param  float              $min120      The 120-minute equilibrium deviations value at the last update.
         * @param  float              $min180      The 180-minute equilibrium deviations value at the last update.
         * @param  float              $min240      The 240-minute equilibrium deviations value at the last update.
         * @param  float              $day1        The 1-day equilibrium deviations value at the last update.
         * @param  bool               $isDirty     Whether the data used to generate this gauge value is potentially dirty 
         *                                         (values are missing) or stale (not the most recent data).
         * @param  SubscriptionError  $error       Holds error information, if a subscription error occured.
         */
        function __construct(\DateTime $updateTime, string $symbol, string $stream, float $min5, float $min10, 
                             float $min15, float $min30, float $min45, float $min60, float $min120, float $min180,
                             float $min240, float $day1, bool $isDirty, ?SubscriptionError $error)
        {
            $this->updateTime = $updateTime;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->min5 = $min5;
            $this->min10 = $min10;
            $this->min15 = $min15;
            $this->min30 = $min30;
            $this->min45 = $min45;
            $this->min60 = $min60;
            $this->min120 = $min120;
            $this->min180 = $min180;
            $this->min240 = $min240;
            $this->day1 = $day1;
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
         * Returns the 5-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get5Min() : float
        {
            return $this->min5;
        }
                
        /**
         * Returns the 10-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get10Min() : float
        {
            return $this->min10;
        }
        
        /**
         * Returns the 15-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get15Min() : float
        {
            return $this->min15;
        }
        
        /**
         * Returns the 30-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get30Min() : float
        {
            return $this->min30;
        }
        
        /**
         * Returns the 45-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get45Min() : float
        {
            return $this->min45;
        }

        /**
         * Returns the 60-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get60Min() : float
        {
            return $this->min60;
        }

        /**
         * Returns the 120-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get120Min() : float
        {
            return $this->min120;
        }

        /**
         * Returns the 180-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get180Min() : float
        {
            return $this->min180;
        }

        /**
         * Returns the 240-minute equilibrium deviations value at the last update.
         * @return float
         */
        public function get240Min() : float
        {
            return $this->min240;
        }

        /**
         * Returns the 1-day equilibrium deviations value at the last update.
         * @return float
         */
        public function get1Day() : float
        {
            return $this->day1;
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