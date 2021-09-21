<?php

    namespace QuantGate\API\Signals;

    class Utilities
    {     
        /*
          Converts a timestamp from seconds from 1800-01-01 to a DateTime value.
            $timestamp - The timestamp to convert.
        */
        public static function timestampSecondsToDate(int $timestamp) : \DateTime
        {
            return  (new \DateTime('1800-01-01Z'))->add(new \DateInterval('PT'.$timestamp.'S'));
        }  
    }

?>