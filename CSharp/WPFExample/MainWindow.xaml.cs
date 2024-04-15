using Epcylon.Net.APIs.Account;
using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace QuantGate.WPFExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private APIClient? _client;
        private TopSymbolsEventArgs? _topSymbols;
        private string _symbol = "NQ M4";
        private readonly string _strategyId = "Crb9.0";

        #region Client Property

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

        #endregion

        #region Dependency Properties

        public SentimentEventArgs Sentiment
        {
            get { return (SentimentEventArgs)GetValue(SentimentProperty); }
            set { SetValue(SentimentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sentiment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SentimentProperty =
            DependencyProperty.Register("Sentiment", typeof(SentimentEventArgs), typeof(MainWindow), new PropertyMetadata(null));

        public double Perception
        {
            get { return (double)GetValue(PerceptionProperty); }
            set { SetValue(PerceptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sentiment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PerceptionProperty =
            DependencyProperty.Register("Perception", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        public double Commitment
        {
            get { return (double)GetValue(CommitmentProperty); }
            set { SetValue(CommitmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Commitment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommitmentProperty =
            DependencyProperty.Register("Commitment", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        public double BookPressure
        {
            get { return (double)GetValue(BookPressureProperty); }
            set { SetValue(BookPressureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BookPressure.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookPressureProperty =
            DependencyProperty.Register("BookPressure", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        public double Headroom
        {
            get { return (double)GetValue(HeadroomProperty); }
            set { SetValue(HeadroomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Headroom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeadroomProperty =
            DependencyProperty.Register("Headroom", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            txtSearch.TextChanged += HandleSearchUpdate;
        }         

        private void HandleFuturesListUpdated(object? sender, FuturesListEventArgs e)
        {
            Console.WriteLine("Futures list for " + e.Underlying + "/" + e.Currency + " Count=" + e.Futures.Count);
        }

        private void HandleInstrumentUpdate(object? sender, InstrumentEventArgs e)
        {
            if (e.Details is not null)
                Console.WriteLine(e.Symbol + " " + e.Details.InstrumentType.ToString() + " " +
                                  e.Details.ExpiryDate.ToString() + " " + e.Error?.Message);
        }

        private void Subscribe(string symbol)
        {
            if (_client is null)
                return;

            // Unsubscribe from all subscriptions for this symbol.
            _client.UnsubscribeAll(_symbol);
            sViewer.ClearSpectrum();

            _symbol = symbol;

            _client.RequestInstrumentDetails(_symbol);
            _client.SubscribePerception(_symbol, 100);
            _client.SubscribeCommitment(_symbol, 100);
            _client.SubscribeEquilibrium(_symbol, "300s", 100);
            _client.SubscribeSentiment(_symbol, "50t");
            _client.SubscribeHeadroom(_symbol);
            _client.SubscribeBookPressure(_symbol);
            _client.SubscribeMultiframeEquilibrium(_symbol);
            _client.SubscribeTrigger(_symbol);
            _client.SubscribeStrategy(_strategyId, _symbol);
        }

        private void SubscribeSearch()
        {
            Client?.SubscribeTopSymbols("paper");
        }

        private void HandleSearchUpdate(object? sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                Client?.SearchSymbols(txtSearch.Text, "paper");
            }
            else if (_topSymbols is not null)
            {
                HandleTopSymbolsUpdate(this, _topSymbols);
            }
        }

        private void HandleError(object? client, ErrorEventArgs args)
        {
            Debug.Print("Error! " + args.Message);
        }

        private void HandleDisconnected(object? client, EventArgs args)
        {
            Debug.Print("Disconnected!");
        }

        private void HandleConnected(object? client, EventArgs args)
        {
            Debug.Print("Connected!");
        }

        private void HandleTopSymbolsUpdate(object? sender, TopSymbolsEventArgs topSymbols)
        {
            _topSymbols = topSymbols;
            if (!string.IsNullOrEmpty(txtSearch.Text))
                return;

            lvSearch.Items.Clear();

            if (_topSymbols?.Symbols is object)
                foreach (TopSymbol symbol in _topSymbols.Symbols)
                    lvSearch.Items.Add(new SearchRow
                    {
                        Symbol = symbol.Symbol,
                        DisplayName = symbol.DisplayName,
                        EntryProgress = symbol.EntryProgress.ToString("p1")
                    });
        }

        private void HandleHeadroomUpdate(object? sender, HeadroomEventArgs e)
        {
            Headroom = e.Value;
        }

        private void HandleBookPressureUpdate(object? sender, BookPressureEventArgs e)
        {
            BookPressure = e.Value;
        }

        private void HandleCommitmentUpdate(object? sender, CommitmentEventArgs e)
        {
            Commitment = e.Value;
        }

        private void HandlePerceptionUpdate(object? sender, PerceptionEventArgs e)
        {
            Perception = e.Value;
        }

        private void HandleSentimentUpdate(object? sender, SentimentEventArgs e)
        {
            Sentiment = e;
        }

        private void HandleSearchUpdate(object? sender, SearchResultsEventArgs e)
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

        private void HandleSubscribeMenuClick(object? sender, RoutedEventArgs e)
        {
            if (lvSearch.SelectedItem is SearchRow row)
                Subscribe(row.Symbol);
        }

        private void HandleConnectClick(object sender, RoutedEventArgs e)
        {
            Client = new APIClient(new ConnectionToken(Environments.Staging, txtUsername.Text, txtPassword.Password),
                                   DataStream.Realtime, System.Threading.SynchronizationContext.Current);

            Client.Connect();

            SubscribeSearch();
            Subscribe(_symbol);
        }

        private void HandleDisconnectClick(object sender, RoutedEventArgs e)
        {
            Client?.Disconnect();
        }
    }
}
