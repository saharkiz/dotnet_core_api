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
    [Produces("application/json")]
    [Route("[controller]")]
    public class UploadController : Controller
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public UploadController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult uploadedvideo([FromQuery(Name = "id")] string id)
        {
            try{
                Regex illegalInFileName = new Regex(@"[\\/:*?""<>|]");
                string clean_filename = illegalInFileName.Replace(id, "");
                //open file from the upload folder
                var file = Path.Combine(Directory.GetCurrentDirectory(),"upload", clean_filename);
                if (System.IO.File.Exists(file)){
                    //return PhysicalFile(file, "image/png");
                    return PhysicalFile(file, "application/octet-stream");
                }
                else
                {
                    return NotFound("File Does not exist");
                }
            }
            catch(Exception){
                return NotFound("File Does not exist");
            }
        }
    }
    
}