﻿using QuantGate.API.Events;
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
        private Subscription<TopSymbolsEventArgs> _topSymbolsStream;
        private TopSymbolsEventArgs _topSymbols;

        #region Dependency Properties

        public Subscription<PerceptionEventArgs> Perception { get; set; }
        public Subscription<CommitmentEventArgs> Commitment { get; set; }
        public Subscription<HeadroomEventArgs> Headroom { get; set; }
        public Subscription<BookPressureEventArgs> BookPressure { get; set; }
        public Subscription<SentimentEventArgs> Sentiment { get; set; }
        public Subscription<EquilibriumEventArgs> Equilibrium { get; set; }
        public Subscription<MultiframeEquilibriumEventArgs> MultiFrame { get; set; }
        public Subscription<TriggerEventArgs> Trigger { get; set; }
        public Subscription<StrategyEventArgs> Strategy { get; set; }

        #endregion

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

        private void _client_PerceptionUpdated(object sender, PerceptionEventArgs e)
        {
            Debug.Print("Perception updated: " + e.Symbol + "  " + e.Value);
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
                //sViewer.ClearSpectrum();
            }
        }

        private void SubscribeSearch()
        {
            _symbolSearch = _client.SubscribeSearch();
            _symbolSearch.Updated += HandleSearchUpdate;
            _topSymbolsStream = _client.SubscribeTopSymbols("ib");
            _topSymbolsStream.Updated += HandleTopSymbolsUpdate;
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