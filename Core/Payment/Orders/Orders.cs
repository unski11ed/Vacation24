using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Models;
using Vacation24.Core.ExtensionMethods;
using System.Configuration;
using Vacation24.Core.Configuration;
using Flurl;

namespace Vacation24.Core.Payment.Orders
{
    public interface IOrders
    {
        string Create(
            int definitionId,
            int serviceId,
            string userId,
            string userIp,
            string notificationUrl,
            string returnUrl
        );
    }

    public class Orders : IOrders
    {
        private string rootUrl
        {
            get
            {
                return configuration.SiteConfiguration.RootPath;
            }
        }

        private readonly IPaymentRequester requester;
        private readonly IOrdersContext dbContext;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly AppConfiguration configuration;
        public Orders(
            IPaymentRequester requester,
            IOrdersContext context,
            AppConfiguration configuration
        )
        {
            this.requester = requester;
            this.dbContext = context;
            this.configuration = configuration;
        }

        public string Create(
            int definitionId,
            int serviceId,
            string userId,
            string userIp, 
            string notificationUrl,
            string returnUrl
        )
        {
            var profile = dbContext.Profiles.Find(userId);

            var serviceDefinition = dbContext.Services.Find(definitionId);

            var uniqueId = Guid.NewGuid().ToString("N");
            var orderEntity = new Order()
            {
                OrderStatus = Models.OrderStatus.Pending,
                ExternalOrderId = uniqueId,

                ProfileId = profile.Id,
                ServiceId = serviceId,
                DefinitionId = serviceDefinition.Id,

                Created = DateTime.Now
            };

            var response = requester.Execute(
                new OrderRequest()
                    {
                        totalAmount = (int)serviceDefinition.Price * 100,
                        customerIp = userIp,
                        extOrderId = uniqueId,
                        notifyUrl = Url.Combine(rootUrl, notificationUrl),
                        continueUrl = returnUrl,
                        buyer = new OrderBuyer()
                        {
                            email = profile.Email,
                            firstName = profile.FirstName,
                            lastName = profile.LastName
                        },
                        products = new List<OrderProduct>()
                        {
                            new OrderProduct(){
                            name = serviceDefinition.Name,
                            unitPrice = (int)serviceDefinition.Price * 100,
                            quantity = 1
                            }
                        }
                    }
            );

            if (response.status.statusCode != "SUCCESS")
            {
                throw new Exception("PayU creation error: " + response.status.statusCode);
            }

            orderEntity.PayUOrderId = response.orderId;

            dbContext.Orders.Add(orderEntity);
            dbContext.SaveChanges();

            return response.redirectUri;
        }
    }
}