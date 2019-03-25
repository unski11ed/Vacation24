using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core.Helpers
{
    public class PromotedObjectsComparer : IEqualityComparer<PromotedObject>
    {
        public bool Equals(PromotedObject x, PromotedObject y)
        {
            return x.Object.Id == y.Object.Id;
        }

        public int GetHashCode(PromotedObject obj)
        {
            return obj.Object.Id.GetHashCode();
        }
    }
}