using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Models;
using Vacation24.Models.DTO;

namespace Vacation24.Controllers
{
    public class SearchController : CustomController
    {
        private DefaultContext _dbContext = new DefaultContext();
        private IUniqueViewCounter _viewsCounter;

        public SearchController(IUniqueViewCounter viewsCounter)
        {
            _viewsCounter = viewsCounter;
        }

        public ActionResult SearchForm()
        {
            return View();
        }

        public ActionResult GetCitiesInVoivoidship(RequestCities voivoidship)
        {
            //TODO: Dodac sortowanie po popularnosci?
            var result = _dbContext.Places.Where(p => p.Voivoidship == voivoidship.Name || (string.IsNullOrEmpty(voivoidship.Name)))
                                          .Select<Place, string>(p => p.City)
                                          .OrderBy(v => v)
                                          .Distinct()
                                          .Take(10)
                                          .ToList();

            return Json(new {
                cities = result
            });
        }

        public ActionResult GetCitiesInVoivoidshipWithCount(RequestCities voivoidship)
        {
            //TODO: Dodac sortowanie po popularnosci?

            var result = _dbContext.Places.Where(p => (p.Voivoidship == voivoidship.Name || (string.IsNullOrEmpty(voivoidship.Name))) && p.IsPaid)
                                        .GroupBy(place => place.City)
                                        .Select(group => new CititesInVoivoidshipWithCount { City = group.Key, PlacesCount = group.Count() })
                                        .OrderByDescending(cities => cities.PlacesCount)
                                        .Take(10)
                                        .ToList<CititesInVoivoidshipWithCount>();

            return Json(new
            {
                cities = result
            });
        }

        public ActionResult ViewMostPopularCities()
        {
            var mostPopular = _dbContext.Places
                                        .Where(p => p.IsPaid)
                                        .GroupBy(place => place.City)
                                        .Select(group => new CititesInVoivoidshipWithCount { City = group.Key, PlacesCount = group.Count() })
                                        .OrderByDescending(cities => cities.PlacesCount)
                                        .Take(10)
                                        .ToList<CititesInVoivoidshipWithCount>();

            ViewBag.MostPopularCities = mostPopular;

            return View();
        }

        public ActionResult ViewSpecialOffersBox(string prefferedVoivoidship = "", string prefferedCity = "", int total = 10)
        {
            //TODO: Add randomness with a fixed length of objects
            var placesQuery = _dbContext.SpecialOffers.Where(so => so.Placement == SpecialOfferPlacement.SideBar && so.Place.IsPaid);
            var preferedQuery = placesQuery;

            if (!string.IsNullOrEmpty(prefferedCity))
                preferedQuery = preferedQuery.Where(p => p.Place.City == prefferedCity);

            if (!string.IsNullOrEmpty(prefferedVoivoidship))
                preferedQuery = preferedQuery.Where(p => p.Place.Voivoidship == prefferedVoivoidship);


            //Choose if prefrfered places by city or voivoidship has at least one element
            //if not fallback to default query
            IQueryable<SpecialOffer> finalQuery = (preferedQuery.Count() > 0) ? preferedQuery : placesQuery;

            //Get random count of elements
            ViewBag.Places = finalQuery.OrderBy(x => Guid.NewGuid()).Take(total).Select(offer => offer.Place).ToList();

            return View();
        }

        public ActionResult ViewNewestObjectsBox(int count = 4)
        {
            ViewBag.Places = _dbContext.Places.Where(p => p.IsPaid)
                                              .OrderByDescending(p => p.Created)
                                              .Take(count);
            
            return View();
        }

        public ActionResult ViewMostPopularBox(int count = 4)
        {
            ViewBag.Places = _dbContext.Places.Where(p => p.IsPaid)
                                              .OrderByDescending(p => p.UniqueViews)
                                              .Take(count);

            return View();
        }

        public ActionResult ViewFooterCities()
        {
            var request = new string[]{"morze", "gory", "jezioro"};
            var result = new Dictionary<string, List<string>>();

            foreach(var r in request){
                var l = _dbContext.Places.Where(p => p.Region == r)
                                         .OrderByDescending(p => p.UniqueViews)
                                         .Select<Place, string>(p => p.City)
                                         .Distinct()
                                         .Take(12)
                                         .OrderBy(n => n)
                                         .ToList<string>();
                result.Add(r, l);
            }

            return View(result);
        }

        public ActionResult ViewHomePageObjects()
        {
            var rng = new Random();

            var promotionPlaces = _dbContext.SpecialOffers.Where(o => o.Placement == SpecialOfferPlacement.HomePage && o.ExpiriationTime > DateTime.Now && o.Place.IsPaid)
                                                          .OrderBy(o => Guid.NewGuid())
                                                          .Take(10)
                                                          .Select<SpecialOffer, PromotedObject>(so => new PromotedObject() { Object = so.Place, Promoted = true });

            var topPlaces = _dbContext.Places.Where(p => !promotionPlaces.Select(pp => pp.Object.Id).Contains(p.Id) && p.IsPaid)
                                             .OrderBy(p => p.UniqueViews)
                                             .Take(30)
                                             .Select<Place, PromotedObject>(p => new PromotedObject(){Object = p, Promoted = false});


            var result = promotionPlaces.Union(topPlaces)
                                        .OrderBy(p => p.Object.Name)
                                        .ToList();

            return View(result);
        }

        [HttpPost]
        public ActionResult GetPopularCitiesByRegion(RequestPopularCitites request)
        {
            var cities = _dbContext.Places.Where(p => p.Region == request.Criteria)
                                          .Select(p => p.City)
                                          .ToList();

            return Json(new {Cities = cities });
        }

        [HttpPost]
        public ActionResult GetPopularCitiesByOption(RequestPopularCitites request)
        {
            var cities = _dbContext.Places.Where(p => p.AdditionalOptions.Contains(request.Criteria))
                                          .Select(p => p.City).ToList();
            return Json(new { Cities = cities });
        }

    }
}
