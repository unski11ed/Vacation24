using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Core;
using Vacation24.Core.Payment;
using Vacation24.Services;
using System.Linq.Expressions;
using Vacation24.Core.Helpers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using X.PagedList;
using Microsoft.AspNetCore.Routing;

namespace Vacation24.Controllers
{
    public class ObjectController : CustomController
    {
        private readonly IUniqueViewCounter viewCounter;
        private readonly IPaymentServices paymentServices;
        private readonly DefaultContext dbContext;
        private readonly ObjectsActivator objectsActivator;
        private readonly CurrentUserProvider currentUserProvider;

        //
        // GET: /Object/

        public ObjectController(
            IUniqueViewCounter viewCounter,
            IPaymentServices paymentServices,
            DefaultContext dbContext,
            ObjectsActivator objectsActivator,
            CurrentUserProvider currentUserProvider
        )
        {
            this.viewCounter = viewCounter;
            this.paymentServices = paymentServices;
            this.dbContext = dbContext;
            this.objectsActivator = objectsActivator;
            this.currentUserProvider = currentUserProvider;
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
            if (dbContext.Places.Where(p => p.Id == id).Count() == 0)
            {
                return NotFound("Nie znaleziono obiektu.");
            }

            ViewBag.Mode = "edit";
            ViewBag.Id = id;
            return View("Create");
        }

