<?php

    namespace QuantGate\API\Signals\Subscriptions;

    /*
      Abstract base subscription class - includes basic properties and message handler definition.
    */
    abstract class SubscriptionBase
    {
        // The destination to subscribe to.
        private string $destination;
        // The (integer) identifier to associate with this subscription on the server's end.
        private int $id;
        // Rate to throttle messages at (in ms). Enter 0 for no throttling.
        private int $throttleRate;

        /*
          Creates a new SubscriptionBase instance.
            $destination - The destination to subscribe to.
            $id - The (integer) identifier to associate with this subscription on the server's end.
            $throttleRate - Rate to throttle messages at (in ms). Enter 0 for no throttling.            
        */
        protected function __construct(string $destination, int $id, int $throttleRate)
        {
            $this->destination = $destination;
            $this->id = $id;
            $this->throttleRate = $throttleRate;
        }

        /*
          Returns the (integer) identifier to associate with this subscription on the server's end.
        */
        public function getID() : int
        {
            return $this->id;
        }

        /*
          Returns the destination to subscribe to.
        */
        public function getDestination() : string
        {
            return $this->destination;
        }

        /*
          Returns the rate to throttle messages at (in ms). (0 = no throttling.)
        */
        public function getThrottleRate() : int
        {
            return $this->throttleRate;
        }

        /*
          Called whenever a new stream update message is received.
            $body - The raw message received from the stream.
        */
        public abstract function handleMessage($body);
    }

?>