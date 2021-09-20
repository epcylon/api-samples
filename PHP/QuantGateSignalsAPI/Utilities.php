<?php

    namespace QuantGate\API\Signals;

    class Utilities
    {     
        public static function timestampSecondsToDate(int $timestamp) : \DateTime
        {
            return  (new \DateTime('1800-01-01Z'))->add(new \DateInterval('PT'.$timestamp.'S'));
        }  
    }

?>