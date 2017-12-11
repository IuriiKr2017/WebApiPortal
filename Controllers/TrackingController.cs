using B2BWebApi.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;

namespace B2BWebApi.Controllers
{
    public class TrackingController : ApiController
    {
        [Route("api/Tracking/GetBackData")]
        public HttpResponseMessage GetBackData(string id)
        {
            var taskNew = Task.Run(() => SetOpeningEmailTimeToCrm(id) );    
                 

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //var stream = new FileStream()       
                      
            var image = new Bitmap(100, 100);
            image.MakeTransparent();
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Jpeg);
            
            memoryStream.Position = 0;        
           
            result.Content = new StreamContent(memoryStream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;




            // var fileName =  "11.gif"; //"pixel-1x1-clear.gif";
            // string virtualFilePath = @"C:\Users\y.krivenko\Documents\B2BWebApi\B2BWebApi\B2BWebApi\Controllers\Resources\" + fileName;
            // //Path.Combine(Environment.CurrentDirectory, @"Controllers\Resources\", fileName);

            // //var path = @"C:\Temp\test.exe";
            // HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            // var stream = new FileStream(virtualFilePath, FileMode.Open, FileAccess.Read);
            // result.Content = new StreamContent(stream);
            // result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            // return result;

            //// return File(virtualFilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(virtualFilePath));

            //return Json("ok");
        }

        private void SetOpeningEmailTimeToCrm(string id)
        {        
            try
            {
                CrmOpportunityService crmOpportunityService = new CrmOpportunityService();

                var newId = id.Substring(0, id.Length - (id.Length - id.IndexOf('/')));
                var accoundID = crmOpportunityService.GetAccountIdByOpportunityId(new Guid(newId));

                Entity entityUpdate = new Entity("account", accoundID);
                               
                entityUpdate["b2b_emailopened"] = DateTime.Now;
                crmOpportunityService.UpdateEntity(entityUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}

