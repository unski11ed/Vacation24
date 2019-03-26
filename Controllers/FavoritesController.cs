using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core;
using Vacation24.Models;
using Vacation24.Models.DTO;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class FavoritesController : CustomController
    {
        private const int MAX_FAVS = 20;
        private DefaultContext _dbContext = new DefaultContext();

        #region Embeded
        public ActionResult ShowUserBox()
        {
            var favs = _dbContext.Favorites.Where(fav => fav.UserId == WebSecurity.CurrentUserId)
                                           .OrderByDescending(fav => fav.Added)
                                           .ToList();

            return View(favs);
        }
        #endregion

        #region AJAX CruD
        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Create(Favorite favorite) 
        {
            favorite.UserId = WebSecurity.CurrentUserId;
            favorite.Added = DateTime.Now;

            //Sprawdź czy ilosc nie przekracza MAX_FAVS
            var favoritesCount = _dbContext.Favorites.Where(fav => fav.UserId == WebSecurity.CurrentUserId).Count();
            if (favoritesCount >= MAX_FAVS)
            {
                return Json(new ResultViewModel() {
                    Status = (int)ResultStatus.Info, 
                    Message = string.Format("Można mieć maksymalnie {0} ulubionych obiektów.", MAX_FAVS)
                } );
            }

            var favExists = _dbContext.Favorites.Where(fav => fav.UserId == WebSecurity.CurrentUserId &&
                                                              fav.PlaceId == favorite.PlaceId).Count() > 0;
            if (favExists)
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Info,
                    Message = "Obiekt znajduje się już na liście ulubionych."
                });
            }

            try
            {
                _dbContext.Favorites.Add(favorite);
                _dbContext.SaveChanges();
            }catch(Exception){
                return Json(new ResultViewModel(){Status = (int)ResultStatus.Error, Message = "Nieudany zapis do bazy danych"});
            }
            
            return  Json(new ResultViewModel(){Status = (int)ResultStatus.Success});
        }

        [Authorize(Roles = "user, owner, admin")]
        public ActionResult Delete(RequestId requrestFavorite){
            var favorite = _dbContext.Favorites.Where(fav => fav.Id == requrestFavorite.Id).FirstOrDefault();

            if (favorite == null || favorite.UserId != WebSecurity.CurrentUserId)
            {
                return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Nie znaleziono rekordu" });
            }

            _dbContext.Favorites.Remove(favorite);
            _dbContext.SaveChanges();

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }
        #endregion
    }
}
