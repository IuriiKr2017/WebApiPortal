using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class PaymentMethodPerStore
    {
        public Payments Payments { get; set; }
        public List<Locations> Locations { get; set; }

        [Required(ErrorMessage = "Invalid OpportunityId")]
        public Guid? OpportunityId { get; set; }
    }

    public class Payments
    {
        public  List<Paymentach> PaymentACH { get; set; }
        public List<PayCard> PaymentCard { get; set; }
    }

    public class Locations
    {
        public MainInfoAddress AddressStore { get; set; }
        public MainInfoAddress AddressBilling { get; set; }
        public string PaymentGuid { get; set; }    
        public string FusebillId { get; set; }   
    }

    public class Paymentach  //Automated Clearing House, an electronic banking network often used for direct deposit and electronic bill payment
    {
        public string PaymentGuid { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "ABARoutingNumber cannot exceed 20 characters and be less than 4 ")]
        public string ABARoutingNumber { get; set; }

        [Required]     
        [StringLength(20, ErrorMessage = "Account number must not exceed 20 characters ")]
        public string BankAccountNumber { get; set; }
        [Required]
        public string AccountType { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "First name must not exceed 50 characters ")]
        public string AccountHolderFirstName { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters ")]
        public string AccountHolderLastName { get; set; }
    }

    public class PayCard
    {
        [Required]
        public string PaymentGuid { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "First name must not exceed 50 characters ")]
        public string FirstNameCard { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters ")]
        public string LastNameCard { get; set; }
        [Required]      
        [StringLength(20,  ErrorMessage = "Card number must not exceed 20 digits ")]
        public string CardNumber { get; set; }

        [Required]
        public decimal ExpirationDate { get; set; }
        [Required]
        [StringLength(5, MinimumLength = 3, ErrorMessage = "Security code must be at least 3 digits long")]
        public string SecurityCode { get; set; }

    }
    

}

