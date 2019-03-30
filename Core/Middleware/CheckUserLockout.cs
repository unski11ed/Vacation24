using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Vacation24.Core;
using Vacation24.Models;

namespace Vacation24.Middleware {
    public class CheckUserLockout : IMiddleware
    {
        private readonly SignInManager<Profile> signInManager;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly DefaultContext dbContext;

        public CheckUserLockout(
            SignInManager<Profile> signInManager,
            ICurrentUserProvider currentUserProvider,
            DefaultContext dbContext
        )
        {
            this.signInManager = signInManager;
            this.currentUserProvider = currentUserProvider;
            this.dbContext = dbContext;
        }
        public async Task ExecuteAsync(HttpContext context, Func<Task> next)
        {
            if (currentUserProvider.IsAuthenticated)
            {
                var profile = dbContext.Profiles.Find(currentUserProvider.UserId);
                
                if (profile == null || profile.Locked)
                {
                    await signInManager.SignOutAsync();
                }
            }
        }
    }
}