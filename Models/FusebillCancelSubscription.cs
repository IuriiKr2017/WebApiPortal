using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillCancelSubscription
    {
        public int subscriptionId { get; set; }
        public string cancellationOption { get; set; }
    }
}