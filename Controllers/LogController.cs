using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;
using System.Text.Json;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.IO;

using Microsoft.AspNetCore.Authorization;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MailKit.Net.Smtp;

namespace myvapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : Controller
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public LogController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Get()
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Username/Password Incorrect! Try Again!";
                ViewBag.css = "alert alert-success";
            }
            else
            {
                ViewBag.msg = "Enter your username and password to sign in. You will be redirect back upon successful sign in.";
                ViewBag.css = "alert";
            }
            return View("index");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post([FromForm]FormModel model)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@email",model.email.ToString()),
                    new SqlParameter("@password",model.password.ToString()),
                };

                var tokenuser = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
                "select top 1 id from view_members where isQnetUser is not null and email=@email and NonEncPassword=@password", param);

                if (tokenuser.Length < 1)
                    return Redirect("/log?msg=Username or password is incorrect");
                    //return BadRequest(new { message = "Username or password is incorrect" });

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(appSettings.Value.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] 
                    {
                        new Claim(ClaimTypes.Name, "UID")
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Audience = "ats",
                    Issuer = "aresh",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var finalToken = tokenHandler.WriteToken(token);

                var output = new {ver=3.141, auth_time= DateTime.Now.Ticks, token_type="Bearer", access_token=finalToken, aud=tokenuser, lang="en"};
                //return Ok(output);
                return Redirect("http://vtube.net?token="+ finalToken + "&aud="+tokenuser);
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Invalid Parameters" });
            }
        }

        [HttpPost("forgotpassword")]
        [AllowAnonymous]
        public IActionResult forgotpassword([FromForm]FormModel model)
        {
            var message = new Message(new string[] { "s.aresh@yahoo.com" }, "Test email", "This is the content from our email.");
            EmailConfiguration _mailconfig = new EmailConfiguration(); 
            EmailSender _emailSender = new EmailSender(_mailconfig);
            _emailSender.SendEmail(message);
            return Ok("sending Email");
        }

    }
    public class FormModel
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}