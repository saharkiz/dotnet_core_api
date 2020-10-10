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

using Microsoft.Extensions.Caching.Memory;

namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class VideoController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        private readonly IMemoryCache _cache;

        public VideoController(IOptions<MySettingsModel> app, IMemoryCache memoryCache)  
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
//Video Categories
        [HttpGet("category")]
        [AllowAnonymous]
        public ActionResult category()
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "video.category";
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    SqlParameter[] param = {};
                    lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    "select category, COUNT(Id) as count from View_VideoList_API where isapproved = 1 and category<>'' group by category order by count desc", param);
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
///Video Details for All
        [HttpGet("{vid}")]
        [AllowAnonymous]
        public ActionResult Get(string vid)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "video.detail." + vid;
            if (!_cache.TryGetValue(CacheEntry, out lst))
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
                    lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"select TOP 1 * from View_VideoList_API where videoprivacy='public' and (id=@idorname or name=@idorname)", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                    
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
        }
///Video Details for All
        [HttpGet("translate/{language}/{vid}")]
        [AllowAnonymous]
        public ActionResult GetVidLanguage(string language, string vid)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "video.detail." + vid + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
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
                    if (language != "en")
                    {
                        if (language.Length == 2)
                        {
                            lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                                    @"select TOP 1 *
                                        ,(SELECT top 1  " + language + "_title  FROM [vs_translation] where bcid=vv.bcid) as t_title "+
                                        ",(SELECT top 1 " + language + "_description FROM [vs_translation] where bcid=vv.bcid) as t_desc "+
                                        " from View_VideoList_API vv where videoprivacy='public' and (id=@idorname or name=@idorname)", param);
                            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                            _cache.Set(CacheEntry, lst, cacheEntryOptions);
                            return Ok(lst);
                        }
                        else
                        {
                            return BadRequest(new {error = "Request Terminated!" });
                        }
                    }
                    else{
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                        @"select TOP 1 * from View_VideoList_API where videoprivacy='public' and (id=@idorname or name=@idorname)", param);
                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                        _cache.Set(CacheEntry, lst, cacheEntryOptions);
                        return Ok(lst);
                    }
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
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

                gameHelper.givePoint(appSettings.Value.VShop, "WatchVideo", "Watched a video", user, vid, string.Format("Watched {0} video", vid));
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///Video Details for All
        [HttpGet("translate/{language}/{vid}/{user}")]
        [Authorize]
        public ActionResult GetVidLoginLanguage(string language, string vid, string user)
        {
            try{
                SqlParameter[] paramTemp = {
                    new SqlParameter("@idorname",vid),
                };

                SqlHelper.ExecuteStatement(appSettings.Value.Vtube, 
                @"UPDATE vs_entry_details SET Plays = isnull(Plays,0) + 1, Views= isnull(Plays,0) + 1  WHERE Id = @idorname", paramTemp);

                gameHelper.givePoint(appSettings.Value.VShop, "WatchVideo", "Watched a video", user, vid, string.Format("Watched {0} video", vid));
                
                SqlParameter[] param = {
                    new SqlParameter("@idorname",vid),
                };
                //check if user is a paid member  string membershipType = 
                //check if video is protected     string privacyType = 
                // if (membershipType == "Free") && privacyType == "private") return BadRequest(new {error = "Request Terminated!" });
                //TO DO ARESH
                if (language != "en")
                {
                    if (language.Length == 2)
                    {
                        var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                                @"select TOP 1 *
                                    ,(SELECT top 1  " + language + "_title  FROM [vs_translation] where bcid=vv.bcid) as t_title "+
                                    ",(SELECT top 1 " + language + "_description FROM [vs_translation] where bcid=vv.bcid) as t_desc "+
                                    " from View_VideoList_API vv where (id=@idorname or name=@idorname)", param);
                        return Ok(lst);
                    }
                    else
                    {
                        return BadRequest(new {error = "Request Terminated!" });
                    }
                }
                else{
                    var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"select TOP 1 * from View_VideoList_API where (id=@idorname or name=@idorname)", param);
                    return Ok(lst);
                }
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!"});
            }
        }

