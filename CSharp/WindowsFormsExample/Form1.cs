﻿using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsFormsExample
{
    public partial class Form1 : Form
    {
        private readonly APIClient _client;
        private TopSymbolsEventArgs _topSymbols;
        private string _symbol = "NQ Z1";
        private readonly string _strategyId = "Crb9.0";

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
            _client.SymbolSearchUpdated += HandleSearchUpdate;
            _client.TopSymbolsUpdated += HandleTopSymbolsUpdate;

            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                            "eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz" +
                            "A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0." +
                            "xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8");

            SubscribeSearch();
            Subscribe(_symbol);
        }

        private void Subscribe(string symbol)
        {
            // Unsubscribe from all subscriptions for this symbol.
            _client.UnsubscribeAll(_symbol);
            //sViewer.ClearSpectrum();

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

        private void SubscribeSearch()
        {
            _client.SubscribeTopSymbols("ib");
        }

        private void HandleSearchTextUpdate(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                _client.SearchSymbols(txtSearch.Text, "paper");
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

        private void HandleSearchUpdate(object sender, SearchResultsEventArgs e)
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
