<?php

    namespace QuantGate\API\Signals\Subscriptions;

    abstract class SubscriptionBase
    {
        private string $destination;
        private int $id;
        private int $throttleRate;

        protected function __construct(string $destination, int $id, int $throttleRate)
        {
            $this->destination = $destination;
            $this->id = $id;
            $this->throttleRate = $throttleRate;
        }

        public function getID() : int
        {
            return $this->id;
        }

        public function getDestination() : string
        {
            return $this->destination;
        }

        public function getThrottleRate() : int
        {
            return $this->throttleRate;
        }

        public abstract function handleMessage($body);
    }

?>