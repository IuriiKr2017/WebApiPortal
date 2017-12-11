using B2BWebApi.Models;
using B2BWebApi.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;


namespace B2BWebApi.Controllers
{
    public class PaymentController : ApiController
    {

        [HttpGet]
        public IHttpActionResult GetMainInfo(Guid crmid)
        {
            CrmOpportunityService crmOpportunityService = new CrmOpportunityService();

            OverallInformationModel overallInfo = crmOpportunityService.GetCrmOpportunity(crmid);

            OverallInformationModel overalProductInfo = crmOpportunityService.GetCrmOpportunityProduct(crmid);

            if (overallInfo != null && overalProductInfo != null)
            {
                overallInfo.ProductInfo = overalProductInfo.ProductInfo;
                overallInfo.CarrierName = overalProductInfo.CarrierName;
            }
            return Json(overallInfo);
        }

        [Route("api/Payment/PostAddresses")]
        [HttpPost]
        public IHttpActionResult PostAddresses(UpdatedMainInfoAddress address) // (MainInfoAddress address)//отдельным параметром opportunityID в JSON приходит
        {
            if (!ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }

            CrmOpportunityService crmOpportunityService = new CrmOpportunityService();

            Guid opportunityGuid = address.OpportunityId.Value;   //new Guid("F626E494-EC6D-E711-80DA-000C29838C4E");        // addressId                

            if (opportunityGuid != null)
            {
                string entityType;
                var entityId = crmOpportunityService.GetEntityId(opportunityGuid, out entityType);

                if (entityId != null && entityId != default(Guid) && entityType != null)
                {
                    Entity entityUpdate = new Entity(entityType, entityId);

                    if (address != null)
                    {
                        entityUpdate["address1_city"] = address.MainAddress.City ?? "";
                        entityUpdate["address1_country"] = address.MainAddress.Country ?? "";
                        entityUpdate["address1_stateorprovince"] = address.MainAddress.StateId ?? ""; // Instead of State used Shortcuts
                        entityUpdate["address1_line1"] = address.MainAddress.AddressLine1 ?? "";
                        entityUpdate["address1_line2"] = address.MainAddress.AddressLine2 ?? "";
                        entityUpdate["address1_postalcode"] = address.MainAddress.Zip ?? "";
                        entityUpdate["telephone1"] = address.MainAddress.Phone ?? "";
                        entityUpdate["emailaddress1"] = address.MainAddress.Email ?? "";     // address.Email;
                        crmOpportunityService.UpdateEntity(entityUpdate);

                        return Json(address);
                    }
                }
            }
            return BadRequest("Can't update \"entityUpdate\" Entity");
        }



