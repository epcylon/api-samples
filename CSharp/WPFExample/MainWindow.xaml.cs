using QuantGate.API.Events;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Values;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BridgeRock.CSharpExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly APIClient _client;
        private SymbolSearch _symbolSearch;
        private TopSymbolsEventArgs _topSymbols;
        private string _symbol = "NQ U1";
        private string _strategyId = "Crb9.0";

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

            //_client = new ProtoStompClient("wss://feed.stealthtrader.com");
            _client = new APIClient("wss://test.stealthtrader.com", stream: DataStream.Realtime);
            //_client = new ProtoStompClient("ws://localhost", 2432);
            _client.Connected += HandleConnected;
            _client.Disconnected += HandleDisconnected;
            _client.Error += HandleError;

            _client.InstrumentUpdated += HandleInstrumentUpdate;
            _client.TopSymbolsUpdated += HandleTopSymbolsUpdate;
            _client.PerceptionUpdated += (s, e) => Perception = e.Value;
            _client.CommitmentUpdated += (s, e) => Commitment = e.Value;
            _client.BookPressureUpdated += (s, e) => BookPressure = e.Value;
            _client.HeadroomUpdated += (s, e) => Headroom = e.Value;
            _client.SentimentUpdated += (s, e) => Sentiment = e;

            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                            "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw" +
                            "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9." +
                            "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");

            SubscribeSearch();
            Subscribe(_symbol);
        }

        private void _client_SentimentUpdated(object sender, SentimentEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void HandleInstrumentUpdate(object sender, InstrumentEventArgs e)
        {
            Console.WriteLine(e.Symbol + " " + e.InstrumentType.ToString() + " " +
                              e.ExpiryDate.ToString() + " " + e.ErrorMessage);
        }

        private void Subscribe(string symbol)
        {
            Unsubscribe();

            _symbol = symbol;

            _client.SubscribeInstrument(_symbol);
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
            sViewer.ClearSpectrum();
        }

        private void SubscribeSearch()
        {
            _symbolSearch = _client.SubscribeSearch();
            _symbolSearch.Updated += HandleSearchUpdate;
            _client.SubscribeTopSymbols("ib");
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

        private void HandleTopSymbolsUpdate(object sender, TopSymbolsEventArgs topSymbols)
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _client.UnsubscribePerception(_symbol);
        }
    }
}
