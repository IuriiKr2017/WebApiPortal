using System;
using System.Collections.Generic;
using System.Linq;
using B2BWebApi.Models.CrmModel;
using B2BWebApi.Repository;
using Microsoft.Xrm.Sdk;
using B2BWebApi.Helper;
using Microsoft.Xrm.Sdk.Query;
using B2BWebApi.Application;
using B2BWebApi.Cache;
using B2BWebApi.Models;
using System.Net;
using System.IO;
using System.Web.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Crm.Sdk.Messages;
using System.Web.Configuration;

namespace B2BWebApi.Services
{

    public class CrmOpportunityService
    {
        private IOrganizationService service;
        private CrmRepository<Opportunity> opportunityRepository;
        private CrmRepository<OpportunityProduct> opportunityRepositoryProduct;      
        private string apiKey = WebConfigurationManager.AppSettings["apiKeyFusebill"];
         // private string apiKey = "MDpPRkJNb2Q4ek9uZUxnQ2I5b3loR2hPY2RHdktWaWFUWlNab0N3M2ZseVVnTVNaZTdvVjJIdHBJY0NBV0RwU0hN";

        public CrmOpportunityService()
        {
            service = ApplicationContext.Current.CacheManager.Get<IOrganizationService>(string.Format(CacheKeyPatterns.CRM_SERVICE, "service"), (int)TimeSpan.FromMinutes(10).TotalMilliseconds, () =>
            {              
                return ConnectionHelper.GetCrmConnection();
            });

            opportunityRepository = new CrmRepository<Opportunity>(service);
            opportunityRepositoryProduct = new Repository.CrmRepository<Models.CrmModel.OpportunityProduct>(service);

        }

        public OverallInformationModel GetCrmOpportunityProduct(Guid CrmId)
        {
            QueryExpression query = new QueryExpression(OpportunityProduct.EntityLogicalName);
            query.ColumnSet = new ColumnSet("priceperunit", "productid", "b2b_licensesnumber", "baseamount");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, CrmId);

            var foundOppProducts = opportunityRepositoryProduct.GetEntitiesByQuery(query);
            string carrierName = null;

            if (foundOppProducts != null)
            {
                var productList = new List<Models.OverallProductInfo>();
              

                foreach (var product in foundOppProducts)
                {
                    var prodd = product.GetAttributeValue<EntityReference>("productid").Id;
                    var entProduct = service.Retrieve("product", prodd, new ColumnSet("b2b_fusebillproductid", "b2b_fusebillplanid", "b2b_carrierid"));
                    var fusebillProductId = entProduct.GetAttributeValue<string>("b2b_fusebillproductid");
                    var fusebillPlanId = entProduct.GetAttributeValue<string>("b2b_fusebillplanid");

                    var entRefCarrierName = entProduct.GetAttributeValue<EntityReference>("b2b_carrierid");
                    if (carrierName == null)
                    {
                        carrierName = entRefCarrierName.Name;
                    }

                    OverallProductInfo productInfo = new Models.OverallProductInfo()
                    {
                        Price = product.GetAttributeValue<Money>("priceperunit").Value,
                        ProductName = product.GetAttributeValue<EntityReference>("productid").Name,
                        Qty = product.GetAttributeValue<int>("b2b_licensesnumber").ToString(), //quantity
                        TotalAmount = product.GetAttributeValue<Money>("baseamount").Value,
                        //("b2b_fusebillproductid"),
                        FusebillProductId = Convert.ToInt32(fusebillProductId),
                        FusebillPlanId = Convert.ToInt32(fusebillPlanId)                      
                    };
                    productList.Add(productInfo);
                }

                return new OverallInformationModel()
                {
                    ProductInfo = productList,
                    CarrierName = carrierName
                };
            }
            return null;
        }


        public OverallInformationModel GetCrmOpportunity(Guid CrmId)
        {
            QueryExpression query = new QueryExpression(Opportunity.EntityLogicalName);
            query.ColumnSet = new ColumnSet("parentaccountid", "b2b_locationsnumber");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, CrmId);

            query.LinkEntities.Add(new LinkEntity(Opportunity.EntityLogicalName, Account.EntityLogicalName, "parentaccountid", "accountid", JoinOperator.Natural)
            { EntityAlias = "account", Columns = new ColumnSet("address1_city", "address1_country", "address1_stateorprovince", "address1_line1", "address1_line2", "address1_postalcode", "telephone1", "emailaddress1") });

