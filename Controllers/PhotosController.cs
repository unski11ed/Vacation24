using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Helpers;
using Vacation24.Core.ExtensionMethods;
using Vacation24.Models;
using Vacation24.Core.Configuration.Images;
using System.IO;
using Vacation24.Core;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Vacation24.Core.Configuration;

namespace Vacation24.Controllers
{
    public class PhotosController : CustomController
    {
        private readonly DefaultContext dbContext;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly ThumbnailConfig thumbnailConfig;
        private readonly ImagesConfiguration imagesConfiguration;

        public PhotosController(
            DefaultContext dbContext,
            ICurrentUserProvider currentUserProvider,
            ThumbnailConfig thumbnailConfig,
            AppConfiguration appConfiguration
        )
        {
            this.dbContext = dbContext;
            this.currentUserProvider = currentUserProvider;
            this.thumbnailConfig = thumbnailConfig;
            this.imagesConfiguration = appConfiguration.ImagesConfiguration;
        }

        public ActionResult GetThumbnails(int objectId)
        {
            var imagesList = dbContext.Photos
                .Where(photo => photo.PlaceId == objectId)
                .ToList();

            return Json(imagesList);
        }

        public ActionResult Get(int photoId)
        {
            var image = dbContext.Photos
                .Where(photo => photo.Id == photoId)
                .FirstOrDefault();

            return Json(image);
        }

        [Authorize(Roles = "owner, admin")]
        public async Task<IActionResult> Add(int placeId, bool isMain)
        {
            if (
                !IsCurrentUserAdmin() &&
                !dbContext.Places.Any(
                    p => p.Id == placeId && p.OwnerId == currentUserProvider.UserId
                )
            )
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Access denied"
                });
            }


            for (int i = 0; i < Request.Form.Files.Count; i++)
            {
                var file = Request.Form.Files[i];
                if (
                    file.ContentType == "image/jpeg" ||
                    file.ContentType == "image/png"
                )
                {
                    using(var fileStream = file.OpenReadStream()) {
                        //Generate thumbnails
                        var thumbnails = thumbnailConfig.Sizes
                            .Select(
                                size => new
                                    {
                                        stream = ImageResizer.ResizeToCenter(fileStream, size),
                                        size = size
                                    }
                            );

                        //Resize image itself
                        var resizedImage = ImageResizer.Resize(
                            fileStream,
                            imagesConfiguration.PhotoMaxWidth,
                            imagesConfiguration.PhotoMaxWidth
                        );

                        // Generate image filename
                        var fileName = MD5.Create().CalculateStringHash(
                                DateTime.Now.Ticks.ToString()
                            )
                            .ToBase64String() + ".jpg";

                        // Replace main photo if it was already assigned
                        if (isMain)
                        {
                            var currentMainPhoto = dbContext.Photos
                                .Where(p => p.Type == PhotoType.Main && p.PlaceId == placeId)
                                .FirstOrDefault();
                            if (currentMainPhoto != null)
                            {
                                // Delete thumbnails and the photo itself
                                var filePath = Path.Combine(
                                    imagesConfiguration.PhotosPath,
                                    currentMainPhoto.Filename
                                );
                                System.IO.File.Delete(filePath);
                                deleteAllThumbnails(currentMainPhoto.Filename);
                                
                                dbContext.Photos.Remove(currentMainPhoto);
                            }
                        }
                        // Save thumbnails
                        try
                        {
                            foreach (var thumbnail in thumbnails)
                            {
                                var path = thumbnailConfig.Path(thumbnail.size, fileName);
                                // Create directory if not existing
                                (new FileInfo(path)).Directory.Create();
                                // Save each thumbnail
                                var thumbnailFile = System.IO.File.Create(path);
                                using (var thumbnailWriter = new System.IO.StreamWriter(thumbnailFile)) {
                                    await thumbnail.stream.CopyToAsync(thumbnailWriter.BaseStream);
                                }
                            }

                            //Save image
                            var photoFile = System.IO.File.Create(fileName);
                            using (var photoWriter = new System.IO.StreamWriter(photoFile)) {
                                await resizedImage.CopyToAsync(photoWriter.BaseStream);
                            }
                        }
                        catch (Exception)
                        {
                            return Json(new ResultViewModel() { Status = (int)ResultStatus.Error, Message = "Niewłaściwy plik obrazu. Spróbuj jeszcze raz." });
                        }

                        dbContext.Photos.Add(new Photo()
                        {
                            Filename = fileName,
                            PlaceId = placeId,
                            Type = isMain ? PhotoType.Main : PhotoType.Additional
                        });

                        dbContext.SaveChanges();
                    }
                }
            }
            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }

        [Authorize(Roles = "owner, admin")]
        public ActionResult Delete(int photoId)
        {
            var photo = dbContext.Photos.Find(photoId);

            //Check for access permission
            if (
                !IsCurrentUserAdmin() && !
                dbContext.Places.Any(
                    p => p.Id == photo.PlaceId && 
                    p.OwnerId == currentUserProvider.UserId
                )
            )
            {
                return Json(new ResultViewModel()
                {
                    Status = (int)ResultStatus.Error,
                    Message = "Access denied"
                });
            }

            //Delete thumbnail and actual photo
            var filePath = Path.Combine(
                imagesConfiguration.PhotosPath,
                photo.Filename
            );
            deleteAllThumbnails(photo.Filename);
            System.IO.File.Delete(filePath);
            
            dbContext.Photos.Remove(photo);
            dbContext.SaveChanges();

            return Json(new ResultViewModel() { Status = (int)ResultStatus.Success });
        }

        private void deleteAllThumbnails(string fileName)
        {
            thumbnailConfig.Sizes.ForEach((size) =>
            {
                System.IO.File.Delete(thumbnailConfig.Path(size, fileName));
            });
        }
    }
}
