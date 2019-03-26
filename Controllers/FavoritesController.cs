using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Models;

namespace Vacation24.Controllers
{
    public class FavoritesController : CustomController
    {
        private const int MAX_FAVS = 20;
        private readonly DefaultContext dbContext;
        private readonly CurrentUserProvider currentUserProvider;

        public FavoritesController(
            DefaultContext dbContext,
            CurrentUserProvider currentUserProvider
        )
        {
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
        }

        #region Embeded
        public ActionResult ShowUserBox()
        {
            var favs = dbContext.Favorites
                .Where(fav => fav.UserId == currentUserProvider.UserId)
                .OrderByDescending(fav => fav.Added)
                .ToList();

            return View(favs);
        }
        #endregion

        #region AJAX CruD
        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Create(Favorite favorite) 
        {
            favorite.UserId = currentUserProvider.UserId;
            favorite.Added = DateTime.Now;

            // Check if user hasn't passed MAX_FAVS
            var favoritesCount = dbContext.Favorites
                .Where(fav => fav.UserId == favorite.UserId)
                .Count();
            if (favoritesCount >= MAX_FAVS)
            {
                return Json(new ResultViewModel() {
                    Status = (int)ResultStatus.Info, 
                    Message = string.Format("You can have total of {0} favorited places.", MAX_FAVS)
                } );
            }

            var favExists = dbContext.Favorites
                .Where(fav => (
                        fav.UserId == currentUserProvider.UserId &&
                        fav.PlaceId == favorite.PlaceId
                    )
                )
                .Count() > 0;
            if (favExists)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Info,
                    Message = "This object is already in your favorites list."
                });
            }

            try
            {
                dbContext.Favorites.Add(favorite);
                dbContext.SaveChanges();
            } catch(Exception) {
                return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Failed to save to favorites" });
            }
            
            return  Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Delete(RequestId requrestFavorite){
            var favorite = dbContext.Favorites
                .Where(fav => fav.Id == requrestFavorite.Id)
                .FirstOrDefault();

            if (
                favorite == null ||
                favorite.UserId != currentUserProvider.UserId
            )
            {
                return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Nie znaleziono rekordu" });
            }

            dbContext.Favorites.Remove(favorite);
            dbContext.SaveChanges();

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }
        #endregion
    }
}
