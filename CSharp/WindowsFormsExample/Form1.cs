using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Values;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsFormsExample
{
    public partial class Form1 : Form
    {
        private readonly APIClient _client;
        private SymbolSearch _symbolSearch;
        private TopSymbolsEventArgs _topSymbols;
        private string _symbol = "NQ U1";
        private string _strategyId = "Crb9.0";

        public Form1()
        {
            InitializeComponent();

            //DataContext = this;

            txtSearch.TextChanged += HandleSearchTextUpdate;

            //_client = new ProtoStompClient("wss://feed.stealthtrader.com");
            _client = new APIClient("wss://test.stealthtrader.com", stream: DataStream.Demo);
            //_client = new ProtoStompClient("ws://localhost", 2432);
            _client.Connected += HandleConnected;
            _client.Disconnected += HandleDisconnected;
            _client.Error += HandleError;

            _client.PerceptionUpdated += _client_PerceptionUpdated;
            _client.TopSymbolsUpdated += HandleTopSymbolsUpdate;

            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                            "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw" +
                            "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9." +
                            "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");

            SubscribeSearch();
            Subscribe(_symbol);
        }

        private void Subscribe(string symbol)
        {
            Unsubscribe();

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

        private void _client_PerceptionUpdated(object sender, PerceptionEventArgs e)
        {
            Debug.Print("Perception updated: " + e.Symbol + "  " + e.Value);
        }

        private void Unsubscribe()
        {            
            _client.UnsubscribePerception(_symbol);
            _client.UnsubscribeCommitment(_symbol);
            _client.UnsubscribeEquilibrium(_symbol, "300s");
            _client.UnsubscribeSentiment(_symbol, "50t");
            _client.UnsubscribeHeadroom(_symbol);
            _client.UnsubscribeBookPressure(_symbol);
            _client.UnsubscribeMultiframeEquilibrium(_symbol);
            _client.UnsubscribeTrigger(_symbol);
            _client.UnsubscribeStrategy(_strategyId, _symbol);
            //sViewer.ClearSpectrum();            
        }

        private void SubscribeSearch()
        {
            _symbolSearch = _client.SubscribeSearch();
            _symbolSearch.Updated += HandleSearchUpdate;
            _client.SubscribeTopSymbols("ib");
        }

        private void HandleSearchTextUpdate(object sender, EventArgs e)
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

        private void HandleTopSymbolsUpdate(object sender, TopSymbolsEventArgs topSymbols)
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

        private void HandleSearchUpdate(object sender, SearchUpdateEventArgs e)
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

        //private void HandleSubscribeMenuClick(object sender, RoutedEventArgs e)
        //{
        //    if (lvSearch.SelectedItem is SearchRow row)
        //        Subscribe(row.Symbol);
        //}
    }
}
