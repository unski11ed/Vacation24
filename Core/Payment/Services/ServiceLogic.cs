using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Vacation24.Core.Payment;
using Vacation24.Models;

namespace Vacation24.Services
{
    public abstract class ServiceLogic<TEntity> : IService where TEntity : IActiveService
    {
        public string Name
        {
            get { return name; }
        }

        public decimal Price
        {
            get { return price; }
        }

        public bool IsActive
        {
            get
            {
                return activeEntity.ExpiriationTime > DateTime.Now;
            }
        }

        public DateTime Expiriation
        {
            get
            {
                return activeEntity.ExpiriationTime;
            }
        }

        public int Id
        {
            get
            {
                return activeEntity.Id;
            }
        }

        public string HandlerName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        protected string name;
        protected decimal price;

        protected IPaymentServicesContext dbContext;
        protected TEntity activeEntity;

        public virtual void Init(IPaymentServicesContext context, Service baseEntity, IActiveService activeServiceEntity)
        {
            name = baseEntity.Name;
            price = baseEntity.Price;

            dbContext = context;
            activeEntity = (TEntity)activeServiceEntity;
        }

        public virtual void Create(IPaymentServicesContext context, Service baseEntity)
        {
            name = baseEntity.Name;
            price = baseEntity.Price;

            dbContext = context;
        }

        public virtual void Activate(Service serviceToActivate)
        {
            activeEntity.ExpiriationTime = (
                activeEntity.ExpiriationTime < DateTime.Now ?
                    DateTime.Now :
                    activeEntity.ExpiriationTime
            ).AddDays(serviceToActivate.Days);
            updateDataInDb();
        }

        public virtual void Deactivate()
        {
            activeEntity.ExpiriationTime = DateTime.MinValue;
            updateDataInDb();
        }

        protected virtual void updateDataInDb()
        {
            dbContext.Entry((IActiveService)activeEntity).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        public abstract bool Exists(out IActiveService activeService);
    }
}