///List of all videos by language
        [HttpGet("{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult video(string language, int page, int count)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "video.list." + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                    SqlParameter[] param = {
                        new SqlParameter("@start",start),
                        new SqlParameter("@end",end),
                        new SqlParameter("@language",language),
                    };
                    if (language != "en")
                    {
                        if (language.Length == 2)
                        {
                             lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                                    @"SELECT * FROM 
                                        (SELECT 
                                        ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                                        CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                                        *
                                        ,(SELECT top 1  " + language + "_title  FROM [vs_translation] where bcid=vv.bcid) as t_title "+
                                        ",(SELECT top 1 " + language + "_description FROM [vs_translation] where bcid=vv.bcid) as t_desc "+
                                        "FROM View_VideoList_API vv where isapproved=1 and CHARINDEX(@language,[language]) > 0) v "+
                                        "WHERE rowNumber between @start and @end "+
                                        "order by rowNumber", param);
                                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                                _cache.Set(CacheEntry, lst, cacheEntryOptions);
                            return Ok(lst);
                        }
                        else
                        {
                            return BadRequest(new {error = "Request Terminated!" });
                        }
                    }
                    else{
                         lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                        @"SELECT * FROM 
                            (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                            CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                            * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                            WHERE rowNumber between @start and @end
                            order by rowNumber", param);
                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                        _cache.Set(CacheEntry, lst, cacheEntryOptions);
                        return Ok(lst);
                    }
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
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
                if (language != "en")
                {
                    if (language.Length == 2)
                    {
                        var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                                @"SELECT * FROM 
                                    (SELECT 
                                    ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                                    videoUrlReal as videoUrl,
                                    *
                                    ,(SELECT top 1  " + language + "_title  FROM [vs_translation] where bcid=vv.bcid) as t_title "+
                                    ",(SELECT top 1 " + language + "_description FROM [vs_translation] where bcid=vv.bcid) as t_desc "+
                                    "FROM View_VideoList_API vv where isapproved=1 and CHARINDEX(@language,[language]) > 0) v "+
                                    "WHERE rowNumber between @start and @end "+
                                    "order by rowNumber", param);
                        return Ok(lst);
                    }
                    else
                    {
                        return BadRequest(new {error = "Request Terminated!" });
                    }
                }
                else{
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
            }
            catch(Exception)
            {
                return BadRequest(new {error = "Request Terminated!" });
            }
        }
///List of all videos tag by language
        [HttpGet("tag/{tag}/{language}/{page}/{count}")]
        [AllowAnonymous]
        public ActionResult tagvideo(string tag, string language, int page, int count)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "video.tag." + tag + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                    SqlParameter[] param = {
                        new SqlParameter("@start",start),
                        new SqlParameter("@end",end),
                        new SqlParameter("@language",language),
                        new SqlParameter("@tags",tag),
                    };

                    lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                        CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                        * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and tags like '%'+@tags+'%') v
                        WHERE rowNumber between @start and @end
                        order by rowNumber", param);
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3));
                    _cache.Set(CacheEntry, lst, cacheEntryOptions);
                }
                catch(Exception)
                {
                    return BadRequest(new {error = "Request Terminated!" });
                }
            }
            return Ok(lst);
        }
///List of all videos tag by language of user
        [HttpGet("tag/{tag}/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult videotaguser(string tag, string language, int page, int count, string user)
        {
            try{
                int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                SqlParameter[] param = {
                    new SqlParameter("@start",start),
                    new SqlParameter("@end",end),
                    new SqlParameter("@language",language),
                    new SqlParameter("@tags",tag),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"
                    SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and tags like '%'+@tags+'%') v
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
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "video.highlight." + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = Convert.ToInt32(count) * (Convert.ToInt32(page) - 1) + 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);

                    SqlParameter[] param = {
                        new SqlParameter("@start",start),
                        new SqlParameter("@end",end),
                        new SqlParameter("@language",language),
                    };

                     lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                    @"
                        SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                        CASE WHEN videoPrivacy<>'private' THEN videoUrlReal ELSE '' END as videoUrl,
                        * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0 and isHighlighted=1) v
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
        public ActionResult MostViewVideo(string language, int page, int count)
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
        public ActionResult MostViewVideouser(string language, int page, int count, string user)
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
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
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
        [HttpPost("detail/{user}")]
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
                return BadRequest(new {error = "Request Terminated!"});
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
//get comment list
        [HttpGet("comment/list/{vid}")]
        [AllowAnonymous]
        public ActionResult commentList(string vid)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@title",vid),
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
//get comment list
        [HttpGet("comment/list/{vid}/{user}")]
        [Authorize]
        public ActionResult commentListUser(string vid,string user)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@title",vid),
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
///comment on video user
        [HttpPost("comment/{vid}/{user}")]
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
///search video
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
///premium recommended
        [HttpGet("premium/recommend/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult premiumrecommend(string language, int page, int count, string user)
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
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_premium where isapproved=1 and CHARINDEX(@language,[language]) > 0 and is_recommended=1) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }
///premium view
        [HttpGet("premium/view/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult premiumview(string language, int page, int count, string user)
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
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY views desc) rowNumber,
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
///premium Hottest
        [HttpGet("premium/hot/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult premiumhot(string language, int page, int count, string user)
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
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY likes desc) rowNumber,
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
///premium Top Rated
        [HttpGet("premium/rate/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult premiumrate(string language, int page, int count, string user)
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
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY plays desc) rowNumber,
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
///video Hottest
        [HttpGet("hot/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult videohot(string language, int page, int count, string user)
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
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY likes desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }
///video Top Rated
        [HttpGet("rate/{language}/{page}/{count}/{user}")]
        [Authorize]
        public ActionResult videorate(string language, int page, int count, string user)
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
                @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY plays desc) rowNumber,
                    videoUrlReal as videoUrl,
                    * FROM View_VideoList_API where isapproved=1 and CHARINDEX(@language,[language]) > 0) v
                    WHERE rowNumber between @start and @end
                    order by rowNumber", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }


    }

}