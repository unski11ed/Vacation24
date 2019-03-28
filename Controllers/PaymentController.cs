using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Payment;
using Vacation24.Models;
using Vacation24.Services;
using Vacation24.Core.ExtensionMethods;
using Vacation24.Core.Payment.Orders;
using System.Configuration;
using Vacation24.Core;
using Vacation24.Core.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Vacation24.Controllers
{
    public class PaymentController : CustomController
    {
        private readonly IOrders orders;
        private readonly DefaultContext dbContext;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly AppConfiguration configuration;
        private readonly IPaymentServices paymentServices;

        private string rootPath => configuration.SiteConfiguration.RootPath;

        public PaymentController(
            IPaymentServices paymentServices,
            IOrders orders,
            DefaultContext dbContext,
            ICurrentUserProvider currentUserProvider,
            AppConfiguration configuration
        )
        {
            this.orders = orders;
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
            this.configuration = configuration;
            this.paymentServices = paymentServices;
        }

        #region Embeded Actions
        [Authorize(Roles = "owner, admin")]
        public ActionResult ViewObjectPromotions(int objectId)
        {
            //Promotions - one per HandlerName
            var promotions = dbContext.Services
                .Where(s => s.HandlerName.Contains("Promotion"))
                .GroupBy(s => s.HandlerName)
                .Select(g => g.FirstOrDefault())
                .ToList();

            var activeServices = paymentServices.GetObjectServices(objectId);

            var promoList = new List<ObjectPromotionItem>();

            //Create promotions list based on HandlerName - no duplicates
            foreach (var promotion in promotions)
            {
                var activeService = activeServices
                    .Where(s => s.HandlerName == promotion.HandlerName)
                    .FirstOrDefault();

                promoList.Add(new ObjectPromotionItem(){
                    Name = promotion.Name,
                    Expiriation = activeService == null ? DateTime.MinValue : activeService.Expiriation,
                    IsActive = activeService != null && activeService.Expiriation > DateTime.Now,
                    Handler = promotion.HandlerName
                });
            }

            ViewBag.ObjectId = objectId;

            return View(promoList);
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult ViewSubscriptionAlert()
        {
            var viewUserId = currentUserProvider.UserId;

            ViewBag.userId = viewUserId;

            var service = paymentServices
                .GetUserServices<SubscriptionService>(viewUserId)
                .FirstOrDefault();

            return View(service);
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult ViewSubscriptionStatus()
        {
            var viewUserId = currentUserProvider.UserId;

            ViewBag.userId = viewUserId;

            var service = paymentServices
                .GetUserServices<SubscriptionService>(viewUserId)
                .FirstOrDefault();

            return View(service);
        }
        #endregion

        #region Admin Actions
        [Authorize(Roles = "admin")]
        public ActionResult Deactivate(int serviceId, string handlerName)
        {
            var service = paymentServices.FindService(serviceId, handlerName);
            if (service == null)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znaleziono usługi"
                });
            }

            service.Deactivate();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = ""
            });
        }

        [Authorize(Roles = "admin")]
        public ActionResult Edit()
        {
            var servicesList = dbContext.Services.ToList();

            ViewBag.Subscriptions = servicesList
                .Where(s => s.HandlerName == "SubscriptionService")
                .ToList();

            ViewBag.Promotions = servicesList
                .Where(s => s.HandlerName != "SubscriptionService")
                .ToList();

            ViewBag.PromotionsHandlers = paymentServices.SupportedHandlers
                .Where(h => h != typeof(SubscriptionService))
                .Select(h => h.Name)
                .ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult Update(Service service)
        {
            int serviceId;

            try
            {
                if (service.Id == 0)
                {
                    dbContext.Services.Add(service);
                }
                else
                {
                    dbContext.Entry<Service>(service).State = EntityState.Modified;
                }
                dbContext.SaveChanges();

                serviceId = service.Id;
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel()
                {
                    Message = "Wystąpił błąd: " + e.Message,
                    Status = (int)ResultStatus.Error
                });
            }

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = serviceId.ToString()
            });
        }

        [HttpGet]
        [Authorize(Roles="admin")]
        public ActionResult Orders(int page = 1)
        {
            var TOTAL_ORDERS_PER_PAGE = 20;

            var orders = dbContext.Orders
                .OrderByDescending(o => o.Created)
                .ToPagedList(page, TOTAL_ORDERS_PER_PAGE);

            return View(orders);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult Delete(RequestId dataId){
            var service = dbContext.Services.Find(dataId.Id);

            if (service == null)
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znalezion usługi"
                });

            dbContext.Services.Remove(service);
            dbContext.SaveChanges();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success
            });
        }
        #endregion

        #region Owner Actions
        [HttpGet]
        public ActionResult Index(int? objectId)
        {
            ViewBag.IsOwner = currentUserProvider.IsUserInRole("owner");

            var services = dbContext.Services
                .OrderBy(s => s.Price)
                .GroupBy(g => g.HandlerName)
                .Select(g => g.FirstOrDefault())
                .ToDictionary(s => s.HandlerName, s => s);
            ViewBag.ServicePrices = services;

            ViewBag.UserId = currentUserProvider.IsAuthenticated ?
                currentUserProvider.UserId : "";
            
            ViewBag.ObjectId = objectId;

            //                  TODO: SHOULD BE WRAPPED IN CLASS
            //Find object promotions
            var objectServicesState = new Dictionary<string, ObjectPromotionItem>();
            if (objectId != null)
            {
                var activeObjectServices = paymentServices.GetObjectServices((int)objectId);

                foreach (KeyValuePair<string, Service> entry in services)
                {
                    //Object promotions only
                    if (entry.Key.Contains("Promotion"))
                    {
                        var activeService = activeObjectServices
                            .Where(s => s.HandlerName == entry.Key)
                            .FirstOrDefault();
                        //Add to dictionary
                        objectServicesState.Add(entry.Key, new ObjectPromotionItem()
                        {
                            Name = entry.Value.Name,
                            Expiriation = activeService == null ? DateTime.MinValue : activeService.Expiriation,
                            IsActive = activeService != null && activeService.Expiriation > DateTime.Now,
                            Handler = entry.Key
                        });
                    }
                }
            }
            //Find users promotions
            var userServicesState = new Dictionary<string, ObjectPromotionItem>();
            if (currentUserProvider.IsAuthenticated)
            {
                var activeUserServices = paymentServices.GetUserServices(currentUserProvider.UserId);
                foreach(KeyValuePair<string, Service> entry in services){
                    if (entry.Key.Contains("Subscription"))
                    {
                        var activeService = activeUserServices
                            .Where(s => s.HandlerName == entry.Key)
                            .FirstOrDefault();
                        userServicesState.Add(entry.Key, new ObjectPromotionItem()
                        {
                            Name = entry.Value.Name,
                            Expiriation = activeService == null ? DateTime.MinValue : activeService.Expiriation,
                            IsActive = activeService != null && activeService.Expiriation > DateTime.Now,
                            Handler = entry.Key
                        });
                    }
                }
            }

            var activeServices = objectServicesState
                .Union(userServicesState)
                .ToDictionary(k => k.Key, v => v.Value);
            ViewBag.ActiveServices = activeServices;
            
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult ShowPaymentModal(string handler)
        {
            var services = dbContext.Services
                .Where(s => s.HandlerName == handler)
                .OrderBy(s => s.Id)
                .ToList();

            return View(services);
        }

        [HttpPost]
        [Authorize(Roles = "owner, admin")]
        public ActionResult BuyService(BuyRequest request)
        {
            var service = paymentServices.CreateNewService(
                request.DefinitionId,
                request.Data.ToObjectDictionary()
            );

            if (IsCurrentUserAdmin())
            {
                var serviceDefinitionToActivate = dbContext.Services.Find(request.DefinitionId);
                service.Activate(serviceDefinitionToActivate);

                return Redirect(Request.Headers["Referer"].ToString());
            }

            string redirectUrl = string.Empty;
            try
            {
                redirectUrl = orders.Create(
                    request.DefinitionId, 
                    service.Id,
                    currentUserProvider.UserId, 
                    Request.HttpContext.Connection.RemoteIpAddress.ToString(), 
                    "/Payment/Notify/",
                    Flurl.Url.Combine(rootPath, "/Payment/Final")
                );
            }
            catch (Exception)
            {
                //TODO: Log and display error
            }

            return Redirect(redirectUrl);
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult Final(int? error)
        {
            ViewBag.Success = error == null;

            return View();
        }

        [HttpPost]
        public ActionResult Notify(OrderNotification notification)
        {
            //TODO: check if payUServer request
            var order = dbContext.Orders
                .Where(o => o.ExternalOrderId == notification.order.extOrderId)
                .Where(o => o.PayUOrderId == notification.order.orderId)
                .FirstOrDefault();

            if (order != null)
            {
                if (notification.order.status == "COMPLETED")
                {
                    order.OrderStatus = OrderStatus.Success;
                    dbContext.Entry<Order>(order).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    var serviceDefinition = dbContext.Services.Find(order.DefinitionId);

                    var service = paymentServices.FindService(order.ServiceId, serviceDefinition.HandlerName);

                    service.Activate(serviceDefinition);
                }
                else if (notification.order.status == "CANCELLED" || notification.order.status == "REJECTED")
                {
                    order.OrderStatus = OrderStatus.Failed;
                    dbContext.Entry<Order>(order).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }

            return View();
        }
        #endregion

        #region Anonymous Actions
        public ActionResult Pricing()
        {
            var services = dbContext.Services.ToList();

            ViewBag.subscribtions = services.Where(s => s.HandlerName == typeof(SubscriptionService).Name).ToList();
            ViewBag.searchPromotions = services.Where(s => s.HandlerName == typeof(PromotionSearchService).Name).ToList();
            ViewBag.sidePromotions = services.Where(s => s.HandlerName == typeof(PromotionSideService).Name).ToList();
            ViewBag.homePromotions = services.Where(s => s.HandlerName == typeof(PromotionHomeService).Name).ToList();

            return View();
        }
        #endregion
    }
}
