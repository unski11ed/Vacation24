using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Core;
using Vacation24.Core.Payment;
using Vacation24.Models;

namespace Vacation24.Services
{
    public class SubscriptionService : ServiceLogic<Subscription>, IServiceUserBased
    {
        private string userId = null;
        private readonly ObjectsActivator objectsActivator;

        public string UserId
        {
            get { return userId; }
            set{ userId = value; }
        }

        public SubscriptionService(ObjectsActivator objectsActivator) {
            this.objectsActivator = objectsActivator;
        }

        public override void Create(IPaymentServicesContext context, Service baseEntity)
        {
            base.Create(context, baseEntity);

            IActiveService activeService;

            if (!Exists(out activeService))
            {
                var subscriptionEntity = new Subscription()
                {
                    ExpiriationTime = DateTime.MinValue,
                    UserId = userId,
                    HandlerName = baseEntity.HandlerName
                };

                context.Subscriptions.Add(subscriptionEntity);
                context.SaveChanges();

                activeEntity = subscriptionEntity;

                return;
            }

            Init(context, baseEntity, activeService);
        }

        public override void Init(IPaymentServicesContext context, Service baseEntity, IActiveService activeServiceEntity)
        {
            base.Init(context, baseEntity, activeServiceEntity);

            userId = activeEntity.UserId;
        }

        public override bool Exists(out IActiveService activeService)
        {
            activeService = dbContext.Subscriptions.Where(
                sub =>
                    sub.HandlerName == HandlerName &&
                    sub.UserId == userId
                ).FirstOrDefault();
            return activeService != null;
        }

        public override void Activate(Service serviceToActivate)
        {
            base.Activate(serviceToActivate);

            //Activate objects of newly subscribed user
            objectsActivator.ActivateObjects(UserId);
        }
    }
}