using Epcylon.Net.APIs.Account;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System.Diagnostics;

namespace QuantGate.WindowsFormsExample
{
    public partial class Form1 : Form
    {
        private APIClient? _client;
        private TopSymbolsEventArgs? _topSymbols;
        private string _symbol = "NQ F3";
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
                    _client.SymbolSearchUpdated -= HandleSearchUpdate;
                    _client.TopSymbolsUpdated -= HandleTopSymbolsUpdate;
                    _client.PerceptionUpdated -= HandlePerceptionUpdate;
                    _client.Disconnect();
                    _client.Dispose();
                }

                _client = value;

                if (_client is not null)
                {
                    _client.Connected += HandleConnected;
                    _client.Disconnected += HandleDisconnected;
                    _client.Error += HandleError;
                    _client.SymbolSearchUpdated += HandleSearchUpdate;
                    _client.TopSymbolsUpdated += HandleTopSymbolsUpdate;
                    _client.PerceptionUpdated += HandlePerceptionUpdate;
                }
            }
        }

        public Form1()
        {
            InitializeComponent();

            txtSearch.TextChanged += HandleSearchTextUpdate;
        }

        private void Subscribe(string symbol)
        {
            if (_client is null)
                return;

            // Unsubscribe from all subscriptions for this symbol.
            _client.UnsubscribeAll(_symbol);

            _symbol = symbol;

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

        private void HandlePerceptionUpdate(object? sender, PerceptionEventArgs e)
        {
            Debug.Print("Perception updated: " + e.Symbol + "  " + e.Value);
        }

        private void SubscribeSearch()
        {
            _client?.SubscribeTopSymbols("ib");
        }

        private void HandleSearchTextUpdate(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                _client?.SearchSymbols(txtSearch.Text, "paper");
            }
            else if (_topSymbols is not null)
            {
                HandleTopSymbolsUpdate(this, _topSymbols);
            }
        }

        private void HandleError(object? client, API.Events.ErrorEventArgs args)
        {
            Console.WriteLine("Error! " + args.Message);
        }

        private void HandleDisconnected(object? client, EventArgs args)
        {
            Console.WriteLine("Disconnected!");
        }

        private void HandleConnected(object? client, EventArgs args)
        {
            Console.WriteLine("Connected!");
        }

        private void HandleTopSymbolsUpdate(object? sender, TopSymbolsEventArgs topSymbols)
        {
            _topSymbols = topSymbols;
            if (!string.IsNullOrEmpty(txtSearch.Text))
                return;

            lvSearch.Items.Clear();

            foreach (TopSymbol symbol in _topSymbols.Symbols)
                lvSearch.Items.Add(new ListViewItem(new string[]
                {
                    symbol.Symbol,
                    symbol.DisplayName,
                    symbol.EntryProgress.ToString("p1")
                }));
        }

        private void HandleSearchUpdate(object? sender, SearchResultsEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            lvSearch.Items.Clear();

            foreach (SearchResult result in e.Results)
            {
                lvSearch.Items.Add(new ListViewItem(new string[]
                {
                    result.Symbol,
                    result.DisplayName,
                    string.Empty
                }));
            }
        }

        private void HandleConnectClicked(object? client, EventArgs args)
        {
            Client = new APIClient(new ConnectionToken(Environments.Staging,
                                                       txtUsername.Text, txtPassword.Text),
                                   stream: DataStream.Realtime);

            Client.Connect();

            SubscribeSearch();
            Subscribe(_symbol);
        }

        private void HandleDisconnectClicked(object client, EventArgs args)
        {
            Client?.Disconnect();
            Client?.Dispose();
        }
    }
}