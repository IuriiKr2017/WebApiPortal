using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillActivateCustomer
    {
        public string customerID { get; set; } // filled after a response from fusebill      
        
        public string status { get; set; } // check if customer is active
    }
}