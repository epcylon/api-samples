<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds Strategy update information from a Strategy stream.
     */
    class StrategyUpdate
    {
        /**
         * The time of this update.
         * @var DateTime
         */
        private \DateTime $updateTime;
        /**
         * The strategy subscribed to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @var string
         */
        private string $strategyId;
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
         * Entry progress value.
         * @var float
         */
        private float $entryProgress;
        /**
         * Exit progress value.
         * @var float
         */
        private float $exitProgress;
        /**
         * Signal tied to the Perception level. "Unknown" if unset.
         * @var string
         */
        private string $perceptionSignal;
        /**
         * The Perception level. null if unset.
         * @var null|float
         */
        private ?float $perceptionLevel;
        /**
         * Signal tied to the Commitment level. "Unknown" if unset.
         * @var string
         */
        private string $commitmentSignal;
        /**
         * The Commitment level. null if unset.
         * @var null|float
         */
        private ?float $commitmentLevel;
        /**
         * Signal tied to the Equilibrium level. "Unknown" if unset.
         * @var string
         */
        private string $equilibriumSignal;
        /**
         * The Equilibrium level. null if unset.
         * @var null|float
         */
        private ?float $equilibriumLevel;
        /**
         * Signal tied to the Sentiment level. "Unknown" if unset.
         * @var string
         */
        private string $sentimentSignal;
        /**
         * The Sentiment level. null if unset.
         * @var null|float
         */
        private ?float $sentimentLevel;
        /**
         * Entry signal for the strategy.
         * @var string
         */
        private string $signal;

        /**
         * Creates a new StrategyUpdate instance.
         * @param   DateTime    $updateTime         The time of this update.
         * @param   string      $strategyId         The strategy subscribed to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @param   string      $symbol             The symbol being subscribed to for this Strategy.
         * @param   string      $stream             The stream from which the update is being received (realtime/delay/demo).
         * @param   float       $entryProgress      Entry progress value.
         * @param   float       $exitProgress       Exit progress value.
         * @param   string      $perceptionSignal   Signal tied to the Perception level. "Unknown" if unset.
         * @param   null|float  $perceptionLevel    The Perception level. null if unset.
         * @param   string      $commitmentSignal   Signal tied to the Commitment level. "Unknown" if unset.
         * @param   null|float  $commitmentLevel    The Commitment level. null if unset.
         * @param   string      $equilibriumSignal  Signal tied to the Equilibrium level. "Unknown" if unset.
         * @param   null|float  $equilibriumLevel   The Equilibrium level. null if unset.
         * @param   string      $sentimentSignal    Signal tied to the Sentiment level. "Unknown" if unset.
         * @param   null|float  $sentimentLevel     The Sentiment level. null if unset.
         * @param   string      $signal             Entry signal for the strategy.
         */
        function __construct(\DateTime $updateTime, string $strategyId, string $symbol, string $stream, 
                             float $entryProgress, float $exitProgress, string $perceptionSignal, ?float $perceptionLevel, 
                             string $commitmentSignal, ?float $commitmentLevel, string $equilibriumSignal,
                             ?float $equilibriumLevel, string $sentimentSignal, ?float $sentimentLevel, string $signal)
        {
            $this->updateTime = $updateTime;
            $this->strategyId = $strategyId;
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->entryProgress = $entryProgress;
            $this->exitProgress = $exitProgress;
            $this->perceptionSignal = $perceptionSignal;
            $this->perceptionLevel = $perceptionLevel;
            $this->commitmentSignal = $commitmentSignal;
            $this->commitmentLevel = $commitmentLevel;
            $this->equilibriumSignal = $equilibriumSignal;
            $this->equilibriumLevel = $equilibriumLevel;
            $this->sentimentSignal = $sentimentSignal;
            $this->sentimentLevel = $sentimentLevel;
            $this->signal = $signal;
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
         * Returns the strategy subscribed to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
         * @return string
         */
        public function getStrategyId() : string
        {
            return $this->strategyId;
        }

        /**
         * Returns the symbol being subscribed to for this Strategy.
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
         * Returns the entry progress value.
         * @return float
         */
        public function getEntryProgress() : float
        {
            return $this->entryProgress;
        }

        /**
         * Returns the exit progress value.
         * @return float
         */
        public function getExitProgress() : float
        {
            return $this->exitProgress;
        }

        /**
         * Returns the signal tied to the Perception level. "Unknown" if unset.
         * @var string
         */
        public function getPerceptionSignal() : string
        {
            return $this->perceptionSignal;
        }

        /**
         * Returns the Perception level. null if unset.
         * @var null|float
         */
        public function getPerceptionLevel() : ?float
        {
            return $this->perceptionLevel;
        }

        /**
         * Returns the signal tied to the Commitment level. "Unknown" if unset.
         * @var string
         */
        public function getCommitmentSignal() : string
        {
            return $this->commitmentSignal;
        }

        /**
         * Returns the Commitment level. null if unset.
         * @var null|float
         */
        public function getCommitmentLevel() : ?float
        {
            return $this->commitmentLevel;
        }
        
        /**
         * Returns the signal tied to the Equilibrium level. "Unknown" if unset.
         * @var string
         */
        public function getEquilibriumSignal() : string
        {
            return $this->equilibriumSignal;
        }

        /**
         * Returns the Equilibrium level. null if unset.
         * @var null|float
         */
        public function getEquilibriumLevel() : ?float
        {
            return $this->equilibriumLevel;
        }

        /**
         * Returns the signal tied to the Sentiment level. "Unknown" if unset.
         * @var string
         */
        public function getSentimentSignal() : string
        {
            return $this->sentimentSignal;
        }

        /**
         * Returns the Sentiment level. null if unset.
         * @var null|float
         */
        public function getSentimentLevel() : ?float
        {
            return $this->sentimentLevel;
        }
        
        /**
         * Returns the entry signal for the strategy.
         * @var string
         */
        public function getSignal() : string
        {
            return $this->signal;
        }
    }

?>