using rappi.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace rappi_client
{
    public class AvailabilityServicesHelper
    {

        #region Fields 

        private string _availabilityServicesLoginUrl;
        private string _bearerToken;
        private string _clientId;
        private string _clientSecret;
        private string _audience;
        private DateTime _bearerExpires;
        readonly ILogger<AvailabilityServicesHelper> _logger;

        #endregion

        #region Events

        public class LoggedInEventArgs : EventArgs
        {
            public DateTime Expires { get; set; }
        }

        public event EventHandler<LoggedInEventArgs> LoggedIn;

        #endregion

        #region Initialization

        public AvailabilityServicesHelper(string clientId,
                                          string clientSecret,
                                          string audience,
                                          string availabilityServicesLoginUrl,
                                          ILogger<AvailabilityServicesHelper> logger)
        {
            _logger = logger;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _audience = audience;
            _availabilityServicesLoginUrl = availabilityServicesLoginUrl;

            _logger.LogDebug("Initializing AvailabilityServicesHelper");
            _logger.LogDebug($"Client Id: {clientId}");
            _logger.LogDebug($"Client Secret: {clientSecret}");
            _logger.LogDebug($"Audience: {audience}");
            _logger.LogDebug($"LoginUrl: {availabilityServicesLoginUrl}");

        }

        #endregion

        #region Login

        // Requests and returns the bearer token for API interaction
        private string GetBearerToken()
        {
            // If doesn't have a bearer yet or it's expired, gets one
            if (string.IsNullOrEmpty(_bearerToken) || _bearerExpires <= DateTime.Now)
            {
                Login();
            }

            return "Bearer " + _bearerToken;
        }

        private void Login()
        {

            _logger.LogDebug("Logging in ...");

            // Builds the request
            string body = JsonSerializer.Serialize(new
            {
                client_id = _clientId,
                client_secret = _clientSecret,
                audience = _audience,
                grant_type = "client_credentials"
            });

            // Do the login
            var client = new HttpClient();
            var httpResponse = client.PostAsync(new Uri(_availabilityServicesLoginUrl),
                                                      new StringContent(body, Encoding.UTF8, "application/json")).Result;
            if (!httpResponse.IsSuccessStatusCode)
            {
                var message = $"Error Logging into Availability Services. StatusCode: {httpResponse.StatusCode} ";
                _logger.LogError(message);
                _logger.LogDebug("Login request: " + body);
                throw new ApplicationException(message);
            }

            string jsonString = httpResponse.Content.ReadAsStringAsync().Result;

            _logger.LogDebug("Login Response: " + jsonString);

            // Gets the token from the response
            if (jsonString != "[]")
            {
                var response = System.Text.Json.JsonSerializer.Deserialize(
                                    jsonString, typeof(AvailabilityServicesLoginResponse)) as AvailabilityServicesLoginResponse;

                _bearerToken = response.access_token;
                _bearerExpires = DateTime.Now.AddSeconds(response.expires_in);

                EventHandler<LoggedInEventArgs> handler = LoggedIn;
                if (handler != null)
                    handler(this, new LoggedInEventArgs() { Expires = _bearerExpires });

            }
            else
                throw new ApplicationException("No access token received");

        }

        #endregion

        #region Store Availability

        public class StoreAvailabilityRequest
        {
            /// <summary>
            /// Array with the stores id to be turned on</param>
            /// </summary>
            public List<string> turn_on { get; set; } = new List<string>();
            /// <summary>
            /// Array with the stores id to be turned off
            /// </summary>
            public List<string> turn_off { get; set; } = new List<string>();
        }

        /// <summary>
        /// Turns on and/or off stores
        /// </summary>
        public async void StoreAvailability(StoreAvailabilityRequest request)
        {

            // Builds the request
            string body = JsonSerializer.Serialize(request);

            // Post the request
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-authorization", GetBearerToken());
            var httpResponse = await client.PutAsync(new Uri(_audience + "/availability/stores"), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!httpResponse.IsSuccessStatusCode)
                throw new ApplicationException("Error setting Stores Availability: " + httpResponse.StatusCode);

        }

        #endregion

        #region Items Availability

        public class ItemAvailabilityRequest
        {
            public string store_integration_id { get; set; }
            public ItemsOnOff items { get; set; } = new ItemsOnOff();

            public class ItemsOnOff
            {
                /// <summary>
                /// Array with the stores id to be turned on</param>
                /// </summary>
                public List<string> turn_on { get; set; } = new List<string>();
                /// <summary>
                /// Array with the stores id to be turned off
                /// </summary>
                public List<string> turn_off { get; set; } = new List<string>();
            }

        }

        public async Task ItemsAvailability(List<ItemAvailabilityRequest> request)
        {
            // Builds the request
            string body = JsonSerializer.Serialize(request);

            _logger.LogDebug("ItemsAvailability request body:" + body);

            // Post the request
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-authorization", GetBearerToken());
            var httpResponse = await client.PutAsync(new Uri(_audience + "/availability/stores/items"), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!httpResponse.IsSuccessStatusCode)
            {
                var message = $"Error setting Items availability: {httpResponse.StatusCode}";
                _logger.LogError(message);
                _logger.LogError($"Body:{body}");
                throw new ApplicationException();
            }


            _logger.LogDebug($"ItemsAvailability response Success StatusCode: {httpResponse.StatusCode}" );

        }

        #endregion

    }
}
