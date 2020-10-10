using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using System.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;


namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class ChannelController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        private readonly IMemoryCache _cache;
        public ChannelController(IOptions<MySettingsModel> app, IMemoryCache memoryCache)  
        {  
            appSettings = app;  
            _cache = memoryCache; 
        } 
        [HttpGet("reset")]
        [AllowAnonymous]
        public ActionResult GetReset()
        {
            var cacheEntriesCollectionDefinition = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cacheEntriesCollection = cacheEntriesCollectionDefinition.GetValue(_cache) as dynamic;
            List<Microsoft.Extensions.Caching.Memory.ICacheEntry> cacheCollectionValues = new List<Microsoft.Extensions.Caching.Memory.ICacheEntry>();
            foreach (var cacheItem in cacheEntriesCollection)
            { 
                Microsoft.Extensions.Caching.Memory.ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);
                cacheCollectionValues.Add(cacheItemValue);
            }
            var items = new List<string>();
            foreach (var item in cacheCollectionValues)
            {
                var methodInfo = item.GetType().GetProperty("Key");
                var val = methodInfo.GetValue(item);
                items.Add(val.ToString());
                _cache.Remove(val.ToString());
            }
            return Ok(items);
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
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "channel.page." + page;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                    SqlParameter[] param = {
                        new SqlParameter("@start",start),
                        new SqlParameter("@end",end),
                    };

                     lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* FROM View_ChannelList_API where privacy='public') c
                        WHERE rowNumber between @start and @end
                        order by rowNumber", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                    return Ok(lst);
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
        }
///Channel Details for All ID 
        [HttpGet("{channelid}")]
        [AllowAnonymous]
        public ActionResult Get(string channelid)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "channel.detail." + channelid;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    SqlParameter[] param = {
                        new SqlParameter("@idorname",channelid),
                    };
                     lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"select TOP 1 * from View_ChannelList_API where id=@idorname", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                    return Ok(lst);
                }
                catch(Exception)
                {
                    return BadRequest();
                }
            }
            return Ok(lst);
        }
///Channel Details for All Name
        [HttpGet("name/{channelname}")]
        [AllowAnonymous]
        public ActionResult GetByName(string channelname)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "channel.detail." + channelname;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    SqlParameter[] param = {
                        new SqlParameter("@idorname",channelname),
                    };
                    lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"select TOP 1 * from View_ChannelList_API where name=@idorname", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                    return Ok(lst);
                }
                catch(Exception)
                {
                    return BadRequest();
                }
            }
            return Ok(lst);
        }
//Channels of Moderator
        [HttpGet("moderator/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult moderator(int page, int count)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "channel.moderator." + page;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                    SqlParameter[] param = {
                        new SqlParameter("@start",start),
                        new SqlParameter("@end",end),
                    };

                     lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* FROM View_ChannelList_API WHERE user_id = 'AF3102AD-09C6-4E44-9872-2C3695F5A1F6') c
                        WHERE rowNumber between @start and @end
                        order by rowNumber", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                    return Ok(lst);
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
        }
//Channels Recommended
        [HttpGet("recommend/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult recommend(int page, int count)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "channel.recommend." + page;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                    SqlParameter[] param = {
                        new SqlParameter("@start",start),
                        new SqlParameter("@end",end),
                    };

                     lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn asc) rowNumber,* FROM View_ChannelList_API where is_recommended=1) c
                        WHERE rowNumber between @start and @end
                        order by rowNumber", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                    return Ok(lst);
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
        }
