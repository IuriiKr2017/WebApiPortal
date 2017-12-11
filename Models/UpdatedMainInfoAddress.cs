using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class UpdatedMainInfoAddress
    {
        public MainInfoAddress MainAddress { get; set; }

        [Required(ErrorMessage = "Invalid OpportunityId")]       
        public Guid? OpportunityId { get; set; }
    }
}