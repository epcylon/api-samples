using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Values;
using System;
using System.Windows;
using System.Windows.Controls;
using Trigger = QuantGate.API.Signals.Values.Trigger;

namespace BridgeRock.CSharpExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly APIClient _client;
        private SymbolSearch _symbolSearch;
        private Subscription<TopSymbols> _topSymbolsStream;
        private TopSymbols _topSymbols;

        #region Dependency Properties

        public Subscription<Perception> Perception { get; set; }
        public Subscription<Commitment> Commitment { get; set; }
        public Subscription<Headroom> Headroom { get; set; }
        public Subscription<BookPressure> BookPressure { get; set; }
        public Subscription<Sentiment> Sentiment { get; set; }
        public Subscription<Equilibrium> Equilibrium { get; set; }
        public Subscription<MultiframeEquilibrium> MultiFrame { get; set; }
        public Subscription<Trigger> Trigger { get; set; }
        public Subscription<StrategyValues> Strategy { get; set; }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            txtSearch.TextChanged += HandleSearchUpdate;

            //_client = new ProtoStompClient("wss://feed.stealthtrader.com");
            _client = new APIClient("wss://test.stealthtrader.com", stream: DataStream.Realtime);
            //_client = new ProtoStompClient("ws://localhost", 2432);
            _client.Connected += HandleConnected;
            _client.Disconnected += HandleDisconnected;
            _client.Error += HandleError;

            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                            "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw" +
                            "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9." +
                            "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");

            SubscribeSearch();
            Subscribe("NQ U1");
        }

        private void Subscribe(string symbol)
        {
            Unsubscribe();

            Perception = _client.SubscribePerception(symbol);
            Commitment = _client.SubscribeCommitment(symbol);
            Equilibrium = _client.SubscribeEquilibrium(symbol, "300s");
            Sentiment = _client.SubscribeSentiment(symbol, "50t");
            Headroom = _client.SubscribeHeadroom(symbol);
            BookPressure = _client.SubscribeBookPressure(symbol);
            MultiFrame = _client.SubscribeMultiframeEquilibrium(symbol);
            Trigger = _client.SubscribeTrigger(symbol);
            Strategy = _client.SubscribeStrategy("Crb9.0", symbol);
        }

        private void Unsubscribe()
        {
            if (Perception is object)
            {
                _client.Unsubscribe(Perception);
                _client.Unsubscribe(Commitment);
                _client.Unsubscribe(Equilibrium);
                _client.Unsubscribe(Sentiment);
                _client.Unsubscribe(Headroom);
                _client.Unsubscribe(BookPressure);
                _client.Unsubscribe(MultiFrame);
                _client.Unsubscribe(Trigger);
                _client.Unsubscribe(Strategy);
                sViewer.ClearSpectrum();
            }
        }

        private void SubscribeSearch()
        {
            _symbolSearch = _client.SubscribeSearch();
            _symbolSearch.Updated += HandleSearchUpdate;
            _topSymbolsStream = _client.SubscribeTopSymbols("ib");
            _topSymbolsStream.Updated += HandleTopSymbolsUpdate;
        }

        private void HandleSearchUpdate(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                _symbolSearch.Search(txtSearch.Text, "paper");
            }
            else
            {
                HandleTopSymbolsUpdate(this, _topSymbols);
            }
        }        

        private void HandleError(object client, ErrorEventArgs args)
        {
            Console.WriteLine("Error! " + args.Message);
        }

        private void HandleDisconnected(object client, EventArgs args)
        {
            Console.WriteLine("Disconnected!");
        }

        private void HandleConnected(object client, EventArgs args)
        {
            Console.WriteLine("Connected!");
        }

        private void HandleTopSymbolsUpdate(object sender, TopSymbols topSymbols)
        {
            _topSymbols = topSymbols;
            if (!string.IsNullOrEmpty(txtSearch.Text))
                return;

            lvSearch.Items.Clear();

            foreach (TopSymbol symbol in _topSymbols.Symbols)
                lvSearch.Items.Add(new SearchRow
                {
                    Symbol = symbol.Symbol,
                    DisplayName = symbol.DisplayName,
                    EntryProgress = symbol.EntryProgress.ToString("p1")
                });
        }

        private void HandleSearchUpdate(object sender, SearchUpdateEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            lvSearch.Items.Clear();

            foreach (SearchResult result in e.Results)
                lvSearch.Items.Add(new SearchRow
                {
                    Symbol = result.Symbol,
                    DisplayName = result.DisplayName,
                    EntryProgress = string.Empty
                });
        }

        private void HandleSubscribeMenuClick(object sender, RoutedEventArgs e)
        {
            if (lvSearch.SelectedItem is SearchRow row)
                Subscribe(row.Symbol);
        }
    }
}
