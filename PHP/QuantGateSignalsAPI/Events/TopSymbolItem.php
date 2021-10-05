<?php    

    namespace QuantGate\API\Signals\Events;

    /**
     * Holds basic information for an individual symbol within a top symbols update.      
     */
    class TopSymbolItem extends SymbolItem
    {
        /**
         * The timestamp of the latest update.
         * @var \DateTime
         */
        private \DateTime $updateTime;
        /**
         * The entry progress value.
         * @var float
         */
        private float $entryProgress;
        /**
         * The entry signal for the strategy.
         * @var string
         */
        private string $signal;
        /**
         * The signal tied to the perception level.
         * @var string
         */
        private string $perceptionSignal;
        /**
         * The signal tied to the commitment level.
         * @var string
         */
        private string $commitmentSignal;
        /**
         * The signal tied to the equilibrium level.    
         * @var string
         */
        private string $equilibriumSignal;
        /**
         * The signal tied to the 50t sentiment indication.
         * @var string
         */
        private string $sentimentSignal;

        /**
         * Creates a new TopSymbolItem instance.
         * @param   \DateTime   $updateTime         The timestamp of the latest update.
         * @param   string      $symbol             The symbol being subscribed to for this gauge.
         * @param   string      $underlying         The underlying symbol.
         * @param   string      $currency           The currency the instrument is traded in.
         * @param   string      $instrumentType     The type of instrument (Stock, Future, Forex, etc.).
         * @param   string      $exchange           The primary exchange (MIC) the instrument is traded on.
         * @param   string      $displayName        The display name of the instrument.
         * @param   float       $entryProgress      The entry progress value.
         * @param   string      $signal             The entry signal for the strategy.
         * @param   string      $perceptionSignal   The signal tied to the perception level.
         * @param   string      $commitmentSignal   The signal tied to the commitment level.
         * @param   string      $equilibriumSignal  The signal tied to the equilibrium level. 
         * @param   string      $sentimentSignal    The signal tied to the 50t sentiment indication.
         */
        public function __construct(\DateTime $updateTime, string $symbol, string $underlying, string $currency, 
                                    string $instrumentType, string $exchange, string $displayName, 
                                    float $entryProgress, string $signal, string $perceptionSignal,
                                    string $commitmentSignal, string $equilibriumSignal, string $sentimentSignal) 
        {
            parent::__construct($symbol, $underlying, $currency, $instrumentType, $exchange, $displayName);

            $this->updateTime = $updateTime;
            $this->entryProgress = $entryProgress;
            $this->signal = $signal;
            $this->perceptionSignal = $perceptionSignal;
            $this->commitmentSignal = $commitmentSignal;
            $this->equilibriumSignal = $equilibriumSignal;
            $this->sentimentSignal = $sentimentSignal;
        }

        /**
         * The timestamp of the latest update.
         * @return \DateTime
         */
        public function getUpdateTime() : \DateTime
        {
            return $this->updateTime;
        }
     
        /**
         * The entry progress value.
         * @return float
         */
        public function getEntryProgress() : float
        {
            return $this->entryProgress;
        }
     
        /**
         * The entry signal for the strategy.
         * @return string
         */
        public function getSignal() : string
        {
            return $this->signal;
        }
      
        /**
         * The signal tied to the perception level.
         * @return string
         */
        public function getPerceptionSignal() : string
        {
            return $this->perceptionSignal;
        }      

        /**
         * The signal tied to the commitment level.
         * @return string
         */
        public function getCommitmentSignal() : string
        {
            return $this->commitmentSignal;
        }    

        /**
         * The signal tied to the equilibrium level.
         * @return string
         */
        public function getEquilibriumSignal() : string
        {
            return $this->equilibriumSignal;
        }

        /**
         * The signal tied to the 50t sentiment indication.         
         * @return string
         */
        public function getSentimentSignal() : string
        {
            return $this->sentimentSignal;
        }       
    }
?>