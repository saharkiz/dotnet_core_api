 using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using RestSharp;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/xml")]
    [Route("[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<string> testvideo()
        {
            return new string[] { "value1", "value2" };
        }
    }
}