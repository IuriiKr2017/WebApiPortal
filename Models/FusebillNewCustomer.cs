using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillNewCustomer
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Title { get; set; }


        public string CompanyName { get; set; }
        public string PrimaryEmail { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryEmail { get; set; }
        public string SecondaryPhone { get; set; }
        public string Reference { get; set; }
        public string ParentId { get; set;}
        public string id { get; set; } // filled after a response from fusebill
    }
}