        [Route("api/Payment/PostToFusebill")]
        [HttpPost]
        public IHttpActionResult PostToFusebill(PaymentMethodPerStore paymps) //string json
        {
            if (!ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();
            CrmOpportunityService crmOpportunityService = new CrmOpportunityService();

            #region Creating and activating customer

            int stateCodeOpen = 0; // opened oportunity
            string fusebillIdNewOrCreated = null;
            string companyNameToFusebill = null;
            int numberOfStores = paymps.Locations.Count;
            int numberOfPayments = paymps.Payments.PaymentCard.Count + paymps.Payments.PaymentACH.Count;

            if (crmOpportunityService.GetStateCode(paymps.OpportunityId.Value) != stateCodeOpen)
            {
                return BadRequest("Opportunity is not Open");
            }

            Guid opportunityGuid = paymps.OpportunityId.Value;
            var fusebillID = crmOpportunityService.GetFusebillId(opportunityGuid, out companyNameToFusebill);
            if (fusebillID != null)
            {
                fusebillIdNewOrCreated = fusebillID;
                if (!crmOpportunityService.CheckIfCustomerActive(fusebillID))
                {
                    crmOpportunityService.ActivateCustomerFusebill(fusebillID);  // activate Customer
                }
            }
            else
            {
                FusebillNewCustomer fusebillNewCustomer = new Models.FusebillNewCustomer();
                Guid accountUpdateGuid;
                fusebillNewCustomer = crmOpportunityService.GetCustomerData(paymps.OpportunityId.Value, out accountUpdateGuid);

                if (fusebillNewCustomer.CompanyName != null) // if noCompanyName it is hard to understand who the Customer is
                {
                    companyNameToFusebill = fusebillNewCustomer.CompanyName;
                    FusebillNewCustomer newCustomerObject = crmOpportunityService.PostNewCustomer(fusebillNewCustomer);            //--Create new Customer             

                    if (accountUpdateGuid != null && newCustomerObject != null)
                    {
                        fusebillIdNewOrCreated = newCustomerObject.id;
                        Entity ent = new Entity("account", accountUpdateGuid);  // add fusebullId in CRM
                        ent["b2b_fusebillid"] = newCustomerObject.id;
                        crmOpportunityService.UpdateEntity(ent); // Update Crm field b2b_fusebillid                              
                        crmOpportunityService.ActivateCustomerFusebill(newCustomerObject.id); // return some Invoice Data--- so check!             // activate Customer                         
                    }
                }
                else
                {
                    return BadRequest("No Company Name in a new Customer");
                }

                //post billing address
                if (fusebillIdNewOrCreated != null)
                {
                    crmOpportunityService.PostBillingOrShippingAddress(paymps, Convert.ToInt32(fusebillIdNewOrCreated), companyNameToFusebill, "Billing");

                    //if (numberOfStores == 1)
                    //{
                    //    crmOpportunityService.PostBillingOrShippingAddress(paymps, Convert.ToInt32(fusebillIdNewOrCreated), companyNameToFusebill, "Shipping");
                    //}
                }
                else
                {
                    return BadRequest("No Customer Fusebill Id");
                }
            }

            OverallInformationModel productsForOpportunity = crmOpportunityService.GetCrmOpportunityProduct(paymps.OpportunityId.Value); // unique for all customers

            // find Plan ID
            int fusebillPlanIdCurrent = 0;
            foreach (var id in productsForOpportunity.ProductInfo)
            {
                if (id.FusebillPlanId != 0)
                {
                    fusebillPlanIdCurrent = id.FusebillPlanId;
                    break;
                }
            }
            if (fusebillPlanIdCurrent == 0)
            {
                return BadRequest("Error no fusebillPlanIdCurrent");
            }

            Dictionary<FusebillSubscriptionModel, string> listOfSubscriptionResultFusebill = new Dictionary<FusebillSubscriptionModel, string>();

            if (numberOfStores > 1)
            {
                if (numberOfPayments == 1)
                {
                    for (int i = 0; i < numberOfStores; i++)
                    {
                        crmOpportunityService.PostNotesFusebill(fusebillIdNewOrCreated, paymps.Locations[i].AddressStore); // add notes with few store addresses
                    }
                }
                else if (numberOfPayments > 1)
                {
                    for (int storeNum = 0; storeNum < numberOfStores; storeNum++)
                    {
                        FusebillNewCustomer fusebillNewCustomerChild = new FusebillNewCustomer();
                        fusebillNewCustomerChild.CompanyName = paymps.Locations[storeNum].AddressStore.AddressLine1;
                        fusebillNewCustomerChild.ParentId = fusebillIdNewOrCreated; //this customer will have a parent, which was already created as a parent with no subscription and no payment methods
                        var customerChild = crmOpportunityService.PostNewCustomer(fusebillNewCustomerChild);            //--Create new Customer 

                        if (customerChild != null)
                        {
                            crmOpportunityService.ActivateCustomerFusebill(customerChild.id);  // activate Customer
                            crmOpportunityService.PostBillingOrShippingAddress(paymps, Convert.ToInt32(customerChild.id), companyNameToFusebill, "Billing");
                            paymps.Locations[storeNum].FusebillId = customerChild.id;

                            bool paymentMethodResult1 = crmOpportunityService.PostPaymentMethodFusebill(paymps, storeNum); // Convert.ToDecimal(customerChild.id), paymps.Locations[i].PaymentGuid, paymps.Locations[i]);
                            if (!paymentMethodResult1)
                            {
                                return BadRequest("Error in payment Method with numberOfStores. Store number " + storeNum);
                            }

                            FusebillSubscriptionModel subscriptionResultFusebill1 = crmOpportunityService.PostSubscriptionToFusebill(Convert.ToDecimal(customerChild.id), fusebillPlanIdCurrent);
                            listOfSubscriptionResultFusebill.Add(subscriptionResultFusebill1, customerChild.id); // Same to find, which address was paid and which is not
                        }
                    }
                }
            }

            // for one store with one payment
            if (numberOfPayments == 1)
            {
                int firstStore = 0;
                paymps.Locations[firstStore].FusebillId = fusebillIdNewOrCreated;
                ///////////////////////////////// createPayment Method ACH/Credit Card
                bool paymentMethodResult = crmOpportunityService.PostPaymentMethodFusebill(paymps, firstStore); // Convert.ToDecimal(fusebillIdNewOrCreated), paymps.Locations[0].PaymentGuid, paymps.Locations[0]);
                if (!paymentMethodResult)
                {
                    return BadRequest("Error in payment Method");
                }
                // POST Subscription
                FusebillSubscriptionModel subscriptionResultFusebill = crmOpportunityService.PostSubscriptionToFusebill(Convert.ToDecimal(fusebillIdNewOrCreated), fusebillPlanIdCurrent);
                listOfSubscriptionResultFusebill.Add(subscriptionResultFusebill, fusebillIdNewOrCreated);
            }


            #endregion

            // Update Data       

            Dictionary<int, bool> hasPaidInvoiceDict = new Dictionary<int, bool>();    // Dictionary for Subscription Id and if hasPaidInvoice for this subscription              
            int fusebillIdCounter = 0;

            foreach (var subscr in listOfSubscriptionResultFusebill)
            {
                foreach (var productSubcsr in subscr.Key.subscriptionProducts)
                {
                    foreach (var prodCrm in productsForOpportunity.ProductInfo)
                    {
                        if (productSubcsr.planProduct.productId == prodCrm.FusebillProductId)
                        {
                            var product = crmOpportunityService.GetCurrentFusebillProduct(productSubcsr.id);
                            if (product != null)
                            {
                                //here we Update Quantity and id from Crm to fusebill
                                Subscriptionproduct subscrProduct = Helper.JsonHelper.GetObjectFromJSON<Subscriptionproduct>(product);
                                subscrProduct.isIncluded = true; /// add Product for pay

                                subscrProduct.quantity = Convert.ToDecimal(prodCrm.Qty) / listOfSubscriptionResultFusebill.Count;
                                subscrProduct.subscriptionId = subscr.Key.id;
                                // find matched products in Crm and Update FusebillProducts
                                crmOpportunityService.UpdateFusebillProduct(subscrProduct); //and add it 

                                //here we Update price and id from Crm to fusebill
                                FusebillSubscriptionProductPrice fusebillUpdatePrice = new Models.FusebillSubscriptionProductPrice();
                                fusebillUpdatePrice.id = productSubcsr.id;
                                fusebillUpdatePrice.chargeAmount = Convert.ToDecimal(prodCrm.Price);
                                crmOpportunityService.UpdateFusebillProductPrice(fusebillUpdatePrice);   // Update Price 
                                                                                                         // Include   !!! 
                            }
                            else { return BadRequest("Bad product with id " + subscr.Key.id); }
                        }
                    }
                }

                if (subscr.Key != null)
                {
                    var resultActivation = crmOpportunityService.ActivateSubscriptionFusebill(subscr.Key.id);
                    if (resultActivation != null)
                    {
                        List<InvoiceData> draftInvoices = crmOpportunityService.GetAllDraftInvoices(paymps.Locations[fusebillIdCounter].FusebillId);

                        foreach (var activeInvoice in draftInvoices)
                        {
                            if (activeInvoice.status == "Ready")
                            {
                                // if (Auto post draft invoices BUTTON is OFF) then we activate invoices                                                                         
                                crmOpportunityService.ActivateDraftInvoice(Convert.ToString(activeInvoice.id));
                            }
                        }

                        List<InvoiceProductsData> allFusebillInvoices = crmOpportunityService.GetAllInvoicesProducts(paymps.Locations[fusebillIdCounter].FusebillId);   //get allInvoices
                        bool hasPaidInvoice = false;

                        foreach (var inv in allFusebillInvoices)
                        {
                            foreach (var pmtShedule in inv.paymentSchedules)
                            {
                                if (pmtShedule.status == "Paid")
                                {
                                    hasPaidInvoice = true;
                                    break;
                                }
                            }
                            if (hasPaidInvoice)
                            {
                                break;
                            }

                            hasPaidInvoice = false;
                        }
                        hasPaidInvoiceDict.Add(subscr.Key.id, hasPaidInvoice);

                    }
                }
                fusebillIdCounter++;
            } // end of the cycle

            bool allInvoicesArePaid = false;
            string errorNotAllPaid = null;
            List<string> notPaidInvFusebillId = new List<string>();

            //-- Create Error Message 
            foreach (var item in hasPaidInvoiceDict)
            {
                if (item.Value == false)
                {
                    allInvoicesArePaid = false; // so some invoices are not paid                       

                    foreach (var findbyId in listOfSubscriptionResultFusebill)
                    {
                        if (findbyId.Key.id == item.Key)
                        {
                            notPaidInvFusebillId.Add(findbyId.Value); // here we get Fusebilll Ids not Paid
                        }
                    }
                }
            }

            foreach (var loc in paymps.Locations)
            {
                foreach (var idNotPaid in notPaidInvFusebillId)
                {
                    if (loc.FusebillId == idNotPaid)
                    {
                        errorNotAllPaid += " " + loc.AddressStore.AddressLine1 + " = " + loc.FusebillId + " ; ";
                    }
                }
            }

            //-- end

            if (errorNotAllPaid == null) // null, so no errors
            {
                allInvoicesArePaid = true;          
                crmOpportunityService.CloseOpportunity(Convert.ToString(paymps.OpportunityId.Value), allInvoicesArePaid);    //won or lost   // but now only Won possible
            }

            if (!allInvoicesArePaid)   // Cancel Subscription
            {
                foreach (var item in hasPaidInvoiceDict)
                {
                    if (!item.Value)   // Cancel only not paid Invoices
                    {
                        crmOpportunityService.CancelSubscription(item.Key);
                    }
                }
                return BadRequest(" These Invoices were not paid (address line = Customer id ) " + errorNotAllPaid);
            }

            // the code that you want to measure comes here
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds / 1000;
            return Json("OK " + elapsedMs);



            #region Payment TYpe From Json

            #endregion
        }


        [Route("api/Payment/PostAuthentication")]
        [HttpPost]
        public IHttpActionResult PostAuthentication()
        {
            return Json("Ok");

        }

        // [HttpPost]
        // public IHttpActionResult PostSingleAddress()
        // {    
        //     return Json("ok");
        // }


    }
}
