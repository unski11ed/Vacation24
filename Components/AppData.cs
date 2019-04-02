using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Configuration;
using Vacation24.Core.Configuration.Images;

namespace Vacation24.Components
{
    public class JsDataProvider
    {
        public AppConfiguration AppConfig { get; set; }
        public ThumbnailConfig ThumbnailConfig { get; set; }
    }
    public class AppDataViewComponent : ViewComponent {
        private readonly AppConfiguration appConfig;
        private readonly ThumbnailConfig thumbnailConfig;
        public AppDataViewComponent(
            AppConfiguration appConfig,
            ThumbnailConfig thumbnailConfig
        )
        {
            this.appConfig = appConfig;
            this.thumbnailConfig = thumbnailConfig;
        }
        public async Task<IViewComponentResult> InvokeAsync()
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