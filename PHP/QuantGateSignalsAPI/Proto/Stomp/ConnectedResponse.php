<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stomp_v0.1.proto

namespace Stomp;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * The ConnectedResponse message is sent by the server whenever a connection has been
 * successfully made. 
 *
 * Generated from protobuf message <code>stomp.ConnectedResponse</code>
 */
class ConnectedResponse extends \Google\Protobuf\Internal\Message
{
    /**
     * The version of the server being used.
     *
     * Generated from protobuf field <code>string version = 1;</code>
     */
    private $version = '';

    public function __construct() {
        \GPBMetadata\StompV01::initOnce();
        parent::__construct();
    }

    /**
     * The version of the server being used.
     *
     * Generated from protobuf field <code>string version = 1;</code>
     * @return string
     */
    public function getVersion()
    {
        return $this->version;
    }

    /**
     * The version of the server being used.
     *
     * Generated from protobuf field <code>string version = 1;</code>
     * @param string $var
     * @return $this
     */
    public function setVersion($var)
    {
        GPBUtil::checkString($var, True);
        $this->version = $var;

        return $this;
    }

}
