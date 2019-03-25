using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Core.Payment;
using Vacation24.Models;

namespace Vacation24.Services
{
    public abstract class PromotionService : ServiceLogic<SpecialOffer>, IServiceObjectBased
    {
        protected SpecialOfferPlacement placement;
        private int objectId = -1;
        public int ObjectId
        {
            get { return objectId; }
            set { objectId = value; }
        }

        public SpecialOfferPlacement Placement
        {
            get
            {
                return placement;
            }
        }

        public override void Create(IPaymentServicesContext context, Service baseEntity)
        {
            base.Create(context, baseEntity);

            IActiveService activeService;

            if (!Exists(out activeService))
            {
                var promotionEntity = new SpecialOffer()
                {
                    PlaceId = objectId,
                    Placement = placement,
                    ExpiriationTime = DateTime.MinValue,
                    HandlerName = baseEntity.HandlerName
                };
                dbContext.SpecialOffers.Add(promotionEntity);
                dbContext.SaveChanges();

                activeEntity = promotionEntity;

                return;
            }

            Init(context, baseEntity, activeService);
        }

        public override void Init(IPaymentServicesContext context, Service baseEntity, IActiveService activeServiceEntity)
        {
            base.Init(context, baseEntity, activeServiceEntity);

            objectId = activeEntity.PlaceId;
        }

        public override bool Exists(out IActiveService activeService)
        {
            activeService = dbContext.SpecialOffers
                .Where(so => 
                    so.PlaceId == objectId && 
                    so.HandlerName == this.HandlerName
                )
                .FirstOrDefault();
            return activeService != null;
        }
    }

    public class PromotionSearchService : PromotionService
    {
        public PromotionSearchService()
        {
            placement = SpecialOfferPlacement.SearchResults;
        }
    }

    public class PromotionSideService : PromotionService
    {
        public PromotionSideService()
        {
            placement = SpecialOfferPlacement.SideBar;
        }
    }

    public class PromotionHomeService : PromotionService
    {
        public PromotionHomeService()
        {
            placement = SpecialOfferPlacement.HomePage;
        }
    }
}