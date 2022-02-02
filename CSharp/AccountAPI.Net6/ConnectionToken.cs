using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace Epcylon.Net.APIs.Account
{
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
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        #endregion

        #region Private Member Variables

        /// <summary>
        /// Lock object for public fields.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// The host address of the REST API to request from.
        /// </summary>
        private readonly string _restHost;
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
        /// Refresh timer.
        /// </summary>
        private readonly Timer _timer;

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
        public ConnectionToken(Environments environment, string username, string password)
        {
            // Set the environment and REST host address.
            Environment = environment;
            _restHost = GetRestHost(environment);

            // Set the username.
            Username = username;

            // Form the URI to retrieve the token for.
            string uri = _restHost + "auth/credentials?UserName=" + username +
                                     "&Password=" + password + "&format=json";

            // Get the tokens for the user.
            GetTokens(Get(uri), out string token, out string refreshToken, out DateTime tokenExpiry);
            _token = token;
            _refreshToken = refreshToken;
            _tokenExpiry = tokenExpiry;

            // Get the username from the token.
            ClientID = GetJSONField(GetJWTPayload(_token), "sub");

            // Create the refresh timer.
            _timer = new Timer(HandleTimer, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Creates a new ConnectionToken object. Token will not refresh.
        /// </summary>
        /// <param name="environment">The environment to connect to.</param>
        /// <param name="token">The token to connect with.</param>
        public ConnectionToken(Environments environment, string token) : 
            this(environment, string.Empty, token, string.Empty) { }

        /// <summary>
        /// Creates a new ConnectionToken object. Token will automatically refresh if username and refresh token are included.
        /// </summary>
        /// <param name="environment">The environment to connect to.</param>
        /// <param name="username">The username to connect with.</param>
        /// <param name="token">The token to connect with.</param>
        /// <param name="refreshToken">The token used to refresh the connection.</param>
        public ConnectionToken(Environments environment, string username, string token, string refreshToken)
        {
            // Set the environment and REST host address.
            Environment = environment;
            _restHost = GetRestHost(environment);

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

        #endregion

        #region Refresh Timer Handling

        /// <summary>
        /// Timer event handler - will refresh the token, if possible.
        /// </summary>
        /// <param name="state">Timer state object (not used).</param>
        private void HandleTimer(object state)
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
                        uri = _restHost + "auth/refresh?UserName=" + Username +
                                          "&RefreshToken=" + _refreshToken + "&format=json";
                    }
                    
                    // Refresh the tokens (retrieve and handle tokens).
                    GetTokens(Get(uri), out string token, out string refreshToken, out DateTime tokenExpiry);

                    // Set the tokens and expiry.
                    lock (_lock)
                    {
                        _token = token;
                        _refreshToken = refreshToken;
                        _tokenExpiry = tokenExpiry;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Tmr - " + ex.Message);
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
            switch (environment)
            {
                case Environments.Local: return @"http://localhost:59398/";
                case Environments.Development: return @"https://mdev.pilottrading.co/";
                case Environments.Production: return @"https://mercury.pilottrading.co/";
                default: return @"https://mstage.pilottrading.co/";
            }
        }

        /// <summary>
        /// Used to retrieve a value from the REST API.
        /// </summary>
        /// <param name="uri">The URI to get from (including paramters).</param>
        /// <returns>The string retrieved from the REST endpoint.</returns>
        private static string Get(string uri)
        {            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage result = client.GetAsync(uri).Result;
                    return result.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cn - " + ex.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the JWT bearer and refresh tokens from the JSON response message supplied.
        /// </summary>
        /// <param name="jsonResponse">The message to retrieve the tokens from.</param>
        private static void GetTokens(string jsonResponse, out string token, out string refreshToken, out DateTime tokenExpiry)
        {
            try
            {
                // Get the token and refresh token from the result.
                token = GetJSONField(jsonResponse, "BearerToken");
                refreshToken = GetJSONField(jsonResponse, "RefreshToken");
                tokenExpiry = GetExpiry(token);
            }
            catch (Exception ex)
            {
                token = string.Empty;
                refreshToken = string.Empty;
                tokenExpiry = default;
                Trace.TraceError(_moduleID + ":UdTkns - " + ex.Message);
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
                // Calculate the token expiry.
                if (long.TryParse(GetJSONField(GetJWTPayload(token), "exp"), out long expiry))
                    return _epoch.AddSeconds(expiry);
                else
                    return DateTime.UtcNow.AddHours(12); 
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":GExp - " + ex.Message);
                return default;
            }
        }

        /// <summary>
        /// Retrieves a JSON field from a JSON string.
        /// </summary>
        /// <param name="source">The source string to retrieve the field from.</param>
        /// <param name="field">The name of the field to retrieve.</param>
        /// <returns>The field value associated with the field name.</returns>
        private static string GetJSONField(string source, string field) =>
            source.Substring(0, source.Length - 1).Split(new string[] { "\"" + field + "\":" },
                StringSplitOptions.None)[1].Replace("\"", "").Split(new char[] { ',' })[0];

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
        private static string GetJWTPayload(string jwtToken) =>
            System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(PadBase64String(jwtToken.Split(new char[] { '.' })[1])));

        #endregion
    }
}
