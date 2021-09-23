<?php

    namespace QuantGate\API\Signals;

    use \QuantGate\API\Signals\APIClient;

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

        /**
         * Returns the proper stream ID for the given security type.
         * @param   string  $streamId   The base stream ID in use by the broker.
         * @param   string  $symbol     The symbol to return the stream ID for.
         * @return  string
         */
        public static function streamIdForSymbol(string $streamId, string $symbol) : string
        {
            // If delayed for currency pair, just use realtime. Otherwise, Set the stream ID.
            if (($streamId == APIClient::DELAY_STREAM) && 
                ((strpos($symbol, '.') !== false) || (strpos($symbol, ':') !== false)))
                return APIClient::REALTIME_STREAM;
            else
                return $streamId;
        }

        /**
         * Cleans the compression string so that it is normalized.
         * @param   string  $compression    The compression string to clean.
         * @return  string
         */
        public static function cleanCompression(string $compression) : string
        {
            // Put in lower-case and get rid of any whitespace.
            return str_replace(' ', '', strtolower($compression));
        }
    }

?>