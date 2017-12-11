using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BWebApi.Models
{
    public class MainInfoModel
    {
        public string OpportunityCrmId { get; set; }
        public bool SeparatePayment { get; set; }
        public List<OverallInformationModel> OverallInfo { get; set; }
    }
}