using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    //public class FusebillDraftInvoices
    //{               
    //   public List <InvoiceData> draftInvoices { get; set; }  
    //}
    public class InvoiceData
    {
        public object effectiveDate { get; set; }
        public object createdDate { get; set; }
        public float pendingCharges { get; set; }
        public int customerId { get; set; }
        public string customerReference { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public string billingPeriod { get; set; }
        public object scheduledChargeDate { get; set; }
        public string customerFullName { get; set; }
        public string companyName { get; set; }
        public int id { get; set; }
        public string uri { get; set; }
    }
}