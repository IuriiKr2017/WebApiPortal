using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class FusebillSubscriptionModel
    {
        public List<Subscriptionproduct> subscriptionProducts { get; set; }
        public int id { get; set; }
        public int customerId { get; set; }
        // public Planfrequency planFrequency { get; set; }
        public string planCode { get; set; }
        public string planName { get; set; }
        public string planDescription { get; set; }
        public object planReference { get; set; }
        public string status { get; set; }
        public object reference { get; set; }
        public object subscriptionOverride { get; set; }
        public bool hasPostedInvoice { get; set; }
        public object createdTimestamp { get; set; }
        public object activatedTimestamp { get; set; }
        public object provisionedTimestamp { get; set; }
        public object nextPeriodStartDate { get; set; }
        public object scheduledActivationTimestamp { get; set; }
        public object remainingInterval { get; set; }
        public object remainingIntervalPushOut { get; set; }
        public object openSubscriptionPeriodEndDate { get; set; }
        public object chargeDiscount { get; set; }
        public object setupFeeDiscount { get; set; }
        public object chargeDiscounts { get; set; }
        public object setupFeeDiscounts { get; set; }
        public object customFields { get; set; }
        public bool planAutoApplyChanges { get; set; }
        public bool autoApplyCatalogChanges { get; set; }
        public float monthlyRecurringRevenue { get; set; }
        public float netMonthlyRecurringRevenue { get; set; }
        public decimal amount { get; set; }
        public object contractStartTimestamp { get; set; }
        public object contractEndTimestamp { get; set; }
        public object expiredTimestamp { get; set; }
        public object[] coupons { get; set; }
        public bool subscriptionHasRecurringEndOfPeriodCharge { get; set; }
        public string uri { get; set; }
    }

    public class Planfrequency
    {
        public int planRevisionId { get; set; }
        public int numberOfIntervals { get; set; }
        public string interval { get; set; }
        public int numberOfSubscriptions { get; set; }
        public string status { get; set; }
        public List<object> setupFees { get; set; }
        public List<object> charges { get; set; }
        public bool isProrated { get; set; }
        public object prorationGranularity { get; set; }
        public int planFrequencyUniqueId { get; set; }
        public object remainingInterval { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }

    public class Subscriptionproduct
    {
        public int subscriptionId { get; set; }
        public Planproduct planProduct { get; set; }
        public decimal quantity { get; set; }
        public bool isIncluded { get; set; }
        public object startDate { get; set; }
        public object subscriptionProductOverride { get; set; }
        public object subscriptionProductPriceOverride { get; set; }
        public bool chargeAtSubscriptionActivation { get; set; }
        public bool isCharged { get; set; }
        public object subscriptionProductDiscount { get; set; }
        public List<object> subscriptionProductDiscounts { get; set; }
        public object customFields { get; set; }
        public float monthlyRecurringRevenue { get; set; }
        public float netMonthlyRecurringRevenue { get; set; }
        public float amount { get; set; }
        public string status { get; set; }
        public object lastPurchaseDate { get; set; }
        public Earningsettings earningSettings { get; set; }
        public object remainingInterval { get; set; }
        public bool groupQuantityChangeCharges { get; set; }
        public bool priceUpliftsEnabled { get; set; }
        public List<object> priceUplifts { get; set; }
        public List<object> historicalPriceUplifts { get; set; }
        public int customServiceDateNumberOfIntervals { get; set; }
        public string customServiceDateInterval { get; set; }
        public string customServiceDateProjection { get; set; }
        public int id { get; set; }
        public string uri { get; set; }
    }

    public class Planproduct
    {
        public string status { get; set; }
        public int productId { get; set; }
        public int planId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productStatus { get; set; }
        public string productDescription { get; set; }
        public string productType { get; set; }
        public string productGLCode { get; set; }
        public decimal quantity { get; set; }
        public object maxQuantity { get; set; }
        public bool isRecurring { get; set; }
        public bool isFixed { get; set; }
        public bool isOptional { get; set; }
        public bool isIncludedByDefault { get; set; }
        public bool isTrackingItems { get; set; }
        public bool chargeAtSubscriptionActivation { get; set; }
        public List<Ordertocashcycle> orderToCashCycles { get; set; }
        public string resetType { get; set; }
        public int planProductUniqueId { get; set; }
        public int id { get; set; }
        public string uri { get; set; }
    }

    public class Ordertocashcycle
    {
        public int planFrequencyId { get; set; }
        public int planProductId { get; set; }
        public int numberOfIntervals { get; set; }
        public string interval { get; set; }
        public List<Chargemodel> chargeModels { get; set; }
        public object remainingInterval { get; set; }
        public bool groupQuantityChangeCharges { get; set; }
        public object planProductPriceUplifts { get; set; }
        //  public int customServiceDateNumberOfIntervals { get; set; }
        public string customServiceDateInterval { get; set; }
        public string customServiceDateProjection { get; set; }
        public string earningInterval { get; set; }
        public int? earningNumberOfIntervals { get; set; }
        public string earningTimingInterval { get; set; }
        public string earningTimingType { get; set; }
        public Pricingmodel pricingModel { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }

    public class Pricingmodel
    {
        public string pricingModelType { get; set; }
        public List<Quantityrange> quantityRanges { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }

    public class Quantityrange
    {
        public decimal min { get; set; }
        public object max { get; set; }
        public List<Price> prices { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }

    public class Price
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }

    public class Chargemodel
    {
        public string chargeModelType { get; set; }
        public string chargeTimingType { get; set; }
        public string prorationGranularity { get; set; }
        public bool prorateOnPositiveQuantity { get; set; }
        public bool prorateOnNegativeQuantity { get; set; }
        public bool reverseChargeOnNegativeQuantity { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }

    public class Earningsettings
    {
        public string earningTimingInterval { get; set; }
        public string earningTimingType { get; set; }
    }
}












