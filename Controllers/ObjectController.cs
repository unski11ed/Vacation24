using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Models.DTO;
using WebMatrix.WebData;
using PagedList;
using PagedList.Mvc;
using Vacation24.Core;
using Vacation24.Core.Payment;
using Vacation24.Services;
using System.Linq.Expressions;
using Vacation24.Core.Helpers;
using System.Web.Routing;
using Rotativa;
using System.Text;

namespace Vacation24.Controllers
{
    public class ObjectController : CustomController
    {
        private DefaultContext _dbContext = new DefaultContext();
        private IUniqueViewCounter _viewCounter;
        private IPaymentServices _paymentServices;
        //
        // GET: /Object/

        public ObjectController(IUniqueViewCounter viewCounter, IPaymentServices paymentServices)
        {
            _viewCounter = viewCounter;
            _paymentServices = paymentServices;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles="owner, admin")]
        public ActionResult Create()
        {
            ViewBag.Mode = "create";
            return View();
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult Edit(int id)
        {
            if (_dbContext.Places.Where(p => p.Id == id).Count() == 0)
            {
                return HttpNotFound("Nie znaleziono obiektu.");
            }

            ViewBag.Mode = "edit";
            ViewBag.Id = id;
            return View("Create");
        }

        [Authorize(Roles = "admin")]
        public ActionResult Delete(int id)
        {
            var place = _dbContext.Places.Find(id);

            if (place == null)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znaleziono obiektu"
                }, JsonRequestBehavior.AllowGet);
            }

            _dbContext.Photos.RemoveRange(
                    _dbContext.Photos.Where(p => p.PlaceId == id).ToList()
                );

            _dbContext.Comments.RemoveRange(
                    _dbContext.Comments.Where(c => c.PlaceId == id).ToList()
                );

            _dbContext.Prices.RemoveRange(
                    _dbContext.Prices.Where(p => p.PlaceId == id).ToList()
                );

            _dbContext.Places.Remove(place);
            _dbContext.SaveChanges();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult Save(ObjectViewModel model)
        {
            var objectActivator = new ObjectsActivator();

            var insertId = 0;

            if (model.Id <= 0)
            {
                var newObject = (Models.Place)model;

                newObject.OwnerId = WebSecurity.CurrentUserId;

                objectActivator.ActivateObjectIfOwnerSubscribed(newObject, newObject.OwnerId);

                _dbContext.Places.Add(newObject);
                _dbContext.SaveChanges();

                insertId = newObject.Id;
            }
            else
            {
                var place = _dbContext.Places.Where(p => p.Id == model.Id)
                                             .FirstOrDefault();

                //Verify if object exists and belongs to logged user
                if (place == null || !(place.OwnerId == WebSecurity.CurrentUserId || IsCurrentUserAdmin()))
                {
                    return Json(new ResultViewModel()
                    {
                        Status = (int)ResultStatus.Error,
                        Message = "Invalid entity"
                    });
                }

                insertId = place.Id;

                //Mark every price for removal
                _dbContext.Prices.RemoveRange(place.Prices);

                place.Extend(model);

                foreach (var price in place.Prices)
                {
                    price.PlaceId = place.Id;
                }
                place.Options.PlaceId = place.Id;

                _dbContext.Entry(place).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    _dbContext.SaveChanges();
                }
                catch (Exception exception)
                {
                    return Json(new ResultViewModel()
                    {
                        Status = (int)ResultStatus.Error,
                        Message = exception.Message
                    });
                }
            }

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = insertId.ToString()
            });
        }

        [HttpPost]
        [Authorize(Roles = "owner, admin")]
        public ActionResult Get(RequestById request)
        {
            var place = _dbContext.Places.Where(p => p.Id == request.Id).FirstOrDefault();
            //If null return empty object
            var output = place == null ? new ObjectViewModel() : (ObjectViewModel)place;
            return Json(output);
        }


        [HttpGet]
        public ActionResult View(int Id, string Title = "")
        {
            var place = _dbContext.Places.Find(Id);
            if (place == null)
                return new HttpNotFoundResult();

            //Hide to users if Owners subscription is not active
            if (!IsCurrentUserAdmin())
            {
                var ownerSubscriptionService = _paymentServices.GetUserServices<SubscriptionService>(place.OwnerId)
                                                               .FirstOrDefault();
                if (ownerSubscriptionService == null || !ownerSubscriptionService.IsActive)
                {
                    return View("ViewLocked", place);
                }
            }

            updateViewCount(place);

            ViewBag.IsLogged = WebSecurity.IsAuthenticated;

            ViewBag.Place = place;
            ViewBag.MainPhoto = place.Photos.Where(p => p.Type == PhotoType.Main).FirstOrDefault();
            ViewBag.Photos = place.Photos.Where(p => p.Type == PhotoType.Additional).ToList();


            ViewBag.Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
                                (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

            ViewBag.MyEmail = WebSecurity.CurrentUserName;

            ViewBag.EncodedPhone = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(place.Phone));

            return View("View");
        }

        public ActionResult Print(int Id)
        {
            var place = _dbContext.Places.Find(Id);
            if (place == null)
                return new HttpNotFoundResult();

            ViewBag.Place = place;
            ViewBag.MainPhoto = place.Photos.Where(p => p.Type == PhotoType.Main).FirstOrDefault();
            ViewBag.Photos = place.Photos.Where(p => p.Type == PhotoType.Additional).ToList();

            return View("Print");
        }

        [HttpGet]
        public ActionResult Pdf(int Id)
        {
            //return new Rotativa.UrlAsPdf("http://localhost:8080/Object/Print/1") { FileName = "Oferta.pdf" };
            return new ActionAsPdf("Print", new { Id = Id }) { FileName = "Oferta.pdf" };
        }

        public ActionResult List(int page = 1, string voiv = "", string type = "", string city = "", decimal maxprice = decimal.MaxValue, string sort_crit = "popular", string sort_price = "asc", string options = "", int count = 20, int? user = null)
        {
            //TODO: REWRITE THIS CRAP!
            var MAX_ELEMENTS = count;

            var promotedObjects = _dbContext.SpecialOffers.Where(so => so.Placement == SpecialOfferPlacement.SearchResults &&
                                                                       so.ExpiriationTime > DateTime.Now &&

                                                                       (!string.IsNullOrEmpty(voiv) ? so.Place.Voivoidship == voiv : true) && 
                                                                       (!string.IsNullOrEmpty(city) ? so.Place.City == city : true) && 
                                                                       so.Place.MinimumPrice < maxprice && 
                                                                       (!string.IsNullOrEmpty(type) ? so.Place.Type == type : true) && 
                                                                       so.Place.AdditionalOptions.Contains(options) &&
                                                                       so.Place.IsPaid
                                                                 )
                                                           .Select<SpecialOffer, PromotedObject>(so => new PromotedObject()
                                                           {
                                                               Promoted = true,
                                                               Object = so.Place
                                                           });

            var allObjects = _dbContext.Places.Where(p => (
                                                    !promotedObjects.Select(pp => pp.Object.Id).Contains(p.Id)) && 
                                                    (!string.IsNullOrEmpty(voiv) ? p.Voivoidship == voiv : true) && 
                                                    (!string.IsNullOrEmpty(city) ? p.City == city : true) && 
                                                    p.MinimumPrice < maxprice && 
                                                    (!string.IsNullOrEmpty(type) ? p.Type == type : true) && 
                                                    p.AdditionalOptions.Contains(options) &&
                                                    p.IsPaid
                                                ).Select<Place, PromotedObject>(p => new PromotedObject(){
                                                  Promoted = false,
                                                  Object = p
                                              });

            var base_query = promotedObjects.Union(allObjects);

            //IQueryable<Place> base_query = _dbContext.Places.Where(p => p.Voivoidship.Contains(voiv) && p.City.Contains(city) && p.MinimumPrice < maxprice && p.Type.Contains(type) && p.AdditionalOptions.Contains(options));

            //Znalezienie nazwy użytkownika jesli query zawiera tą informację
            var userName = "";
            if (user != null)
            {
                base_query = base_query.Where(p => p.Object.OwnerId == user);
                userName = _dbContext.Profiles.Where(u => u.UserId == user)
                                              .Select(u => u.Name)
                                              .FirstOrDefault();
            }

            //Sortowanie promocją
            base_query = base_query.OrderByDescending(po => po.Promoted);

            //Sortowanie kryteriami
            switch (sort_crit)
            {
                case "city":
                    base_query = ((IOrderedQueryable<PromotedObject>)base_query).ThenBy(p => p.Object.City);
                    break;

                case "alphabet":
                    base_query = ((IOrderedQueryable<PromotedObject>)base_query).ThenBy(p => p.Object.Name);
                    break;

                case "popular":
                default:
                    base_query = ((IOrderedQueryable<PromotedObject>)base_query).ThenBy(p => p.Object.UniqueViews);
                    break;
            }
            //Sortowanie ceny
            switch (sort_price)
            {
                default:
                case "asc":
                    base_query = ((IOrderedQueryable<PromotedObject>)base_query).ThenBy(p => p.Object.MinimumPrice);
                    break;

                case "desc":
                    base_query = ((IOrderedQueryable<PromotedObject>)base_query).OrderByDescending(p => p.Object.MinimumPrice);
                    break;
            }

            var places = base_query.Skip((page - 1) * MAX_ELEMENTS)
                                   .Take(MAX_ELEMENTS)
                                   .ToList();

            var countElements = base_query.Count();

            ViewBag.Places = new StaticPagedList<PromotedObject>(places, page, MAX_ELEMENTS, countElements);
            ViewBag.Count = countElements;
            ViewBag.Pages = Math.Ceiling((decimal)countElements / MAX_ELEMENTS);


            var queryFilters = new List<QueryFilter>()
            {
                new QueryFilter(){Name = "Województwo", UrlParam="voiv", Value= voiv, IsActive = voiv != ""},
                new QueryFilter(){Name = "Miasto", UrlParam="city", Value = city, IsActive = city != ""},
                new QueryFilter(){Name = "Typ", UrlParam="type", Value = type, IsActive = type != ""},
                new QueryFilter(){Name = "Cena do", UrlParam="maxprice", Value = maxprice.ToString().Replace('.', ','), IsActive = maxprice != decimal.MaxValue},
                new QueryFilter(){Name = "Obiekty użytkownika", UrlParam="user", Value = userName, IsActive = user != null}
            };

            var currentRouteValues = new RouteValueDictionary
            {
                {"sort_crit", sort_crit},
                {"sort_price", sort_price}
            };
            queryFilters.Where(filter => filter.IsActive)
                        .ToList()
                        .ForEach(qf => currentRouteValues.Add(qf.UrlParam, qf.Value));


            ViewBag.Filters = queryFilters.Where(filter => filter.IsActive).ToList();
            ViewBag.CurrentRouteValues = currentRouteValues;
            ViewBag.CountPerPage = MAX_ELEMENTS;
            ViewBag.IsAdmin = IsCurrentUserAdmin();

            return View("List");
        }

        private void updateViewCount(Place place)
        {
            if (_viewCounter.AddView(place.Id, WebSecurity.IsAuthenticated ? (int?)WebSecurity.CurrentUserId : null, Request.UserHostAddress))
            {
                place.UniqueViews++;
            }
            place.Views++;
            _dbContext.Entry<Place>(place).State = System.Data.Entity.EntityState.Modified;
            _dbContext.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _dbContext.Dispose();

            base.Dispose(disposing);
        }
    }
}
