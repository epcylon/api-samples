using QuantGate.API;
using QuantGate.API.Utilities;
using QuantGate.API.Values;
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
            _client = new APIClient("wss://test.stealthtrader.com", streamID: ParsedDestination.DemoStreamID);
            //_client = new ProtoStompClient("ws://localhost", 2432);
            _client.Connected += HandleConnected;
            _client.Disconnected += HandleDisconnected;
            _client.Error += HandleError;
            _client.OnHeartbeat += HandleHeartbeat;

            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                            "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyNjk3NjExMiwiZXhwIjoxNjI4MTIx" +
                            "NjAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9." +
                            "DoeYRaAnK15I4LscisTHJm72zOqJhc1zKqbexP9vLro");
        }

        private void HandleSearchUpdate(object sender, TextChangedEventArgs e)
        {
            _symbolSearch.Search(txtSearch.Text, "paper");
        }

        private void HandleHeartbeat(APIClient client)
        {
            Console.WriteLine("Heartbeat");
        }

        private void HandleError(APIClient client, string message)
        {
            Console.WriteLine("Error! " + message);
        }

        private void HandleDisconnected(APIClient client)
        {
            Console.WriteLine("Disconnected!");
        }

        private void HandleConnected(APIClient client)
        {
            Console.WriteLine("Connected!");

            Dispatcher.Invoke(delegate
            {
                string symbol = "NQ U1";
                
                Perception = _client.SubscribePerception(symbol);
                Commitment = _client.SubscribeCommitment(symbol);
                Headroom = _client.SubscribeHeadroom(symbol);
                BookPressure = _client.SubscribeBookPressure(symbol);
                Sentiment = _client.SubscribeSentiment(symbol, "50t");
                Equilibrium = _client.SubscribeEquilibrium(symbol, "300s");
                MultiFrame = _client.SubscribeMultiframeEquilibrium(symbol);
                Strategy = _client.SubscribeStrategy("Crb9.0", symbol);
                _symbolSearch = _client.SubscribeSearch();
                _symbolSearch.Update += HandleSearchUpdate;
            });
        }

        private void HandleSearchUpdate(object sender, SearchUpdateEventArgs e)
        {
            lvSearch.Items.Clear();

            foreach (var x in e.Results)
                lvSearch.Items.Add(x.Symbol);
        }
    }
}
