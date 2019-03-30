using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core.Payment
{
    public class ServiceBuilder : ConcreteBuilder
    {
        private IPaymentServicesContext _dbContext;

        public ServiceBuilder(IPaymentServicesContext dbContext)
        {
            _dbContext = dbContext;
        }

        [ServiceBuild]
        private void buildUserData(IServiceUserBased service, Dictionary<string, object> data)
        {
            service.UserId = (string)data["userId"];
        }

        [ServiceBuild]
        private void buildObjectData(IServiceObjectBased service, Dictionary<string, object> data)
        {
            service.ObjectId = (int)data["objectId"];
        }
    }
}