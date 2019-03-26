using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Helpers;
using Vacation24.Core.ExtensionMethods;
using System.Data.Entity;
using Vacation24.Models;
using Vacation24.Core.Configuration.Images;
using Vacation24.Models.DTO;
using System.IO;
using System.Drawing.Imaging;
using Vacation24.Core;
using WebMatrix.WebData;

namespace Vacation24.Controllers
{
    public class PhotosController : CustomController
    {
        private DefaultContext _dbContext = new DefaultContext();

        public ActionResult GetThumbnails(int objectId)
        {
            var imagesList = _dbContext.Photos.Where(photo => photo.PlaceId == objectId).ToList();

            return Json(imagesList);
        }

        public ActionResult Get(int photoId)
        {
            var image = _dbContext.Photos.Where(photo => photo.Id == photoId).FirstOrDefault();

            return Json(image);
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult Add(int placeId, bool isMain)
        {
            if (!IsCurrentUserAdmin() && !_dbContext.Places.Any(p => p.Id == placeId && p.OwnerId == WebSecurity.CurrentUserId))
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Access denied"
                });
            }


            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                if(file.ContentType == "image/jpeg" ||
                   file.ContentType == "image/png")
                {
                    var img = Image.FromStream(file.InputStream);

                    //Generate thumbnails
                    var thumbnails = new List<Image>();
                    foreach (var size in Thumbnail.Sizes)
                        thumbnails.Add(ImageResizer.ResizeToCenter(size, img));

                    //Resize image itself
                    var resizedImage = ImageResizer.Resize(img, ImagesConfiguration.PhotoMaxWidth, ImagesConfiguration.PhotoMaxWidth);

                    var fileName = MD5.Create().CalculateStringHash(DateTime.Now.Ticks.ToString()).ToBase64String() + ".jpg";

                    //Zastąp istniejące zdjęcie głowne, jeżeli było ono już ustawione
                    if (isMain)
                    {
                        var currentMainPhoto = _dbContext.Photos.Where(p => p.Type == PhotoType.Main && p.PlaceId == placeId).FirstOrDefault();
                        if (currentMainPhoto != null)
                        {
                            //Usuń thumbnail i wlasciwe zdjecie
                            System.IO.File.Delete(ImagesConfiguration.PhotosPath + "\\" + currentMainPhoto.Filename);
                            deleteAllThumbnails(currentMainPhoto.Filename);
                            
                            _dbContext.Photos.Remove(currentMainPhoto);
                        }
                    }

                    try
                    {
                        //Save thumbnails
                        foreach (var thumbnail in thumbnails)
                        {
                            var path = Thumbnail.Path(thumbnail.Size, fileName);
                            //Create directory if not existing
                            (new FileInfo(path)).Directory.Create();
                            //save each thumbnail
                            thumbnail.Save(path, ImageFormat.Jpeg);
                        }

                        //Save image
                        resizedImage.Save(ImagesConfiguration.PhotosPath + "\\" + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch (Exception)
                    {
                        return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Niewłaściwy plik obrazu. Spróbuj jeszcze raz." });
                    }

                    _dbContext.Photos.Add(new Photo()
                    {
                        Filename = fileName,
                        PlaceId = placeId,
                        Type = isMain ? PhotoType.Main : PhotoType.Additional
                    });

                    _dbContext.SaveChanges();
                }
            }
            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult Delete(int photoId)
        {
            var photo = _dbContext.Photos.Find(photoId);

            //Check for access permission
            if (!IsCurrentUserAdmin() && !
                _dbContext.Places.Any(p => p.Id == photo.PlaceId && 
                                      p.OwnerId == WebSecurity.CurrentUserId))
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Access denied"
                });
            }

            //Delete thumbnail and actual photo
            deleteAllThumbnails(photo.Filename);
            System.IO.File.Delete(ImagesConfiguration.PhotosPath + "\\" + photo.Filename);
            
            _dbContext.Photos.Remove(photo);
            _dbContext.SaveChanges();

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success});
        }

        private void deleteAllThumbnails(string fileName)
        {
            Thumbnail.Sizes.ForEach((size) =>
            {
                System.IO.File.Delete(Thumbnail.Path(size, fileName));
            });
        }
    }
}
