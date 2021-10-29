using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;

namespace BridgeRock.MauiExample.Controls
{
    public partial class SimpleGauge : ContentView
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SGg";

        public SimpleGauge()
        {
            InitializeComponent();
        }

        private double _value = 0;
        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    topRow.Height = new Microsoft.Maui.GridLength(1000*(1.0 - _value), Microsoft.Maui.GridUnitType.Star);
                    bottomRow.Height = new Microsoft.Maui.GridLength(1000*(1.0 + _value), Microsoft.Maui.GridUnitType.Star);
                }
            }
        }

    }
}