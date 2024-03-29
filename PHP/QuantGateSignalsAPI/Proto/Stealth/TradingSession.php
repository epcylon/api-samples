<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * Holds information about the current trading session for an instrument. 
 *
 * Generated from protobuf message <code>stealth.TradingSession</code>
 */
class TradingSession extends \Google\Protobuf\Internal\Message
{
    /**
     * The close time of the trading session in minutes from midnight.
     *
     * Generated from protobuf field <code>sint32 close = 1;</code>
     */
    private $close = 0;
    /**
     * The length of the trading session in minutes from midnight.
     *
     * Generated from protobuf field <code>sint32 length = 2;</code>
     */
    private $length = 0;

    public function __construct() {
        \GPBMetadata\StealthApiV20::initOnce();
        parent::__construct();
    }

    /**
     * The close time of the trading session in minutes from midnight.
     *
     * Generated from protobuf field <code>sint32 close = 1;</code>
     * @return int
     */
    public function getClose()
    {
        return $this->close;
    }

    /**
     * The close time of the trading session in minutes from midnight.
     *
     * Generated from protobuf field <code>sint32 close = 1;</code>
     * @param int $var
     * @return $this
     */
    public function setClose($var)
    {
        GPBUtil::checkInt32($var);
        $this->close = $var;

        return $this;
    }

    /**
     * The length of the trading session in minutes from midnight.
     *
     * Generated from protobuf field <code>sint32 length = 2;</code>
     * @return int
     */
    public function getLength()
    {
        return $this->length;
    }

    /**
     * The length of the trading session in minutes from midnight.
     *
     * Generated from protobuf field <code>sint32 length = 2;</code>
     * @param int $var
     * @return $this
     */
    public function setLength($var)
    {
        GPBUtil::checkInt32($var);
        $this->length = $var;

        return $this;
    }

}

