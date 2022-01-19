namespace BridgeRock.MauiExample.Controls
{
    public partial class SimpleGauge : ContentView
    {
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
                    topRow.Height = new GridLength(1000*(1.0 - _value), GridUnitType.Star);
                    bottomRow.Height = new GridLength(1000*(1.0 + _value), GridUnitType.Star);
                }
            }
        }
    }
}