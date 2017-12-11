using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class OverallInformationModel
    {        
        public List<OverallProductInfo> ProductInfo { get; set; }
        public MainInfoAddress MainAddress { get; set; }
        public int LocationsCount { get; set; }
        public string CarrierName { get; set; }

    }
}