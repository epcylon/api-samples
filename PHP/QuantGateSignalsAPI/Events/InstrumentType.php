<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Instrument type enumeration values.
     */
    class InstrumentType
    {
        /**
         * Not a valid instrument.
         * @var string
         */
        public const NO_INSTRUMENT = "None";
        /**
         * Common stock.
         * @var string
         */
        public const COMMON_STOCK = "Stock";
        /**
         * Stock option.
         * @var string
         */
        public const STOCK_OPTION = "Option";
        /**
         * Future.
         * @var string
         */
        public const FUTURE = "Future";
        /**
         * Index.
         * @var string
         */
        public const INDEX = "Index";
        /**
         * Foreign Exchange (Currency).
         * @var string
         */
        public const FOREX_CONTRACT = "Forex";
        /**
         * Future option.
         * @var string
         */
        public const FUTURE_OPTION = "FutOpt";
        /**
         * Future instrument (underlying).
         * @var string
         */
        public const FUTURE_INSTRUMENT = "FutInst";
        /**
         * Combo pairing.
         * @var string
         */
        public const COMBO = "Combo";
        /**
         * Combo underlying.
         * @var string
         */
        public const COMBO_INSTRUMENT = "ComboInst";
        /**
         * Crypto Currency pair (Bitcoin, etc.)
         * @var string
         */
        public const CRYPTO_CURRENCY = "Crypto";
        /**
         * Perpetual Crypto Currency.
         * @var string
         */
        public const PERPETUAL_CRYPTO = "PerpCrypto";

        /**
         * Returns the InstrumentType tied to the given enumeration ID.
         * @param   int   $enumID   The instrument enumeration ID.
         * @return  string
         */
        public static function getTypeFromInt(int $enumID) : string
        {
            switch ($enumID)
            {
                case 0: return self::NO_INSTRUMENT;
                case 1: return self::COMMON_STOCK;
                case 2: return self::STOCK_OPTION;
                case 3: return self::FUTURE;
                case 4: return self::INDEX;
                case 5: return self::FOREX_CONTRACT;
                case 6: return self::FUTURE_OPTION;
                case 7: return self::FUTURE_INSTRUMENT;
                case 8: return self::COMBO;
                case 9: return self::COMBO_INSTRUMENT;
                case 10: return self::CRYPTO_CURRENCY;
                case 11: return self::PERPETUAL_CRYPTO;
                default: return self::NO_INSTRUMENT;              
            };
        }
    }
    
?>