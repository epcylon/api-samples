<?php

    namespace QuantGate\API\Signals\Events;      

    /**
     * Holds the details of an Error from the server.
     */
    class ErrorDetails
    {   
        /**
         * A message that describes the error that occured.
         * @var string
         */
        private string $message;

        /**
         * Creates a new ErrorDetails instance.
         * @param string    $message     A message that describes the error that occured.
         */
        function __construct(string $message)
        {            
            $this->message = $message;
        }

        /**
         * Returns a message that describes the error that occured.
         * @return string
         */
        public function getMessage() : string
        {
            return $this->message;            
        }
    }

?>