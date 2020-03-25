using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using Microsoft.AspNetCore.Http;
using System.IO;

using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class SiteController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public SiteController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 

        [HttpGet("language")]
        [AllowAnonymous]
        public ActionResult Language()
        {
            return Ok(new { 
                en = "English",
                ar = "العربية",
                id = "Bahasa Indonesia",
                fa = "فارسی",
                fr = "Français",
                ku = "Kurdish",
                ru = "Русский",
                tr = "Türkçe",
                my = "ဗမာ",
                si = "සිංහල",
                hi = "Hindi",
                ta = "தமிழ்"
             });
        }
    }
}