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
        private IUniqueViewsContext _dbContext;

        public UniqueViewCounter(IUniqueViewsContext context)
        {
            _dbContext = context;
        }

        public bool AddView(int placeId, string userId, string IP)
        {
            //Check for duplicate
            if(
                _dbContext.UniqueViews
                    .Any(
                        v => v.IpAddress == IP &&
                            v.PlaceId == placeId &&
                            v.UserId == userId
                    )
            )
            {
                return false;
            }

            _dbContext.UniqueViews.Add(
                new UniqueView()
                    {
                        PlaceId = placeId,
                        UserId = userId,
                        IpAddress = IP
                    }
            );
            _dbContext.SaveChanges();

            return true;
        }

        public int GetViews(int placeId)
        {
            return _dbContext.UniqueViews.Where(v => v.PlaceId == placeId).Count();
        }
    }
}