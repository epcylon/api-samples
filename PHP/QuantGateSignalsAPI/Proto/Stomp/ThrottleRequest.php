<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stomp_v0.1.proto

namespace Stomp;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * The ThrottleRequest message is used to change the throttle rate of an existing 
 * subscription. Use within a RequestFrame. 
 *
 * Generated from protobuf message <code>stomp.ThrottleRequest</code>
 */
class ThrottleRequest extends \Google\Protobuf\Internal\Message
{
    /**
     * The client-specific subscription identifier.
     *
     * Generated from protobuf field <code>uint64 subscription_id = 1;</code>
     */
    private $subscription_id = 0;
    /**
     * The rate to throttle messages at (in ms, 0 for no throttling).
     *
     * Generated from protobuf field <code>uint32 throttle_rate = 2;</code>
     */
    private $throttle_rate = 0;
    /**
     * A unique (optional) receipt id to include if a receipt is desired.
     *
     * Generated from protobuf field <code>uint64 receipt_id = 3;</code>
     */
    private $receipt_id = 0;

    public function __construct() {
        \GPBMetadata\StompV01::initOnce();
        parent::__construct();
    }

    /**
     * The client-specific subscription identifier.
     *
     * Generated from protobuf field <code>uint64 subscription_id = 1;</code>
     * @return int|string
     */
    public function getSubscriptionId()
    {
        return $this->subscription_id;
    }

    /**
     * The client-specific subscription identifier.
     *
     * Generated from protobuf field <code>uint64 subscription_id = 1;</code>
     * @param int|string $var
     * @return $this
     */
    public function setSubscriptionId($var)
    {
        GPBUtil::checkUint64($var);
        $this->subscription_id = $var;

        return $this;
    }

    /**
     * The rate to throttle messages at (in ms, 0 for no throttling).
     *
     * Generated from protobuf field <code>uint32 throttle_rate = 2;</code>
     * @return int
     */
    public function getThrottleRate()
    {
        return $this->throttle_rate;
    }

    /**
     * The rate to throttle messages at (in ms, 0 for no throttling).
     *
     * Generated from protobuf field <code>uint32 throttle_rate = 2;</code>
     * @param int $var
     * @return $this
     */
    public function setThrottleRate($var)
    {
        GPBUtil::checkUint32($var);
        $this->throttle_rate = $var;

        return $this;
    }

    /**
     * A unique (optional) receipt id to include if a receipt is desired.
     *
     * Generated from protobuf field <code>uint64 receipt_id = 3;</code>
     * @return int|string
     */
    public function getReceiptId()
    {
        return $this->receipt_id;
    }

    /**
     * A unique (optional) receipt id to include if a receipt is desired.
     *
     * Generated from protobuf field <code>uint64 receipt_id = 3;</code>
     * @param int|string $var
     * @return $this
     */
    public function setReceiptId($var)
    {
        GPBUtil::checkUint64($var);
        $this->receipt_id = $var;

        return $this;
    }

}

