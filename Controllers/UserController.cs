using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using Microsoft.AspNetCore.Http;
using System.IO;

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace myvapi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class UserController : ControllerBase //for Web API ONLY
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public UserController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 
        [AllowAnonymous]
        [HttpGet("authenticate")]
        public IActionResult SeeAuthenticate()
        {
            return NotFound(new {error = "Request Terminated!" });
        }
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]Dictionary<string, object> model)
        {
            try{
                if (model.Count < 4)
                    return BadRequest(new { message = "Incomplete Parameters" });
                
                if (model["audience"] == null)
                    return BadRequest(new { message = "Invalid Audience" });
                if (model["issuer"] == null)
                    return BadRequest(new { message = "Invalid Issuer" });
                
                SqlParameter[] param = {
                    new SqlParameter("@email",model["username"].ToString()),
                    new SqlParameter("@password",model["password"].ToString()),
                };

                var tokenuser = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
                "select top 1 id from view_members where isQnetUser is not null and email=@email and NonEncPassword=@password", param);

                if (tokenuser.Length < 1)
                    return BadRequest(new { message = "Username or password is incorrect" });

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(appSettings.Value.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] 
                    {
                        new Claim(ClaimTypes.Name, "UID")
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Audience = model["audience"].ToString(),
                    Issuer = model["issuer"].ToString(),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var finalToken = tokenHandler.WriteToken(token);

                var output = new {ver=3.141, auth_time= DateTime.Now.Ticks, token_type="Bearer", access_token=finalToken, aud=tokenuser, lang="en"};
                return Ok(output);
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Invalid Parameters" });
            }
        }

        [AllowAnonymous]
        [HttpPost("Validate")]
        public IActionResult Validate([FromBody]string model)
        {
            return Ok(model);
        }

        [HttpGet]
        public ActionResult Get()
        {
            return NotFound("Request Terminated!");
        }
        
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(string id)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@userid",id),
                };

                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VMembers, 
                "select top 1 irid,membership,first_name,membership_end,points from view_members where id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("data/{id}")]
        [Authorize]
        public IActionResult data(string id)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@userid",id),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VMembers, 
                "select TOP 1 * from View_Members_API where id=@userid", param);
                return Ok(lst);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }
//forgot password
///TO DOOOOOO
        [HttpPost]
        public ActionResult Post([FromBody] Dictionary<string, object> obj)
        {
            return Ok(obj);
        }

        [HttpPost("upload/{user}")]
        [Authorize]
        public ActionResult upload([FromForm(Name = "files")] List<IFormFile> files, string user)
        {
            string subDirectory = "upload";
             var target = Path.Combine(Environment.CurrentDirectory, subDirectory);
                                      
            Directory.CreateDirectory(target);

            files.ForEach(async file =>
            {
                if (file.Length <= 0) return;
                String ret = Regex.Replace(file.FileName.Trim(), "[^A-Za-z0-9.]+", "");
                var finalFileName = user + "_profile_" + ret.Replace(" ", String.Empty);
                var filePath = Path.Combine(target, finalFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            });
            return Ok(new { files.Count, Size = SqlHelper.SizeConverter(files[0].Length) });
        } 
    }
    
}
