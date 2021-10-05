<?php    

    namespace QuantGate\API\Signals\Events;

    /**
     * Holds basic information for an individual symbol.
     */
    class SymbolItem
    {
        /**
         * The symbol as listed by our servers.
         * @var string
         */
        private string $symbol;
        /**
         * The underlying symbol.
         * @var string
         */
        private string $underlying;
        /**
         * The currency the instrument is traded in.
         * @var string
         */
        private string $currency;
        /**
         * The type of instrument (Stock, Future, Forex, etc.).
         * @var string
         */
        private string $instrumentType;
        /**
         * The primary exchange (MIC) the instrument is traded on.
         * @var string
         */
        private string $exchange;
        /**
         * The display name of the instrument.
         * @var string
         */
        private string $displayName;

        /**
         * Creates a new SymbolItem instance.
         * @param string    $symbol         The symbol being subscribed to for this gauge.
         * @param string    $underlying     The underlying symbol.
         * @param string    $currency       The currency the instrument is traded in.
         * @param string    $instrumentType The type of instrument (Stock, Future, Forex, etc.).
         * @param string    $exchange       The primary exchange (MIC) the instrument is traded on.
         * @param string    $displayName    The display name of the instrument.
         */
        public function __construct(string $symbol, string $underlying, string $currency,
                                    string $instrumentType, string $exchange, string $displayName) 
        {
          $this->symbol = $symbol;
          $this->underlying = $underlying;
          $this->currency = $currency;
          $this->instrumentType = $instrumentType;
          $this->exchange = $exchange;
          $this->displayName = $displayName;          
        }

        /**
         * The symbol as listed by our servers.
         * @return string
         */
        public function getSymbol()
        {
            return $this->symbol;
        }      

        /**
         * The underlying symbol.
         * @return string
         */
        public function getUnderlying()
        {
            return $this->underlying;
        }
        
        /**
         * The currency the instrument is traded in.
         * @return string
         */
        public function getCurrency()
        {
            return $this->currency;
        }
        
        /**
         * The type of instrument (Stock, Future, Forex, etc.).
         * @return int
         */
        public function getInstrumentType()
        {
            return $this->instrumentType;
        }

        /**
         * The primary exchange (MIC) the instrument is traded on.
         * @return string
         */
        public function getExchange()
        {
            return $this->exchange;
        }       

        /**
         * The display name of the instrument.
         * @return string
         */
        public function getDisplayName()
        {
            return $this->displayName;
        }               
    }

?>