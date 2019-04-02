using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Vacation24.Models;
using Vacation24.Core.Configuration;

namespace Vacation24.Core.Configuration.Images
{
    public class ThumbnailConfig
    {
        public Size Small => imagesConfig.ThumbnailSizes.Small;
        public Size Medium => imagesConfig.ThumbnailSizes.Medium;
        public Size Large => imagesConfig.ThumbnailSizes.Large;
        public Size ExtraLarge => imagesConfig.ThumbnailSizes.ExtraLarge;
        public Size Mega => imagesConfig.ThumbnailSizes.Mega;

        public List<Size> Sizes
        {
            get
            {
                return new List<Size>(){
                        Small,
                        Medium,
                        Large,
                        ExtraLarge,
                        Mega
                    };
            }
        }

        private readonly ImagesConfiguration imagesConfig;

        public ThumbnailConfig(AppConfiguration appConfiguration) {
            this.imagesConfig = appConfiguration.ImagesConfiguration;
        }

        public string Path(Size size, string fileName)
        {
            var thumbnailPath = System.IO.Path.Combine(
                imagesConfig.ThumbnailUrl,
                $"{size.Width.ToString()}x{size.Height.ToString()}",
                fileName
            );
            return thumbnailPath;
        }

        public string Uri(Size size, string fileName)
        {
            if (
                fileName != "" &&
                (
                    fileName == null ||
                    !File.Exists(this.Path(size, fileName))
                )
            )
            {
                fileName = "nophoto.jpg";
            }
            
            var thumbnailUrl = Flurl.Url.Combine(
                imagesConfig.ThumbnailUrl,
                $"${size.Width.ToString()}x${size.Height.ToString()}",
                fileName
            );

            return thumbnailUrl;
        }

        public string UriFromPhoto(Size size, Photo photo)
        {
            return Uri(size, photo != null ? photo.Filename : null);
        }
    }
}