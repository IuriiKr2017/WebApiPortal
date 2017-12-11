using B2BWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace B2BWebApi.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Test()
        {
            return Json("Hello world!");
        }

        [HttpPost]
        public IHttpActionResult PostDatatoCRM(TestClass data)
        {
            if (data != null)
            {
            }
            return Json("ok");
        }
    }
}
