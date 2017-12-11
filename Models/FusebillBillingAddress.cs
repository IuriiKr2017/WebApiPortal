using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillBillingAddress
    {
        public int customerAddressPreferenceId { get; set; }
        public string companyName { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string countryId { get; set; }
        public string country { get; set; }
        public string stateId { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string postalZip { get; set; }
        public string addressType { get; set; }

    }
}