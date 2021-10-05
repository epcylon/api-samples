<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds the details of an Error from the server.
     */
    class SubscriptionError
    {   
        /**
         * A message that describes the error that occured.
         * @var string
         */
        private string $message;
        /**
         * Detailed description about the error (if supplied).
         * @var string
         */
        private string $details;

        /**
         * Creates a new SubscriptionError instance.
         * @param  string  $message  A message that describes the error that occured.
         * @param  string  $details  Detailed description about the error (if supplied).
         */
        function __construct(string $message, string $details)
        {            
            $this->message = $message;
            $this->details = $details;
        }

        /**
         * Returns a message that describes the error that occured.
         * @return string
         */
        public function getMessage() : string
        {
            return $this->message;            
        }

        /**
         * Returns detailed description about the error (if supplied).
         * @return string
         */
        public function getDetails() : string
        {
            return $this->details;            
        }
    }

?>