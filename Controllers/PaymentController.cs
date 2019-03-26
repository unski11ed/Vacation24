using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Payment;
using Vacation24.Models;
using Vacation24.Models.DTO;
using Vacation24.Services;
using WebMatrix.WebData;
using Vacation24.Core.ExtensionMethods;
using Vacation24.Core.Payment.Orders;
using System.Configuration;
using Vacation24.Core;
using PagedList;
using PagedList.Mvc;
using System.Web.Security;
using Vacation24.Core.Configuration;

namespace Vacation24.Controllers
{
    public class PaymentController : CustomController
    {
        private string _domain
        {
            get
            {
                return AppConfiguration.Get().SiteConfiguration.Domain;
            }
        }

        private DefaultContext _dbContext = new DefaultContext();
        private IPaymentServices _paymentServices;
        private IOrders _orders;

        public PaymentController(IPaymentServices paymentServices, IOrders orders)
        {
            _paymentServices = paymentServices;
            _orders = orders;
        }

        #region Embeded Actions
        [Authorize(Roles = "owner, admin")]
        public ActionResult ViewObjectPromotions(int objectId)
        {
            //Promotions - one per HandlerName
            var promotions = _dbContext.Services.Where(s => s.HandlerName.Contains("Promotion"))
                                                .GroupBy(s => s.HandlerName)
                                                .Select(g => g.FirstOrDefault())
                                                .ToList();

            var activeServices = _paymentServices.GetObjectServices(objectId);

            var promoList = new List<ObjectPromotionItem>();

            //Create promotions list based on HandlerName - no duplicates
            foreach (var promotion in promotions)
            {
                var activeService = activeServices.Where(s => s.HandlerName == promotion.HandlerName)
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
            var viewUserId = WebSecurity.CurrentUserId;

            ViewBag.userId = viewUserId;

            var service = _paymentServices.GetUserServices<SubscriptionService>(viewUserId)
                                          .FirstOrDefault();

            return View(service);
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult ViewSubscriptionStatus()
        {
            var viewUserId = WebSecurity.CurrentUserId;

            ViewBag.userId = viewUserId;

            var service = _paymentServices.GetUserServices<SubscriptionService>(viewUserId)
                                          .FirstOrDefault();

            return View(service);
        }
        #endregion

        #region Admin Actions
        [Authorize(Roles = "admin")]
        public ActionResult Deactivate(int serviceId, string handlerName)
        {
            var service = _paymentServices.FindService(serviceId, handlerName);
            if (service == null)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znaleziono usługi"
                }, JsonRequestBehavior.AllowGet);
            }

            service.Deactivate();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        public ActionResult Edit()
        {
            var servicesList = _dbContext.Services.ToList();

            ViewBag.Subscriptions = servicesList.Where(s => s.HandlerName == "SubscriptionService")
                                                .ToList();

            ViewBag.Promotions = servicesList.Where(s => s.HandlerName != "SubscriptionService")
                                             .ToList();

            ViewBag.PromotionsHandlers = _paymentServices.SupportedHandlers.Where(h => h != typeof(SubscriptionService))
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
                    _dbContext.Services.Add(service);
                }
                else
                {
                    _dbContext.Entry<Service>(service).State = EntityState.Modified;
                }
                _dbContext.SaveChanges();

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

            var orders = _dbContext.Orders.OrderByDescending(o => o.Created)
                                          .ToPagedList(page, TOTAL_ORDERS_PER_PAGE);

            return View(orders);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult Delete(RequestId dataId){
            var service = _dbContext.Services.Find(dataId.Id);

            if (service == null)
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znalezion usługi"
                });

            _dbContext.Services.Remove(service);
            _dbContext.SaveChanges();

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
            ViewBag.IsOwner = Roles.GetRolesForUser().Contains("owner");

            var services = _dbContext.Services.OrderBy(s => s.Price)
                                              .GroupBy(g => g.HandlerName)
                                              .Select(g => g.FirstOrDefault())
                                              .ToDictionary(s => s.HandlerName, s => s);
            ViewBag.ServicePrices = services;

