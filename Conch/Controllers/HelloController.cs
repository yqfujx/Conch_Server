using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Conch.Models;

namespace Conch.Controllers
{
    [AllowAnonymous]
    public class HelloController : ApiController
    {
 //       [Route("api/Hello/Get")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello");
        }
    }
}
