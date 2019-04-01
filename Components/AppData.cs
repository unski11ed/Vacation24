using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.Configuration;

namespace Vacation24.Components
{
    public class AppDataViewComponent : ViewComponent {
        private readonly AppConfiguration configuration;

        public AppDataViewComponent(AppConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(configuration);
        }
    }
}