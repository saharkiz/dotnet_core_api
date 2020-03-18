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
    public class VideoController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public VideoController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Get()
        {
            return NotFound(new {error = "Request Terminated!" });
        }
//Video Categories
        [HttpGet("category")]
        [AllowAnonymous]
        public ActionResult category()
        {
            try{
                SqlParameter[] param = {
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                "select category, COUNT(Id) as count from View_VideoList_API where isapproved = 1 and category<>'' group by category order by count desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Video Details for All
        [HttpGet("{vid}")]
        [AllowAnonymous]
        public ActionResult Get(string vid)
        {
            try{
                SqlParameter[] paramTemp = {
                    new SqlParameter("@idorname",vid),
                };

                SqlHelper.ExecuteStatement(appSettings.Value.Vtube, 
                @"UPDATE vs_entry_details SET Plays = isnull(Plays,0) + 1, Views= isnull(Plays,0) + 1  WHERE Id = @idorname", paramTemp);

                SqlParameter[] param = {
                    new SqlParameter("@idorname",vid),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"select TOP 1 * from View_VideoList_API where premium<>'private' and (id=@idorname or name=@idorname)", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Video Details for the loggedin User        
        [HttpGet("{vid}/{user}")]
        [Authorize]
        public ActionResult Get(string vid, string user)
        {
            try{
                SqlParameter[] paramTemp = {
                    new SqlParameter("@idorname",vid),
                };

                SqlHelper.ExecuteStatement(appSettings.Value.Vtube, 
                @"UPDATE vs_entry_details SET Plays = isnull(Plays,0) + 1, Views= isnull(Plays,0) + 1  WHERE Id = @idorname", paramTemp);

                SqlParameter[] param = {
                    new SqlParameter("@idorname",vid),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"select TOP 1 * from View_VideoList_API where (id=@idorname or name=@idorname)", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///List of all videos by language
        [HttpGet("{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult video(string language, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///List of all videos by language of user
        [HttpGet("{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult videouser(string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Highlights for All
        [HttpGet("highlight/{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult highlight(string language, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and isHighlighted=1) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Highlights for the loggedin User
        [HttpGet("highlight/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult highlightuser(string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and isHighlighted=1) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Premium for All
        [HttpGet("premium/{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult premium(string language, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                    * FROM View_VideoList_premium where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///premium for the loggedin User
        [HttpGet("premium/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult premium(string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_premium where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }






///Category for All
        [HttpGet("category/{category}/{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult category(string category, string language, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                    new SqlParameter("@category",category),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and Category like '%'+@category+'%') v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Category for the loggedin User
        [HttpGet("category/{category}/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult categoryuser(string category, string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                    new SqlParameter("@category",category),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and Category like '%'+@category+'%') v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Related Video for All
        [HttpGet("related/{vidid}/{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult relatedVideo(string vidid, string language, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                    new SqlParameter("@vidid",vidid),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Related Video for the loggedin User
        [HttpGet("related/{vidid}/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult relatedVideouser(string vidid, string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                    new SqlParameter("@vidid",vidid),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Most View Video for All
        [HttpGet("view/{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult MostViewVideo(string vidid, string language, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY Views desc) rowNumber,
                    CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Most View Video for the loggedin User
        [HttpGet("view/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult MostViewVideouser(string vidid, string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                    new SqlParameter("@vidid",vidid),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY Views desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//upload video 
        [HttpPost("upload/{user}")]
        [Authorize]
        public ActionResult upload([FromForm(Name = "files")] List<IFormFile> files, string user)
        {
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
                finalFileName = user + "_video_" + ret.Replace(" ", String.Empty);
                filePath = Path.Combine(target,  finalFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            });
            return Ok(new {filename= finalFileName,Statuscode="Completed Upload" , files.Count, Size = SqlHelper.SizeConverter(files[0].Length) });
            }
            catch(Exception)
            {
                return NotFound(new {error = "Upload Terminated!" });
            }
        } 
//uploaded video detail
        [HttpGet("detail/{user}")]
        [Authorize]
        public ActionResult DetailVideo([FromBody]Dictionary<string, object> model, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@name",model["name"].ToString()),
                    new SqlParameter("@tags",model["tags"].ToString()),
                    new SqlParameter("@description",model["description"].ToString()),
                    new SqlParameter("@levelId",model["levelid"].ToString()),
                    new SqlParameter("@is_comments_allowed",model["is_comments_allowed"].ToString()),
                    new SqlParameter("@categories",model["categories"].ToString()),
                    new SqlParameter("@is_share_allowed",model["is_share_allowed"].ToString()),
                    new SqlParameter("@fileName",model["filename"].ToString()),
                    new SqlParameter("@runningnum",model["runningnum"].ToString()),
                    new SqlParameter("@access_type",model["access_type"].ToString()),
                    new SqlParameter("@market_location",model["market_location"].ToString()),
                    new SqlParameter("@createdBy",model["createdby"].ToString()),
                    new SqlParameter("@userId",model["userid"].ToString()),
                    new SqlParameter("@isApproved",model["isapproved"].ToString()),
                    new SqlParameter("@allow_ads",model["allow_ads"].ToString()),
                };
                var lst = SqlHelper.ExecuteProcedureReturnString(appSettings.Value.Vtube, 
                    @"sp_Video_Create", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//search video
//like video  
        [HttpGet("like/{vid}")]
        [AllowAnonymous]
        public ActionResult Like(string vid)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",vid),
                };
                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_entry_details SET Likes = isnull(Likes,0) + 1  WHERE Id = @id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///like video user
        [HttpGet("like/{vid}/{user}")]
        [AllowAnonymous]
        public ActionResult LikeUser(string vid, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",vid),
                };
                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"UPDATE vs_entry_details SET Likes = isnull(Likes,0) + 1  WHERE Id = @id", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//comment on video
        [HttpGet("comment/list/{vid}")]
        [AllowAnonymous]
        public ActionResult commentList(string vid)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@title",vid),
                };
                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"elect (select first_name from t_Members where id=cl.CreatedBy) as CreatedBy,
                Comment,CreatedOn,CreatedBy as UserId,Id from View_CommentList cl where referenceid=@title 
                and parentid is null and IsVisible=1  order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//comment on video user
        [HttpGet("comment/list/{vid}/{user}")]
        [AllowAnonymous]
        public ActionResult commentListUser(string vid,string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@title",vid),
                };
                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                @"elect (select first_name from t_Members where id=cl.CreatedBy) as CreatedBy,
                Comment,CreatedOn,CreatedBy as UserId,Id from View_CommentList cl where referenceid=@title 
                and parentid is null and IsVisible=1  order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///get comment list
        [HttpGet("comment/{vid}/{user}")]
        [Authorize]
        public ActionResult CommentVideo([FromBody]Dictionary<string, object> model, string vid, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",vid),
                    new SqlParameter("@Comment",model["comment"].ToString()),
                    new SqlParameter("@CreatedBy",user),
                    new SqlParameter("@ctype","video"),
                };
                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
                    @"INSERT INTO t_Comments 
                    (ReferenceId,Comment,CreatedBy,CreatedOn,CommentType) 
                    VALUES  (@Refid,@Comment, @CreatedBy, GETDATE(),@ctype)", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }



    }

}
