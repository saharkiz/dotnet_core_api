using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using myvapi.Utility;

using Microsoft.AspNetCore.Authorization;


namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class ChannelController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public ChannelController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Get()
        {
            return NotFound(new {error = "Request Terminated!" });
        }
//All Other Channels
        [HttpGet("{page}/{count}")]
        [AllowAnonymous]
        public ActionResult Get(int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* FROM View_ChannelList_API where privacy='public') c
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Channel Details for All ID or Name
        [HttpGet("{channelid}")]
        [AllowAnonymous]
        public ActionResult Get(string channelid)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@idorname",channelid),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"select TOP 1 * from View_ChannelList_API where (name=@idorname or cast(id as varchar(10))=@idorname)", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }
//Channels of Moderator
        [HttpGet("moderator/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult moderator(int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* FROM View_ChannelList_API WHERE user_id = 'AF3102AD-09C6-4E44-9872-2C3695F5A1F6') c
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//Channels Recommended
        [HttpGet("recommend/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult recommend(int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn asc) rowNumber,* FROM View_ChannelList_API where is_recommended=1) c
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///search channel
        [HttpGet("search/{type}")]
        [AllowAnonymous]
        public ActionResult search([FromBody] Dictionary<string, object> obj, string searchtype)
        {
            try{
                string keyword = obj["search"].ToString();
                string searchWord = keyword.Replace(",", "").Replace("'", "").Replace("’", "");

                SqlParameter[] param = {
                    new SqlParameter("@SearchKey",searchWord),
                    new SqlParameter("@Type",searchtype),
                };
                var resulttable = SqlHelper.ExecuteProcedureReturnData(appSettings.Value.Vtube,"sp_SearchResult",param);
                if (resulttable.Count > 0)
                {
                    SqlParameter[] paramresult = {
                        new SqlParameter("@table",resulttable),
                        new SqlParameter("@Type",searchtype),
                        new SqlParameter("@orderby","latest"),
                    };
                    var result = SqlHelper.ExecuteProcedureReturnData(appSettings.Value.Vtube,"sp_Results",paramresult);
                    return Ok(result);
                }
                else{
                    return NotFound();
                }
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }

///channel videos
///channel by category
///channel following

    }
}