using QuantGate.API.Signals;
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
        private APIClient _client;
        private SymbolSearch _symbolSearch;
        private TopSymbols _topSymbols;

        public Perception Perception
        {
            get => (Perception)GetValue(PerceptionProperty);
            set => SetValue(PerceptionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Perception.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PerceptionProperty =
            DependencyProperty.Register("Perception", typeof(Perception), typeof(MainWindow), new PropertyMetadata(null));


        public Commitment Commitment
        {
            get => (Commitment)GetValue(CommitmentProperty);
            set => SetValue(CommitmentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Commitment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommitmentProperty =
            DependencyProperty.Register("Commitment", typeof(Commitment), typeof(MainWindow), new PropertyMetadata(null));


        public Headroom Headroom
        {
            get => (Headroom)GetValue(HeadroomProperty);
            set => SetValue(HeadroomProperty, value);
        }

        // Using a DependencyProperty as the backing store for Headroom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeadroomProperty =
            DependencyProperty.Register("Headroom", typeof(Headroom), typeof(MainWindow), new PropertyMetadata(null));


        public BookPressure BookPressure
        {
            get => (BookPressure)GetValue(BookPressureProperty);
            set => SetValue(BookPressureProperty, value);
        }

        // Using a DependencyProperty as the backing store for BookPressure.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookPressureProperty =
            DependencyProperty.Register("BookPressure", typeof(BookPressure), typeof(MainWindow), new PropertyMetadata(null));


        public Sentiment Sentiment
        {
            get => (Sentiment)GetValue(SentimentProperty);
            set => SetValue(SentimentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Sentiment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SentimentProperty =
            DependencyProperty.Register("Sentiment", typeof(Sentiment), typeof(MainWindow), new PropertyMetadata(null));


        public Equilibrium Equilibrium
        {
            get { return (Equilibrium)GetValue(EquilibriumProperty); }
            set { SetValue(EquilibriumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Equilibrium.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EquilibriumProperty =
            DependencyProperty.Register("Equilibrium", typeof(Equilibrium), typeof(MainWindow), new PropertyMetadata(null));


        public MultiframeEquilibrium MultiFrame
        {
            get { return (MultiframeEquilibrium)GetValue(MultiFrameProperty); }
            set { SetValue(MultiFrameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MultiFrame.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MultiFrameProperty =
            DependencyProperty.Register("MultiFrame", typeof(MultiframeEquilibrium), typeof(MainWindow), new PropertyMetadata(null));


        public QuantGate.API.Signals.Values.Trigger Trigger
        {
            get { return (QuantGate.API.Signals.Values.Trigger)GetValue(TriggerProperty); }
            set { SetValue(TriggerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Trigger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerProperty =
            DependencyProperty.Register("Trigger", typeof(QuantGate.API.Signals.Values.Trigger), typeof(MainWindow), new PropertyMetadata(null));


        public StrategyValues Strategy
        {
            get { return (StrategyValues)GetValue(StrategyProperty); }
            set { SetValue(StrategyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Strategy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrategyProperty =
            DependencyProperty.Register("Strategy", typeof(StrategyValues), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            txtSearch.TextChanged += HandleSearchUpdate;

            //_client = new ProtoStompClient("wss://feed.stealthtrader.com");
            _client = new APIClient("wss://test.stealthtrader.com", stream: DataStream.Delayed);
            //_client = new ProtoStompClient("ws://localhost", 2432);
            _client.Connected += HandleConnected;
            _client.Disconnected += HandleDisconnected;
            _client.Error += HandleError;

            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYwMDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9.Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA");

            //Subscribe("QGSI-OTC");
            Subscribe("NQ U1");
        }

        private void Subscribe(string symbol)
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
                _client.Unsubscribe(_topSymbols);
                _symbolSearch.Dispose();
            }

            Perception = _client.SubscribePerception(symbol);
            Commitment = _client.SubscribeCommitment(symbol);
            Equilibrium = _client.SubscribeEquilibrium(symbol, "300s");
            Sentiment = _client.SubscribeSentiment(symbol, "50t");
            Headroom = _client.SubscribeHeadroom(symbol);
            BookPressure = _client.SubscribeBookPressure(symbol);
            MultiFrame = _client.SubscribeMultiframeEquilibrium(symbol);
            Trigger = _client.SubscribeTrigger(symbol);
            Strategy = _client.SubscribeStrategy("Crb9.0", symbol);
            _symbolSearch = _client.SubscribeSearch();
            _symbolSearch.Updated += HandleSearchUpdate;
            _topSymbols = _client.SubscribeTopSymbols("ib");
            _topSymbols.Updated += HandleTopSymbolsUpdate;            
        }

        private void HandleSearchUpdate(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                _symbolSearch.Search(txtSearch.Text, "paper");
            }
            else
            {
                HandleTopSymbolsUpdate(_topSymbols, EventArgs.Empty);
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

        private void HandleTopSymbolsUpdate(object sender, EventArgs e)
        {
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
    }
}
