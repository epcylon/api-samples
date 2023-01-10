// See https://aka.ms/new-console-template for more information
using Epcylon.Net.APIs.Account;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;

/// <summary>
/// The strategy ID to use.
/// </summary>
const string _strategyID = "Crb7.6";

/// <summary>
/// Used to wait for Ctrl-C events.
/// </summary>
ManualResetEvent quitEvent = new(false);

/// <summary>
/// The main server instance.
/// </summary>
APIClient? _api = null;

try
{
    // Symbols to listen for.
    string[] symbols = new string[]
    {
        "USD.CAD", "AUD.CAD", "AUD.USD", "CAD.JPY", "CHF.JPY", "EUR.AUD", "EUR.GBP", "EUR.NOK",
        "EUR.USD", "GBP.CAD", "GBP.JPY", "NZD.USD", "USD.CAD", "USD.JPY", "USD.SEK", "CCM X1-B3",
        "DOL V1-B3", "IND V1-B3", "BGI V1-B3", "WDO V1-B3", "WIN V1-B3"
    };
    
    Console.WriteLine("Starting up.");

    Console.Write("Username: ");
    string? username = Console.ReadLine();
    Console.Write("Password: ");
    string? password = Console.ReadLine();


    // Create the API client, and subscribe to the updated event.
    _api = new APIClient(new ConnectionToken(Environments.Staging, username, password));
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

try
{
    // Set up the Ctrl+C exit.
    Console.TreatControlCAsInput = false;
    Console.CancelKeyPress += (s, e) =>
    {
        quitEvent.Set();
        e.Cancel = true;
    };

    // Wait until the user attempted to quit the program.
    quitEvent.WaitOne();
}
catch (Exception ex)
{
    Console.WriteLine("Exception (Main): " + ex.ToString());
}

/// <summary>
/// Shuts down the program.
/// </summary>
try
{
    // Inform that we are shutting down.
    Console.WriteLine("Shutting down.");

    // Shut down all the services.
    _api?.Dispose();
}
catch (Exception ex)
{
    Console.WriteLine("Exception (Shutdown): " + ex.ToString());
}

/// <summary>
/// Handles any strategy update messages.
/// </summary>
/// <param name="sender">Event source.</param>
/// <param name="e">Strategy event arguments (strategy update values).</param>
void _api_StrategyUpdated(object? sender, StrategyEventArgs e)
{
    Console.WriteLine(e.Symbol + ", " + e.EntryProgress.ToString("P2") + ", " + e.Signal.ToString());
}