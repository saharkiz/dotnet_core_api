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
    public class VwebController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        private readonly IMemoryCache _cache;

        public VwebController(IOptions<MySettingsModel> app, IMemoryCache memoryCache)  
        {  
            appSettings = app; 
            _cache = memoryCache; 
        } 

        [HttpGet("news/{count}/{page}/{language}")]
        [AllowAnonymous]
        public ActionResult getOldsNews(string count, string page, string language)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.news." + page + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                    SqlParameter[] param = {
                            new SqlParameter("@start",start),
                            new SqlParameter("@end",end),
                            new SqlParameter("@lang",language),
                        };
                    lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                    @"SELECT * FROM 
                    (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_news  where Language=@lang) p
                    WHERE status <> 'Inactive' and language=@lang and rowNumber between @start and @end
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
        [HttpGet("newscategory/{count}/{page}/{language}/{category}")]
        [AllowAnonymous]
        public ActionResult getOldsNewsCategory(string count, string page, string language, string category)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.newscat." + page + language + category;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                    int start = 1;
                    int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                    SqlParameter[] param = {
                            new SqlParameter("@start",start),
                            new SqlParameter("@Category",category),
                            new SqlParameter("@lang",language),
                        };
                    lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                            SELECT top 20 * FROM 
                            (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_news  where Language=@lang) p
                            WHERE status <> 'Inactive' and language=@lang and SubCategory=@Category
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
        [HttpGet("newsmarket/{count}/{page}/{language}/{market}")]
        [AllowAnonymous]
        public ActionResult getOldsNewsMarket(string count, string page, string language, string market)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.newsmarket." + page + language + market;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        int start = 1;
                        int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                        SqlParameter[] param = {
                                new SqlParameter("@start",start),
                                new SqlParameter("@end",end),
                                new SqlParameter("@Market",market),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                            SELECT * FROM 
                            (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_news  where Language=@lang) p
                            WHERE status <> 'Inactive' and language=@lang and Market=@Market and rowNumber between @start and @end
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
        [HttpGet("newscountry/{count}/{page}/{language}/{country}")]
        [AllowAnonymous]
        public ActionResult getOldsNewsCountry(string count, string page, string language, string country)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.newscountry." + page + language + country;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        int start = 1;
                        int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                        SqlParameter[] param = {
                                new SqlParameter("@start",start),
                                new SqlParameter("@end",end),
                                new SqlParameter("@Country",country),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                        SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_news  where Language=@lang) p
                        WHERE status <> 'Inactive' and language=@lang and Country=@Country and rowNumber between @start and @end
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
        [HttpGet("events/{count}/{page}/{language}")]
        [AllowAnonymous]
        public ActionResult getOldsEvents(string count, string page, string language)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.events." + page + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        int start = 1;
                        int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                        SqlParameter[] param = {
                                new SqlParameter("@start",start),
                                new SqlParameter("@end",end),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                        select * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_events  where Language=@lang) p
                        WHERE status <> 'Inactive' and language=@lang and SubParentID is null and rowNumber between @start and @end
                        order by EventStartOn desc", param);
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
        [HttpGet("eventscountry/{count}/{page}/{language}/{country}")]
        [AllowAnonymous]
        public ActionResult getOldsEventsCountry(string count, string page, string language, string country)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.eventscountry." + page + language + country;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        int start = 1;
                        int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                        SqlParameter[] param = {
                                new SqlParameter("@start",start),
                                new SqlParameter("@end",end),
                                new SqlParameter("@Country",country),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                        select * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_events  where Language=@lang) p
                        WHERE status <> 'Inactive' and language=@lang  and Country=@Country and SubParentID is null and rowNumber between @start and @end
                        order by EventStartOn desc", param);
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
        [HttpGet("eventsmarket/{count}/{page}/{language}/{market}")]
        [AllowAnonymous]
        public ActionResult getOldsEventsMarket(string count, string page, string language, string market)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.eventscountry." + page + language + market;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        int start = 1;
                        int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                        SqlParameter[] param = {
                                new SqlParameter("@start",start),
                                new SqlParameter("@end",end),
                                new SqlParameter("@Market",market),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                        SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_events  where Language=@lang and Market=@Market and status <> 'Inactive') p
                        WHERE status <> 'Inactive' and language=@lang  and Market=@Market and SubParentID is null and rowNumber between @start and @end
                        order by EventStartOn desc", param);
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
        [HttpGet("eventscategory/{count}/{page}/{language}/{category}")]
        [AllowAnonymous]
        public ActionResult getOldsEventsCategory(string count, string page, string language, string category)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.eventscountry." + page + language + category;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        int start = 1;
                        int end = Convert.ToInt32(count) * Convert.ToInt32(page);
                        SqlParameter[] param = {
                                new SqlParameter("@start",start),
                                new SqlParameter("@end",end),
                                new SqlParameter("@Category",category),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                        SELECT * FROM 
                        (SELECT ROW_NUMBER() OVER (ORDER BY CreatedOn desc) rowNumber,* from view_events  where Language=@lang and SubCategory=@Category and status <> 'Inactive') p
                        WHERE status <> 'Inactive' and language=@lang  and SubCategory=@Category and SubParentID is null and rowNumber between @start and @end
                        order by EventStartOn desc", param);
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
        [HttpGet("subevents/{id}/{language}")]
        [AllowAnonymous]
        public ActionResult getOldsSubEvents(string id, string language)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.subevents." + id + language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                                new SqlParameter("@id",id),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, @"
                        select * from View_Events where SubParentId=@id and language=@lang and RecordStatus='Published' order by EventStartOn ", param);
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
        [HttpGet("latestnews")]
        [AllowAnonymous]
        public ActionResult getLatestNews()
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.latestnews";
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                        @"select * from view_news where Cast(CreatedOn as date) = cast(GETDATE() as date)", param);
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
        [HttpGet("getnews/{url}/{language}")]
        [AllowAnonymous]
        public ActionResult getNews(string url, string language)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.getnews."+ url+language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                                new SqlParameter("@URL",url),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                        @"select * from view_news where URL=@URL and language=@lang", param);
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
        [HttpGet("newsdate/{mydate}")]
        [AllowAnonymous]
        public ActionResult getLatestNewsByDate(String mydate)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.newsdate."+ mydate;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                                new SqlParameter("@mydate",mydate),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                        @"select * from view_news where Cast(CreatedOn as date) = cast(@mydate as date)", param);
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
        [HttpGet("latestevents")]
        [AllowAnonymous]
        public ActionResult getLatestEvent()
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.latestevents.";
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                        @"select *  from view_events where Cast(CreatedOn as date) = cast(GETDATE() as date)", param);
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
        [HttpGet("getevents/{url}/{language}")]
        [AllowAnonymous]
        public ActionResult getEvents(string url, string language)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.getevents."+ url+language;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                                new SqlParameter("@URL",url),
                                new SqlParameter("@lang",language),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                        @"select *  from view_events where URL=@URL and language=@lang and subparentid is null", param);
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
        [HttpGet("eventsdate/{mydate}")]
        [AllowAnonymous]
        public ActionResult getLatestEventByDate(String mydate)
        {
            var lst = new List<Dictionary<string, object>>();
            string CacheEntry = "website.eventsdate."+ mydate;
            if (!_cache.TryGetValue(CacheEntry, out lst))
            {
                try{
                        SqlParameter[] param = {
                                new SqlParameter("@mydate",mydate),
                            };
                        lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                        @"select replace(replace(REPLACE(description,'""',''''),char(10),' '),char(13),' ') as description,id, 
                        runningNum, Title,localTitle, replace(replace(REPLACE(Summary,'""',''''),char(10),' '),char(13),' ') as Summary,
                        ImageLink,Language,Hits,Rating,language,status,CreatedOn from t_events where Cast(CreatedOn as date) = cast(@mydate as date)", param);
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
        [HttpPost("search/{mytype}")]
        [AllowAnonymous]
        public ActionResult get_Search([FromQuery(Name = "keyword")] String keyword, string mytype)
        {
            /*string searchWord = keyword.Replace(",", "").Replace("'", "").Replace("’", "");
            SqlCommand cmd = new SqlCommand("sp_SearchResult"); //returns only IDs of specific Search
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SearchKey", SqlDbType.VarChar).Value = searchWord;
            cmd.Parameters.Add("@Type", SqlDbType.VarChar).Value = mytype;

            DataSet ds = general.getSet(cmd);
            if (ds.Tables.Count > 0)
            {
                SqlCommand cm = new SqlCommand("sp_Results"); //gets actual values
                cm.CommandType = CommandType.StoredProcedure;
                cm.Parameters.Add("@Type", SqlDbType.VarChar).Value = mytype;
                cm.Parameters.Add("@table", SqlDbType.Structured).Value = ds.Tables[0];
                cm.Parameters.Add("@orderby", SqlDbType.VarChar).Value = "latest";
                DataSet dsResult = general.getSet(cm);
                return returnCheck(dsResult);
            }

            return general.TextToJson("No Search results.");*/
            return BadRequest(new {error = "Request Terminated!" });
        }
    }
}