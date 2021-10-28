using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using System.Threading;

namespace MauiExample
{
	public partial class MainPage : ContentPage
	{
		int count = 0;

		//private readonly BallField _field;
        //private readonly SkiaGraphicsView _view;
		//private readonly Timer _timer;

		public MainPage()
		{
            InitializeComponent();

			//_field = new BallField(100);
			//_view = new SkiaGraphicsView(_field);

			//Grid.SetRow(_view, 4);			
			//gridMain.Children.Add(_view);

			//_timer = new(HandleTimer, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));
		}

		//private void HandleTimer(object state)
  //      {
		//	Dispatcher.BeginInvokeOnMainThread(() =>
		//	{				
		//		_field.Advance(10, _view.ActualWidth, _view.ActualHeight);
		//		_view.Invalidate();
		//	});
		//}

		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";

			SemanticScreenReader.Announce(CounterLabel.Text);
		}
	}
}
