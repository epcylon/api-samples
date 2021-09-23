<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;
    use \QuantGate\API\Signals\Events\SentimentUpdate;    
    use \QuantGate\API\Signals\Utilities;

    /**
     * Used to subscribe to a sentiment gauge stream subscription and convert the received messages.
     */
    class SentimentSubscription extends SubscriptionBase
    {
        /**
         * The total number of bars in the Sentiment gauge.
         * @var int
         */
        public const TOTAL_BARS = 55;

        /**
         * Symbol to get the Sentiment gauge updates data for.
         * @var string
         */
        private string $symbol;
        /**
         * Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @var string
         */
        private string $stream;
        /**
         * Compression timeframe to apply to the gauge. Default value is 50t.
         * @var string
         */
        private string $compression;
        /**
         * Holds a reference to the parent APIClient instance to send updates to.
         * @var callback
         */        
        private APIClient $client;

        /** 
         * Creates a new SentimentSubscription instance.          
         * @param int      $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param string   $symbol         Symbol to get the Sentiment gauge updates data for.
         * @param string   $stream         Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param string   $compression    Compression timeframe to apply to the gauge. Default value is 50t.
         * @param int      $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param          $client         Reference to the parent APIClient instance to send updates to.
         */
        function __construct(int $id, string $symbol, string $stream, string $compression, int $throttleRate, APIClient $client)
        {
            // Set the properties.
            $this->symbol = $symbol;
            $this->stream = $stream;
            $this->client = $client;
            $this->compression = Utilities::cleanCompression($compression);

            // Create the target destination.
            $destination = $this->createDestination($symbol, $stream, $compression);

            // Initialize values in the parent class.
            parent::__construct($destination, $id, $throttleRate);
        }

        /**
         * Handles new messages from the stream and converts to subscription updates.
         * @param   $body   The raw message received from the stream.
         * @return void
         */
        public function handleMessage($body)
        {
            // Convert the message to a (Protobuf) strategy update object.
            $update = new \Stealth\SentimentUpdate();
            $update->mergeFromString($body);

            // Convert the values into usable values.
            $updateTime = Utilities::timestampSecondsToDate($update->getTimestamp());
            
            // Interpolate the length and color values.
            $lengths = self::interpolateTo55($update->getLengths()->getI(),
                                             $update->getLengths()->getJ(),
                                             $update->getLengths()->getX() / 1000.0,
                                             $update->getLengths()->getY() / 1000.0,
                                             $update->getLengths()->getZ() / 1000.0);

            $colors = self::interpolateTo55($update->getColors()->getI(),
                                            $update->getColors()->getJ(),
                                            $update->getColors()->getX() / 1000.0,
                                            $update->getColors()->getY() / 1000.0,
                                            $update->getColors()->getZ() / 1000.0);

            // Get the average length and average color.
            $avgLength = $update->getLengths()->getAverage() / 1000.0;
            $avgColor = $update->getColors()->getAverage() / 1000.0;

            $isDirty = $update->getIsDirty();         

            // Create the update object.
            $result = new SentimentUpdate($updateTime, $this->symbol, $this->stream, $this->compression,
                                          $lengths, $colors, $avgLength, $avgColor, $isDirty);

            // Send the results back to the APIClient class.
            $this->client->emit('sentimentUpdated', [$result]);
        }

        /**
         * Interpolates a sentiment from it's compressed definition to 55 height or color values.         
         * @param   int     $i  The first point to calculate to.
         * @param   int     $j  The second point to calculate to.
         * @param   double  $x  Value at center.
         * @param   double  $y  Value at point i.
         * @param   double  $z  Value at point j.
         * @return  array
         */
        private static function interpolateTo55(int $i, int $j, float $x, float $y, float $z) : array
        {
            $result = [];
            $values;
            $remaining = 0;            

            try
            {
                // Calculate length between j and end.
                $remaining = self::TOTAL_BARS - 1 - $j;

                // Interpolate from 0 to i.
                $values = array( 2 * $x - $y, $x, $y, 
                                 self::linearInterpolate($i, $y, $j, $z) );

                for ($index = 0.0; $index <= $i; $index++)
                    $result[] = self::cubicInterpolate($values, $index / $i);

                // Adjust j to simplify further calculations.
                $j -= $i;

                // Interpolate from i to j.
                $values = array( self::linearInterpolate($j, $y, $j + $i, $x), $y, $z,
                                 self::linearInterpolate($j, $z, $j + $remaining, 0) );

                for ($index = 1.0; $index <= $j; $index++)
                    $result[] = self::cubicInterpolate($values, $index / $j);

                // Interpolate from j to end.
                $values = array ( self::linearInterpolate($remaining, $z, $remaining + $j, $y), $z, 0, -$z );

                for ($index = 1.0; $index <= $remaining; $index++)
                    $result[] = self::cubicInterpolate($values, $index / $remaining);
            }
            catch (Exception $ex)
            {
                echo "interpolateTo55 Exception: " + $ex.getMessage();
            }

            return $result;
        }

        /**
         * Returns the y-interpolation between x1,y1 and x2,y2 to distant point x5.
         * @param   int     $x2   x-position of point 2.
         * @param   double  $y2   y-position of point 2.
         * @param   int     $x1   x-position of point 1.
         * @param   double  $y1   y-position of point 1.
         * @return  double
         */
        private static function linearInterpolate(int $x2, float $y2, int $x1, float $y1) : float
        {
            if ($x2 - $x1 === 0)
                return ($y2 + $y1) / 2.0;
            else 
                return $x2 * ($y2 - $y1) / ($x2 - $x1) + $y2;
        }

        /**
         * Calculates the cubic interpolation between equidistant points p at x percent from p2 to p3.
         * @param  array    $p    The equidistant points to calculate the interpolation between.
         * @param  double   $x    The distance between point p2 and point p3 as a percentage.
         * @return double
         */
        private static function CubicInterpolate(array $p, float $x) : float
        {
            return $p[1] + 0.5 * $x * ($p[2] - $p[0] +
                ($x * (2.0 * $p[0] - 5.0 * $p[1] + 4.0 * $p[2] - $p[3] + $x * (3.0 * ($p[1] - $p[2]) + $p[3] - $p[0]))));
        }

        /**
         * Creates the destination string that identifies this Sentiment gauge stream.
         * @param   string  $symbol       Symbol to get the Sentiment update data for.
         * @param   string  $stream       Stream ID associated with the stream the client is connected to (realtime, delay, demo).
         * @param   string  $compression  Compression timeframe to apply to the gauge. Default value is 50t.
         * @return  string
         */
        public static function createDestination(string $symbol, string $stream, string $compression) : string
        {            
            return "/gauge/sent/".Utilities::streamIdForSymbol($stream, $symbol).
                    "/".$symbol."/".Utilities::cleanCompression($compression);
        }
    }

?>
