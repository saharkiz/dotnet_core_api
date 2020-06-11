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

using System.Net.Mail;
//using MailKit.Net.Smtp;
//using MimeKit;

using System.Threading.Tasks;

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
        public ActionResult Get([FromQuery(Name = "return")] string url)
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Username/Password Incorrect! Try Again!";
            }
            else
            {
                ViewBag.msg = "";
            }
            ViewBag.url = "https://vtube.net?q=vtube";
            if (url != null)
            {
                ViewBag.url = url;
            }
            return View("index");
        }
        [HttpGet("forgot_password")]
        [AllowAnonymous]
        public ActionResult GetForgotPassword()
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Password Incorrect! Try Again!";
            }
            else
            {
                ViewBag.msg = "";
            }
            return View("forgot_password");
        }
        [HttpGet("forgot_username")]
        [AllowAnonymous]
        public ActionResult GetForgotUsername()
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Password Incorrect! Try Again!";
            }
            else
            {
                ViewBag.msg = "";
            }
            return View("forgot_username");
        }
        [HttpGet("change_password")]
        [AllowAnonymous]
        public ActionResult GetChangePassword([FromQuery(Name = "token")] string token)
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Password does not meet minimum requirements. Try Again!";
            }
            else
            {
                ViewBag.msg = "";
            }
            ViewBag.token = token;
            return View("change_password");
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
                //"select top 1 id from view_members where isQnetUser is not null and email=@email and NonEncPassword=@password", param);
                "select top 1 id from view_members where email=@email and NonEncPassword=@password", param);

                if (tokenuser.Length < 1){
                    return Redirect("/log?msg=Username or password is incorrect&return=" + model.returnurl.ToString());
                    //return BadRequest(new { message = "Username or password is incorrect" });
                }
                SqlParameter[] parama = {
                    new SqlParameter("@id",tokenuser.ToString()),
                };

                SqlHelper.ExecuteStatement(appSettings.Value.VMembers, 
                "UPDATE view_members SET SharingDetection='vtube.net',last_login_date=GETDATE()  where id=@id", parama);

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
                if (!model.returnurl.Contains("?"))
                {
                    model.returnurl = model.returnurl + "?";
                }
                return Redirect(model.returnurl + "&token="+ finalToken + "&aud="+tokenuser);
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Invalid Parameters" });
            }
        }

        [HttpPost("forgotpassword")]
        [AllowAnonymous]
        public ActionResult forgotpassword([FromForm]FormModel model)
        {
            SqlParameter[] param = {
                new SqlParameter("@email",model.email.ToString()),
            };

            var tokenuser = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
            "select top 1 id from view_members where email=@email", param);

            if (tokenuser.Length > 1){
                try{
                MailAddress fromEmail = new MailAddress("vbox@the-v.net", "The-V");  
                MailMessage message = new MailMessage();
                message.From = fromEmail;
                message.To.Add(model.email.ToLower());
                message.Subject = "Forgot password: Reset here";
                string bodycontent = System.IO.File.ReadAllText(Environment.CurrentDirectory + "/Views/email/forgot_password.html");
                bodycontent = bodycontent.Replace("$$$_Link_$$$","https://api.the-v.net/log/change_password?token=" + tokenuser);
                bodycontent = bodycontent.Replace("$$$_Name_$$$",model.email.ToString());
                message.Body = bodycontent;
                message.IsBodyHtml = true;
                
                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = true;
                client.Port = appSettings.Value.SMTPPort;
                client.Host = appSettings.Value.SMTPServer;
                client.EnableSsl = true;
                
                    client.Send(message);
                    ViewBag.msg = "Forgot Password Sent to your Email: " + model.email.ToLower();
                }
                catch (Exception)
                {
                    ViewBag.msg = "Email Not found. Contact vbox@the-v.net ";
                }
            }
            else{
                ViewBag.msg = "Email Not found.You may Contact vbox@the-v.net";
            }

            return View("forgot_password");
        }
        [HttpPost("forgotusername")]
        [AllowAnonymous]
        public IActionResult forgotusername([FromForm]FormModel model)
        {
            SqlParameter[] param = {
                new SqlParameter("@irid",model.irid.ToString()),
            };

            var tokenuser = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers, 
            "select top 1 email from view_members where irid=@irid", param);

            if (tokenuser.Length > 1){
                MailAddress fromEmail = new MailAddress("vbox@the-v.net", "The-V");  
                MailMessage message = new MailMessage();
                message.From = fromEmail;
                message.To.Add(model.email.ToLower());
                message.Subject = "The-V Forgot Username";
                message.Body = "Account Username: " + tokenuser;
                message.IsBodyHtml = true;
                
                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = true;
                client.Port = appSettings.Value.SMTPPort;
                client.Host = appSettings.Value.SMTPServer;
                client.EnableSsl = true;
                try{
                    client.Send(message);
                    ViewBag.msg = "Username Sent to your Registered Email";
                }
                catch (Exception ex)
                {
                    ViewBag.msg = "User Not found. Contact vbox@the-v.net " + ex.ToString();
                }
            }
            else{
                ViewBag.msg = "Email Not found.You may Contact vbox@the-v.net";
            }

            return View("forgot_username");
        }
        [HttpPost("changepassword")]
        [AllowAnonymous]
        public IActionResult changepassword([FromForm]FormModel model)
        {
            SqlParameter[] param = {
                new SqlParameter("@token",model.token.ToString()),
                new SqlParameter("@pass",model.password.ToString()),
            };

            SqlHelper.ExecuteStatement(appSettings.Value.VMembers, 
            "UPDATE t_members SET NonEncPassword=@pass where id=@token", param);

            ViewBag.msg = "Password changed successfully.";
            return View("change_password");
        }
        
        [HttpGet("activate")]
        [AllowAnonymous]
        public IActionResult activate([FromQuery(Name = "token")] string mytoken)
        {
            try{
                SqlParameter[] param = {
                    new SqlParameter("@token",mytoken.ToString()),
                };

                var tokenuser = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VMembers,
                "select top 1 id from t_members where token=@token", param);

                if (tokenuser.Length < 1){
                    return Redirect("/log?msg=Your membership has Expired&return=https://vtube.net?q=vtube");
                }
                SqlParameter[] parama = {
                    new SqlParameter("@id",tokenuser.ToString()),
                };

                SqlHelper.ExecuteStatement(appSettings.Value.VMembers, 
                "UPDATE view_members SET SharingDetection='vtube.net',last_login_date=GETDATE()  where id=@id", parama);

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
                return Redirect("https://vtube.net?token="+ finalToken + "&aud="+tokenuser);
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Invalid Parameters" });
            }
        }

    }
    public class FormModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string password2 { get; set; }
        public string irid { get; set; }
        public string returnurl { get; set; }
        public string token { get; set; }
    }
    
}