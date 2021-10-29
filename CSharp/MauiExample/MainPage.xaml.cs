using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using System.Threading;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals;
using QuantGate.API.Events;
using System.Diagnostics;

namespace MauiExample
{
	public partial class MainPage : ContentPage
	{
		private readonly APIClient _client;
		private TopSymbolsEventArgs _topSymbols;
		private string _symbol = "EUR.USD";
		private readonly string _strategyId = "Crb9.0";

		int count = 0;

		//private readonly BallField _field;
		//private readonly SkiaGraphicsView _view;
		//private readonly Timer _timer;


		public MainPage()
		{
            InitializeComponent();

			BindingContext = this;

			//_client = new ProtoStompClient("wss://feed.stealthtrader.com");
			_client = new APIClient("wss://test.stealthtrader.com", stream: DataStream.Realtime);
			//_client = new ProtoStompClient("ws://localhost", 2432);
			_client.Connected += HandleConnected;
			_client.Disconnected += HandleDisconnected;
			_client.Error += HandleError;

            _client.InstrumentUpdated += _client_InstrumentUpdated;
            _client.PerceptionUpdated += _client_PerceptionUpdated;

			_client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
							"eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz" +
							"A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0." +
							"xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8");

			_client.RequestInstrumentDetails(_symbol);
			_client.SubscribePerception(_symbol);

			//_field = new BallField(100);
			//_view = new SkiaGraphicsView(_field);

			//Grid.SetRow(_view, 4);			
			//gridMain.Children.Add(_view);

			//_timer = new(HandleTimer, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));
		}

        private void _client_InstrumentUpdated(object sender, InstrumentEventArgs e)
        {
			Trace.TraceInformation("Got instrument for " + e.Symbol);
        }

        private void _client_PerceptionUpdated(object sender, PerceptionEventArgs e)
        {
			sldPerception.Value = e.Value;
			sgPerception.Value = e.Value;
			Trace.TraceInformation("Perception for " + e.Symbol + " = " + e.Value.ToString("0.00"));
        }

        //private void HandleTimer(object state)
        //      {
        //	Dispatcher.BeginInvokeOnMainThread(() =>
        //	{				
        //		_field.Advance(10, _view.ActualWidth, _view.ActualHeight);
        //		_view.Invalidate();
        //	});
        //}

        private void HandleError(object client, ErrorEventArgs args)
		{
			Trace.TraceInformation("Error! " + args.Message);
		}

		private void HandleDisconnected(object client, EventArgs args)
		{
			Trace.TraceInformation("Disconnected!");
		}

		private void HandleConnected(object client, EventArgs args)
		{
			Trace.TraceInformation("Connected!");
		}
	}
}
