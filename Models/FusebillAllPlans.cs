using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    //public class FusebillAllPlans
    //{
    //    public List<AllPlans> listPlans { get; set; }
    //}
    public class AllPlans
    {
        public string code { get; set; }
        public string name { get; set; }
        public object reference { get; set; }
        public string description { get; set; }
        public object longdescription { get; set; }
        public string status { get; set; }
        public object modificationTimestamp { get; set; }
        public List<PlanfrequencyNew> planFrequencies { get; set; }
        public bool autoApplyChanges { get; set; }
        public int id { get; set; }
        public string uri { get; set; }
    }

    public class PlanfrequencyNew
    {
        public int planRevisionId { get; set; }
        public int numberOfIntervals { get; set; }
        public string interval { get; set; }
        public int numberOfSubscriptions { get; set; }
        public string status { get; set; }
        public object[] setupFees { get; set; }
        public object[] charges { get; set; }
        public bool isProrated { get; set; }
        public object prorationGranularity { get; set; }
        public int planFrequencyUniqueId { get; set; }
        public object remainingInterval { get; set; }
        public int id { get; set; }
        public object uri { get; set; }
    }












}
