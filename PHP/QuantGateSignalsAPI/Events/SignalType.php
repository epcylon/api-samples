<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Signal type enumeration values.
     */
    class SignalType
    {
        /**
         * Unknown or unset signal type.
         * @var string
         */
        public const SIGNAL_UNKNOWN = "Unknown";
        /**
         * Flat signal type (no direction).
         * @var string
         */
        public const SIGNAL_FLAT = "Flat";
        /**
         * Long signal type.
         * @var string
         */
        public const SIGNAL_LONG = "Long";
        /**
         * Short signal type.
         * @var string
         */
        public const SIGNAL_SHORT = "Short";
        /**
         * Dual signal type (either Long or Short).
         * @var string
         */
        public const SIGNAL_DUAL = "Dual";

        /** 
         * Converts a strategy signal value from an integer to a (readable) constant string value.
         * @param   int $value  The strategy signal value to convert.
         * @return  string
         */
        public static function getFromStrategySignal(int $value) : string
        {
            switch ($value)
            {
                case 0: return SignalType::SIGNAL_FLAT;
                case 1: return SignalType::SIGNAL_LONG;
                case 2: return SignalType::SIGNAL_SHORT;
                default: return SignalType::SIGNAL_UNKNOWN;                    
            }
        }

        /** 
         * Converts a strategy gauge signal value from an integer to a (readable) constant string value.
         * @param   string  $value  The strategy gauge signal value to convert.
         * @return  string
         */
        public static function getFromGaugeSignal(int $value) : string
        {
            switch ($value)
            {
                case 0: return SignalType::SIGNAL_UNKNOWN;
                case 1: return SignalType::SIGNAL_SHORT;
                case 2: return SignalType::SIGNAL_FLAT;
                case 3: return SignalType::SIGNAL_LONG;
                case 4: return SignalType::SIGNAL_DUAL;
                default: return SignalType::SIGNAL_UNKNOWN;                    
            }
        }
    }
    
?>