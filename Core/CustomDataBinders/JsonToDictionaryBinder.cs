
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Vacation24.Core.ExtensionMethods;

namespace Vacation24.Core.CustomDataBinders
{
    public class JsonToDictionaryBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return new Task<Dictionary<string, string>>(() => {
                var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);;
                Dictionary<string, string> dictionaryOutput = null;

                try {
                    var singleValue = value.FirstOrDefault();
                    dictionaryOutput = JsonConvert.DeserializeObject<Dictionary<string, string>>(singleValue);
                } catch {
                    throw new Exception("Invalid JSON object supplied.");
                }
                return dictionaryOutput;
            });
        }
    }
}