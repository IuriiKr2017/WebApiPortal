using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace B2BWebApi.Models
{
    public class MainInfoAddress
    {
        [Required]        
        public string Country { get; set; }
        [Required]
        [StringLength(100)]
        public string AddressLine1 { get; set; }
        [StringLength(100)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }       
        public string State { get; set; } // all data is in State Id.
       // [Required]
        public string StateId { get; set; }//all states are shortCuts from them - Ontario = ON
        [Required]
        public string Zip { get; set; }

        [RegularExpression(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", ErrorMessage = "Please enter valid phone no.")]

        [StringLength(20, MinimumLength = 7, ErrorMessage = "ABARoutingNumber cannot exceed 20 characters and be less than 7 ")]
        public string Phone { get; set; }
       
        [Display(Name = "Email address")]        
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}