using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Vacation24.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2
    }
    
    public class Order
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey("ServiceDefinition")]
        public int DefinitionId { get; set; }

        public string ExternalOrderId { get; set; }
        public string PayUOrderId { get; set; }

        public int ProfileId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public DateTime Created { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual Service ServiceDefinition { get; set; }
    }
}