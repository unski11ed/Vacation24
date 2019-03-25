using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public class OrderProduct{
        public string name {get; set;}
        public int unitPrice {get; set;}
        public int quantity{get;set;}
    }

    public class OrderBuyer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

    public class OrderRequest
    {
        public string notifyUrl { get; set; }
        public string continueUrl { get; set; }
        public string customerIp { get; set; }
        public string merchantPosId { get; set; }
        public string description {get; set;}

        public string currencyCode{get;set;}

        public int totalAmount {get; set;}

        public string extOrderId { get; set; }

        public OrderBuyer buyer { get; set; }
        public List<OrderProduct> products {get;set;}

        public OrderRequest()
        {
            currencyCode = "EUR";

            description = "Vacation24 service payment";
        }
    }

    public class OrderResponseStatus{
        public string statusCode {get;set;}
        public int code {get;set;}
        public string codeLiteral { get; set; }
        public string statusDesc { get; set; }
    }

    public class OrderResponse
    {
        public string orderId { get; set; }
        public string extOrderId { get; set; }
        public OrderResponseStatus status { get; set; }
        public string redirectUri { get; set; }
    }

    [Serializable]
    public class OrderNotifyDetails
    {
        public string orderId { get; set; }

        public string orderCreateDate { get; set; }

        public string notifyUrl { get; set; }
        public string customerIp { get; set; }
        public string merchantPosId { get; set; }
        public string description {get; set;}

        public string currencyCode{get;set;}

        public int totalAmount {get; set;}

        public string extOrderId { get; set; }

        public OrderBuyer buyer { get; set; }
        public List<OrderProduct> products {get;set;}

        public string status { get; set; }
    }

    [Serializable]
    public class OrderNotification
    {
        public OrderNotifyDetails order { get; set; }
    }
}