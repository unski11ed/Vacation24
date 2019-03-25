using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vacation24.Core.CustomDataBinders;

namespace Vacation24.Models
{
    public class ObjectPromotionItem
    {
        public string Name { get; set; }
        public DateTime Expiriation { get; set; }
        public bool IsActive { get; set; }
        public string Handler { get; set; }
    }

    public class ModalRequest
    {
        public string Handler { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }


    [Serializable]
    public class BuyRequest
    {
        public int DefinitionId { get; set; }
        [BindProperty(BinderType = typeof(JsonToDictionaryBinder))]
        public Dictionary<string, string> Data { get; set; }
    }
}