<?php

    namespace QuantGate\API\Signals\Subscriptions;

    use \QuantGate\API\Signals\APIClient;

    /**
     * Abstract base subscription class - includes basic properties and message handler definition.
     */
    abstract class SubscriptionBase
    {
        /**
         * The destination to subscribe to.
         * @var string
         */        
        private string $destination;
        /**
         * The (integer) identifier to associate with this subscription on the server's end.
         * @var int
         */
        private int $id;
        /**
         * Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @var int
         */
        private int $throttleRate;
        /**
         * Holds a reference to the parent APIClient instance to send updates to.
         * @var callback
         */        
        protected APIClient $client;

        /**
         * Creates a new SubscriptionBase instance.
         * @param   string      $destination    The destination to subscribe to.
         * @param   int         $id             The (integer) identifier to associate with this subscription on the server's end.
         * @param   int         $throttleRate   Rate to throttle messages at (in ms). Enter 0 for no throttling.
         * @param   APIClient   $client         Reference to the parent APIClient instance to send updates to.
         */
        protected function __construct(string $destination, int $id, int $throttleRate, APIClient $client)
        {
            $this->destination = $destination;
            $this->id = $id;
            $this->throttleRate = $throttleRate;
            $this->client = $client;
        }

        /**
         * Returns the (integer) identifier to associate with this subscription on the server's end.
         * @return int
         */
        public function getId() : int
        {
            return $this->id;
        }

        /**
         * Returns the (integer) identifier to associate with this subscription on the server's end.
         * @param   int $id     The new identifier to set to.
         * @return void
         */
        public function setId(int $id)
        {
            $this->id = $id;
        }

        /**
         * Returns the destination to subscribe to.
         * @return string
         */
        public function getDestination() : string
        {
            return $this->destination;
        }

        /**
         * Returns the rate to throttle messages at (in ms). (0 = no throttling.)
         * @return int
         */
        public function getThrottleRate() : int
        {
            return $this->throttleRate;
        }

        /**
         * Returns the rate to throttle messages at (in ms). (0 = no throttling.)
         * @param   int $throttleRate   The new throttle rate to set to.
         * @return  void
         */
        public function setThrottleRate(int $throttleRate)
        {
            $this->throttleRate = $throttleRate;
        }

        /**
         * Called whenever a new stream update message is received.
         * @param   $body   The raw message received from the stream.
         * @return  void
         */
        public abstract function handleMessage($body);
    }

?>