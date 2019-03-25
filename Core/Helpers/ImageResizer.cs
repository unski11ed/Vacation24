using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using SkiaSharp;

namespace Vacation24.Core.Helpers
{
    public class ImageResizer
    {
        public static Stream Resize(Stream inputStream, int maxWidth, int maxHeight)
        {
            var bitmap = SKBitmap.Decode(inputStream);

            var ratioX = (double)maxWidth / bitmap.Width;
            var ratioY = (double)maxHeight / bitmap.Height;
            var ratio = Math.Max(ratioX, ratioY);

            var newWidth = (int)(bitmap.Width * ratio);
            var newHeight = (int)(bitmap.Height * ratio);

            var resizedBitmap = bitmap.Resize(
                new SKImageInfo
                {
                    Width = maxWidth,
                    Height = maxHeight,
                },
                SKFilterQuality.Medium
            );
            var outputImage = SKImage.FromBitmap(bitmap);

            return outputImage
                .Encode(SKEncodedImageFormat.Jpeg, 8)
                .AsStream();
        }

        public static Stream ResizeToCenter(Stream inputStream, Size requiredSize)
        {
            var bitmap = SKBitmap.Decode(inputStream);

            var ratioX = (double)requiredSize.Width / bitmap.Width;
            var ratioY = (double)requiredSize.Height / bitmap.Height;

            var ratio = Math.Max(ratioX, ratioY);

            var newWidth = (int)(bitmap.Width * ratio);
            var newHeight = (int)(bitmap.Height * ratio);

            var offsetX = (newWidth - requiredSize.Width) / 2;
            var offsetY = (newHeight - requiredSize.Height) / 2;

            var newBitmap = new SKBitmap(requiredSize.Width, requiredSize.Height);
            var canvas = new SKCanvas(newBitmap);

            canvas.DrawBitmap(
                bitmap,
                SKRect.Create(-offsetX, -offsetY, newWidth, newHeight),
                SKRect.Create(0, 0, bitmap.Width, bitmap.Height)
            );
            canvas.Save();

            var outputImage = SKImage.FromBitmap(newBitmap);

            return outputImage
                .Encode(SKEncodedImageFormat.Jpeg, 8)
                .AsStream();
        }
    }
}