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
    public class CreativelabController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public CreativelabController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 

        [HttpGet("jo/list")]
        [AllowAnonymous]
        public ActionResult JoList()
        {
            try{
                SqlParameter[] param = {
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.CreativeLab, 
                "select * from view_jo order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("task/list")]
        [AllowAnonymous]
        public ActionResult TaskList()
        {
            try{
                SqlParameter[] param = {
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.CreativeLab, 
                "select * from view_task order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
    }
}