using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vacation24.Models;

namespace Vacation24.Core.Payment
{
    public interface IService
    {
        int Id { get; }

        void Init(IPaymentServicesContext context, Service baseEntity, IActiveService activeServiceEntity);
        void Create(IPaymentServicesContext context, Service baseEntity);

        string Name { get; }
        decimal Price { get; }
        DateTime Expiriation { get; }

        string HandlerName {get;}

        bool IsActive { get; }

        void Activate(Service serviceToActivate);
        void Deactivate();
    }
}
