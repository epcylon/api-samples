<?php

    namespace QuantGate\API\Signals;

    class Utilities
    {     
        /**
         * Converts a timestamp from seconds from 1800-01-01 to a DateTime value.
         * @param   int $timestamp  The timestamp to convert.
         * @return \DateTime
        */
        public static function timestampSecondsToDate(int $timestamp) : \DateTime
        {
            return  (new \DateTime('1800-01-01Z'))->add(new \DateInterval('PT'.$timestamp.'S'));
        }  

        /**
         * Retrieves the user ('sub') from the given JWT token.
         * @param   string  $jwtToken   The token to retrieve the user from.
         * @return  string
         */
        public static function getUserFromJWT(string $jwtToken) : string
        {
            $payload = base64_decode(explode(".", $jwtToken, 3)[1]);
            $user = explode("\"", explode("\"sub\":\"", $payload, 2)[1], 2)[0];
            return $user;
        }
    }

?>