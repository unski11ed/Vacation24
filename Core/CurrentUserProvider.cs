using Microsoft.AspNetCore.Http;

namespace Vacation24.Core {
    public interface ICurrentUserProvider {
        string UserId { get; }
        bool IsUserInRole(string role);
    }
    public class CurrentUserProvider: ICurrentUserProvider {
        private readonly IHttpContextAccessor httpContextAccessor;

        CurrentUserProvider(IHttpContextAccessor httpContextAccessor) {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
            httpContextAccessor.HttpContext.User.Identity.IsAuthenticated ?
                httpContextAccessor.HttpContext.User.Identity.Name : null;

        public bool IsUserInRole(string role) {
            if (UserId != null) {
                return httpContextAccessor.HttpContext.User.IsInRole(role);
            }
            return false;
        }
    }
}