        [Authorize(Roles = "admin")]
        public ActionResult Delete(int id)
        {
            var place = dbContext.Places.Find(id);

            if (place == null)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Nie znaleziono obiektu"
                });
            }

            dbContext.Photos.RemoveRange(
                dbContext.Photos.Where(p => p.PlaceId == id).ToList()
            );

            dbContext.Comments.RemoveRange(
                dbContext.Comments.Where(c => c.PlaceId == id).ToList()
            );

            dbContext.Prices.RemoveRange(
                dbContext.Prices.Where(p => p.PlaceId == id).ToList()
            );

            dbContext.Places.Remove(place);
            dbContext.SaveChanges();

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success,
                Message = ""
            });
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult Save(ObjectViewModel model)
        {
            var insertId = 0;

            if (model.Id <= 0)
            {
                var newObject = (Models.Place)model;

                newObject.OwnerId = currentUserProvider.UserId;

                objectsActivator.ActivateObjectIfOwnerSubscribed(newObject, newObject.OwnerId);

                dbContext.Places.Add(newObject);
                dbContext.SaveChanges();

                insertId = newObject.Id;
            }
            else
            {
                var place = dbContext.Places.Where(p => p.Id == model.Id)
                                             .FirstOrDefault();

                //Verify if object exists and belongs to logged user
                if (
                    place == null ||
                    !(
                        place.OwnerId == currentUserProvider.UserId ||
                        currentUserProvider.IsUserInRole("admin")
                    )
                )
                {
                    return Json(new ResultViewModel()
                    {
                        Status = (int)ResultStatus.Error,
                        Message = "Invalid entity"
                    });
                }

                insertId = place.Id;

                //Mark every price for removal
                dbContext.Prices.RemoveRange(place.Prices);

                place.Extend(model);

                foreach (var price in place.Prices)
                {
                    price.PlaceId = place.Id;
                }
                place.Options.PlaceId = place.Id;

                dbContext.Entry(place).State = EntityState.Modified;

                try
                {
                    dbContext.SaveChanges();
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
            var place = dbContext.Places.Where(p => p.Id == request.Id).FirstOrDefault();
            //If null return empty object
            var output = place == null ? new ObjectViewModel() : (ObjectViewModel)place;
            return Json(output);
        }


        [HttpGet]
        public ActionResult View(int Id, string Title = "")
        {
            var place = dbContext.Places.Find(Id);
            var user = dbContext.Profiles.Find(currentUserProvider.UserId);

            if (place == null)
                return new NotFoundResult();

            //Hide to users if Owners subscription is not active
            if (!IsCurrentUserAdmin())
            {
                var ownerSubscriptionService = paymentServices
                    .GetUserServices<SubscriptionService>(place.OwnerId)
                    .FirstOrDefault();
                if (ownerSubscriptionService == null || !ownerSubscriptionService.IsActive)
                {
                    return View("ViewLocked", place);
                }
            }

            updateViewCount(place);

            ViewBag.IsLogged = currentUserProvider.IsAuthenticated;

            ViewBag.Place = place;
            ViewBag.MainPhoto = place.Photos.Where(p => p.Type == PhotoType.Main).FirstOrDefault();
            ViewBag.Photos = place.Photos.Where(p => p.Type == PhotoType.Additional).ToList();

            // TODO: Why?
            ViewBag.Domain = Request.Host.ToString();

            ViewBag.MyEmail = user.Email;

            ViewBag.EncodedPhone = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(place.Phone));

            return View("View");
        }

        public ActionResult Print(int Id)
        {
            var place = dbContext.Places.Find(Id);
            if (place == null)
                return new NotFoundResult();

            ViewBag.Place = place;
            ViewBag.MainPhoto = place.Photos.Where(p => p.Type == PhotoType.Main).FirstOrDefault();
            ViewBag.Photos = place.Photos.Where(p => p.Type == PhotoType.Additional).ToList();

            return View("Print");
        }

        [HttpGet]
        public IActionResult Pdf(int Id)
        {
            var pdfResult = new ViewAsPdf("Print", new { Id = Id })
            {
                FileName = "Oferta.pdf"
            };
            return pdfResult;
        }

        public ActionResult List(
            int page = 1,
            string voiv = "",
            string type = "",
            string city = "",
            decimal maxprice = decimal.MaxValue,
            string sort_crit = "popular",
            string sort_price = "asc",
            string options = "",
            int count = 20,
            string user = null
        )
        {
            //TODO: REWRITE THIS CRAP!
            var MAX_ELEMENTS = count;

            var promotedObjects = dbContext.SpecialOffers
                .Where(so => so.Placement == SpecialOfferPlacement.SearchResults)
                .Where(so => so.ExpiriationTime > DateTime.Now)
                .Where(so => !string.IsNullOrEmpty(voiv) ? so.Place.Voivoidship == voiv : true)
                .Where(so => !string.IsNullOrEmpty(city) ? so.Place.City == city : true)
                .Where(so => so.Place.MinimumPrice < maxprice)
                .Where(so => !string.IsNullOrEmpty(type) ? so.Place.Type == type : true)
                .Where(so => so.Place.AdditionalOptions.Contains(options))
                .Where(so => so.Place.IsPaid)
                .Select<SpecialOffer, PromotedObject>(so => new PromotedObject()
                {
                    Promoted = true,
                    Object = so.Place
                });

            var allObjects = dbContext.Places
                .Where(p => !promotedObjects.Select(pp => pp.Object.Id).Contains(p.Id)) 
                .Where(p => !string.IsNullOrEmpty(voiv) ? p.Voivoidship == voiv : true)
                .Where(p => (!string.IsNullOrEmpty(city) ? p.City == city : true))
                .Where(p => p.MinimumPrice < maxprice)
                .Where(p => !string.IsNullOrEmpty(type) ? p.Type == type : true)
                .Where(p => p.AdditionalOptions.Contains(options))
                .Where(p => p.IsPaid)
                .Select<Place, PromotedObject>(p => new PromotedObject(){
                    Promoted = false,
                    Object = p
                });

            var base_query = promotedObjects.Union(allObjects);

            var userName = "";
            if (user != null)
            {
                base_query = base_query.Where(p => p.Object.OwnerId == user);
                userName = dbContext.Profiles
                    .Where(u => u.Id == user)
                    .Select(u => u.Name)
                    .FirstOrDefault();
            }

            // Sort by promotion
            base_query = base_query.OrderByDescending(po => po.Promoted);

            // Sort by criteria
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
            // Sort by price
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

            var places = base_query
                .Skip((page - 1) * MAX_ELEMENTS)
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
            queryFilters
                .Where(filter => filter.IsActive)
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
            if (
                viewCounter.AddView(
                    place.Id,
                    currentUserProvider.IsAuthenticated ?
                        currentUserProvider.UserId : null,
                    Request.HttpContext.Connection.RemoteIpAddress.ToString()
                )
            )
            {
                place.UniqueViews++;
            }
            place.Views++;
            dbContext.Entry<Place>(place).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                dbContext.Dispose();

            base.Dispose(disposing);
        }
    }
}