            ViewBag.UserId = WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserId : -1;
            
            ViewBag.ObjectId = objectId;

            //                  TODO: SHOULD BE WRAPPED IN CLASS
            //Find object promotions
            var objectServicesState = new Dictionary<string, ObjectPromotionItem>();
            if (objectId != null)
            {
                var activeObjectServices = _paymentServices.GetObjectServices((int)objectId);

                foreach (KeyValuePair<string, Service> entry in services)
                {
                    //Object promotions only
                    if (entry.Key.Contains("Promotion"))
                    {
                        var activeService = activeObjectServices.Where(s => s.HandlerName == entry.Key)
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
            if (WebSecurity.IsAuthenticated)
            {
                var activeUserServices = _paymentServices.GetUserServices((int)WebSecurity.CurrentUserId);
                foreach(KeyValuePair<string, Service> entry in services){
                    if (entry.Key.Contains("Subscription"))
                    {
                        var activeService = activeUserServices.Where(s => s.HandlerName == entry.Key)
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

            var activeServices = objectServicesState.Union(userServicesState)
                                                    .ToDictionary(k => k.Key, v => v.Value);
            ViewBag.ActiveServices = activeServices;
            
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "owner, admin")]
        public ActionResult ShowPaymentModal(string handler)
        {
            var services = _dbContext.Services.Where(s => s.HandlerName == handler)
                                              .OrderBy(s => s.Id)
                                              .ToList();

            return View(services);
        }

        [HttpPost]
        [Authorize(Roles = "owner, admin")]
        public ActionResult BuyService(BuyRequest request)
        {
            var service = _paymentServices.CreateNewService(request.DefinitionId, request.Data.ToObjectDictionary());

            if (IsCurrentUserAdmin())
            {
                var serviceDefinitionToActivate = _dbContext.Services.Find(request.DefinitionId);
                service.Activate(serviceDefinitionToActivate);

                return Redirect(System.Web.HttpContext.Current.Request.UrlReferrer.ToString());
            }

            string redirectUrl = string.Empty;
            try
            {
                redirectUrl = _orders.Create(request.DefinitionId, 
                                            service.Id,
                                            WebSecurity.CurrentUserId, 
                                            Request.UserHostAddress, 
                                            "/Payment/Notify/",
                                            "http://" + _domain + "/Payment/Final"
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
            var order = _dbContext.Orders.Where(o => o.ExternalOrderId == notification.order.extOrderId &&
                                                     o.PayUOrderId == notification.order.orderId)
                                  .FirstOrDefault();

            if (order != null)
            {
                if (notification.order.status == "COMPLETED")
                {
                    order.OrderStatus = OrderStatus.Success;
                    _dbContext.Entry<Order>(order).State = EntityState.Modified;
                    _dbContext.SaveChanges();

                    var serviceDefinition = _dbContext.Services.Find(order.DefinitionId);

                    var service = _paymentServices.FindService(order.ServiceId, serviceDefinition.HandlerName);

                    service.Activate(serviceDefinition);
                }
                else if (notification.order.status == "CANCELLED" || notification.order.status == "REJECTED")
                {
                    order.OrderStatus = OrderStatus.Failed;
                    _dbContext.Entry<Order>(order).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }
            }

            return View();
        }
        #endregion

        #region Anonymous Actions
        public ActionResult Pricing()
        {
            var services = _dbContext.Services.ToList();

            ViewBag.subscribtions = services.Where(s => s.HandlerName == typeof(SubscriptionService).Name).ToList();
            ViewBag.searchPromotions = services.Where(s => s.HandlerName == typeof(PromotionSearchService).Name).ToList();
            ViewBag.sidePromotions = services.Where(s => s.HandlerName == typeof(PromotionSideService).Name).ToList();
            ViewBag.homePromotions = services.Where(s => s.HandlerName == typeof(PromotionHomeService).Name).ToList();

            return View();
        }
        #endregion
    }
}
