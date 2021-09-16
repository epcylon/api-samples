<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: stealth-api-v2.0.proto

namespace Stealth;

/**
 * Types of tick formats available to display prices in.
 *
 * Protobuf enum <code>Stealth\TickValue\TickFormat</code>
 */
class TickValue_TickFormat
{
    /**
     * Decimal display (regular 0.000, etc.) 
     *
     * Generated from protobuf enum <code>Decimal = 0;</code>
     */
    const Decimal = 0;
    /**
     * Fractional format, such as 34 1/4.
     * In this case, the denominator that will be used for non-integer portions of the
     * number will be that supplied with the format. Note, that the fraction is generally 
     * displayed in its simplified format (divide numerator and denominator by GCD). 
     *
     * Generated from protobuf enum <code>Fraction = 1;</code>
     */
    const Fraction = 1;
    /**
     * Tick format, such as 34'120.
     * In this case, the value after the tick is the non-integer portion of the number,
     * multiplied by the denominator value supplied, zero padded to the left to fit the
     * number of digits required to display the denominator value as a full integer. 
     *
     * Generated from protobuf enum <code>Tick = 2;</code>
     */
    const Tick = 2;
}
