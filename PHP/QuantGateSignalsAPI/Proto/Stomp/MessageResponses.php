<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stomp_v0.1.proto

namespace Stomp;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * Holds a collection of MessageResponse elements. Used in batch messages. 
 *
 * Generated from protobuf message <code>stomp.MessageResponses</code>
 */
class MessageResponses extends \Google\Protobuf\Internal\Message
{
    /**
     * A collection of messages to handle.
     *
     * Generated from protobuf field <code>repeated .stomp.MessageResponse message = 1;</code>
     */
    private $message;

    public function __construct() {
        \GPBMetadata\StompV01::initOnce();
        parent::__construct();
    }

    /**
     * A collection of messages to handle.
     *
     * Generated from protobuf field <code>repeated .stomp.MessageResponse message = 1;</code>
     * @return \Google\Protobuf\Internal\RepeatedField
     */
    public function getMessage()
    {
        return $this->message;
    }

    /**
     * A collection of messages to handle.
     *
     * Generated from protobuf field <code>repeated .stomp.MessageResponse message = 1;</code>
     * @param \Stomp\MessageResponse[]|\Google\Protobuf\Internal\RepeatedField $var
     * @return $this
     */
    public function setMessage($var)
    {
        $arr = GPBUtil::checkRepeatedField($var, \Google\Protobuf\Internal\GPBType::MESSAGE, \Stomp\MessageResponse::class);
        $this->message = $arr;

        return $this;
    }

}
