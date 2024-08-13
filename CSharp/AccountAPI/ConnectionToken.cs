using Epcylon.Common.Logging;

namespace Epcylon.APIs.Account;

/// <summary>
/// Holds the token information for a connection and keeps the token up to date.
/// </summary>
public class ConnectionToken : IDisposable
{
    #region Constants

    /// <summary>
    /// Module-level Identifier.
    /// </summary>
    private const string _moduleID = "CnTkn";

    /// <summary>
    /// Epoch time for calculating UNIX time.
    /// </summary>
    private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    #endregion

    #region Static Product Conversion

    /// <summary>
    /// Dictionary of product id strings by enumeration.
    /// </summary>
    private static readonly Dictionary<Products, string> _products = new()
    {
        [Products.Pilot] = "pilot",
        [Products.CoPilot] = "copilot",
        [Products.Stealth] = "stealth",
        [Products.StealthPro] = "stealthpro",
        [Products.Hydra] = "hydra",
        [Products.InvestGuide] = "investguide",
    };

    /// <summary>
    /// Dictionary of products by product id string.
    /// </summary>
    private static readonly Dictionary<string, Products> _productsReverse =
        _products.ToDictionary(p => p.Value, p => p.Key);

    /// <summary>
    /// Dictionary of environment strings by enumeration.
    /// </summary>
    private static readonly Dictionary<Environments, string> _environments = new()
    {
        [Environments.Production] = "Production",
        [Environments.Staging] = "Staging",
        [Environments.Development] = "Development",
        [Environments.Local] = "Local",
    };

    /// <summary>
    /// Dictionary of environments by environment strings.
    /// </summary>
    private static readonly Dictionary<string, Environments> _environmentsReverse =
        _environments.ToDictionary(p => p.Value, p => p.Key);

    /// <summary>
    /// Returns the product id string from the product.
    /// </summary>
    /// <param name="product">The product to get the id for.</param>
    /// <returns>The product id string from the product</returns>
    public static string ProductIDByProduct(Products product)
    {
        if (_products.TryGetValue(product, out string? id))
            return id;

        return "none";
    }

    /// <summary>
    /// Returns the product type for the product id string.
    /// </summary>
    /// <param name="id">The product id to get the product for.</param>
    /// <returns>The product type for the product id string.</returns>
    public static Products ProductByProductID(string id)
    {
        if (_productsReverse.TryGetValue(id.Trim().ToLower(), out Products product))
            return product;

        return Products.None;
    }

    /// <summary>
    /// Returns the environment id string from the environment.
    /// </summary>
    /// <param name="environment">The environment to get the id for.</param>
    /// <returns>The environment id string from the environment</returns>
    public static string ConvertEnvironment(Environments environment)
    {
        if (_environments.TryGetValue(environment, out string? enumValue))
            return enumValue;

        return "Production";
    }

    /// <summary>
    /// Returns the environment type for the environment id string.
    /// </summary>
    /// <param name="environment">The environment id to get the environment for.</param>
    /// <returns>The environment type for the environment id string.</returns>
    public static Environments ConvertEnvironment(string environment)
    {
        environment = environment.Trim().ToLower();
        environment = char.ToUpper(environment[0]) + environment[1..];

        if (_environmentsReverse.TryGetValue(environment, out Environments enumValue))
            return enumValue;

        return Environments.Production;
    }

    #endregion

    #region Private Member Variables

    /// <summary>
    /// Lock object for public fields.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// The password for the current connection (JWT Token).
    /// </summary>
    private string _token;
    /// <summary>
    /// The expiry time of the token (when to refresh).
    /// </summary>
    private DateTime _tokenExpiry;
    /// <summary>
    /// The token used to refresh the connection.
    /// </summary>
    private string _refreshToken;
    /// <summary>
    /// Whether the login is for trial purposes only.
    /// </summary>
    private bool _trial = false;
    /// <summary>
    /// Error message, if an error occured during login.
    /// </summary>
    private string _loginMessage = string.Empty;

    /// <summary>
    /// Refresh timer.
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// The HTTP client object to use for requests.
    /// </summary>
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// True if the object has been disposed.
    /// </summary>
    private bool _isDisposed;

    #endregion

    #region Initialization / Finalization

