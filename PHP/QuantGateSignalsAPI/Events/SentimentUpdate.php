<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Sentiment update information from a Sentiment gauge stream.
     */
    class SentimentUpdate
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
         * Holds the lengths of each bar.
         * @var array
         */
        private array $lengths;
        /**
         * Holds the colors of each bar.
         * @var array
         */
        private array $colors;
        /**
         * Average bar length.
         * @var float
         */
        private float $avgLength;
        /**
         * Average bar color.
         * @var float
         */
        private float $avgColor;
        /**
         * Whether the data used to generate this gauge value is potentially dirty (values are missing) 
         * or stale (not the most recent data).
         * @var bool
         */
        private bool $isDirty;

        /**
         * Creates a new SentimentUpdate instance.
         * @param DateTime  $updateTime   The time of this update.
         * @param string    $symbol       The symbol being subscribed to for this gauge.
         * @param string    $stream       The stream from which the update is being received (realtime/delay/demo).
         * @param string    $compression  Compression timeframe applied to the gauge.
         * @param array     $lengths      Holds the lengths of each bar.
         * @param array     $colors       Holds the colors of each bar.
         * @param float     $avgLength    Average bar length.
         * @param float     $avgColor     Average bar color.
         * @param bool      $isDirty      Whether the data used to generate this gauge value is potentially dirty 
         *                                (values are missing) or stale (not the most recent data).
         */
        function __construct(\DateTime $updateTime, string $symbol, string $stream, string $compression, 
                             array $lengths, array $colors, float $avgLength, float $avgColor, bool $isDirty)
        {
            $this->updateTime = $updateTime;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->compression = $compression;
            $this->lengths = $lengths;
            $this->colors = $colors;
            $this->avgLength = $avgLength;
            $this->avgColor = $avgColor;
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
         * Returns the compression timeframe applied to the gauge.
         * @return string
         */
        public function getCompression() : string
        {
            return $this->compression;
        }

        /**
         * Returns an array that holds the lengths of each bar.
         * @return array
         */
        public function getLengths() : array
        {
            return $this->lengths;
        }

        /**
         * Returns an array that holds the colors of each bar.
         * @return array
         */
        public function getColors() : array
        {
            return $this->colors;
        }
        
        /**
         * Returns the average bar length.
         * @return float
         */
        public function getAvgLength() : float
        {
            return $this->avgLength;
        }

        /**
         * Returns the average bar color.
         * @return float
         */
        public function getAvgColor() : float
        {
            return $this->avgColor;
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