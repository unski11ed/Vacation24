using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Configuration;
using Vacation24.Core.Configuration.Images;

namespace Vacation24.Components.Account
{
    public class RegisterSellerViewComponent : ViewComponent {
        private readonly AppConfiguration appConfig;
        private readonly ThumbnailConfig thumbnailConfig;
        public RegisterSellerViewComponent(
            AppConfiguration appConfig,
            ThumbnailConfig thumbnailConfig
        )
        {
            this.appConfig = appConfig;
            this.thumbnailConfig = thumbnailConfig;
        }
        public IViewComponentResult Invoke()
        {
            var dataProvider = new JsDataProvider
            {
                AppConfig = appConfig,
                ThumbnailConfig = thumbnailConfig
            };

            return View(dataProvider);
        }
    }
}