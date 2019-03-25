using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core.Configuration.Images
{
    public static class Thumbnail
    {
        public static Size Small { get { return new Size(140, 100); } }
        public static Size Medium { get { return new Size(165, 110); } }
        public static Size Large { get { return new Size(260, 230); } }
        public static Size ExtraLarge { get { return new Size(300, 240); } }
        public static Size Mega { get { return new Size(500, 500); } }

        public static List<Size> Sizes
        {
            get
            {
                return new List<Size>(){
                        Thumbnail.Small,
                        Thumbnail.Medium,
                        Thumbnail.Large,
                        Thumbnail.ExtraLarge,
                        Thumbnail.Mega
                    };
            }
        }

        public static string Path(Size size, string fileName)
        {
            return ImagesConfiguration.ThumbnailPath + "\\" + (size.Width.ToString() + "x" + size.Height.ToString()) + "\\" + fileName;
        }

        public static string Uri(Size size, string fileName)
        {
            if (fileName != "" && (fileName == null|| !File.Exists(Thumbnail.Path(size, fileName))))
                fileName = "nophoto.jpg";

            return ImagesConfiguration.ThumbnailUrl + (size.Width.ToString() + "x" + size.Height.ToString()) + "/" + fileName;
        }

        public static string UriFromPhoto(Size size, Photo photo)
        {
            return Uri(size, photo != null ? photo.Filename : null);
        }
    }

    public static class ImagesConfiguration
    {
        public static int PhotoMaxWidth
        {
            get
            {
                return 1024;
            }
        }

        public static int PhotoMaxHeight
        {
            get
            {
                return 768;
            }
        }

        public static string ThumbnailPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"Photos\Thumbnails";
            }
        }

        public static string PhotosPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"Photos";
            }
        }

        public static string ThumbnailUrl
        {
            get
            {
                return "/Photos/Thumbnails/";
            }
        }

        public static string PhotosUrl
        {
            get
            {
                return "/Photos/";
            }
        }
    }
}