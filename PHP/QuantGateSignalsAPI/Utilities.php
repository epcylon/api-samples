<?php

    namespace QuantGate\API\Signals;

    use \QuantGate\API\Signals\APIClient;

    /**
     * Utility functions for conversions and cleanup of values.
     */
    class Utilities
    {
        /**
         * Maximum values for each level.
         * @var array
         */
        private const MAX_MANTISSAS = array( 127, 8191, 524287, 33554431, 2147483647, 
                                             274877906943, 35184372088831, 4503599627370495 );
        /**
         * The lengths of each mantissa (in bits).
         * @var array
         */
        private const MANTISSA_LENGTHS = array( 7, 13, 19, 25, 31, 38, 45, 52 );
        /**
         * The maximum allowable scale value for each level.
         * @var array
         */
        private const MAX_SCALES = array( 1, 2, 4, 8, 16, 16, 16, 16 );
        /**
         * The mask for the defining bits of each level.
         * @var array
         */
        private const LEVEL_MASKS = array( 127, 16256, 2080768, 266338304, 34091302912, 
                                           4363686772736, 558551906910208, 71494644084506624 );

        /**
         * Maximum possible value.
         * @var int
         */
        private const MAX_VALUE = 72057594037927935;
        /**
         * NaN is first zero value that has a non-zero scale.
         * @var int
         */
        private const NAN_VALUE = 8192;
        
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
                
        /**
         * Decode a proto price (long) back to a price (double).
         * @param   int     $encoded    The encoded price.
         * @return  float
         */
        public static function decodePrice(int $encoded) : float
        {
            $mantissa = 0;
            $scale = 0;

            // If NaN or too large, return as NaN.
            if ($encoded === self::NAN_VALUE || $encoded > self::MAX_VALUE)
                return float.NaN;

            for ($level = 7; $level > -1; $level--)
            {
                if (($encoded & self::LEVEL_MASKS[$level]) !== 0)
                {
                    // Get the mantissa and scale.
                    $mantissa = $encoded & self::MAX_MANTISSAS[$level];
                    $scale = ($encoded >> self::MANTISSA_LENGTHS[$level]) + 1;

                    // Convert to a float value.
                    return $mantissa * pow(0.1, $scale);
                }
            }

            // If didn't hit any mask, the value is zero.
            return 0.0;
        }
    }

?>