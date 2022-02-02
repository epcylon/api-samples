using Epcylon.Net.APIs.Account;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using System;
using System.Threading;

namespace QuantGate.Examples.ConsoleStrategy
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
                                      "eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz" +
                                      "A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0." +
                                      "xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8";

        /// <summary>
        /// The strategy ID to use.
        /// </summary>
        private const string _strategyID = "Crb7.6";

        /// <summary>
        /// Used to wait for Ctrl-C events.
        /// </summary>
        private static readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);

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
                _api = new APIClient(new ConnectionToken(Environments.Staging, _token));
                _api.StrategyUpdated += _api_StrategyUpdated;

                // Connect to the API using a valid token.
                _api.Connect();

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