    /// <summary>
    /// Creates a new ConnectionToken object. Token will automatically refresh.
    /// </summary>
    /// <param name="environment">The environment to connect to.</param>
    /// <param name="username">The username to connect with.</param>
    /// <param name="password">The password to connect with.</param>
    /// <param name="product">The type of product we are connecting to.</param>
    public ConnectionToken(Environments environment, string username,
                           string password, Products product = Products.Pilot)
    {
        string uri, productId;

        // Set the environment and REST host address.
        Environment = environment;
        Product = product;
        RestHost = GetRestHost(environment);

        // Set the username.
        Username = username;

        // Get the product ID for the product - default is pilot.
        productId = ProductIDByProduct(product);

        // Form the URI to retrieve the token for.
        if ((product & Products.Stealth) == Products.Stealth)
            uri = $"{RestHost}stealth/auth/login?email={username}&password={password}&product={productId}&format=json";
        else
            uri = $"{RestHost}auth/credentials?UserName={username}&Password={password}&format=json";

        // Get the tokens for the user.
        GetTokens(Get(uri), out string token, out string refreshToken,
                  out DateTime tokenExpiry, out bool trial, out string message);

        _token = token;
        _refreshToken = refreshToken;
        _tokenExpiry = tokenExpiry;
        _trial = trial;
        _loginMessage = message;

        // Get the username from the token.
        if (!string.IsNullOrEmpty(_token))
            ClientID = GetJSONField(GetJWTPayload(_token), "sub");
        else
            ClientID = string.Empty;

        // Create the refresh timer.
        _timer = new Timer(HandleTimer, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Creates a new ConnectionToken object. Token will not refresh.
    /// </summary>
    /// <param name="environment">The environment to connect to.</param>
    /// <param name="token">The token to connect with.</param>
    /// <param name="product">The type of product we are connecting to.</param>
    public ConnectionToken(Environments environment, string token, Products product = Products.Pilot) :
        this(environment, string.Empty, token, string.Empty, product)
    { }

    /// <summary>
    /// Creates a new ConnectionToken object. Token will automatically refresh if username and refresh token are included.
    /// </summary>
    /// <param name="environment">The environment to connect to.</param>
    /// <param name="username">The username to connect with.</param>
    /// <param name="token">The token to connect with.</param>
    /// <param name="refreshToken">The token used to refresh the connection.</param>
    /// <param name="product">The type of product we are connecting to.</param>
    public ConnectionToken(Environments environment, string username, string token,
                           string refreshToken, Products product = Products.Pilot)
    {
        // Set the environment and REST host address.
        Environment = environment;
        Product = product;
        RestHost = GetRestHost(environment);

        // Set the username.
        Username = username;
        _token = token;
        _refreshToken = refreshToken;
        _tokenExpiry = GetExpiry(token);

        // Get the username from the token.
        ClientID = GetJSONField(GetJWTPayload(token), "sub");

        // Create the refresh timer.
        _timer = new Timer(HandleTimer, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Disposes the object and clears the timer.
    /// </summary>
    /// <param name="disposing">Called from the Dispose method?</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            // If not yet disposed, dispose of the timer.
            _timer.Dispose();
            _httpClient.Dispose();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Disposes of the token when the object is no longer in use (stop timer).
    /// </summary>
    ~ConnectionToken()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The environment to connect to.
    /// </summary>
    public Environments Environment { get; }

    /// <summary>
    /// The product that the token is for.
    /// </summary>
    public Products Product { get; }

    /// <summary>
    /// The host address of the REST API to request from.
    /// </summary>
    public string RestHost { get; }

    /// <summary>
    /// The username of the user that created the connection (usually email).
    /// </summary>
    private string Username { get; }

    /// <summary>
    /// The currently connected client id.
    /// </summary>
    public string ClientID { get; }

    /// <summary>
    /// Returns the current active JWT token.
    /// </summary>
    public string Token
    {
        get
        {
            lock (_lock)
                return _token;
        }
    }

    /// <summary>
    /// Returns the current active refresh JWT token.
    /// </summary>
    public string RefreshToken
    {
        get
        {
            lock (_lock)
                return _refreshToken;
        }
    }

    /// <summary>
    /// Returns the expiry date of the current active JWT token.
    /// </summary>
    public DateTime TokenExpiry
    {
        get
        {
            lock (_lock)
                return _tokenExpiry;
        }
    }

    /// <summary>
    /// Whether the login is for trial purposes only.
    /// </summary>
    public bool Trial
    {
        get
        {
            lock (_lock)
                return _trial;
        }
    }

    /// <summary>
    /// Login message - success or error message as a result of the last login attempt.
    /// </summary>
    public string LoginMessage
    {
        get
        {
            lock (_lock)
                return _loginMessage;
        }
    }

    #endregion

    #region Refresh Timer Handling

    /// <summary>
    /// Timer event handler - will refresh the token, if possible.
    /// </summary>
    /// <param name="state">Timer state object (not used).</param>
    private void HandleTimer(object? state)
    {
        long utcTicks;

        try
        {
            // Can't refresh if no username or token.
            if (_isDisposed || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(_refreshToken))
                return;

            // Get the current time.
            utcTicks = DateTime.UtcNow.Ticks;

            if (utcTicks + TimeSpan.TicksPerHour >= TokenExpiry.Ticks)
            {
                // If within one hour of the expiry of the token, refresh the token.
                string uri;

                lock (_lock)
                {
                    // Form the URI to retrieve the token for.
                    uri = RestHost + "auth/refresh?UserName=" + Username +
                                     "&RefreshToken=" + _refreshToken + "&format=json";
                }

                // Refresh the tokens (retrieve and handle tokens).
                GetTokens(Get(uri), out string token, out string refreshToken,
                          out DateTime tokenExpiry, out bool trial, out string errorMessage);

                // Set the tokens and expiry.
                lock (_lock)
                {
                    _token = token;
                    _refreshToken = refreshToken;
                    _tokenExpiry = tokenExpiry;
                    _trial = trial;
                    _loginMessage = errorMessage;
                }
            }
        }
        catch (Exception ex)
        {
            SharedLogger.LogException(_moduleID + ":Tmr", ex);
            lock (_lock)
                _loginMessage = ex.Message;
        }
    }

    #endregion

    #region Utility Functions

    /// <summary>
    /// Returns the correct REST host address for the given environment.
    /// </summary>
    /// <param name="environment">The environment to get the REST host address for.</param>
    /// <returns>The REST host address for the given environment.</returns>
    private static string GetRestHost(Environments environment)
    {
        return environment switch
        {
            Environments.Local => @"http://localhost:59398/",
            Environments.Development => @"https://mdev.pilottrading.co/",
            Environments.Production => @"https://mercury.pilottrading.co/",
            _ => @"https://mstage.pilottrading.co/",
        };
    }

    /// <summary>
    /// Used to retrieve a value from the REST API.
    /// </summary>
    /// <param name="uri">The URI to get from (including paramters).</param>
    /// <returns>The string retrieved from the REST endpoint.</returns>
    private string Get(string uri)
    {
        try
        {
            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            SharedLogger.LogException(_moduleID + ":Cn", ex);
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the JWT bearer and refresh tokens from the JSON response message supplied.
    /// </summary>
    /// <param name="jsonResponse">The message to retrieve the tokens from.</param>
    private static void GetTokens(string jsonResponse, out string token, out string refreshToken,
                                  out DateTime tokenExpiry, out bool trial, out string message)
    {
        token = string.Empty;
        refreshToken = string.Empty;
        tokenExpiry = default;
        trial = false;

        try
        {
            // Get the token and refresh token from the result.
            if (jsonResponse.Contains("BearerToken"))
            {
                token = GetJSONField(jsonResponse, "BearerToken");
                refreshToken = GetJSONField(jsonResponse, "RefreshToken");
                message = GetJSONField(jsonResponse, "Message");
                tokenExpiry = GetExpiry(token);
            }
            else if (jsonResponse.Contains("jwt"))
            {
                token = GetJSONField(jsonResponse, "jwt");
                refreshToken = GetJSONField(jsonResponse, "jwtr");
                tokenExpiry = GetExpiry(token);
                _ = bool.TryParse(GetJSONField(jsonResponse, "trial"), out trial);
                message = GetJSONField(jsonResponse, "message");
            }
            else if (jsonResponse.Contains("Message"))
            {
                message = GetJSONField(jsonResponse, "Message");
            }
            else
            {
                message = "Error during login.";
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
            SharedLogger.LogException(_moduleID + ":UdTkns", ex);
        }
    }

    /// <summary>
    /// Gets the expiry date from the given token.
    /// </summary>
    /// <param name="token">The token to get the expiry date for.</param>
    /// <returns>The expiry date for the given token.</returns>
    private static DateTime GetExpiry(string token)
    {
        try
        {
            // If no token, just return default.
            if (string.IsNullOrEmpty(token))
                return default;

            // Calculate the token expiry.
            if (long.TryParse(GetJSONField(GetJWTPayload(token), "exp"), out long expiry))
                return _epoch.AddSeconds(expiry);
            else
                return DateTime.UtcNow.AddHours(12);
        }
        catch (Exception ex)
        {
            SharedLogger.LogException(_moduleID + ":GExp", ex);
            return default;
        }
    }

    /// <summary>
    /// Retrieves a JSON field from a JSON string.
    /// </summary>
    /// <param name="source">The source string to retrieve the field from.</param>
    /// <param name="field">The name of the field to retrieve.</param>
    /// <returns>The field value associated with the field name.</returns>
    private static string GetJSONField(string source, string field)
    {
        string key = "\"" + field + "\":";
        string result;

        if (!source.Contains(key))
            return string.Empty;

        result = source[..^1].Split(new string[] { key },
            StringSplitOptions.None)[1].Replace("\"", "").Split(new char[] { ',' })[0];

        if (result == "null")
            return string.Empty;

        return result;
    }


    /// <summary>
    /// Pads a base-64 string, as necessary to decode.
    /// </summary>
    /// <param name="base64">The unpadded string.</param>
    /// <returns>The padded base-64 string.</returns>
    private static string PadBase64String(string base64) =>
        base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

    /// <summary>
    /// Gets the JWT payload JSON string from the JWT token supplied.
    /// </summary>
    /// <param name="jwtToken">The token to retrieve the payload from.</param>
    /// <returns>The payload of the JWT string as a JSON string.</returns>
    private static string GetJWTPayload(string jwtToken)
    {
        try
        {
            return System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(PadBase64String(jwtToken.Split(new char[] { '.' })[1])));
        }
        catch (Exception ex)
        {
            SharedLogger.LogException(_moduleID + ":GJWTP", ex);
            return string.Empty;
        }
    }

    #endregion
}