///search channel
        [HttpPost("search/{type}")]
        [AllowAnonymous]
        public ActionResult search([FromBody] Dictionary<string, object> obj, string type)
        {
            try{
                string keyword = obj["search"].ToString();
                string searchWord = keyword.Replace(",", "").Replace("'", "").Replace("’", "");

                SqlParameter[] param = {
                    new SqlParameter("@SearchKey",searchWord),
                    new SqlParameter("@Type",type),
                };
                var resulttable = SqlHelper.ExecuteProcedureReturnData(appSettings.Value.VShop,"sp_SearchResult",param);
                if (resulttable.Count > 0)
                {
                    DataTable table = new DataTable();
                    foreach (IDictionary<string, object> row in resulttable)
                    {
                        foreach (KeyValuePair<string, object> entry in row)
                        {
                            if (!table.Columns.Contains(entry.Key.ToString()))
                            {
                                table.Columns.Add(entry.Key);
                            }
                        }
                        String[] foos = new String[row.Count];
                        row.Values.CopyTo(foos, 0);
                        table.Rows.Add(foos);
                    }
                    SqlParameter[] paramresult = {
                        new SqlParameter("@table",table),
                        new SqlParameter("@Type",type),
                        new SqlParameter("@orderby","latest"),
                    };
                    var result = SqlHelper.ExecuteProcedureReturnData(appSettings.Value.VShop,"sp_Results",paramresult);
                    return Ok(result);
                }
                else{
                    return NotFound( new {error = " Empty Results"});
                }
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///channels most viewed
        [HttpGet("view/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult mostViewChannel(int page, int count)
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
                    (SELECT ROW_NUMBER() OVER (ORDER BY views desc) rowNumber,* FROM View_ChannelList_API) c
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///channel videos
        [HttpGet("video/list/{channel}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult videoList(string channel, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@title",channel),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    '' as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and (cast(ChannelId as varchar(10))=@title or ChannelName=@title
                     or cast(Channel1 as varchar(10))=@title or cast(Channel2 as varchar(10))=@title or cast(Channel3 as varchar(10))=@title
                      or cast(Channel4 as varchar(10))=@title  or cast(Channel5 as varchar(10))=@title
                     )) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///channel videos by user
        [HttpGet("video/list/{channel}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult videoListUser(string channel, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@title",channel),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and (cast(ChannelId as varchar(10))=@title or ChannelName=@title
                    or cast(Channel1 as varchar(10))=@title or cast(Channel2 as varchar(10))=@title or cast(Channel3 as varchar(10))=@title
                      or cast(Channel4 as varchar(10))=@title  or cast(Channel5 as varchar(10))=@title
                    )) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }

///channel by category
///channel following
        [HttpGet("following/list/{channel}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult following(string channel, int page, int count)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@title",channel),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from View_FollowList_API where 
                    is_approved=1 and (channelname=@title or cast(channelid as varchar(10))=@title)) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }

///channel followers
        [HttpGet("followers/list/{channel}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult followers(string channel, int page, int count)
        {
            try{
                
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@title",channel),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from View_FollowList_API where 
                    is_approved=1 and (channelname=@title or cast(channelid as varchar(10))=@title)) p
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//comment on channel
        [HttpGet("comment/list/{channel}")]
        [AllowAnonymous]
        public ActionResult commentList(string channel)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@title",channel),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"select (select first_name from DBF_V_Members.dbo.t_Members where id=cl.CreatedBy) as CreatedBy,
                Comment,CreatedOn,CreatedBy as UserId,Id from View_CommentList cl where referenceid=@title 
                and parentid is null and IsVisible=1  order by runningNum desc", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
//comment on channel user
        [HttpGet("comment/list/{channel}/{user}")]
        [Authorize]
        public ActionResult commentListUser(string channel,string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@title",channel),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"select (select first_name from DBF_V_Members.dbo.t_Members where id=cl.CreatedBy) as CreatedBy,
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
        [HttpPost("comment/{channel}/{user}")]
        [Authorize]
        public ActionResult CommentVideo([FromBody]Dictionary<string, object> model, string channel, string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",channel),
                    new SqlParameter("@Comment",model["comment"].ToString()),
                    new SqlParameter("@CreatedBy",user),
                    new SqlParameter("@ctype","channel"),
                };
                var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VShop, 
                    @"INSERT INTO t_Comments 
                    (ReferenceId,Comment,CreatedBy,CreatedOn,CommentType) 
                    VALUES  (@id,@Comment, @CreatedBy, GETDATE(),@ctype)", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }


    }
}