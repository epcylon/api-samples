using Microsoft.Maui.Controls;

namespace BridgeRock.MauiExample.Controls
{
    public partial class SentimentBar : ContentView
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SBr";

        public SentimentBar()
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

                    if (_value > 0)
                    {
                        topRow.Height = new Microsoft.Maui.GridLength(1000 * (1.0 - _value), Microsoft.Maui.GridUnitType.Star);
                        barRow.Height = new Microsoft.Maui.GridLength(1000 * _value, Microsoft.Maui.GridUnitType.Star);
                        bottomRow.Height = new Microsoft.Maui.GridLength(1000, Microsoft.Maui.GridUnitType.Star);
                    }
                    else
                    {
                        topRow.Height = new Microsoft.Maui.GridLength(1000, Microsoft.Maui.GridUnitType.Star);
                        barRow.Height = new Microsoft.Maui.GridLength(1000 * -_value, Microsoft.Maui.GridUnitType.Star);
                        bottomRow.Height = new Microsoft.Maui.GridLength(1000 * (1.0 + _value), Microsoft.Maui.GridUnitType.Star);
                    }
                }
            }
        }

    }
}