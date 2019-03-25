using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Vacation24.Core.Payment
{
    public class ConcreteBuilder : IBuilder<IService>
    {
        public void Build(IService objectToBuild, Dictionary<string, object> buildData)
        {
            var type = objectToBuild.GetType();

            var builderType = this.GetType();

            var buildMethods = builderType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                          .Where(mi => mi.GetCustomAttributes(typeof(ServiceBuild), true).Any())
                                          .ToList();
            foreach (var buildMethod in buildMethods)
            {
                if (!buildMethod.IsGenericMethod)
                {
                    var buildMethodArgType = buildMethod.GetParameters().First().ParameterType;
                    if (type.GetInterfaces().Any(iType => iType == buildMethodArgType))
                    {
                        buildMethod.Invoke(this, System.Reflection.BindingFlags.NonPublic, null, new object[]{objectToBuild, buildData}, null);
                    }
                }
            }
        }
    }
}