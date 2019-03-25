using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vacation24.Models;

namespace Vacation24.Core.Payment
{
    public interface IServiceFactory
    {
        IService CreateNew(int serviceListId, Dictionary<string, object> data);
        IService CreateExisting(IActiveService entityService);
    }
}