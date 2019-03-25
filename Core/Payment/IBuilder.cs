using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vacation24.Core.Payment
{
    public interface IBuilder<T>
    {
        void Build(T objectToBuild, Dictionary<string, object> buildData);
    }
}
