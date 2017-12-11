using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace B2BWebApi.Controllers
{
    public class AuthenticationController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("api/data/forall")]

        public IHttpActionResult Get()
        {
            return Ok("You came in: " + DateTime.Now.ToString() + "  so expiration time will be : " + DateTime.Now.AddHours(1) + "  <------");
        }

        [Authorize]
        [HttpGet]
        [Route("api/data/authenticate")]
        public IHttpActionResult GetForAuthenticate()
        {
            var identity = (ClaimsIdentity)User.Identity;

            return Ok(" hello + " + identity.Name +
              " You came in: " + DateTime.Now.ToString() + "  so expiration time will be : " + DateTime.Now.AddHours(1) + "  <------");
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("api/data/authorize")]
        public IHttpActionResult GetForAdmin()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var roles = identity.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return Ok("Hello " + identity.Name + " Role " + string.Join(",", roles.ToList()));
        }
    }
}
