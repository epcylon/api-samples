<?php

    class APIClient
    {
        public const REALTIME_STREAM = "realtime";
        public const DELAY_STREAM = "delay";
        public const DEMO_STREAM = "demo";

        public $host;
        public $port;
        public $stream;

        function __construct($host, $port = 443, $stream = self::REALTIME_STREAM)
        {
            $this->host = $host;
            $this->port = $port;
            $this->stream = $stream;
        }
    }

?>