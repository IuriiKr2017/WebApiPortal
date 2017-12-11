using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillCreditCardPayment
    {
        public decimal customerId { get; set; }
        public string cardNumber { get; set; } // or maskedCardNumber
        public string cardType { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public decimal expirationMonth { get; set; }

        public decimal expirationYear { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }

        public string country { get; set; }
        public decimal stateId { get; set; }
        public string state { get; set; }

        public string city { get; set; }
        public string postalZip { get; set; }
        public bool isDefault { get; set; }

        public decimal cvv { get; set; }
        public decimal countryId { get; set; }
        
    }
}