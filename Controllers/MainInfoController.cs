using B2BWebApi.Models;
using B2BWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2BWebApi.Controllers
{
    public class MainInfoController : Controller
    {
        // GET: MainInfo
        public ActionResult MainInfoView(string CrmId)
        {
            //List<OverallInformationModel> overallInfo = new List<OverallInformationModel>();
            //overallInfo.Add(new OverallInformationModel() { ProductName = "Product1", Price = "10", Qty = "2", TotalAmount = "20" });
            //overallInfo.Add(new OverallInformationModel() { ProductName = "Product2", Price = "20", Qty = "3", TotalAmount = "60" });
            MainInfoModel model = new MainInfoModel() { OpportunityCrmId = CrmId, SeparatePayment = true/*, OverallInfo = overallInfo*/ };

            return View(model);
        }


        public ActionResult ToNextPage(MainInfoModel model)
        {
            if (model.SeparatePayment)
            {
                return RedirectToAction("SeparatePaymentView", "SeparatePayment", model.OpportunityCrmId);
            }
            else
            {
                return RedirectToAction("OverallPaymentView", "OverallPayment", new OverallPaymentModel() { OpportunityCrmId = model.OpportunityCrmId });
            }
        }
    }
}