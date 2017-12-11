using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class OverallProductInfo
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Qty { get; set; }
        public decimal TotalAmount { get; set; }
        public int FusebillProductId { get; set; }
        public int FusebillPlanId { get; set; }

       

    }
}