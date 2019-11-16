using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using rappi.Models;

namespace rappi
{

    public class rappiHelper
    {

        #region " Fields "
        private string _rappiURL = "";

        #endregion

        #region " Initalization "

        /// <summary>
        /// Initializes the rappi helper
        /// </summary>
        /// <param name="token">token for authentication. Given by Rappi</param>
        /// <param name="rappiURL">URL for the rest api. For testing: http://microservices.dev.rappi.com/api/restaurants-integrations-public-api</param>
        public rappiHelper(string token, string rappiURL)
        {
            _token = token;
            _rappiURL = rappiURL;
        }

        #endregion

        #region " Authentication "

        private string _token;
        private string _bearerToken;
        private DateTime _bearerExpires;

        // Requests and returns the bearer token for API interaction
        private string GetBearerToken()
        {
            // If doesn't have a bearer yet or it's expired, gets one
            if (string.IsNullOrEmpty(_bearerToken) || _bearerExpires <= DateTime.Now)
            {
                _bearerToken = Login().Result;
                _bearerExpires = DateTime.Now.AddMinutes(30); // Token expiration 30 minutes
            }

            return _bearerToken;
        }

        // Logs in the API and returns the bearer token
        private async Task<string> Login()
        {

            // Builds the request string     
            string json = JsonSerializer.Serialize(new { token = _token });

            // Do the login
            var client = new HttpClient();
            var httpResponse = await client.PostAsync(new Uri(_rappiURL + "/login"),
                                                      new StringContent(json));
            if (httpResponse.IsSuccessStatusCode)
            {
                // Get the bearer from the header attributes
                IEnumerable<string> values;
                if (httpResponse.Headers.TryGetValues("X-Auth-Int", out values))
                    return values.FirstOrDefault();
                else
                    throw new ApplicationException("No bearer token received");
            }
            else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new ApplicationException("Invalid token");
            }
            else
            {
                throw new ApplicationException("Login error: " + httpResponse.StatusCode);
            }
        }

        #endregion

        #region " Orders "

        /// <summary>
        /// Returns pending orders from Rappi server
        /// </summary>
        /// <returns></returns>
        public async Task<List<RootObject>> GetOrders()
        {

            List<RootObject> orders = new List<RootObject>();

            // Get pending orders
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Int", GetBearerToken());
            var httpResponse = await client.GetAsync(new Uri(_rappiURL + "/orders"));
            if (!httpResponse.IsSuccessStatusCode)
                throw new ApplicationException("Error getting orders: " + httpResponse.StatusCode);

            string jsonString = await httpResponse.Content.ReadAsStringAsync();

            if (jsonString != "[]")
            {
                orders = System.Text.Json.JsonSerializer.Deserialize(
                                    jsonString, typeof(List<RootObject>)) as List<RootObject>;
            }

            return orders;

        }

        /// <summary>
        /// Takes the order
        /// </summary>
        /// <param name="orderId">Order Id to be taken</param>
        /// <returns>True if the process is completed sucessfuly</returns>
        public async Task<bool> TakeOrder(string orderId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Int", GetBearerToken());
            var httpResponse = await client.GetAsync(new Uri(_rappiURL + "/orders/take/" + orderId));
            if (!httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var jsonString = await httpResponse.Content.ReadAsStringAsync();
                    var orderError = System.Text.Json.JsonSerializer.Deserialize(
                                    jsonString, typeof(OrderError)) as OrderError;
                    throw new OrderErrorException("Error taking the order", orderError);
                }
                else
                {
                    throw new Exception("Error taking the order " + orderId);
                }
            }

            return true;
        }

        /// <summary>
        /// Rejects an order
        /// </summary>
        /// <param name="orderId">Order id to be rejected</param>
        /// <param name="reason">Reason phrase for rejection</param>
        /// <returns>True if process completed succesfuly</returns>
        public async Task<bool> RejectOrder(string orderId, string reason)
        {
            string json = JsonSerializer.Serialize(new { order_id = orderId, reason = reason });

            // Invoca al endpoint /reject
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Int", GetBearerToken());
            var httpResponse = await client.PostAsync(new Uri(_rappiURL + "/orders/reject"),
                                                      new StringContent(json));
            if (!httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var jsonString = await httpResponse.Content.ReadAsStringAsync();
                    var orderError = System.Text.Json.JsonSerializer.Deserialize(
                                    jsonString, typeof(OrderError)) as OrderError;
                    throw new OrderErrorException("Error rejecting the order", orderError);
                }
                else
                {
                    throw new Exception("Error rejecting the order " + orderId);
                }
            }

            return true;
        }

        #endregion

    }
}