using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Models;
using Vacation24.Models.DTO;

namespace Vacation24.Controllers
{
    public class VideosController : Controller
    {
        private DefaultContext _dbContext = DefaultContext.GetContext();
        private static readonly Regex YoutubeVideoRegex
            = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);


        public ActionResult Save(SetVideo video)
        {
            if (_dbContext.Places.Any(p => p.Id == video.postId))
            {
                var videoEntity = _dbContext.Videos.FirstOrDefault(v => v.PlaceId == video.postId);
                var embededUrl = string.Empty;
                 
                try{
                    embededUrl = prepareEmbedUrl(video.url);
                }catch(Exception e){
                    return Json(new ResultViewModel(){
                        Status = (int)ResultStatus.Success,
                        Message = e.Message
                    });
                }

                //Add new entity or replace url in existing
                if (videoEntity == null)
                {
                    _dbContext.Videos.Add(new Video()
                    {
                        PlaceId = video.postId,
                        EmbedUrl = embededUrl,
                        OriginalUrl = video.url
                    });
                }
                else
                {
                    videoEntity.EmbedUrl = embededUrl;
                    videoEntity.OriginalUrl = video.url;
                    _dbContext.Entry<Video>(videoEntity).State = System.Data.Entity.EntityState.Modified;
                }

                _dbContext.SaveChanges();

                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Success,
                    Message = embededUrl
                });
            }

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Error,
                Message = "Błąd krytyczny: Nie znaleziono obiektu."
            });
        }

        public ActionResult View(RequestId placeId)
        {
            var video = _dbContext.Videos.FirstOrDefault(v => v.PlaceId == placeId.Id);

            return Json(video);
        }

        public ActionResult Clear(RequestId placeId)
        {
            var video = _dbContext.Videos.FirstOrDefault(v => v.PlaceId == placeId.Id);
            if (video != null)
            {
                _dbContext.Videos.Remove(video);
                _dbContext.SaveChanges();
            }

            return Json(new ResultViewModel()
            {
                Status = (int)ResultStatus.Success
            });
        }

        private string prepareEmbedUrl(string inputUrl)
        {
            var ytMatch = YoutubeVideoRegex.Match(inputUrl);

            if (ytMatch.Success)
                return ytMatch.Groups[1].Value;

            throw new Exception("Nieprawidłowy adres do filmu YouTube.");
        }
    }
}