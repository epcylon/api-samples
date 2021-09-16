<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

use Google\Protobuf\Internal\GPBType;
use Google\Protobuf\Internal\RepeatedField;
use Google\Protobuf\Internal\GPBUtil;

/**
 * Holds an update of the current top items of a top symbols subscription. 
 *
 * Generated from protobuf message <code>stealth.TopSymbolsUpdate</code>
 */
class TopSymbolsUpdate extends \Google\Protobuf\Internal\Message
{
    /**
     * The top symbol results;
     *
     * Generated from protobuf field <code>repeated .stealth.TopSymbolItem symbols = 1;</code>
     */
    private $symbols;

    public function __construct() {
        \GPBMetadata\StealthApiV20::initOnce();
        parent::__construct();
    }

    /**
     * The top symbol results;
     *
     * Generated from protobuf field <code>repeated .stealth.TopSymbolItem symbols = 1;</code>
     * @return \Google\Protobuf\Internal\RepeatedField
     */
    public function getSymbols()
    {
        return $this->symbols;
    }

    /**
     * The top symbol results;
     *
     * Generated from protobuf field <code>repeated .stealth.TopSymbolItem symbols = 1;</code>
     * @param \Stealth\TopSymbolItem[]|\Google\Protobuf\Internal\RepeatedField $var
     * @return $this
     */
    public function setSymbols($var)
    {
        $arr = GPBUtil::checkRepeatedField($var, \Google\Protobuf\Internal\GPBType::MESSAGE, \Stealth\TopSymbolItem::class);
        $this->symbols = $arr;

        return $this;
    }

}

