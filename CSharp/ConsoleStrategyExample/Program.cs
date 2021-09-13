using QuantGate.API.Signals;
using QuantGate.API.Signals.Values;
using System;
using System.Threading;

namespace ConsoleStrategyExample
{
    /// <summary>
    /// Main program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The host to connect to.
        /// </summary>
        private const string _host = "wss://test.stealthtrader.com";

        /// <summary>
        /// The JWT token to connect with.
        /// </summary>
        private const string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                                      "eyJzdWIiOiJKb2huSCIsImlhdCI6MTYyODcxMzg2NywiZXhwIjoxNjMyOTYw" +
                                      "MDAwLCJhdWQiOiIyV1VqZW9iUlhSVzlwc05ERWN4ZTFNRDl3dGRmZGgxQyJ9." +
                                      "Up48upDkCINp9znyjTkUXc0F2Rb5BWqfzmumF4mUcXA";

        /// <summary>
        /// The strategy ID to use.
        /// </summary>
        private const string _strategyID = "Crb7.6";

        /// <summary>
        /// Used to wait for Ctrl-C events.
        /// </summary>
        private static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        /// <summary>
        /// The main server instance.
        /// </summary>
        private static APIClient _api;

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static void Main(string[] args)
        {
            Startup();

            try
            {
                // Set up the Ctrl+C exit.
                Console.TreatControlCAsInput = false;
                Console.CancelKeyPress += (s, e) =>
                {
                    _quitEvent.Set();
                    e.Cancel = true;
                };

                // Wait until the user attempted to quit the program.
                _quitEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception (Main): " + ex.ToString());
            }

            Shutdown();
        }

        /// <summary>
        /// Starts up the program - initializes all the services.
        /// </summary>
        static void Startup()
        {
            string[] symbols = new string[]
            {
                "USD.CAD", "AUD.CAD", "AUD.USD", "CAD.JPY", "CHF.JPY", "EUR.AUD", "EUR.GBP", "EUR.NOK",
                "EUR.USD", "GBP.CAD", "GBP.JPY", "NZD.USD", "USD.CAD", "USD.JPY", "USD.SEK", "CCM X1-B3",
                "DOL V1-B3", "IND V1-B3", "BGI V1-B3", "WDO V1-B3", "WIN V1-B3"
            };

            try
            {
                Console.WriteLine("Starting up.");

                // Create the API client, and subscribe to the updated event.
                _api = new APIClient(_host, stream: DataStream.Realtime);
                _api.StrategyUpdated += _api_StrategyUpdated;

                // Connect to the API using a valid token.
                _api.Connect(_token);

                // Subscribe to the strategy for each symbol.
                foreach (string symbol in symbols)
                    _api.SubscribeStrategy(_strategyID, symbol);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception (Startup): " + ex.ToString());
            }
        }

        /// <summary>
        /// Handles any strategy update messages.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Strategy event arguments (strategy update values).</param>
        private static void _api_StrategyUpdated(object sender, StrategyEventArgs e)
        {
            Console.WriteLine(e.Symbol + ", " + e.EntryProgress.ToString("P2") + ", " + e.Signal.ToString());
        }

        /// <summary>
        /// Shuts down the program.
        /// </summary>
        static void Shutdown()
        {
            try
            {
                // Inform that we are shutting down.
                Console.WriteLine("Shutting down.");

                // Shut down all the services.
                _api.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception (Shutdown): " + ex.ToString());
            }
        }
    }
}
