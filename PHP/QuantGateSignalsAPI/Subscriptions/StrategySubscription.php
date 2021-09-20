<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\Events\StrategyUpdate;    
    use \QuantGate\API\Signals\Utilities;

    class StrategySubscription extends SubscriptionBase
    {
        private string $strategyId;
        private string $symbol;
        private string $stream;

        function __construct(int $id, string $strategyId, string $symbol, string $stream, int $throttleRate = 0)
        {
            $this->strategyId = $strategyId;
            $this->symbol = $symbol;
            $this->stream = $stream;

            $destination = $this->createDestination($strategyId, $symbol, $stream);

            parent::__construct($destination, $id, $throttleRate);
        }

        public function handleMessage($body)
        {
            $update = new \Stealth\StrategyUpdate();
            $update->mergeFromString($body);

            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            $entryProgress = $update->getEntryProgress() / 1000.0;
            $exitProgress = $update->getExitProgress() / 1000.0;
            $perceptionLevel = $this->convertGaugeLevel($update->getPerceptionLevel());
            $commitmentLevel = $this->convertGaugeLevel($update->getCommitmentLevel());
            $equilibriumLevel = $this->convertGaugeLevel($update->getEquilibriumLevel());
            $sentimentLevel = $this->convertGaugeLevel($update->getSentimentLevel());
            $perceptionSignal = $this->convertGaugeSignal($update->getPerceptionSignal());
            $commitmentSignal = $this->convertGaugeSignal($update->getCommitmentSignal());
            $equilibriumSignal = $this->convertGaugeSignal($update->getEquilibriumSignal());
            $sentimentSignal = $this->convertGaugeSignal($update->getSentimentSignal());
            $signal = $this->convertStrategySignal($update->getSignal());            

            $result = new StrategyUpdate($updateTime, $this->strategyId, $this->symbol, $this->stream, 
                                         $entryProgress, $exitProgress, $perceptionSignal, $perceptionLevel, 
                                         $commitmentSignal, $commitmentLevel, $equilibriumSignal,
                                         $equilibriumLevel, $sentimentSignal, $sentimentLevel, $signal);

            echo "Entry Progress: ".$entryProgress."\n";
        }

        private function convertGaugeLevel(int $level) : ?float
        {
            if ($level === 0)
                return null;

            return ($level - 1001) / 1000.0;
        }

        private function convertStrategySignal(int $value) : string
        {
            switch ($value)
            {
                case 0: return StrategyUpdate::SIGNAL_FLAT;
                case 1: return StrategyUpdate::SIGNAL_LONG;
                case 2: return StrategyUpdate::SIGNAL_SHORT;
                default: return StrategyUpdate::SIGNAL_UNKNOWN;                    
            }
        }

        private function convertGaugeSignal(int $value) : string
        {
            switch ($value)
            {
                case 0: return StrategyUpdate::SIGNAL_UNKNOWN;
                case 1: return StrategyUpdate::SIGNAL_SHORT;
                case 2: return StrategyUpdate::SIGNAL_FLAT;
                case 3: return StrategyUpdate::SIGNAL_LONG;
                case 4: return StrategyUpdate::SIGNAL_DUAL;
                default: return StrategyUpdate::SIGNAL_UNKNOWN;                    
            }
        }

        public static function createDestination(string $strategyID, string $symbol, string $stream)
        {            
            return "/strategy/".$strategyID."/".$stream."/".$symbol;
        }
    }

?>
    