using System;
using System.Net.Http;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using rappi.Models;
using rappi_client;
using System.Drawing;

namespace rappi
{
    class Program
    {
        static void Main(string[] args)
        {

            string option = "";
            while (option != "X")
            {
                Console.WriteLine("1 - Get orders and cancellations");
                Console.WriteLine("2 - Set store availability");
                Console.WriteLine("3 - Items availability");
                Console.WriteLine("X - Exit");
                option = Console.ReadLine().ToUpper();
                switch (option)
                {
                    case "1":
                        Orders();
                        break;
                    case "2":
                        StoreAvailability();
                        break;
                    case "3":
                        ItemsAvailability();
                        break;
                }
            }

        }

        #region Orders 
        private static void Orders()
        {
            // Ask for the access token
            Console.WriteLine("Enter your token:");
            var token = Console.ReadLine();

            // Uses the given token and test URL
            rappiHelper helper = new rappiHelper(token,
                        "http://microservices.dev.rappi.com/api/restaurants-integrations-public-api");

            // Loop until the user press X key and checks for new orders every 5 seconds
            do
            {

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("--- Looking for new orders ... Press X to exit ---");
                Console.WriteLine("--- Press 'C' to check for cancelled orders");

                // Check for new orders each 5 seconds
                Thread.Sleep(5000);

                // If the user press X key, then exit loop
                if (Console.KeyAvailable)
                    if (Console.ReadKey(true).Key == ConsoleKey.X)
                        break;
                    else if (Console.ReadKey(true).Key == ConsoleKey.C)
                        CheckForCanceledOrders(helper);

                // Get the orders
                try
                {
                    ProcessOrders(helper);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                }

            } while (true);
        }

        private static void CheckForCanceledOrders(rappiHelper helper)
        {
            Console.WriteLine("Enter the cancelation url:");
            var cancelationUrl = Console.ReadLine();
            try
            {
                var canceledOrders = helper.CanceledOrders(cancelationUrl).Result;
                if (canceledOrders.Count == 0)
                {
                    Console.WriteLine("There are no orders cancelled");
                    return;
                }

                Console.WriteLine($"{canceledOrders.Count} canceled orders");

                foreach (CanceledOrder canceledOrder in canceledOrders)
                {
                    Console.WriteLine($"  Order Id:{canceledOrder.orderId} Store Id:{canceledOrder.partnerStoreId}");
                    Console.WriteLine($"    Reasons: {canceledOrder.cancelReason}");
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }

        }

        // Query and process orders
        static void ProcessOrders(rappiHelper helper)
        {

            // Query new orders
            var rootOrders = helper.GetOrders().Result;

            // If there are no orders, exit the process
            if (rootOrders.Count == 0)
                return;

            // Loop through each order and ask the user to take or reject it
            foreach (var root in rootOrders)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                // Show order
                Console.WriteLine($"  Order id:{root.order.id}");

                // Show order items
                foreach (var item in root.order.items)
                    Console.WriteLine($"    {item.name} x {item.units} ${item.price}");

                // Ask user for action for this order
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("   Do you want to Take (T) or Reject (R) the order");
                var action = Console.ReadLine().ToUpper();
                if (action == "T")
                {
                    if (helper.TakeOrder(root.order.id).Result)
                        Console.WriteLine("   Order taken");
                }
                else if (action == "R")
                {
                    Console.WriteLine("   Reason to reject:");
                    var reason = Console.ReadLine();
                    if (helper.RejectOrder(root.order.id, reason).Result)
                        Console.WriteLine("   Order rejected");
                }
                else
                    Console.WriteLine("   Order ignored");

            }
        }

        #endregion

        #region Store and Items Availability

        static string clientId;
        static string clientSecret;
        static string audience;
        static string loginUrl;
        static string storeId;

        static void AskAvailabilityParameters()
        {
            Console.WriteLine("Client Id");
            clientId = Console.ReadLine();
            Console.WriteLine("Client Secret");
            clientSecret = Console.ReadLine();
            Console.WriteLine("Audience (url)");
            audience = Console.ReadLine();
            Console.WriteLine("Availability services login url");
            loginUrl = Console.ReadLine();
            Console.WriteLine("Store id:");
            storeId = Console.ReadLine();

        }

        static string AskOnOff()
        {
            string setOnOff = "";
            while (setOnOff.ToUpper() != "ON" && setOnOff.ToUpper() != "OFF")
            {
                Console.WriteLine("Set On/Off");
                setOnOff = Console.ReadLine();
            }

            return setOnOff;
        }

        static void StoreAvailability()
        {

            AskAvailabilityParameters();


            string setStoreOnOff = AskOnOff();
/*
            try
            {
                var helper = new AvailabilityServicesHelper(clientId, clientSecret, audience, loginUrl);
                AvailabilityServicesHelper.StoreAvailabilityRequest request = new AvailabilityServicesHelper.StoreAvailabilityRequest();
                if (setStoreOnOff == "ON")
                    request.turn_on.Add(storeId);
                else
                    request.turn_off.Add(storeId);

                helper.StoreAvailability(request);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
            }
*/
        }

        static void ItemsAvailability()
        {
            AskAvailabilityParameters();

            Console.WriteLine("Item id:");
            var itemId = Console.ReadLine();

            string setItemOnOff = AskOnOff();
/*
            try
            {
                var helper = new AvailabilityServicesHelper(clientId, clientSecret, audience, loginUrl);
                List<AvailabilityServicesHelper.ItemAvailabilityRequest> request = new List<AvailabilityServicesHelper.ItemAvailabilityRequest>();
                var item = new AvailabilityServicesHelper.ItemAvailabilityRequest() { store_integration_id = storeId };

                if (setItemOnOff == "ON")
                    item.items.turn_on.Add(itemId);
                else
                    item.items.turn_off.Add(itemId);

                request.Add(item);

                helper.ItemsAvailability(request).GetAwaiter().GetResult();

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
            }*/
        }

        #endregion
    }
}
