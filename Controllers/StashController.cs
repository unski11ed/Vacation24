using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Configuration.Images;
using Vacation24.Models;
using Vacation24.Models.DTO;
using Vacation24.Core.ExtensionMethods;
using Newtonsoft.Json;

namespace Vacation24.Controllers
{
    public class StashController : Controller
    {
        public DefaultContext _dbContext = new DefaultContext();

        public ActionResult GetList(RequestStashList ids)
        {
            var places = _dbContext.Places
                .Where(p => ids.Ids.Contains(p.Id))
                .Select(p => new { Id = p.Id, Name = p.Name, City = p.City, Voivoidship = p.Voivoidship })
                .ToList();

            return Json(new {List = places });
        }

        public ActionResult Get(RequestStashItem item)
        {
            var place = _dbContext.Places.Where(p => p.Id == item.Id)
                                        .Select(p => new { 
                                            Id = p.Id, 
                                            Name = p.Name, 
                                            City = p.City, 
                                            Voivoidship = p.Voivoidship, 
                                            Price = p.MinimumPrice
                                        })
                                        .FirstOrDefault();

            if (place == null)
            {
                return Json(new
                {
                    status = ResultStatus.Error,
                    item = new { }
                });
            }

            //Attach thumbnail
            dynamic placeDynamic = place.ToDynamic();

            var placePhotoFilename = _dbContext.Photos.Where(p => p.PlaceId == item.Id)
                                                      .Select(p => p.Filename)
                                                      .FirstOrDefault();

            placeDynamic.ThumbnailSmall = Thumbnail.Uri(Thumbnail.Small, placePhotoFilename);

            return Content(JsonConvert.SerializeObject(new
            {
                status = ResultStatus.Success,
                item = placeDynamic
            }));
        }

        public ActionResult Show()
        {
            return View();
        }

        public ActionResult ShowUserBox()
        {
            return View();
        }
    }
}
