<?php

    require __DIR__ . '/QuantGateSignalsAPI/vendor/autoload.php';

    $loop = \React\EventLoop\Factory::create();
    $counter = 0;

    $loop->addPeriodicTimer(1, function(\React\EventLoop\TimerInterface $timer) use (&$loop, &$counter)
    {
     // timerLoop($timer);
      $counter++;

      if ($counter === 5)
      {       
        $loop->stop(); 
        //$loop->cancelTimer($timer);
      }

      echo "Hello\n";
    });

    echo "Before timer\n";

    $loop->run();

    echo "Done\n";

    function timerLoop(\React\EventLoop\TimerInterface $timer)
    {      
      $this->counter++;

      if ($this->counter === 5)
      {
        $this->loop->stop();
        //$this->loop->cancelTimer($timer);
      }

      echo "Hello\n";      
    }
?>