<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * Holds a single tick range for an Instrument definition. 
 *
 * Generated from protobuf message <code>stealth.TickValue</code>
 */
class TickValue extends \Google\Protobuf\Internal\Message
{
    /**
     * The start of the tick range.
     *
     * Generated from protobuf field <code>double start = 1;</code>
     */
    private $start = 0.0;
    /**
     * The tick value at this range.
     *
     * Generated from protobuf field <code>double tick = 2;</code>
     */
    private $tick = 0.0;
    /**
     * Denominator for fractional formats.
     *
     * Generated from protobuf field <code>uint32 denominator = 3;</code>
     */
    private $denominator = 0;
    /**
     * Number of decimals in decimal format.
     *
     * Generated from protobuf field <code>sint32 decimals = 4;</code>
     */
    private $decimals = 0;
    /**
     * Format to use (Decimal/Fraction/Tick).
     *
     * Generated from protobuf field <code>.stealth.TickValue.TickFormat format = 5;</code>
     */
    private $format = 0;

    public function __construct() {
        \GPBMetadata\StealthApiV20::initOnce();
        parent::__construct();
    }

    /**
     * The start of the tick range.
     *
     * Generated from protobuf field <code>double start = 1;</code>
     * @return float
     */
    public function getStart()
    {
        return $this->start;
    }

    /**
     * The start of the tick range.
     *
     * Generated from protobuf field <code>double start = 1;</code>
     * @param float $var
     * @return $this
     */
    public function setStart($var)
    {
        GPBUtil::checkDouble($var);
        $this->start = $var;

        return $this;
    }

    /**
     * The tick value at this range.
     *
     * Generated from protobuf field <code>double tick = 2;</code>
     * @return float
     */
    public function getTick()
    {
        return $this->tick;
    }

    /**
     * The tick value at this range.
     *
     * Generated from protobuf field <code>double tick = 2;</code>
     * @param float $var
     * @return $this
     */
    public function setTick($var)
    {
        GPBUtil::checkDouble($var);
        $this->tick = $var;

        return $this;
    }

    /**
     * Denominator for fractional formats.
     *
     * Generated from protobuf field <code>uint32 denominator = 3;</code>
     * @return int
     */
    public function getDenominator()
    {
        return $this->denominator;
    }

    /**
     * Denominator for fractional formats.
     *
     * Generated from protobuf field <code>uint32 denominator = 3;</code>
     * @param int $var
     * @return $this
     */
    public function setDenominator($var)
    {
        GPBUtil::checkUint32($var);
        $this->denominator = $var;

        return $this;
    }

    /**
     * Number of decimals in decimal format.
     *
     * Generated from protobuf field <code>sint32 decimals = 4;</code>
     * @return int
     */
    public function getDecimals()
    {
        return $this->decimals;
    }

    /**
     * Number of decimals in decimal format.
     *
     * Generated from protobuf field <code>sint32 decimals = 4;</code>
     * @param int $var
     * @return $this
     */
    public function setDecimals($var)
    {
        GPBUtil::checkInt32($var);
        $this->decimals = $var;

        return $this;
    }

    /**
     * Format to use (Decimal/Fraction/Tick).
     *
     * Generated from protobuf field <code>.stealth.TickValue.TickFormat format = 5;</code>
     * @return int
     */
    public function getFormat()
    {
        return $this->format;
    }

    /**
     * Format to use (Decimal/Fraction/Tick).
     *
     * Generated from protobuf field <code>.stealth.TickValue.TickFormat format = 5;</code>
     * @param int $var
     * @return $this
     */
    public function setFormat($var)
    {
        GPBUtil::checkEnum($var, \Stealth\TickValue_TickFormat::class);
        $this->format = $var;

        return $this;
    }

}

