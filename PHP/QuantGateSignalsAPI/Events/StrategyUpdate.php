<?php

    namespace QuantGate\API\Signals\Events;      

    class StrategyUpdate
    {
        public const SIGNAL_UNKNOWN = "Unknown";
        public const SIGNAL_FLAT = "Flat";
        public const SIGNAL_LONG = "Long";
        public const SIGNAL_SHORT = "Short";
        public const SIGNAL_DUAL = "Dual";

        private \DateTime $updateTime;
        private string $strategyId;
        private string $symbol;
        private string $stream;
        private float $entryProgress;
        private float $exitProgress;
        private string $perceptionSignal;
        private ?float $perceptionLevel;
        private string $commitmentSignal;
        private ?float $commitmentLevel;
        private string $equilibriumSignal;
        private ?float $equilibriumLevel;
        private string $sentimentSignal;
        private ?float $sentimentLevel;
        private string $signal;

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

        public function getUpdateTime() : \DateTime
        {
            return $updateTime;
        }

        public function getStrategyId() : string
        {
            return $strategyId;
        }

        public function getSymbol() : string
        {
            return $symbol;            
        }

        public function getStream() : string
        {
            return $stream;
        }

        public function getEntryProgress() : float
        {
            return $entryProgress;
        }

        public function getExitProgress() : float
        {
            return $exitProgress;
        }

        public function getPerceptionSignal() : string
        {
            return $perceptionSignal;
        }

        public function getPerceptionLevel() : ?float
        {
            return $perceptionLevel;
        }

        public function getCommitmentSignal() : string
        {
            return $commitmentSignal;
        }

        public function getCommitmentLevel() : ?float
        {
            return $commitmentLevel;
        }
        
        public function getEquilibriumSignal() : string
        {
            return $equilibriumSignal;
        }

        public function getEquilibriumLevel() : ?float
        {
            return $equilibriumLevel;
        }

        public function getSentimentSignal() : string
        {
            return $sentimentSignal;
        }

        public function getSentimentLevel() : ?float
        {
            return $sentimentLevel;
        }
        
        public function getSignal() : string
        {
            return $signal;
        }
    }

?>