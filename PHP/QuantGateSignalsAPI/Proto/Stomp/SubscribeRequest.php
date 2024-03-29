<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stomp_v0.1.proto

namespace Stomp;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * The SubscribeRequest message is used to subscribe to a stream of events.
 * Use within a RequestFrame. 
 *
 * Generated from protobuf message <code>stomp.SubscribeRequest</code>
 */
class SubscribeRequest extends \Google\Protobuf\Internal\Message
{
    /**
     * A string that identifies the subscription.
     *
     * Generated from protobuf field <code>string destination = 1;</code>
     */
    private $destination = '';
    /**
     * A unique client identifier for the subscription.
     *
     * Generated from protobuf field <code>uint64 subscription_id = 2;</code>
     */
    private $subscription_id = 0;
    /**
     * The rate to throttle messages at (in ms, 0 for no throttling).
     *
     * Generated from protobuf field <code>uint32 throttle_rate = 3;</code>
     */
    private $throttle_rate = 0;
    /**
     * A unique (optional) receipt id to include if a receipt is desired.
     *
     * Generated from protobuf field <code>uint64 receipt_id = 4;</code>
     */
    private $receipt_id = 0;

    public function __construct() {
        \GPBMetadata\StompV01::initOnce();
        parent::__construct();
    }

    /**
     * A string that identifies the subscription.
     *
     * Generated from protobuf field <code>string destination = 1;</code>
     * @return string
     */
    public function getDestination()
    {
        return $this->destination;
    }

    /**
     * A string that identifies the subscription.
     *
     * Generated from protobuf field <code>string destination = 1;</code>
     * @param string $var
     * @return $this
     */
    public function setDestination($var)
    {
        GPBUtil::checkString($var, True);
        $this->destination = $var;

        return $this;
    }

    /**
     * A unique client identifier for the subscription.
     *
     * Generated from protobuf field <code>uint64 subscription_id = 2;</code>
     * @return int|string
     */
    public function getSubscriptionId()
    {
        return $this->subscription_id;
    }

    /**
     * A unique client identifier for the subscription.
     *
     * Generated from protobuf field <code>uint64 subscription_id = 2;</code>
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
     * Generated from protobuf field <code>uint32 throttle_rate = 3;</code>
     * @return int
     */
    public function getThrottleRate()
    {
        return $this->throttle_rate;
    }

    /**
     * The rate to throttle messages at (in ms, 0 for no throttling).
     *
     * Generated from protobuf field <code>uint32 throttle_rate = 3;</code>
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
     * Generated from protobuf field <code>uint64 receipt_id = 4;</code>
     * @return int|string
     */
    public function getReceiptId()
    {
        return $this->receipt_id;
    }

    /**
     * A unique (optional) receipt id to include if a receipt is desired.
     *
     * Generated from protobuf field <code>uint64 receipt_id = 4;</code>
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