            var foundOppty = opportunityRepository.GetEntitiesByQuery(query).FirstOrDefault();
            if (foundOppty != null)
            {
                MainInfoAddress mainAddress = new Models.MainInfoAddress()
                {
                    Country = foundOppty.Contains("account.address1_country") ? ((AliasedValue)foundOppty["account.address1_country"]).Value.ToString() : "USA",
                    City = foundOppty.Contains("account.address1_city") ? ((AliasedValue)foundOppty["account.address1_city"]).Value.ToString() : "",
                   // State = foundOppty.Contains("account.address1_stateorprovince") ? ((AliasedValue)foundOppty["account.address1_stateorprovince"]).Value.ToString() : "",
                    StateId = foundOppty.Contains("account.address1_stateorprovince") ? ((AliasedValue)foundOppty["account.address1_stateorprovince"]).Value.ToString().ToUpper() : "",

                    AddressLine1 = foundOppty.Contains("account.address1_line1") ? ((AliasedValue)foundOppty["account.address1_line1"]).Value.ToString() : "",
                    AddressLine2 = foundOppty.Contains("account.address1_line2") ? ((AliasedValue)foundOppty["account.address1_line2"]).Value.ToString() : "",
                    Zip = foundOppty.Contains("account.address1_postalcode") ? ((AliasedValue)foundOppty["account.address1_postalcode"]).Value.ToString() : "",
                    Phone = foundOppty.Contains("account.telephone1") ? ((AliasedValue)foundOppty["account.telephone1"]).Value.ToString() : "",
                    Email = foundOppty.Contains("account.emailaddress1") ? ((AliasedValue)foundOppty["account.emailaddress1"]).Value.ToString() : "",
                };
                return new OverallInformationModel()
                {
                    LocationsCount = foundOppty.b2b_LocationsNumber.HasValue ? foundOppty.b2b_LocationsNumber.Value : 0,
                    MainAddress = mainAddress
                };
            }
            else  // if there is no apportunities in Account, we search whem in Contact
            {
                QueryExpression queryContact = new QueryExpression(Opportunity.EntityLogicalName);
                queryContact.ColumnSet = new ColumnSet("parentcontactid", "b2b_locationsnumber");
                queryContact.Criteria = new FilterExpression();
                queryContact.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, CrmId);

                queryContact.LinkEntities.Add(new LinkEntity(Opportunity.EntityLogicalName, Contact.EntityLogicalName, "parentcontactid", "contactid", JoinOperator.Natural)
                { EntityAlias = "contact", Columns = new ColumnSet("address1_city", "address1_country", "address1_stateorprovince", "address1_line1", "address1_line2", "address1_postalcode", "telephone1", "emailaddress1") });

                var foundOpptyContact = opportunityRepository.GetEntitiesByQuery(queryContact).FirstOrDefault(); // we get a list of with a Model Opportunity (with all fields)

