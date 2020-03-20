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
    public class DashboardController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public DashboardController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Get()
        {
            return NotFound(new {error = "Request Terminated!" });
        }
//User follow channel
        [HttpPost("follow/{channel}/{user}")]
        [Authorize]
        public ActionResult follow(string channel, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@idorname",channel),
                    new SqlParameter("@userid",user),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"declare @channelId bigint
                declare @autofollow bit
                SELECT top 1 @channelId=id,@autofollow=followAutoApprove FROM vs_channel where cast(id as varchar(500))=@idorname or name=@idorname
                INSERT INTO vs_subscribe_channel (channel_id,subscriber_user_id,is_approved,create_date)
                     VALUES (@channelId,@userid,@autofollow,getdate())
                select @autofollow", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//User unfollow channel
        [HttpPost("unfollow/{channel}/{user}")]
        [Authorize]
        public ActionResult unfollow(string channel, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@idorname",channel),
                    new SqlParameter("@userid",user),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"DELETE  FROM vs_subscribe_channel
                WHERE   channel_id in (SELECT id FROM vs_channel where cast(id as varchar(500))=@idorname or name=@idorname) 
                    and subscriber_user_id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//follow accept
        [HttpPost("follow/accept/{channel}/{user}")]
        [Authorize]
        public ActionResult followaccept(string channel, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",channel),
                    new SqlParameter("@userid",user),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_subscribe_channel set is_approved=1, update_date=getdate() WHERE Id = @id and subscriber_user_id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//follow reject
        [HttpPost("follow/reject/{channel}/{user}")]
        [Authorize]
        public ActionResult followreject(string channel, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",channel),
                    new SqlParameter("@userid",user),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_subscribe_channel set is_approved=0, update_date=getdate() WHERE Id = @id and subscriber_user_id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//User add to playlist
        [HttpPost("playlist/add/{vidid}/{user}")]
        [Authorize]
        public ActionResult addPlaylist(string vidid, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@videoid",vidid),
                    new SqlParameter("@userid",user),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"INSERT INTO t_playlist (id,userid,videoid,CreatedOn) VALUES (newid(),@userid,@videoid,getdate())", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//User remove from playlist
        [HttpPost("playlist/remove/{vidid}/{user}")]
        [Authorize]
        public ActionResult removePlaylist(string vidid, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@videoid",vidid),
                    new SqlParameter("@userid",user),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"DELETE FROM t_playlist WHERE runningNum=(select top 1 runningNum from view_UserVideoPlaylist where userId=@userid and videoId=@videoid)", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//User load playlist
        [HttpGet("playlist/list/{user}")]
        [Authorize]
        public ActionResult playlist(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@userid",user),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"select * from view_UserVideoPlaylist where userid=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
    }
}