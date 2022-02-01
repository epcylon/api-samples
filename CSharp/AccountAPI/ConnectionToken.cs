using System.Diagnostics;

namespace Epcylon.Net.APIs.Account
{
    internal class ConnectionToken : IDisposable
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "CnTkn";  

        private readonly Timer _timer;

        private bool _isDisposed;

        /// <summary>
        /// The host address of the REST API to request from.
        /// </summary>
        private readonly string _restHost; 

        /// <summary>
        /// Lock object for public fields.
        /// </summary>
        private object _lock = new object();
        private string _username;
        public Environments Environment { get; }
        /// <summary>
        /// The currently connected client id.
        /// </summary>
        public string ClientID { get; }
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
        /// Epoch time for calculating UNIX time.
        /// </summary>
        private readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public string Token 
        {
            get
            {
                lock (_lock)
                    return _token;
            }
            set
            {
                lock (_lock)
                    _token = value;                
            }
        }

        public static ConnectionToken Login(Environments environment, string username, string password)
        {
            string restHost = GetRestHost(environment);

            // Form the URI to retrieve the token for.
            string uri = restHost + "auth/credentials?UserName=" + username +
                                    "&Password=" + password + "&format=json";

            UpdateTokens(Get(uri), out string token, out string refreshToken);
            return new ConnectionToken(environment, username, token, refreshToken);
        }

        public static ConnectionToken LoginByToken(Environments environment, string token)
        {
            return new ConnectionToken(environment, string.Empty, token, string.Empty);
        }        

        private ConnectionToken(Environments environment, string username, string token, string refreshToken)
        {
            Environment = environment;
            _restHost = GetRestHost(environment);
            _username = username;
            _token = token;
            _refreshToken = refreshToken;
            _tokenExpiry = GetExpiry(token);

            // Get the username from the token.
            ClientID = GetJSONField(GetJWTPayload(token), "sub");

            _timer = new Timer(HandleTimer, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));            
        }

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

        private void HandleTimer(object? state)
        {
            long utcTicks;

            try
            {
                // Can't refresh if no username or token.
                if (_isDisposed || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_refreshToken))
                    return;

                // Get the current time.
                utcTicks = DateTime.UtcNow.Ticks;

                if (utcTicks >= _tokenExpiry.Ticks)
                {                    
                    // If past the expiry of the token, refresh the token.
                    string uri;

                    // Form the URI to retrieve the token for.
                    uri = _restHost + "auth/refresh?UserName=" + _username +
                                      "&RefreshToken=" + _refreshToken + "&format=json";
                    
                    // Refresh the tokens (retrieve and handle tokens).
                    UpdateTokens(Get(uri), out string token, out string refreshToken);

                    // Set the tokens and expiry.
                    lock (_lock)
                    {
                        _token = token;
                        _refreshToken = refreshToken;
                        _tokenExpiry = GetExpiry(token);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Tmr - " + ex.Message);
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
                using HttpClient client = new();
                HttpResponseMessage? result = client.GetAsync(uri).Result;                
                return result.Content.ReadAsStringAsync().Result;              
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cn - " + ex.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// Updates the JWT bearer and refresh tokens from the JSON response message supplied.
        /// </summary>
        /// <param name="jsonResponse">The message to retrieve the tokens from.</param>
        private static void UpdateTokens(string jsonResponse, out string token, out string refreshToken)
        {
            try
            {
                // Get the token and refresh token from the result.
                token = GetJSONField(jsonResponse, "BearerToken");
                refreshToken = GetJSONField(jsonResponse, "RefreshToken");
            }
            catch (Exception ex)
            {
                token = string.Empty;
                refreshToken = string.Empty;
                Trace.TraceError(_moduleID + ":UdTkns - " + ex.Message);
            }
        }

        private DateTime GetExpiry(string token)
        {
            DateTime tokenExpiry;

            try
            {                
                // Calculate the token expiry.
                if (long.TryParse(GetJSONField(GetJWTPayload(token), "exp"), out long expiry))
                    tokenExpiry = _epoch.AddSeconds(expiry);
                else
                    tokenExpiry = DateTime.UtcNow.AddHours(12);

                // Move back 10%.
                return tokenExpiry.AddTicks((long)((DateTime.UtcNow.Ticks - tokenExpiry.Ticks) * 0.10));
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

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }
                
                _isDisposed = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ConnectionToken()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
