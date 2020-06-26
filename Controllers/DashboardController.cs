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
//followed channels by user
        [HttpGet("followchannel/list/{user}")]
        [Authorize]
        public ActionResult followedchannel(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@userid",user),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM [vs_subscribe_channel] where subscriber_user_id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//followed channels details by user
        [HttpGet("followchannel/list/detail/{user}")]
        [Authorize]
        public ActionResult followedchanneldetail(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@userid",user),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM [vs_subscribe_channel] sc inner join vs_channel c on c.id = sc.channel_id where sc.subscriber_user_id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//Create channel
        [HttpPost("create/channel/{user}")]
        [Authorize]
        public ActionResult createchannel([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@name",model["name"].ToString()),
                    new SqlParameter("@access_type",model["access_type"].ToString()),
                    new SqlParameter("@user_id",user),
                    new SqlParameter("@description",model["description"].ToString()),
                    new SqlParameter("@is_comment_allowed",model["is_comment_allowed"].ToString()),
                    new SqlParameter("@is_rate_allowed",model["is_rate_allowed"].ToString()),
                };

                var lst = SqlHelper.ExecuteProcedureReturnString(appSettings.Value.Vtube, 
                @"sp_Channel_Create", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//Remove channel
        [HttpPost("remove/channel/{user}")]
        [Authorize]
        public ActionResult removechannel([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                    new SqlParameter("@id",model["channel_id"].ToString()),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_channel SET delete_date=GETDATE(), access_type='DELETED'  WHERE Id = @id  and user_id=@user_id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//Update channel
        [HttpPost("update/channel/{user}")]
        [Authorize]
        public ActionResult updatechannel([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                    new SqlParameter("@name",model["name"].ToString()),
                    new SqlParameter("@access_type",model["access_type"].ToString()),
                    new SqlParameter("@description",model["description"].ToString()),
                    new SqlParameter("@is_comment_allowed",model["is_comment_allowed"].ToString()),
                    new SqlParameter("@id",model["channel_id"].ToString()),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_channel SET access_type = @access_type,name = @name,description = @description,
                is_comment_allowed = @is_comment_allowed,update_date = getdate() WHERE id = @id and user_id=@user_id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }    
//Remove Video
        [HttpPost("remove/video/{user}")]
        [Authorize]
        public ActionResult removevideo([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                    new SqlParameter("@id",model["video_id"].ToString()),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_entry_details SET isApproved=0  WHERE Id = @id and createdBy=@user_id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//upload User Avatar 
        [HttpPost("upload/avatar/{user}")]
        [Authorize]
        public async IAsyncEnumerable<OkObjectResult> uploadavatar([FromForm(Name = "files")] List<IFormFile> files, string user)
        {
            OkObjectResult resultout = Ok(new {error = "Processing..." });;
            try{
                string subDirectory = "upload";
                var target = Path.Combine(Environment.CurrentDirectory, subDirectory);
                var filePath = "";
                string finalFileName = "";
                Directory.CreateDirectory(target);

                files.ForEach(async file =>
                {
                    if (file.Length <= 0) return;
                    String ret = Regex.Replace(file.FileName.Trim(), "[^A-Za-z0-9.]+", "");
                    finalFileName = user + "_avatar_" + ret.Replace(" ", String.Empty);
                    filePath = Path.Combine(target,  finalFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        }
                });
                            if (System.IO.File.Exists(filePath))
                            {
                                byte[] contentoffile = await System.IO.File.ReadAllBytesAsync(filePath);
                                if (contentoffile.Length > 0)
                                {
                                    SqlParameter[] param = {
                                        new SqlParameter("@id",user),
                                        new SqlParameter("@avatarURL", contentoffile)
                                    };

                                    var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
                                    @"UPDATE t_Members SET update_date = GETDATE(), last_modified = GETDATE(),avatarURL = @avatarURL  WHERE id = @id", param);
                                    resultout = Ok(new {filename= finalFileName,Statuscode="Completed Upload" , files.Count, Size = SqlHelper.SizeConverter(files[0].Length) });
                                }
                                else
                                {
                                    resultout = Ok(new {error = "File Content Empty!" });
                                }
                            }
                            else
                            {
                                resultout =  Ok(new {error = "File Not Uploaded!" });
                            }
                    
            }
            catch(Exception)
            {
                resultout =  Ok(new {error = "Upload Terminated!" });
            }
            yield return resultout;
        } 
//upload channel image 
        [HttpPost("upload/channelavatar/{channel}/{user}")]
        [Authorize]
        public async IAsyncEnumerable<OkObjectResult> uploadchannelavatar([FromForm(Name = "files")] List<IFormFile> files, string channel, string user)
        {
            OkObjectResult resultout = Ok(new {error = "Processing..." });;
            try{
                string subDirectory = "upload";
                var target = Path.Combine(Environment.CurrentDirectory, subDirectory);
                var filePath = "";
                string finalFileName = "";
                Directory.CreateDirectory(target);

                files.ForEach(async file =>
                {
                    if (file.Length <= 0) return;
                    String ret = Regex.Replace(file.FileName.Trim(), "[^A-Za-z0-9.]+", "");
                    finalFileName = user + "_channelavatar_" + ret.Replace(" ", String.Empty);
                    filePath = Path.Combine(target,  finalFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        }
                });
                            if (System.IO.File.Exists(filePath))
                            {
                                byte[] contentoffile = await System.IO.File.ReadAllBytesAsync(filePath);
                                if (contentoffile.Length > 0)
                                {
                                    SqlParameter[] param = {
                                        new SqlParameter("@id",channel),
                                        new SqlParameter("@avatarURL", contentoffile)
                                    };

                                    var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
                                    @"UPDATE vs_channel SET update_date = GETDATE(), image = @avatarURL  WHERE id = @id", param);
                                    resultout = Ok(new {filename= finalFileName,Statuscode="Completed Upload" , files.Count, Size = SqlHelper.SizeConverter(files[0].Length) });
                                }
                                else
                                {
                                    resultout = Ok(new {error = "File Content Empty!" });
                                }
                            }
                            else
                            {
                                resultout =  Ok(new {error = "File Not Uploaded!" });
                            }
                    
            }
            catch(Exception)
            {
                resultout =  Ok(new {error = "Upload Terminated!" });
            }
            yield return resultout;
        } 
//Update Billing information
        [HttpPost("update/billing/{user}")]
        [Authorize]
        public ActionResult updatebilling([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                    new SqlParameter("@billing_address1",model["billing_address1"].ToString()),
                    new SqlParameter("@billing_city",model["billing_city"].ToString()),
                    new SqlParameter("@billing_state_region",model["billing_state_region"].ToString()),
                    new SqlParameter("@billing_country",model["billing_country"].ToString()),
                    new SqlParameter("@billing_postal_code",model["billing_postal_code"].ToString()),
                    new SqlParameter("@billing_email",model["billing_email"].ToString()),
                    new SqlParameter("@billing_phone_number",model["billing_phone_number"].ToString()),
                    new SqlParameter("@shipping_address1",model["shipping_address1"].ToString()),
                    new SqlParameter("@shipping_city",model["shipping_city"].ToString()),
                    new SqlParameter("@shipping_state_region",model["shipping_state_region"].ToString()),
                    new SqlParameter("@shipping_country",model["shipping_country"].ToString()),
                    new SqlParameter("@shipping_postal_code",model["shipping_postal_code"].ToString()),
                    new SqlParameter("@shipping_email",model["shipping_email"].ToString()),
                    new SqlParameter("@shipping_phone_number",model["shipping_phone_number"].ToString()),
                };

                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
                @"UPDATE [t_Members]
                    SET [billing_address1] = @billing_address1, 
                        [billing_city] = @billing_city, 
                        [billing_state_region] = @billing_state_region, 
                        [billing_country] = @billing_country, 
                        [billing_postal_code] = @billing_postal_code, 
                        [billing_email] = @billing_email, 
                        [billing_phone_number] = @billing_phone_number, 
                        [shipping_address1] = @shipping_address1,
                        [shipping_city] = @shipping_city, 
                        [shipping_state_region] = @shipping_state_region, 
                        [shipping_country] = @shipping_country, 
                        [shipping_postal_code] = @shipping_postal_code, 
                        [shipping_email] = @shipping_email,
                        [shipping_phone_number] = @shipping_phone_number
                    WHERE id = @user_id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }    
///follower requests
        [HttpGet("follower/request/{user}")]
        [Authorize]
        public ActionResult followerrequest(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"select distinct * from view_UserFollowerRequest where userid=@user_id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///point history
        [HttpGet("point/history/{user}")]
        [Authorize]
        public ActionResult pointhistory(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"SELECT *, isnull(description,activity) activitydesc FROM t_PointHistory WHERE Userid = @user_id order by CreatedOn desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///inbox
        [HttpGet("inbox/list/{user}")]
        [Authorize]
        public ActionResult inboxlist(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VMembers, 
                @"SELECT * FROM [View_t_messages] where Userid = @user_id order by CreatedOn desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///inbox item
        [HttpGet("inbox/{item}/{user}")]
        [Authorize]
        public ActionResult inboxitem(string item, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                    new SqlParameter("@id",item),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VMembers, 
                @"SELECT * FROM [View_t_messages] where Userid = @user_id and id=@id order by CreatedOn desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///user channel
        [HttpGet("channel/list/{user}")]
        [Authorize]
        public ActionResult userchannel(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM View_ChannelList_API where user_id = @user_id order by CreatedOn desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///user videos
        [HttpGet("video/list/{user}")]
        [Authorize]
        public ActionResult uservideo(string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@user_id",user),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"select * from view_UserMyVideos where userid=@user_id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///user password
        [HttpPost("update/password/{user}")]
        [Authorize]
        public ActionResult updatepassword([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                using (MD5 md5Hash = MD5.Create())
                {
                    string hash = SqlHelper.GetMd5Hash(md5Hash, model["newpassword"].ToString());
                    SqlParameter[] param = {
                        new SqlParameter("@user_id",user),
                        new SqlParameter("@oldpassword",model["oldpassword"].ToString()),
                        new SqlParameter("@newpassword",model["newpassword"].ToString()),
                        new SqlParameter("@pass",hash)
                    };

                    var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
                    @"UPDATE t_members SET passwd=@pass, NonEncPassword=@newpassword where NonEncPassword=@oldpassword and userid=@userid", param);
                    return Ok(lst);
                }
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }    
    }
}