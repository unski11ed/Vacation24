using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core
{
    public interface IUniqueViewCounter
    {
        bool AddView(int placeId, string userId, string IP);
        int GetViews(int placeId);
    }

    public class UniqueViewCounter : IUniqueViewCounter
    {
        private IUniqueViewsContext dbContext;

        public UniqueViewCounter(IUniqueViewsContext context)
        {
            dbContext = context;
        }

        public bool AddView(int placeId, string userId, string IP)
        {
            //Check for duplicate
            if(
                dbContext.UniqueViews
                    .Any(
                        v => v.IpAddress == IP &&
                            v.PlaceId == placeId &&
                            v.UserId == userId
                    )
            )
            {
                return false;
            }

            dbContext.UniqueViews.Add(
                new UniqueView()
                    {
                        PlaceId = placeId,
                        UserId = userId,
                        IpAddress = IP
                    }
            );
            dbContext.SaveChanges();

            return true;
        }

        public int GetViews(int placeId)
        {
            return dbContext.UniqueViews.Where(v => v.PlaceId == placeId).Count();
        }
    }
}