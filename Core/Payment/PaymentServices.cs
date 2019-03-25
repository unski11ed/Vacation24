using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core.Payment
{
    public interface IPaymentServices
    {
        List<IService> GetUserServices(int userId);
        List<IService> GetObjectServices(int objectId);

        List<T> GetUserServices<T>(int userId);
        List<T> GetObjectServices<T>(int objectId);

        IService CreateNewService(int serviceId, Dictionary<string, object> data);

        IService FindService(int serviceId, string handlerName);

        List<Type> SupportedHandlers { get;}
    }

    public class PaymentServices : IPaymentServices
    {
        private IPaymentServicesContext _dbContext;
        private IServiceFactory _serviceFactory;

        public PaymentServices(IPaymentServicesContext paymentContext, IServiceFactory serviceFactory)
        {
            _dbContext = paymentContext;
            _serviceFactory = serviceFactory;
        }

        public List<IService> GetUserServices(int userId)
        {
            var subscriptions = _dbContext.Subscriptions.Where(s => s.UserId == userId).ToList();
            var serviceList = new List<IService>();

            foreach (var subscription in subscriptions)
            {
                serviceList.Add((IService)_serviceFactory.CreateExisting(subscription));
            }

            return serviceList;
        }

        public List<IService> GetObjectServices(int objectId)
        {
            var promotions = _dbContext.SpecialOffers.Where(p => p.PlaceId == objectId);
            var serviceList = new List<IService>();

            foreach (var promotion in promotions)
            {
                serviceList.Add((IService)_serviceFactory.CreateExisting(promotion));
            }

            return serviceList;
        }

        public List<T> GetUserServices<T>(int userId)
        {
            var userServices = GetUserServices(userId);

            return userServices.OfType<T>().ToList<T>();
        }

        public List<T> GetObjectServices<T>(int objectId)
        {
            var objectServices = GetObjectServices(objectId);

            return objectServices.OfType<T>().ToList<T>();
        }

        public IService CreateNewService(int serviceId, Dictionary<string, object> data)
        {
            return _serviceFactory.CreateNew(serviceId, data);
        }

        public IService FindService(int serviceId, string handlerName)
        {
            var listOfEntities = new List<IActiveService>(){
                _dbContext.SpecialOffers.Where(s => s.Id == serviceId && s.HandlerName == handlerName)
                                        .FirstOrDefault(),

                _dbContext.Subscriptions.Where(s => s.Id == serviceId && s.HandlerName == handlerName)
                                        .FirstOrDefault()
            };

            var activeEntity = listOfEntities.Where(e => e != null).FirstOrDefault();
            if(activeEntity != null)
                return _serviceFactory.CreateExisting(activeEntity);

            return null;
        }

        public List<Type> SupportedHandlers
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IService)) && !t.IsAbstract).ToList();
            }
        }
    }
}