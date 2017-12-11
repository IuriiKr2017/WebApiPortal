using B2BWebApi.Models;
using B2BWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2BWebApi.Controllers
{
    public class OpportunityController : Controller
    {
        private CrmOpportunityService crmOpportunityService;

        public OpportunityController()
        {
        }

        // GET: Opportunity
        public ActionResult OpportunityView(string CrmId)
        {
            ViewBag.ShowSubmit = true;

            //crmOpportunityService = new CrmOpportunityService();
            Session["CrmId"] = CrmId;

            //var viewData = crmOpportunityService.GetCrmOpportunity(new Guid(CrmId));
            var viewData = TestData(CrmId);
            if (viewData == null)
            {
                ViewBag.ShowSubmit = false;
                viewData = new List<OpptyToProcess>();
            }
            return View(new OpptyToProcess() { partialModel = viewData });            
        }

        private List<OpptyToProcess> TestData(string CrmId)
        {
            List<OpptyToProcess> model = new List<OpptyToProcess>();
            model.Add(new OpptyToProcess() { CrmId = CrmId });
            model.Add(new OpptyToProcess());
            model.Add(new OpptyToProcess());

            return model;
        }

        public void PostDataToCRM(List<OpptyToProcess> model)
        {
            string str = Session["CrmId"].ToString();
           
        }
    }
}