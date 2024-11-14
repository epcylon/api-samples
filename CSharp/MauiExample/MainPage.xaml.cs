using Epcylon.APIs.Account;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System.Diagnostics;

namespace QuantGate.MauiExample
{
    public partial class MainPage : ContentPage
    {
        private APIClient? _client;
        private TopSymbolsEventArgs? _topSymbols;
        private string _symbol = "MSFT";
        private readonly string _strategyId = "Crb7.6";

        private APIClient? Client
        {
            get { return _client; }
            set
            {
                if (_client is not null)
                {
                    _client.Connected -= HandleConnected;
                    _client.Disconnected -= HandleDisconnected;
                    _client.Error -= HandleError;
                    _client.InstrumentUpdated -= HandleInstrumentUpdate;
                    _client.SymbolSearchUpdated -= HandleSearchUpdate;
                    _client.TopSymbolsUpdated -= HandleTopSymbolsUpdate;
                    _client.PerceptionUpdated -= HandlePerceptionUpdate;
                    _client.CommitmentUpdated -= HandleCommitmentUpdate;
                    _client.BookPressureUpdated -= HandleBookPressureUpdate;
                    _client.HeadroomUpdated -= HandleHeadroomUpdate;
                    _client.SentimentUpdated -= HandleSentimentUpdate;
                }

                _client = value;

                if (_client is not null)
                {
                    _client.Connected += HandleConnected;
                    _client.Disconnected += HandleDisconnected;
                    _client.Error += HandleError;
                    _client.InstrumentUpdated += HandleInstrumentUpdate;
                    _client.SymbolSearchUpdated += HandleSearchUpdate;
                    _client.TopSymbolsUpdated += HandleTopSymbolsUpdate;
                    _client.PerceptionUpdated += HandlePerceptionUpdate;
                    _client.CommitmentUpdated += HandleCommitmentUpdate;
                    _client.BookPressureUpdated += HandleBookPressureUpdate;
                    _client.HeadroomUpdated += HandleHeadroomUpdate;
                    _client.SentimentUpdated += HandleSentimentUpdate;
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void SubscribeSearch()
        {
            Client?.SubscribeTopSymbols("ib");
        }

        private void Subscribe(string symbol)
        {
            if (Client is null)
                return;

            // Unsubscribe from all subscriptions for this symbol.
            Client.UnsubscribeAll(_symbol);

            _symbol = symbol;

            Client.RequestInstrumentDetails(_symbol);
            Client.SubscribePerception(_symbol);
            Client.SubscribeCommitment(_symbol);
            Client.SubscribeEquilibrium(_symbol, "300s");
            Client.SubscribeSentiment(_symbol, "50t");
            Client.SubscribeHeadroom(_symbol);
            Client.SubscribeBookPressure(_symbol);
            Client.SubscribeMultiframeEquilibrium(_symbol);
            Client.SubscribeTrigger(_symbol);
            Client.SubscribeStrategy(_strategyId, _symbol);
        }

        private void HandleInstrumentUpdate(object? sender, InstrumentEventArgs e)
        {
            Trace.TraceInformation(e.Symbol + " " + (e.Details?.InstrumentType.ToString() ?? "None") +
                                   " " + (e.Details?.ExpiryDate.ToString() ?? string.Empty) +
                                   " " + e.Error?.Message ?? string.Empty);
        }

        private void HandleError(object? client, API.Signals.Events.ErrorEventArgs args)
        {
            Trace.TraceInformation("Error! " + args.Message);
        }

        private void HandleDisconnected(object? client, EventArgs args)
        {
            Trace.TraceInformation("Disconnected!");
        }

        private void HandleConnected(object? client, EventArgs args)
        {
            Trace.TraceInformation("Connected!");
        }

        private void HandleConnectClicked(object? client, EventArgs args)
        {
            Client = new APIClient(new ConnectionToken(Environments.Development,
                                                       eUsername.Text, ePassword.Text),
                                   stream: DataStream.Realtime);

            Client.Connect();

            SubscribeSearch();
            Subscribe(_symbol);
        }

        private void HandleDisconnectClicked(object client, EventArgs args)
        {
            Client?.Disconnect();
        }

        private void HandleSearchInput(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(eSearch.Text))
            {
                Client?.SearchSymbols(eSearch.Text, "paper");
            }
            else if (_topSymbols is not null)
            {
                HandleTopSymbolsUpdate(this, _topSymbols);
            }
        }

        private void HandleTopSymbolsUpdate(object? sender, TopSymbolsEventArgs topSymbols)
        {
            _topSymbols = topSymbols;
            if (!string.IsNullOrEmpty(eSearch.Text))
                return;

            List<SearchRow> searchRows = [];

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

        private void HandleSearchUpdate(object? sender, SearchResultsEventArgs e)
        {
            if (string.IsNullOrEmpty(eSearch.Text))
                return;

            List<SearchRow> searchRows = [];

            foreach (SearchResult result in e.Results)
                searchRows.Add(new SearchRow
                {
                    Symbol = result.Symbol,
                    DisplayName = result.DisplayName,
                    EntryProgress = string.Empty
                });

            colSearch.ItemsSource = searchRows;
        }

        private void HandleSentimentUpdate(object? sender, SentimentEventArgs e)
        {
            s50t.UpdateSpectrum(e);
        }

        private void HandleHeadroomUpdate(object? sender, HeadroomEventArgs e)
        {
            sgHeadroom.Value = e.Value;
        }

        private void HandleBookPressureUpdate(object? sender, BookPressureEventArgs e)
        {
            sgBookPressure.Value = e.Value;
        }

        private void HandleCommitmentUpdate(object? sender, CommitmentEventArgs e)
        {
            sgCommitment.Value = e.Value;
        }

        private void HandlePerceptionUpdate(object? sender, PerceptionEventArgs e)
        {
            sgPerception.Value = e.Value;
        }
    }
}
