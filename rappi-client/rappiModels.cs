using System;
using System.Collections.Generic;
namespace rappi.Models
{

    public class Item
    {
        public string sku { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public string type { get; set; }
        public string subtype { get; set; }
        public string comments { get; set; }
        public List<object> toppings { get; set; }
        public int units { get; set; }
        public double percentage_price_variation { get; set; }
        public double price_discount { get; set; }
        public double discount_percentage_by_rappi { get; set; }
    }

    public class Order
    {
        public string id { get; set; }
        public double totalValue { get; set; }
        public string createdAt { get; set; }
        public List<Item> items { get; set; }
        public string paymentMethod { get; set; }
        public double tip { get; set; }
        public double whims { get; set; }
        public double totalProducts { get; set; }
        public double totalRappiPay { get; set; }
        public double totalOrderValue { get; set; }
        public string deliveryMethod { get; set; }
    }

    public class AddressDetails
    {
        public string main_street_type { get; set; }
        public string main_street_number { get; set; }
        public string main_street_quadrant { get; set; }
        public string secondary_street_number { get; set; }
        public string meter { get; set; }
        public string secondary_street_quadrant { get; set; }
        public string complete_direction { get; set; }
        public string city { get; set; }
        public string neighborhood { get; set; }
        public string postal_code { get; set; }
    }

    public class Client
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public AddressDetails addressDetails { get; set; }
    }

    public class Store
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class RootObject
    {
        public Order order { get; set; }
        public Client client { get; set; }
        public Store store { get; set; }
    }

    public class OrderError
    {
        public int errorCode { get; set; }
        public string message { get; set; }
    }

    public class OrderErrorException : System.Exception
    {
        public OrderError orderError { get; set; }
        public OrderErrorException(string message, OrderError error) : base(message)
        {
            orderError = error;
        }
    }

    public class CanceledOrder
    {
        public string orderId { get; set; }
        public string storeId { get; set; }
        public DateTime createdAt { get; set; }
        public string cancelReason { get; set; }
    }

}
