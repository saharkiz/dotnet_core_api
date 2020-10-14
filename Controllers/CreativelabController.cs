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
#region lists
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
        [HttpGet("news/list")]
        [AllowAnonymous]
        public ActionResult NewsList()
        {
            try{
                SqlParameter[] param = {
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                "select * from t_news order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("events/list")]
        [AllowAnonymous]
        public ActionResult EventsList()
        {
            try{
                SqlParameter[] param = {
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                "select * from t_events order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
#endregion

#region get
        
        [HttpGet("jo/{id}")]
        [AllowAnonymous]
        public ActionResult JoGet(string id)
        {
            try{
                SqlParameter[] param = {
                        new SqlParameter("@id",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.CreativeLab, 
                "select * from t_jo where id=@id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("task/{id}")]
        [AllowAnonymous]
        public ActionResult TaskGet(string id)
        {
            try{
                SqlParameter[] param = {
                        new SqlParameter("@id",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.CreativeLab, 
                "select * from t_tasks where id=@id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("news/{id}")]
        [AllowAnonymous]
        public ActionResult NewsGet(string id)
        {
            try{
                SqlParameter[] param = {
                        new SqlParameter("@id",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                "select * from t_news where id=@id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("events/{id}")]
        [AllowAnonymous]
        public ActionResult EventsGet(string id)
        {
            try{
                SqlParameter[] param = {
                        new SqlParameter("@id",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                "select * from t_events where id=@id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }

#endregion
    
    
    }
}