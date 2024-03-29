<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * SinglePriceUpdate represents a new subscription update for a guage that returns a single 
 * price result (such as Simple Moving Average).
 * All prices are converted to long values, empty values will be decoded as NaN. 
 *
 * Generated from protobuf message <code>stealth.SinglePriceUpdate</code>
 */
class SinglePriceUpdate extends \Google\Protobuf\Internal\Message
{
    /**
     * The timestamp of the latest update.
     *
     * Generated from protobuf field <code>uint64 timestamp = 1;</code>
     */
    private $timestamp = 0;
    /**
     * The single price value.
     *
     * Generated from protobuf field <code>uint64 value = 2;</code>
     */
    private $value = 0;
    /**
     * Is the data that generated this potentially dirty (or stale)?
     *
     * Generated from protobuf field <code>bool is_dirty = 3;</code>
     */
    private $is_dirty = false;

    public function __construct() {
        \GPBMetadata\StealthApiV20::initOnce();
        parent::__construct();
    }

    /**
     * The timestamp of the latest update.
     *
     * Generated from protobuf field <code>uint64 timestamp = 1;</code>
     * @return int|string
     */
    public function getTimestamp()
    {
        return $this->timestamp;
    }

    /**
     * The timestamp of the latest update.
     *
     * Generated from protobuf field <code>uint64 timestamp = 1;</code>
     * @param int|string $var
     * @return $this
     */
    public function setTimestamp($var)
    {
        GPBUtil::checkUint64($var);
        $this->timestamp = $var;

        return $this;
    }

    /**
     * The single price value.
     *
     * Generated from protobuf field <code>uint64 value = 2;</code>
     * @return int|string
     */
    public function getValue()
    {
        return $this->value;
    }

    /**
     * The single price value.
     *
     * Generated from protobuf field <code>uint64 value = 2;</code>
     * @param int|string $var
     * @return $this
     */
    public function setValue($var)
    {
        GPBUtil::checkUint64($var);
        $this->value = $var;

        return $this;
    }

    /**
     * Is the data that generated this potentially dirty (or stale)?
     *
     * Generated from protobuf field <code>bool is_dirty = 3;</code>
     * @return bool
     */
    public function getIsDirty()
    {
        return $this->is_dirty;
    }

    /**
     * Is the data that generated this potentially dirty (or stale)?
     *
     * Generated from protobuf field <code>bool is_dirty = 3;</code>
     * @param bool $var
     * @return $this
     */
    public function setIsDirty($var)
    {
        GPBUtil::checkBool($var);
        $this->is_dirty = $var;

        return $this;
    }

}

