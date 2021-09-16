<?php

    require __DIR__ . '/QuantGateSignalsAPI/vendor/autoload.php';

    \Ratchet\Client\connect('wss://test.stealthtrader.com:443')->then(function($conn) {
        $conn->on('message', function($msg) use ($conn) {
            echo "Received: {$msg}\n";
            $conn->close();
        });

        echo "Connected!\n";
        $conn->send('Hello World!');
    }, function ($e) {
        echo "Could not connect: {$e->getMessage()}\n";
    });