using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using RestSharp;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class SiteController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public SiteController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 

        [HttpGet("language")]
        [AllowAnonymous]
        public ActionResult Language()
        {
            return Ok(new { 
                en = "English",
                ar = "العربية",
                id = "Bahasa Indonesia",
                fa = "فارسی",
                fr = "Français",
                ku = "Kurdish",
                ru = "Русский",
                tr = "Türkçe",
                my = "ဗမာ",
                si = "සිංහල",
                //hi = "Hindi",
                //ta = "தமிழ்"
             });
        }
        [HttpGet("picture")]
        [AllowAnonymous]
        public IActionResult picture([FromQuery(Name = "id")] string id)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",id),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VMembers, 
                @"select avatarURL from t_members where id=@id", param);

                byte[] imagedata = (byte[])lst[0]["avatarURL"];
                if (imagedata.Length > 5)
                {
                    var outputStream = new MemoryStream();
                    using (System.IO.MemoryStream inputStream = new System.IO.MemoryStream(imagedata, true))
                    {
                        using (var image = Image.Load(inputStream))
                        {
                            image.Mutate(x => x.Resize(80, 80));
                            image.SaveAsJpeg(outputStream);
                        }

                        outputStream.Seek(0, SeekOrigin.Begin);
                        return File(outputStream,  "image/jpeg");
                    }
                }
                else
                {
                    return Redirect("https://vtube.net/Resources/vtube/images/UserAvatar(80x80px).jpg");
                }
            }
            catch(Exception)
            {
                return Redirect("https://vtube.net/Resources/vtube/images/UserAvatar(80x80px).jpg");
            }
        }
        [HttpGet("channel")]
        [AllowAnonymous]
        public IActionResult channel([FromQuery(Name = "id")] string id)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@id",id),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Vtube, 
                @"SELECT image FROM vs_channel where id = @id", param);

                byte[] imagedata = (byte[])lst[0]["image"];
                if (imagedata.Length > 5)
                {
                    var outputStream = new MemoryStream();
                    using (System.IO.MemoryStream inputStream = new System.IO.MemoryStream(imagedata, true))
                    {
                        using (var image = Image.Load(inputStream))
                        {
                            image.Mutate(x => x.Resize(180, 180));
                            image.SaveAsJpeg(outputStream);
                        }

                        outputStream.Seek(0, SeekOrigin.Begin);
                        return File(outputStream,  "image/jpeg");
                    }
                }
                else
                {
                    return Redirect("https://vtube.net/Resources/vtube/images/UserAvatar(80x80px).jpg");
                }
            }
            catch(Exception)
            {
                return Redirect("https://vtube.net/Resources/vtube/images/UserAvatar(80x80px).jpg");
            }
        }
        [HttpGet("video/{width}/{height}")]
        [AllowAnonymous]
        public async Task<IActionResult> video([FromQuery(Name = "id")] string id, int width, int height)
        {
            SqlParameter[] param = {
                new SqlParameter("@idorname",id),
            };
            var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
            @"select TOP 1 image from View_VideoList_API where (id=@idorname)", param);
            string url = lst;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                byte[] imagedata = await response.Content.ReadAsByteArrayAsync();
                if (imagedata.Length > 5)
                {
                    var outputStream = new MemoryStream();
                    using (System.IO.MemoryStream inputStream = new System.IO.MemoryStream(imagedata, true))
                    {
                        using (var image = Image.Load(inputStream))
                        {
                            image.Mutate(x => x.Resize(width, height));
                            image.SaveAsJpeg(outputStream);
                        }

                        outputStream.Seek(0, SeekOrigin.Begin);
                        return File(outputStream,  "image/jpeg");
                    }
                }
                else
                {
                    return NotFound("No Data Found!");
                }
            }
        }
        [HttpGet("fix")]
        [AllowAnonymous]
        public IActionResult fix([FromQuery(Name = "id")] string id)
        {
            dynamic json = SimpleJson.DeserializeObject(brightcove.brightcove_token());
            dynamic videoDetails = SimpleJson.DeserializeObject(brightcove.brightcove_images(id, json.access_token));

            SqlParameter[] paramVid = {
                new SqlParameter("@idorname",id),
                new SqlParameter("@ThumbnailUrl",videoDetails.thumbnail.sources[1].src),
                new SqlParameter("@VideoStillUrl",videoDetails.poster.sources[1].src),
            };
            SqlHelper.ExecuteStatement(appSettings.Value.Vtube, 
            @"UPDATE vs_entry_details SET [ThumbnailUrl]=@ThumbnailUrl,[VideoStillUrl]=@VideoStillUrl  where (id=@idorname)", paramVid);
            
            return Ok(videoDetails.poster.sources[1].src);
        }
        [HttpGet("checkfix")]
        [AllowAnonymous]
        public async Task<IActionResult> checkfix([FromQuery(Name = "id")] string id)
        {
            try{
            SqlParameter[] param = {
                new SqlParameter("@idorname",id),
            };
            var lst = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Vtube, 
            @"select TOP 1 image from View_VideoList_API where (id=@idorname)", param);
            string url = lst;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                byte[] imagedata = await response.Content.ReadAsByteArrayAsync();
                if (imagedata.Length > 5)
                {
                    
                    dynamic json = SimpleJson.DeserializeObject(brightcove.brightcove_token());
                    dynamic videoDetails = SimpleJson.DeserializeObject(brightcove.brightcove_images(id, json.access_token));

                    SqlParameter[] paramVid = {
                        new SqlParameter("@idorname",id),
                        new SqlParameter("@ThumbnailUrl",videoDetails.thumbnail.src),
                        new SqlParameter("@VideoStillUrl",videoDetails.poster.src),
                    };
                    SqlHelper.ExecuteStatement(appSettings.Value.Vtube, 
                    @"UPDATE vs_entry_details SET [ThumbnailUrl]=@ThumbnailUrl,[VideoStillUrl]=@VideoStillUrl  where (id=@idorname)", paramVid);
                    
                    return Ok(videoDetails.poster.src);
                    
                }
                else
                {
                    return NotFound("Image Exists");
                }
            }
            }
            catch(Exception)
            {
                    return NotFound("URL Does NOT Exist");
            }
        }
        
        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> check()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("name","ARESH test video curl");
            dict.Add("description","short description");
            dict.Add("long_description","long description");
            dict.Add("reference_id","dynamic-ingestion0203");
            dict.Add("tags", new string[] {"user","upload"});
            string data = SimpleJson.SerializeObject(dict);

            IDictionary<string, object> filevid = new Dictionary<string, object>();
            filevid.Add("url","http://learning-services-media.brightcove.com/videos/mp4/Bird_Woodpecker.mp4");

            IDictionary<string, object> dictVid = new Dictionary<string, object>();
            dictVid.Add("master", filevid);
            dictVid.Add("profile","Asia-PREMIUM");
            string uplo = SimpleJson.SerializeObject(dictVid);

            await upload_Video(uplo,data); 

            return Ok("Upload Complete");
        }
    
        private async Task upload_Video(string uplo, string data)
        {
            string tokencall = await Task.Run(() => {
                return brightcove.brightcove_token();
            });
            
            dynamic json = SimpleJson.DeserializeObject(tokencall);

            string viddetail = await Task.Run(() => {
                return brightcove.brightcove_create_video(data, json.access_token);
            });
            dynamic videoDetails = SimpleJson.DeserializeObject(viddetail);
            
            string vidupload= await Task.Run(() => {
                return brightcove.brightcove_upload_video(videoDetails.id,uplo, json.access_token);
            });
            //dynamic videoUpload = SimpleJson.DeserializeObject(vidupload);
        }
    }
}