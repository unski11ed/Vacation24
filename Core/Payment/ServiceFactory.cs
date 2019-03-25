using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core.Payment
{
    public class ServiceFactory : IServiceFactory
    {
        private IPaymentServicesContext _dbContext;
        private IBuilder<IService> _serviceBuilder;

        public ServiceFactory(IPaymentServicesContext paymentDbContext, IBuilder<IService> serviceBuilder)
        {
            _dbContext = paymentDbContext;
            _serviceBuilder = serviceBuilder;
        }

        public IService CreateNew(int serviceListId, Dictionary<string, object> data)
        {
            //Get definition from database
            var serviceDefinition = _dbContext.Services.Find(serviceListId);
            //Create a new instance based on definition.HandlerName
            var service = (IService)Activator.CreateInstance(null, "Vacation24.Services." + serviceDefinition.HandlerName);
            //Pupulate with nescessary data
            _serviceBuilder.Build(service, data);
            //Initialize
            service.Create(_dbContext, serviceDefinition);
            
            return service;
        }

        public IService CreateExisting(IActiveService entityService)
        {
            //Get definition from database
            var serviceDefinition = _dbContext.Services.Where(s => s.HandlerName == entityService.HandlerName).FirstOrDefault();

            if (serviceDefinition == null)
            {
                throw new Exception("Handler not found: " + entityService.HandlerName);
            }

            //Create a new instance based on definition.HandlerName
            var service = (IService)Activator.CreateInstance(null, "Vacation24.Services." + serviceDefinition.HandlerName);

            service.Init(_dbContext, serviceDefinition, entityService);

            return service;
        }
    }
}