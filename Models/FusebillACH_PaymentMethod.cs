using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillACH_PaymentMethod
    {     
        public decimal customerId { get; set; }
        public string accountNumber { get; set; }
        public string transitNumber { get; set; }
        public string bankAccountType { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }

        public string country { get; set; }
        public decimal countryId { get; set; }
        public decimal stateId { get; set; }
        public string city { get; set; }
        public string postalZip { get; set; }
        public string source { get; set; }    

      }
}