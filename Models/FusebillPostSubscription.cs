using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillPostSubscription
    {
        public decimal customerid { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string reference { get; set; }
        public string planFrequencyId { get; set; }
       // public DateTime openSubscriptionPeriodEndDate { get; set; }
       public bool hasPostedInvoice { get; set; }

    }
}