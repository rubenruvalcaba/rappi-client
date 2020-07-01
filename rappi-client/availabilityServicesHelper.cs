using rappi.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace rappi_client
{
    public class availabilityServicesHelper
    {

        #region Fields 

        private string _availabilityServicesLoginUrl;
        private string _bearerToken;
        private string _clientId;
        private string _clientSecret;
        private string _audience;
        private DateTime _bearerExpires;

        #endregion

        #region Initialization

        public availabilityServicesHelper(string clientId,
                                          string clientSecret,
                                          string audience,
                                          string availabilityServicesLoginUrl)
        {

            _clientId = clientId;
            _clientSecret = clientSecret;
            _audience = audience;
            _availabilityServicesLoginUrl = availabilityServicesLoginUrl;

        }

        #endregion

        #region Login

        // Requests and returns the bearer token for API interaction
        private string GetBearerToken()
        {
            // If doesn't have a bearer yet or it's expired, gets one
            if (string.IsNullOrEmpty(_bearerToken) || _bearerExpires <= DateTime.Now)
            {
                _bearerToken = Login().Result;
                _bearerExpires = DateTime.Now.AddMinutes(30); // Token expiration 30 minutes
            }

            return "Bearer "+ _bearerToken;
        }

        private async Task<string> Login()
        {

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
            var httpResponse = await client.PostAsync(new Uri(_availabilityServicesLoginUrl),
                                                      new StringContent(body, Encoding.UTF8, "application/json"));
            if (!httpResponse.IsSuccessStatusCode)
                throw new ApplicationException("Error Logging into Availability Services: " + httpResponse.StatusCode);

            string jsonString = await httpResponse.Content.ReadAsStringAsync();

            // Gets the token from the response
            if (jsonString != "[]")
            {
                var response = System.Text.Json.JsonSerializer.Deserialize(
                                    jsonString, typeof(AvailabilityServicesLoginResponse)) as AvailabilityServicesLoginResponse;
                return response.access_token;
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
        public async void StoreAvailability(StoreAvailabilityRequest storeAvailabilityRequest)
        {

            // Builds the request
            string body = JsonSerializer.Serialize(storeAvailabilityRequest);

            // Post the request
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-authorization", GetBearerToken());
            var httpResponse = await client.PutAsync(new Uri(_audience + "/availability/stores"), new StringContent(body, Encoding.UTF8, "application/json"));
            if (!httpResponse.IsSuccessStatusCode)
                throw new ApplicationException("Error setting Stores Availability: " + httpResponse.StatusCode);

        }

        #endregion

    }
}
