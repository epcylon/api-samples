using Microsoft.Maui.Controls;
using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System;
using System.Diagnostics;

namespace MauiExample
{
    public partial class MainPage : ContentPage
	{
		private readonly APIClient _client;
		private TopSymbolsEventArgs _topSymbols;
		private string _symbol = "NQ Z1";
		private readonly string _strategyId = "Crb9.0";

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
			_client.PerceptionUpdated += (s, e) => sgPerception.Value = e.Value;
			_client.CommitmentUpdated += (s, e) => sgCommitment.Value = e.Value;
			_client.BookPressureUpdated += (s, e) => sgBookPressure.Value = e.Value;
			_client.HeadroomUpdated += (s, e) => sgHeadroom.Value = e.Value;			

			_client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
							"eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz" +
							"A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0." +
							"xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8");
			
			Subscribe(_symbol);
		}


		private void Subscribe(string symbol)
		{
			// Unsubscribe from all subscriptions for this symbol.
			_client.UnsubscribeAll(_symbol);
			//sViewer.ClearSpectrum();

			_symbol = symbol;

			_client.RequestInstrumentDetails(_symbol);
			_client.SubscribePerception(_symbol);
			_client.SubscribeCommitment(_symbol);
			_client.SubscribeEquilibrium(_symbol, "300s");
			_client.SubscribeSentiment(_symbol, "50t");
			_client.SubscribeHeadroom(_symbol);
			_client.SubscribeBookPressure(_symbol);
			_client.SubscribeMultiframeEquilibrium(_symbol);
			_client.SubscribeTrigger(_symbol);
			_client.SubscribeStrategy(_strategyId, _symbol);
		}
		
        private void _client_InstrumentUpdated(object sender, InstrumentEventArgs e)
        {
			Trace.TraceInformation("Got instrument for " + e.Symbol);
        }

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
