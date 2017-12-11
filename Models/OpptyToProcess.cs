using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public partial class OpptyToProcess
    {
        public string CrmId { get; set; }
        public string AddressLine1_store { get; set; }
        public string AddressLine2_store { get; set; }
        public string Country_store { get; set; }
        public string City_store { get; set; }
        public string State_store { get; set; }
        public string Zip_store { get; set; }
        public string AddressLine1_billing { get; set; }
        public string AddressLine2_billing { get; set; }
        public string Country_billing { get; set; }
        public string City_billing { get; set; }
        public string State_billing { get; set; }
        public string Zip_billing { get; set; }
    }
    public partial class OpptyToProcess
    {
        public List<OpptyToProcess> partialModel { get; set; }
    }
}