using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vacation24.Models;

namespace Vacation24.Core
{
    public class ObjectsActivator
    {
        private DefaultContext context = DefaultContext.GetContext();
        private readonly UserManager<Profile> userManager;
        private readonly ICurrentUserProvider currentUserProvider;

        public ObjectsActivator(UserManager<Profile> userManager, ICurrentUserProvider currentUserProvider)
        {
            this.userManager = userManager;
            this.currentUserProvider = currentUserProvider;
        }

        public void ActivateObjects(int userId)
        {
            var places = context.Places.Where(p => p.OwnerId == userId).ToList();

            foreach (var place in places)
            {
                place.IsPaid = true;
                context.Entry(place).State = EntityState.Modified;
            }
            context.SaveChanges();
        }

        public bool ActivateObjectIfOwnerSubscribed(Place place, int userId)
        {
            var isUserSubscribed = context.Subscriptions.Any(s => s.UserId == userId && s.ExpiriationTime > DateTime.Now);

            place.IsPaid = isUserSubscribed;

            return place.IsPaid;
        }

        public async void ProcessObjectsToDeactivate()
        {
            var owners = await userManager.GetUsersInRoleAsync("owner");
            foreach (var owner in owners)
            {
                var isSubscribed = context.Subscriptions.Any(
                    s => s.UserId == owner.Id && s.ExpiriationTime > DateTime.Now);

                if (!isSubscribed && context.Places.Any(p => p.OwnerId == owner.Id && p.IsPaid))
                {
                    var places = context.Places.Where(p => p.OwnerId == owner.Id && p.IsPaid).ToList();
                    foreach (var place in places)
                    {
                        place.IsPaid = false;
                        context.Entry(place).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}