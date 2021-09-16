<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * SingleValueUpdate represents a new subscription update for a gauge that returns a single
 * value (such as Perception, Commitment, Headroom, Book Pressure, or SMA).
 * Values should be converted to between -1 and 1 by dividing by 1000. 
 *
 * Generated from protobuf message <code>stealth.SingleValueUpdate</code>
 */
class SingleValueUpdate extends \Google\Protobuf\Internal\Message
{
    /**
     * The timestamp of the latest update.
     *
     * Generated from protobuf field <code>uint64 timestamp = 1;</code>
     */
    private $timestamp = 0;
    /**
     * The single value. (-1000 to 1000)
     *
     * Generated from protobuf field <code>sint32 value = 2;</code>
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
     * The single value. (-1000 to 1000)
     *
     * Generated from protobuf field <code>sint32 value = 2;</code>
     * @return int
     */
    public function getValue()
    {
        return $this->value;
    }

    /**
     * The single value. (-1000 to 1000)
     *
     * Generated from protobuf field <code>sint32 value = 2;</code>
     * @param int $var
     * @return $this
     */
    public function setValue($var)
    {
        GPBUtil::checkInt32($var);
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
