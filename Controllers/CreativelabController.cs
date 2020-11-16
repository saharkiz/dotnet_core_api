using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using Microsoft.AspNetCore.Http;
using System.IO;

using System.Security.Cryptography;
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

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult PostLogin([FromBody]Dictionary<string, object>  model)
        {
            try{
                using (MD5 md5Hash = MD5.Create())
                {
                    string hash = SqlHelper.GetMd5Hash(md5Hash, model["password"].ToString());
                    SqlParameter[] param = {
                        new SqlParameter("@email",model["email"].ToString()),
                        new SqlParameter("@password",model["password"].ToString()),
                    };
                    
                    var tokenuser = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VGPI, 
                    "select top 1 username,role from t_users where email=@email and password=@password", param);
                    return Ok(tokenuser);
                }
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Invalid Parameters" });
            }
        }
#region lists
        [HttpGet("jo/list/{page}")]
        [AllowAnonymous]
        public ActionResult JoList(string page)
        {
            try{
                int start = Convert.ToInt32(10) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(10) * Convert.ToInt32(page);
                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.CreativeLab, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY runningNum desc) rowNumber,*,runningNum as aresh from t_jo) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("task/list/{page}")]
        [AllowAnonymous]
        public ActionResult TaskList(string page)
        {
            try{
                int start = Convert.ToInt32(10) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(10) * Convert.ToInt32(page);
                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.CreativeLab, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY runningNum desc) rowNumber,*,runningNum as aresh from t_task) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("news/list/{page}")]
        [AllowAnonymous]
        public ActionResult NewsList(string page)
        {
            try{
                int start = Convert.ToInt32(10) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(10) * Convert.ToInt32(page);
                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY runningNum desc) rowNumber,*,runningNum as aresh from t_news) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("news/list/search")]
        [AllowAnonymous]
        public ActionResult NewsSearchList([FromQuery(Name = "keyword")] String keyword)
        {
            try{
                
                SqlParameter[] param = {
                    new SqlParameter("@keyword",keyword)
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"SELECT *,runningNum as aresh from t_news WHERE title like '%'+@keyword+'%'", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("events/list/{page}")]
        [AllowAnonymous]
        public ActionResult EventsList(string page)
        {
            try{
                int start = Convert.ToInt32(10) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(10) * Convert.ToInt32(page);
                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY runningNum desc) rowNumber,*,runningNum as aresh from t_events) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("events/list/search")]
        [AllowAnonymous]
        public ActionResult EventsSearchList([FromQuery(Name = "keyword")] String keyword)
        {
            try{
                
                SqlParameter[] param = {
                    new SqlParameter("@keyword",keyword)
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"SELECT *,runningNum as aresh from t_events WHERE title like '%'+@keyword+'%'", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("videos/list/{page}")]
        [AllowAnonymous]
        public ActionResult VideosList(string page)
        {
            try{
                int start = Convert.ToInt32(10) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(10) * Convert.ToInt32(page);
                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY pk desc) rowNumber,*,pk as aresh from vs_entry_details) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("videos/list/search")]
        [AllowAnonymous]
        public ActionResult VideosSearchList([FromQuery(Name = "keyword")] String keyword)
        {
            try{
                
                SqlParameter[] param = {
                    new SqlParameter("@keyword",keyword)
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT *,runningNum as aresh from vs_entry_details WHERE title like '%'+@keyword+'%'", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("channels/list/{page}")]
        [AllowAnonymous]
        public ActionResult ChannelsList(string page)
        {
            try{
                int start = Convert.ToInt32(10) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(10) * Convert.ToInt32(page);
                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY id desc) rowNumber,*,id as aresh from vs_channel) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("channels/list/search")]
        [AllowAnonymous]
        public ActionResult ChannelsSearchList([FromQuery(Name = "keyword")] String keyword)
        {
            try{
                
                SqlParameter[] param = {
                    new SqlParameter("@keyword",keyword)
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT *,runningNum as aresh from vs_channel WHERE title like '%'+@keyword+'%'", param);
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
                "select * from t_task where id=@id", param);
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
                "select * from t_news where runningnum=@id", param);
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
        [HttpGet("videos/{id}")]
        [AllowAnonymous]
        public ActionResult VideosGet(string id)
        {
            try{
                SqlParameter[] param = {
                        new SqlParameter("@id",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                "select * from vs_entry_details where pk=@id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
        [HttpGet("channels/{id}")]
        [AllowAnonymous]
        public ActionResult ChannelsGet(string id)
        {
            try{
                SqlParameter[] param = {
                        new SqlParameter("@id",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                "select * from vs_channel where id=@id", param);
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