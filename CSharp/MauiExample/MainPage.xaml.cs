using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System.Diagnostics;

namespace BridgeRock.MauiExample
{
    public partial class MainPage : ContentPage
	{
		private readonly APIClient _client;
		private TopSymbolsEventArgs _topSymbols;
		private string _symbol = "NQ H2";
		private readonly string _strategyId = "Crb9.0";		

		public MainPage()
		{
			InitializeComponent();

			BindingContext = this;			
			
			_client = new APIClient(Environments.Development, stream: DataStream.Realtime);			
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
			_client.SentimentUpdated += (s, e) => s50t.UpdateSpectrum(e);
			
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
			Trace.TraceInformation(e.Symbol + " " + (e.Details?.InstrumentType.ToString() ?? "None") + 
								   " " + (e.Details?.ExpiryDate.ToString() ?? string.Empty) + 
								   " " + e.Error?.Message ?? string.Empty);
		}

        private void HandleError(object client, QuantGate.API.Events.ErrorEventArgs args)
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

        private void HandleSearchInput(object sender, EventArgs e)
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

			List<SearchRow> searchRows = new();

			if (_topSymbols?.Symbols is object)
				foreach (TopSymbol symbol in _topSymbols.Symbols)
					searchRows.Add(new SearchRow
					{
						Symbol = symbol.Symbol,
						DisplayName = symbol.DisplayName,
						EntryProgress = symbol.EntryProgress.ToString("p1")
					});

			colSearch.ItemsSource = searchRows;
		}

		private void HandleSearchUpdate(object sender, SearchResultsEventArgs e)
		{
			if (string.IsNullOrEmpty(eSearch.Text))
				return;

			List<SearchRow> searchRows = new();

			foreach (SearchResult result in e.Results)
				searchRows.Add(new SearchRow
				{
					Symbol = result.Symbol,
					DisplayName = result.DisplayName,
					EntryProgress = string.Empty
				});
			colSearch.ItemsSource = searchRows;
		}
	}
}
