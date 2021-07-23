using BridgeRock.CSharpExample.API;
using BridgeRock.CSharpExample.API.Subscriptions;
using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using System;
using System.Windows;

namespace BridgeRock.CSharpExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProtoStompClient _client;
        private PerceptionSubscription _perceptionSubscription;
        private CommitmentSubscription _commitmentSubscription;
        private HeadroomSubscription _headroomSubscription;
        private BookPressureSubscription _bookPressureSubscription;
        private SentimentSubscription _sentimentSubscription;

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


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            //_client = new ProtoStompClient("wss://feed.stealthtrader.com");
            _client = new ProtoStompClient("wss://test.stealthtrader.com");
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

        private void HandleHeartbeat(ProtoStompClient client)
        {
            Console.WriteLine("Heartbeat");
        }

        private void HandleError(ProtoStompClient client, string message)
        {
            Console.WriteLine("Error! " + message);
        }

        private void HandleDisconnected(ProtoStompClient client)
        {
            Console.WriteLine("Disconnected!");
        }

        private void HandleConnected(ProtoStompClient client)
        {
            Console.WriteLine("Connected!");

            Dispatcher.Invoke(delegate
            {
                string symbol = "NQ U1";
                string streamId = ParsedDestination.DelayStreamID;

                _perceptionSubscription = new PerceptionSubscription(_client, streamId, symbol);
                Perception = _perceptionSubscription.Values;
                _perceptionSubscription.Subscribe();

                _commitmentSubscription = new CommitmentSubscription(_client, streamId, symbol);
                Commitment = _commitmentSubscription.Values;
                _commitmentSubscription.Subscribe();

                _headroomSubscription = new HeadroomSubscription(_client, streamId, symbol);
                Headroom = _headroomSubscription.Values;
                _headroomSubscription.Subscribe();

                _bookPressureSubscription = new BookPressureSubscription(_client, streamId, symbol);
                BookPressure = _bookPressureSubscription.Values;
                _bookPressureSubscription.Subscribe();

                _sentimentSubscription = new SentimentSubscription(_client, streamId, symbol, "50t");
                Sentiment = _sentimentSubscription.Values;
                _sentimentSubscription.Subscribe();
            });
        }
    }
}