                if (foundOpptyContact != null)
                {
                    MainInfoAddress mainAddress = new Models.MainInfoAddress()
                    {
                        Country = foundOpptyContact.Contains("contact.address1_country") ? ((AliasedValue)foundOpptyContact["contact.address1_country"]).Value.ToString() : "USA",
                        City = foundOpptyContact.Contains("contact.address1_city") ? ((AliasedValue)foundOpptyContact["contact.address1_city"]).Value.ToString() : "",
                        //State = foundOpptyContact.Contains("contact.address1_stateorprovince") ? ((AliasedValue)foundOpptyContact["contact.address1_stateorprovince"]).Value.ToString() : "",
                        StateId = foundOpptyContact.Contains("contact.address1_stateorprovince") ? ((AliasedValue)foundOpptyContact["contact.address1_stateorprovince"]).Value.ToString().ToUpper() : "",
                        AddressLine1 = foundOpptyContact.Contains("contact.address1_line1") ? ((AliasedValue)foundOpptyContact["contact.address1_line1"]).Value.ToString() : "",
                        AddressLine2 = foundOpptyContact.Contains("contact.address1_line2") ? ((AliasedValue)foundOpptyContact["contact.address1_line2"]).Value.ToString() : "",
                        Zip = foundOpptyContact.Contains("contact.address1_postalcode") ? ((AliasedValue)foundOpptyContact["contact.address1_postalcode"]).Value.ToString() : "",
                        Phone = foundOpptyContact.Contains("contact.telephone1") ? ((AliasedValue)foundOpptyContact["contact.telephone1"]).Value.ToString() : "",
                        Email = foundOpptyContact.Contains("contact.emailaddress1") ? ((AliasedValue)foundOpptyContact["contact.emailaddress1"]).Value.ToString() : "",
                    };
                    return new OverallInformationModel()
                    {
                        LocationsCount = foundOpptyContact.b2b_LocationsNumber.HasValue ? foundOpptyContact.b2b_LocationsNumber.Value : 0,
                        MainAddress = mainAddress
                    };
                }
                return null;
            }
        }


        public Guid GetEntityId(Guid OpportunityId, out string entType)
        {
            var entOpportunity = service.Retrieve("opportunity", OpportunityId, new ColumnSet("parentaccountid"));
            var entRef = entOpportunity.GetAttributeValue<EntityReference>("parentaccountid");
            var accoundID = default(Guid);
            if (entRef != null)
            {
                accoundID = entRef.Id;
            }

            if (accoundID != null)
            {
                entType = "account";
                return accoundID;
            }
            else
            {
                var entOpportunity2 = service.Retrieve("opportunity", OpportunityId, new ColumnSet("parentcontactid"));
                var entRef2 = entOpportunity2.GetAttributeValue<EntityReference>("parentcontactid");
                var contactID = default(Guid);
                if (entRef != null)
                {
                    contactID = entRef2.Id;
                }
                entType = "contact";
                return contactID;
            }

        }
        public void UpdateEntity(Entity ent)
        {
            service.Update(ent);
        }

        public Guid GetAccountIdByOpportunityId(Guid oppoId)
        {
            var entOpportunity = service.Retrieve("opportunity", oppoId, new ColumnSet("parentaccountid"));
            var accoundGuid = entOpportunity.GetAttributeValue<EntityReference>("parentaccountid").Id;
            return accoundGuid;
        }

        public string GetFusebillId(Guid OpportunityId, out string companyName)
        {
            var entOpportunity = service.Retrieve("opportunity", OpportunityId, new ColumnSet("parentaccountid"));
            var accoundID = entOpportunity.GetAttributeValue<EntityReference>("parentaccountid").Id;

            var fusebillEntity = service.Retrieve("account", accoundID, new ColumnSet("b2b_fusebillid", "name"));
            var fusebillID = fusebillEntity.GetAttributeValue<string>("b2b_fusebillid");
            companyName = fusebillEntity.GetAttributeValue<string>("name") ?? null;

            return fusebillID;
        }

        public int GetStateCode(Guid OpportunityId)
        {
            var entOpportunity = service.Retrieve("opportunity", OpportunityId, new ColumnSet("statecode"));
            var stateCode = entOpportunity.GetAttributeValue<OptionSetValue>("statecode");
            // 0 - opportunity Open , 1 Won, 2 Lost
            return stateCode.Value;
        }


        // convert time from UTS Js 1970
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        public FusebillNewCustomer GetCustomerData(Guid OpportunityId, out Guid accountUpdate)
        {
            FusebillNewCustomer fNewCustomer = new FusebillNewCustomer();
            var entOpportunity = service.Retrieve("opportunity", OpportunityId, new ColumnSet("parentaccountid"));
            var accRef = entOpportunity.GetAttributeValue<EntityReference>("parentaccountid");

            var accountID = default(Guid);
            if (accRef != null)
            {
                accountID = accRef.Id;
            }

            accountUpdate = accountID;
            if (accountID != null)
            {
                var accountEntity = service.Retrieve("account", accountID, new ColumnSet("primarycontactid", "name", "telephone1", "emailaddress1", "accountnumber", "parentaccountid"));
                fNewCustomer.CompanyName = accountEntity.GetAttributeValue<string>("name") ?? "";
                //fNewCustomer.ParentId = accountEntity.GetAttributeValue<EntityReference>("parentaccountid") ??  "");             
                fNewCustomer.PrimaryPhone = accountEntity.GetAttributeValue<string>("telephone1") ?? "";
                fNewCustomer.PrimaryEmail = accountEntity.GetAttributeValue<string>("emailaddress1") ?? "";
                fNewCustomer.Reference = accountEntity.GetAttributeValue<string>("accountnumber") ?? "";

                var primaryContact = accountEntity.GetAttributeValue<EntityReference>("primarycontactid");


                if (primaryContact != null)
                {
                    var primarycontactid = primaryContact.Id;

                    if (primarycontactid != Guid.Empty)
                    {
                        var entContact = service.Retrieve("contact", primarycontactid, new ColumnSet("b2b_title", "b2b_postfix", "middlename", "firstname", "lastname", "mobilephone", "emailaddress2"));

                        fNewCustomer.Title = entContact.GetAttributeValue<string>("b2b_title") ?? "";
                        fNewCustomer.Suffix = entContact.GetAttributeValue<string>("b2b_postfix") ?? "";
                        fNewCustomer.FirstName = entContact.GetAttributeValue<string>("firstname") ?? "";
                        fNewCustomer.MiddleName = entContact.GetAttributeValue<string>("middlename") ?? "";
                        fNewCustomer.LastName = entContact.GetAttributeValue<string>("lastname") ?? "";
                        fNewCustomer.SecondaryPhone = entContact.GetAttributeValue<string>("mobilephone") ?? "";
                        fNewCustomer.SecondaryEmail = entContact.GetAttributeValue<string>("emailaddress2") ?? "";
                    }
                }
            }
            return fNewCustomer;
        }


        public FusebillNewCustomer PostNewCustomer( FusebillNewCustomer fusebillNewCustomer)
        {
            //Json Payload
            string jsonDataCustomer = Helper.JsonHelper.GetJSONFromObject(fusebillNewCustomer);  //"{'firstName':'John', 'middleName': 'Gono',  'lastName':'Doe', 'suffix': 'sfx', 'title':'new_title'}";                                    
                                                                                                 //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/customers");
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonDataCustomer);
                streamWriter.Flush();
                streamWriter.Close();
            }
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return Helper.JsonHelper.GetObjectFromJSON<FusebillNewCustomer>(result);
        }


        public List<AllPlans> GetAllPlans()
        {
            //query parameter
            string status = "Active";          
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/Plans/?query=status:" + status);
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "GET";
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            List<AllPlans> res =  Helper.JsonHelper.GetObjectFromJSON< List<AllPlans> >(result);// List < FusebillAllPlans >
            return res;
        }

        public FusebillSubscriptionModel PostSubscriptionToFusebill( decimal customerId, int fusebillPlanId )
        {
            List<AllPlans> allActivePlans = GetAllPlans();
            string planNamex = null;
            string planFreqIdx = null;

            foreach (var plan in allActivePlans)
            {
                if (plan.id == fusebillPlanId)
                {
                    foreach (var freq in plan.planFrequencies)                    
                    {
                        planFreqIdx = Convert.ToString(freq.id);
                        planNamex = plan.code;
                        break;
                    }
                }
            }           

            //NextPeriodStartDate
            FusebillPostSubscription fsubscr = new FusebillPostSubscription();

            fsubscr.code = Convert.ToString(fusebillPlanId);                                                       //System.Configuration.ConfigurationManager.AppSettings["planId"];    //"14490";
            fsubscr.customerid = customerId;                                                                        // 279413;  //  279427; // dynamic Customer
            fsubscr.name = planNamex;                         //System.Configuration.ConfigurationManager.AppSettings["planName"];   // "mainplan";     //This is the name of the Plan of which this Subscription is an instance.
            fsubscr.reference = null;
            fsubscr.planFrequencyId = planFreqIdx;                                          //System.Configuration.ConfigurationManager.AppSettings["planFrequencyId"];        //"17766"; 
            fsubscr.hasPostedInvoice = true;                       
            //Json Payload
            string jsonDataSubscr = Helper.JsonHelper.GetJSONFromObject(fsubscr);        
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/subscriptions");
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonDataSubscr);
                streamWriter.Flush();
                streamWriter.Close();
            }
            HttpWebResponse httpResponse = null;
            try
            {               
                httpResponse = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                var ResEX = ex.Message + " Subscription Error " ;                
            }

            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }                   

            FusebillSubscriptionModel newSubscription = Helper.JsonHelper.GetObjectFromJSON<FusebillSubscriptionModel>(result);
            return newSubscription;
        }

        public string ActivateSubscriptionFusebill(int subscriptionId)
        {
            //Json Payload
            // string jsonData = "{customerId:{customerId}, planCode:'PlanCode', planName:'Plan Name'}";
            //path parameter, subscriptionId
            int pathParameter = subscriptionId;

            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/SubscriptionActivation/" + pathParameter);
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(string.Empty);//(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpWebResponse httpResponse = null;
            try
            {
                //Perform the request
                httpResponse = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                var ResEX = ex.Message + " Subscription Activation Error ";
               
            }
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;

        }

        public List<InvoiceData> GetAllDraftInvoices(string fusebillIdNewOrCreated)
        {
            //query parameter
            string customerId = fusebillIdNewOrCreated;        
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/customers/" + customerId + "/DraftInvoices/");
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "GET";
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            List<InvoiceData> res = Helper.JsonHelper.GetObjectFromJSON<List<InvoiceData>>(result);
            return res;
        }

        public FusebillActiveInvoice ActivateDraftInvoice(string draftInvoiceId)
        {          
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/Invoices?draftInvoiceId=" + draftInvoiceId);
            //Set Content Length
            request.ContentLength = 0;
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            FusebillActiveInvoice activeInvoice = Helper.JsonHelper.GetObjectFromJSON<FusebillActiveInvoice>(result);

            return activeInvoice;

        }


        public string ActivateCustomerFusebill( string customerIDFusebill)
        {

            FusebillActivateCustomer fActivateCustomer = new Models.FusebillActivateCustomer();
            fActivateCustomer.customerID = customerIDFusebill;

            //fActivateCustomer.prewiev = false;
            //Query string
            string previewQuery = "false";
            //Json Payload
            string jsonData = Helper.JsonHelper.GetJSONFromObject(fActivateCustomer); //  { 'customerID':278575,   'prewiev':false}; //"{'customerID':{id},'activateAllSubscriptions':false}";
            //Setup API key
            //           
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/CustomerActivation?preview=" + previewQuery);
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }


        public bool CheckIfCustomerActive(string customerIDFusebill)
        {
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/customers/" + customerIDFusebill);
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "GET";
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            var newFusebillActivateCustomer = Helper.JsonHelper.GetObjectFromJSON<FusebillActivateCustomer>(result);
            if (newFusebillActivateCustomer != null)
            {
                return (newFusebillActivateCustomer.status == "Active");
            }
            return false;
        }


        public static Tuple<string, int> GetStateById(string stateCode)
        {
            var stateIdCodeList = new Helper.TupleList<string, string, int>
            {
                #region All Codes
                 {  "AB", "Alberta", 1},
                  {  "BC", "British Columbia", 2},
                  {  "MB", "Manitoba", 3},
                  {  "NB", "New Brunswick", 4},
                  {  "NL", "Newfoundland and Labrador", 5},
                  {  "NT", "Northwest Territories", 6},
                  {  "NS", "Nova Scotia", 7},
                  {  "NU", "Nunavut", 8},
                  {  "ON", "Ontario", 9},
                  {  "PE", "Prince Edward Island", 10},
                  {  "QC", "Quebec", 11},
                  {  "SK", "Saskatchewan", 12},
                  {  "YT", "Yukon Territory", 13},
                  {  "AK", "Alaska", 14},
                  {  "AL", "Alabama", 15},
                  {  "AZ", "Arizona", 16},
                  {  "AR", "Arkansas", 17},
                  {  "CA", "California", 18},
                  {  "CO", "Colorado", 19},
                  {  "CT", "Connecticut", 20},
                  {  "DE", "Delaware", 21},
                  {  "DC", "District of Columbia", 22},
                  {  "FL", "Florida", 23},
                  {  "GA", "Georgia", 24},
                  {  "HI", "Hawaii", 25},
                  {  "ID", "Idaho", 26},
                  {  "IL", "Illinois", 27},
                  {  "IN", "Indiana", 28},
                  {  "IA", "Iowa", 29},
                  {  "KS", "Kansas", 30},
                  {  "KY", "Kentucky", 31},
                  {  "LA", "Louisiana", 32},
                  {  "ME", "Maine", 33},
                  {  "MD", "Maryland", 34},
                  {  "MA", "Massachusetts", 35},
                  {  "MI", "Michigan", 36},
                  {  "MN", "Minnesota", 37},
                  {  "MS", "Mississippi", 38},
                  {  "MO", "Missouri", 39},
                  {  "MT", "Montana", 40},
                  {  "NE", "Nebraska", 41},
                  {  "NV", "Nevada", 42},
                  {  "NH", "New Hampshire", 43},
                  {  "NJ", "New Jersey", 44},
                  {  "NM", "New Mexico", 45},
                  {  "NY", "New York", 46},
                  {  "NC", "North Carolina", 47},
                  {  "ND", "North Dakota", 48},
                  {  "OH", "Ohio", 49},
                  {  "OK", "Oklahoma", 50},
                  {  "OR", "Oregon", 51},
                  {  "PA", "Pennsylvania", 52},
                  {  "PR", "Puerto Rico", 53},
                  {  "RI", "Rhode Island", 54},
                  {  "SC", "South Carolina", 55},
                  {  "SD", "South Dakota", 56},
                  {  "TN", "Tennessee", 57},
                  {  "TX", "Texas", 58},
                  {  "UT", "Utah", 59},
                  {  "VT", "Vermont", 60},
                  {  "VA", "Virginia", 61},
                  {  "WA", "Washington", 62},
                  {  "WV", "West Virginia", 63},
                  {  "WI", "Wisconsin", 64},
                  {  "WY", "Wyoming", 65}

            };

            #endregion

            foreach (var item in stateIdCodeList)
            {
                if (item.Item1 == stateCode)
                {
                    return new Tuple<string, int>( item.Item2, item.Item3 );
                }
               
            }
            return null;
        }

   
        public void PostNotesFusebill(string fusebillCustomerId, MainInfoAddress address)
        {
            var res = "Address line 1 " + address.AddressLine1 + " Address line 2 " + address.AddressLine2 + " Country " + address.Country + " State " + address.StateId + " City " +  address.City + " Postal / Zip " + address.Zip;

            FusebillNotes newNote = new FusebillNotes();
            newNote.customerId = fusebillCustomerId;
            newNote.note = res;

            //Json data, Fusebill Id which corresponds to the customerId Field
            string jsonData = Helper.JsonHelper.GetJSONFromObject(newNote);  // "{'customerId':{customerId},'note':'Test Note String'}";
        
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/customerNotes");
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
        }
        public void PostBillingOrShippingAddress(PaymentMethodPerStore paymps, int fusebillCustomerId, string companyNameToFusebill, string ShippingOrBilling)
        {
            //There is no State in incoming (PaymentMethodPerStore paymps) -- only StateId
            FusebillBillingAddress fBillingAdress = new FusebillBillingAddress();
            fBillingAdress.customerAddressPreferenceId = fusebillCustomerId;


            MainInfoAddress typeOfAddress = null;

            if (ShippingOrBilling == null)
            {
                return;
            }
            else if (ShippingOrBilling.ToLower() == "billing")
            {
                typeOfAddress = paymps.Locations[0].AddressBilling;
            }
            else if (ShippingOrBilling.ToLower() == "shipping")
            {
                typeOfAddress = paymps.Locations[0].AddressStore;
            }

            if (paymps.Payments.PaymentACH.Count + paymps.Payments.PaymentCard.Count > 1)
            {
                fBillingAdress.companyName = paymps.Locations[0].AddressBilling.AddressLine1 + " - " + companyNameToFusebill ?? null; // check for shipping
            }
            else
            {
                fBillingAdress.companyName = companyNameToFusebill ?? null;
            }


            fBillingAdress.line1 = typeOfAddress.AddressLine1 ?? null;
            fBillingAdress.line2 = typeOfAddress.AddressLine2 ?? null;
            fBillingAdress.country = typeOfAddress.Country == "Canada" ? "CAN" : "USA";             
            fBillingAdress.countryId = fBillingAdress.country == "CAN" ? "124" : "840";             //124;   for USA its 840( List of Numeric Country Codes)        

            var stateTuple = GetStateById(typeOfAddress.StateId);
            fBillingAdress.state = stateTuple.Item1 ?? null;  //State                                                                  

            fBillingAdress.city = typeOfAddress.City ?? null;
            fBillingAdress.postalZip = typeOfAddress.Zip ?? null;
            fBillingAdress.addressType = ShippingOrBilling;  //"Billing";

            //Json data, Fusebill Id which corresponds to the customerId Field
            string jsonData = Helper.JsonHelper.GetJSONFromObject(fBillingAdress);
            //"{customerAddressPreferenceId:{customerId},companyName:'Fusebill',line1:'232 Herzberg Road',line2:'Suite 203',countryId:124,country:'Canada',stateId:9,state:'Ontario',city:'Kanata',postalZip:'K2K 2A1',addressType:'Billing'}";
         
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/Addresses");
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
        }
        public bool PostPaymentMethodFusebill(PaymentMethodPerStore paymps, int storeNumber) //      decimal fusebillCustomerId, string paymentGuid, Locations loc)
        {
            // if credit Card else ACH
            string paymentMethodResult;
            string jsonData = FillPaymentTypeFusebill(paymps, out paymentMethodResult, storeNumber); //             fusebillCustomerId, paymentGuid, loc);

            // "{customerId:{customerId},cardNumber:4111111111111111,firstName:'John',lastName:'Doe',expirationMonth:01,expirationYear:20,cvv:123,address1:'232 Herzberg Road',address2:'Suite 203',countryId:124,stateId:9,city:'Kanata',postalZip:'K2K 2A1',isDefault:true}";
            if (paymentMethodResult != null && jsonData != null)
            {
                //Configure URI
                WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/paymentMethods/" + paymentMethodResult);  //creditCard
                                                                                                                                     //Add Content type
                request.ContentType = "application/json";
                //Add Api key authorization
                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                //Set request method
                request.Method = "POST";
                //Add the json data to request
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //Perform the request
                var httpResponse = (HttpWebResponse)request.GetResponse();
                //Record the response from our request
                var result = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        
        public string FillPaymentTypeFusebill(PaymentMethodPerStore paymps, out string paymentMethodResult,  int storeNumber) //decimal fusebillCustomerId, string paymentGuid, Locations loc)
        {
            string jsonData = null;
            //There is no State in incoming (PaymentMethodPerStore paymps) -- only StateId   

            PayCard currentPaymentCard = null;
            for (int i = 0; i < paymps.Payments.PaymentCard.Count; i++)
            {
                if (paymps.Locations[storeNumber].PaymentGuid == paymps.Payments.PaymentCard[i].PaymentGuid) 
                {
                    currentPaymentCard = paymps.Payments.PaymentCard[i];
                    break;
                }
            }
            Paymentach currentPaymentACH = null;
            // search in ACH methods 
            if (currentPaymentCard == null)
            {
                for (int i = 0; i < paymps.Payments.PaymentACH.Count; i++)
                {
                    if (paymps.Locations[storeNumber].PaymentGuid == paymps.Payments.PaymentACH[i].PaymentGuid)
                    {
                        currentPaymentACH = paymps.Payments.PaymentACH[i];
                        break;
                    }
                }
            }

            Locations loc = paymps.Locations[storeNumber];

            if (currentPaymentACH != null) // one of the main field have to be filled  if (paymps != null && paymps.Locations[0].PaymentACH != null)
            {
                FusebillACH_PaymentMethod achMethod = new FusebillACH_PaymentMethod();

                achMethod.customerId = Convert.ToDecimal(paymps.Locations[storeNumber].FusebillId);
                achMethod.accountNumber = currentPaymentACH.BankAccountNumber ?? ""; //"01234567890";
                achMethod.transitNumber = currentPaymentACH.ABARoutingNumber ?? ""; // same as routing number  //"1234";
                //I use billing address

                achMethod.bankAccountType = currentPaymentACH.AccountType == "Checking" ? "CHQ" : "SAV";      // "CHQ";      // It stands for Checking, Savings

                achMethod.address1 = loc.AddressBilling.AddressLine1 ?? ""; // "232 Herzberg Road";
                achMethod.address2 = loc.AddressBilling.AddressLine2 ?? ""; //"Suite 203";

                var stateTuple = GetStateById(loc.AddressBilling.StateId);

                achMethod.stateId = stateTuple.Item2;     //9;

                achMethod.city = loc.AddressBilling.City ?? "";   // "Kanata"; 
                achMethod.country = loc.AddressBilling.Country == "Canada" ? "CAN" : "USA";
                achMethod.countryId = achMethod.country == "CAN" ? 124 : 840;    //124;   for USA its 840( List of Numeric Country Codes)
                achMethod.postalZip = loc.AddressBilling.Zip ?? ""; //"K2K 2A1";
                achMethod.source = "Manual" ?? ""; // "Manual"/ "Automatic" and "SelfServicePortal".     

                achMethod.firstName = currentPaymentACH.AccountHolderFirstName; //"John";
                achMethod.lastName = currentPaymentACH.AccountHolderLastName; //"Doe";  

                jsonData = B2BWebApi.Helper.JsonHelper.GetJSONFromObject(achMethod) ?? "";
                paymentMethodResult = "achCard";
                return jsonData;
            }
            else if (currentPaymentCard != null)  // paymps.Locations[0].PaymentCard.CardNumber != null)
            {

                FusebillCreditCardPayment ccPayment = new FusebillCreditCardPayment();
                //17 fields to fill/ 2 of them is stateId/countryId

                ccPayment.customerId = Convert.ToDecimal(paymps.Locations[storeNumber].FusebillId);
                ccPayment.cardNumber = currentPaymentCard.CardNumber ?? ""; //"4111111111111111"; 
                ccPayment.address1 = loc.AddressBilling.AddressLine1 ?? ""; //"232 Herzberg Road"; 
                ccPayment.address2 = loc.AddressBilling.AddressLine2 ?? ""; //"Suite 203";//                   

                var dateTimeUTS = CrmOpportunityService.FromUnixTime(Convert.ToInt64(currentPaymentCard.ExpirationDate / 1000)); // time in ms

                ccPayment.expirationMonth = dateTimeUTS != null ? dateTimeUTS.Month : 0; // 1 test
                ccPayment.expirationYear = dateTimeUTS != null ? Convert.ToDecimal(dateTimeUTS.Year.ToString().Substring(2, 2)) : 0;  // 20 test = 2020

                ccPayment.isDefault = true; // Make this Payment as Default for all !

                ccPayment.country = loc.AddressBilling.Country == "Canada" ? "CAN" : "USA";
                ccPayment.city = loc.AddressBilling.City ?? ""; //"Kanata";


                var stateTuple = GetStateById(loc.AddressBilling.StateId);

                ccPayment.state = stateTuple.Item1 ?? ""; //"Ontario"; 
                ccPayment.postalZip = loc.AddressBilling.Zip ?? ""; //"K2K 2A1";
                ccPayment.cvv = currentPaymentCard.SecurityCode != null ? Convert.ToDecimal(currentPaymentCard.SecurityCode) : 0;

                /////up is 11 fields

                // MasterCard will usually begin with 51 - 55 with 16 digits.                     
                // Visa credit card numbers will begin with a 4 and have 13 or 16 digits.
                int cardNumber = Convert.ToInt32(currentPaymentCard.CardNumber.Substring(0, 1));
                string nameC = null;
                if (cardNumber >= 51 && cardNumber <= 55)
                {
                    nameC = "MasterCard";
                }
                else if (cardNumber == 4)
                {
                    nameC = "Visa";
                }

                ccPayment.cardType = nameC ?? "";    //"Visa" or "Mastercard".   MasterCard                  
                var keyState = loc.AddressBilling.StateId;
                ccPayment.stateId = stateTuple.Item2;    //9;
                ccPayment.countryId = ccPayment.country == "CAN" ? 124 : 840; // for USA its 840( List of Numeric Country Codes)

                ccPayment.firstName = currentPaymentCard.FirstNameCard; //"John";
                ccPayment.lastName = currentPaymentCard.LastNameCard;  //"Doe";   

                jsonData = B2BWebApi.Helper.JsonHelper.GetJSONFromObject(ccPayment);
                paymentMethodResult = "creditCard";
                return jsonData;
            }

            paymentMethodResult = null;

            return null;
        }


        public string GetCurrentFusebillProduct(int subscriptionProductId)
        {
            //path parameter
            int pathParameter = subscriptionProductId;

            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/subscriptionproducts/" + pathParameter);
            //Add Content type
            request.ContentType = "application/json";
            request.ContentLength = 0;
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "GET";
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

        public void UpdateFusebillProduct(Subscriptionproduct updateProd)
        {
            //True to see a preview of your changes, false to apply the changes
            bool preview = false;
            //Json Payload

            //This Json payload should be retrieved by using the Read Subscription Product endpoint
            string jsonData = Helper.JsonHelper.GetJSONFromObject(updateProd);

            //"quantity":1.0,       //"isIncluded":true,        //"subscriptionProductPriceOverride":null,   

            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/SubscriptionProducts?preview=" + preview);
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "PUT";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
        }

        public void UpdateFusebillProductPrice( FusebillSubscriptionProductPrice fUpdatePrice)
        {
            //Json Payload
            string jsonData = Helper.JsonHelper.GetJSONFromObject(fUpdatePrice);                     // String.Format("{\"id\":{0},\"chargeAmount\":{1}}", subscriptionProductId, amount);
            {
                //Configure URI
                WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/SubscriptionProductPriceOverrides/");
                //Add Content type
                request.ContentType = "application/json";
                //Add Api key authorization
                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                //Set request method
                request.Method = "PUT";
                //Add the json data to request
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //Perform the request
                var httpResponse = (HttpWebResponse)request.GetResponse();
                //Record the response from our request
                var result = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
        }

        public List<InvoiceProductsData> GetAllInvoicesProducts(string fusebillIdNewOrCreated)
        {
            //query parameter
            bool showTrackedItems = true;
            //int customerId = { customerId };
          
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/Customers/" + fusebillIdNewOrCreated + "/Invoices/?showTrackedItems=" + showTrackedItems);
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "GET";
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            var listOfInvoiceProducts = Helper.JsonHelper.GetObjectFromJSON<List<InvoiceProductsData>>(result);
            return listOfInvoiceProducts;       
            
        }

        public void CloseOpportunity(string OpportunityId, bool hasInvProduct)
        {          

            if (hasInvProduct)
            {
                // Close the opportunity as won
                var winOppRequest = new WinOpportunityRequest
                {
                    OpportunityClose = new OpportunityClose
                    {
                        OpportunityId = new EntityReference(Opportunity.EntityLogicalName, new Guid(OpportunityId))
                    },
                    Status = new OptionSetValue(3) //(int)opportunity_statuscode.Won
                };
                service.Execute(winOppRequest);
            }
            else
            {
                var loseOppRequest = new LoseOpportunityRequest
                {
                    OpportunityClose = new OpportunityClose
                    {
                        OpportunityId = new EntityReference(Opportunity.EntityLogicalName, new Guid(OpportunityId))
                    },
                    Status = new OptionSetValue(4)
                };

                service.Execute(loseOppRequest);
            }


            //statecode     - statuscode
            //0(Open)       - 1(In Progress), 2(On Hold)
            //1(Won)        - 3(Won)
            //2(Lost)       - 4(Cancelled), 5(Out - Sold)

        }

        public void CancelSubscription(int subscriptionId)
        {
            FusebillCancelSubscription cancelSubscr = new FusebillCancelSubscription();
            cancelSubscr.subscriptionId = subscriptionId;
            cancelSubscr.cancellationOption = "None";
            //Json Payload
            string jsonData = Helper.JsonHelper.GetJSONFromObject(cancelSubscr);
                //"{subscriptionId:{subscriptionId},cancellationOption:'None'}";            
            //Configure URI
            WebRequest request = WebRequest.Create("HTTPS://stg-secure.fusebill.com/v1/subscriptionCancellation/");
            //Add Content type
            request.ContentType = "application/json";
            //Add Api key authorization
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
            //Set request method
            request.Method = "POST";
            //Add the json data to request
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            //Perform the request
            var httpResponse = (HttpWebResponse)request.GetResponse();
            //Record the response from our request
            var result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
        }

    }
}