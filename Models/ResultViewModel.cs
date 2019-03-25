using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public enum ResultStatus
    {
        Success = 1,
        Error = 2,
        Info = 3
    }

    [Serializable]
    public class ResultViewModel
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }
}