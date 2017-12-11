using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    //public class FusebillInvoiceProducts
    //{
    //    public List<InvoiceProductsData> invoiceProductsData { get; set; }
    //}
    public class InvoiceProductsData
    {
        public int invoiceNumber { get; set; }
        public string effectiveTimestamp { get; set; }
        public string postedTimestamp { get; set; }
       // public List<ChargeInv> charges { get; set; }
        public List<Paymentschedule> paymentSchedules { get; set; }
        public int id { get; set; }
    }

    public class Paymentschedule
    {
        public string status { get; set; }
        public float amount { get; set; }
    }


    //public class ChargeInv
    //{
    //    public SubscriptionproductData subscriptionProduct { get; set; }
    //}

    //public class SubscriptionproductData
    //{
    //    public decimal id { get; set; }
    //}





}





