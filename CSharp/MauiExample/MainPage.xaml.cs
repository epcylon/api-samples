using Microsoft.Maui.Controls;
using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BridgeRock.MauiExample
{
    public partial class MainPage : ContentPage
	{
		private readonly APIClient _client;
		private TopSymbolsEventArgs _topSymbols;
		private string _symbol = "NQ Z1";
		private readonly string _strategyId = "Crb9.0";
		private ObservableCollection<SearchRow> _searchRows = new ObservableCollection<SearchRow>();

		public MainPage()
		{
            InitializeComponent();

			BindingContext = this;

			eSearch.TextChanged += HandleSearchInput;
			colSearch.ItemsSource = _searchRows;

			//_client = new ProtoStompClient("wss://feed.stealthtrader.com");
			_client = new APIClient("wss://test.stealthtrader.com", stream: DataStream.Realtime);
			//_client = new ProtoStompClient("ws://localhost", 2432);
			_client.Connected += HandleConnected;
			_client.Disconnected += HandleDisconnected;
			_client.Error += HandleError;

			_client.InstrumentUpdated += HandleInstrumentUpdate;
			_client.SymbolSearchUpdated += HandleSearchUpdate;
			_client.TopSymbolsUpdated += HandleTopSymbolsUpdate;
			_client.PerceptionUpdated += (s, e) => sgPerception.Value = e.Value;
			_client.CommitmentUpdated += (s, e) => sgCommitment.Value = e.Value;
			_client.BookPressureUpdated += (s, e) => sgBookPressure.Value = e.Value;
			_client.HeadroomUpdated += (s, e) => sgHeadroom.Value = e.Value;			

			_client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
							"eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz" +
							"A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0." +
							"xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8");

			SubscribeSearch();
			Subscribe(_symbol);
		}

		private void SubscribeSearch()
		{
			_client.SubscribeTopSymbols("ib");
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

		private void HandleInstrumentUpdate(object sender, InstrumentEventArgs e)
		{
			Trace.TraceInformation(e.Symbol + " " + e.InstrumentType.ToString() + " " +
		 						   e.ExpiryDate.ToString() + " " + e.ErrorMessage);
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

        private void HandleSearchInput(object sender, TextChangedEventArgs e)
        {
			if (!string.IsNullOrEmpty(eSearch.Text))
			{
				_client.SearchSymbols(eSearch.Text, "paper");
			}
			else
			{
				HandleTopSymbolsUpdate(this, _topSymbols);
			}
		}

		private void HandleTopSymbolsUpdate(object sender, TopSymbolsEventArgs topSymbols)
		{
			_topSymbols = topSymbols;
			if (!string.IsNullOrEmpty(eSearch.Text))
				return;

			_searchRows.Clear();

			if (_topSymbols?.Symbols is object)
				foreach (TopSymbol symbol in _topSymbols.Symbols)
					_searchRows.Add(new SearchRow
					{
						Symbol = symbol.Symbol,
						DisplayName = symbol.DisplayName,
						EntryProgress = symbol.EntryProgress.ToString("p1")
					});
		}

		private void HandleSearchUpdate(object sender, SearchResultsEventArgs e)
		{
			if (string.IsNullOrEmpty(eSearch.Text))
				return;

			_searchRows.Clear();

			foreach (SearchResult result in e.Results)
				_searchRows.Add(new SearchRow
				{
					Symbol = result.Symbol,
					DisplayName = result.DisplayName,
					EntryProgress = string.Empty
				});
		}
	}
}
