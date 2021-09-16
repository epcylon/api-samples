<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

/**
 * The Put/Call side ("right") of an option. 
 *
 * Protobuf enum <code>Stealth\PutOrCall</code>
 */
class PutOrCall
{
    /**
     * Instrument is not an option.
     *
     * Generated from protobuf enum <code>NoPutCall = 0;</code>
     */
    const NoPutCall = 0;
    /**
     * Put Option (option to sell at strike).
     *
     * Generated from protobuf enum <code>Put = 1;</code>
     */
    const Put = 1;
    /**
     * Call Option (option to buy at strike).
     *
     * Generated from protobuf enum <code>Call = 2;</code>
     */
    const Call = 2;
}
