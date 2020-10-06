using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Options;
using myvapi.Utility;

using Microsoft.AspNetCore.Authorization;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;

using System.Security.Cryptography;
using System.Collections;
//sudo dotnet publish --configuration Release
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
                "select top 1 id from view_members where email=@email and Password=@password", param);

                if (tokenuser.Length < 1){
                    return Redirect("/log?msg=Username or password is incorrect&return=" + model.returnurl.ToString());
                    //return BadRequest(new { message = "Username or password is incorrect" });
                }
                SqlParameter[] parama = {
                    new SqlParameter("@id",tokenuser.ToString()),
                };

                SqlHelper.ExecuteStatement(appSettings.Value.VMembers, 
                "UPDATE view_members SET last_login_date=GETDATE()  where id=@id", parama);

                gameHelper.givePoint(appSettings.Value.VShop, "login", "You logged-in", tokenuser, tokenuser, string.Format("Vtube.net {0} login", model.email.ToString()));
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

            var tokenuser = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VMembers, 
            "select top 1 email from view_members where irid=@irid", param);

            if (tokenuser.Count > 0){
                MailAddress fromEmail = new MailAddress("vbox@the-v.net", "The-V");  
                MailMessage message = new MailMessage();
                message.From = fromEmail;
                message.To.Add(tokenuser[0]["email"].ToString().ToLower());
                message.Subject = "The-V Forgot Username";
                string bodycontent = System.IO.File.ReadAllText(Environment.CurrentDirectory + "/Views/email/forgot_username.html");
                bodycontent = bodycontent.Replace("$$$_Link_$$$",tokenuser[0]["email"].ToString());
                bodycontent = bodycontent.Replace("$$$_Name_$$$",model.irid.ToString());
                message.Body = bodycontent;
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

 #region QNET Activate       
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
#endregion

#region two factor authentication
        [HttpGet("index_2f")]
        [AllowAnonymous]
        public ActionResult index2f()
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Two Factor Authentication Required";
            }
            else
            {
                ViewBag.msg = "";
            }
            return View("index2f");
        }
        [HttpGet("enableauthenticator")]
        [AllowAnonymous]
        public ActionResult enableauthenticator()
        {
            
                string email = "";
                string unformattedKey = TimeSensitivePassCode.GeneratePresharedKey();
                string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
                string temp = string.Format(
                AuthenticatorUriFormat, "vtube.net", email, unformattedKey);
                ViewBag.msg = "Two Factor Authentication is Enabled.";
                ViewBag.SharedKey = unformattedKey;
                ViewBag.AuthenticatorUri = temp;
                return View("enableAuthenticator");
        }
        [HttpPost("enable_authenticator")]
        [AllowAnonymous]
        public ActionResult enable_authenticator([FromForm]FormModel model)
        {
            string unformattedKey = model.hash;
            IList<string> result = TimeSensitivePassCode.GetListOfOTPs(unformattedKey);
            if (model.qrcode == result[0])
            {
                ViewBag.msg ="Matched";
            }
            else{
                ViewBag.msg ="Not matched";
            }
            return View("enableAuthenticator");
        }
        [HttpPost("index_2f")]
        [AllowAnonymous]
        public ActionResult index_2f([FromForm]FormModel model)
        {
            string unformattedKey = model.hash;
            IList<string> result = TimeSensitivePassCode.GetListOfOTPs(unformattedKey);
            if (model.qrcode == result[0])
            {
                ViewBag.msg ="Matched";
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

                var output = new {ver=3.141, auth_time= DateTime.Now.Ticks, token_type="Bearer", access_token=finalToken, aud=unformattedKey, lang="en"};
            }
            else{
                ViewBag.msg ="ERROR: Two Factor Authentication did Not matched";
            }
            return View("index2f");
        }
 #endregion

 #region ZOOM meeting
        [HttpGet("zoom")]
        [AllowAnonymous]
        public ActionResult zoom([FromQuery(Name = "id")] string id = "Vtube User")
        {
            if (Request.Scheme != "https")
            {
                return NotFound("Please use Https Secure Url");
            }
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Welcome to ZOOM Meeting";
            }
            else
            {
                ViewBag.msg = "";
            }
            string apiKey = "";
            string apiSecret = "";
            string meetingNumber = "";
            String ts = (zoomHelper.ToTimestamp(DateTime.UtcNow.ToUniversalTime()) - 30000).ToString();
            string role = "0"; // 0 for participants & joining webinars
            string token = zoomHelper.GenerateToken (apiKey, apiSecret, meetingNumber, ts, role);

            ViewBag.apiKey = apiKey;
            ViewBag.passWord = ""; //meeting password
            ViewBag.token = token;
            ViewBag.meetingNumber = meetingNumber;
            ViewBag.userName = id;
            return View("index_zoom");
        }
        [HttpGet("zoom_host")]
        [AllowAnonymous]
        public ActionResult zoom_host()
        {
            if (Request.Scheme != "https")
            {
                return NotFound("Please use Https Secure Url");
            }
            if (Request.QueryString.ToString().Length > 0)
            {
                ViewBag.msg = "Welcome to ZOOM Meeting";
            }
            else
            {
                ViewBag.msg = "";
            }
            string apiKey = "";
            string apiSecret = "";
            string meetingNumber = "";
            String ts = (zoomHelper.ToTimestamp(DateTime.UtcNow.ToUniversalTime()) - 30000).ToString();
            string role = "0"; 	//1 for meeting host, 0 for participants & joining webinars
            string token = zoomHelper.GenerateToken (apiKey, apiSecret, meetingNumber, ts, role);

            ViewBag.apiKey = apiKey;
            ViewBag.passWord = ""; //meeting password
            ViewBag.token = token;
            ViewBag.meetingNumber = meetingNumber;
            ViewBag.userName = "Host Name";
            return View("zoom_host");
        }
 #endregion       

#region Payment Link
        [HttpGet("payment")]
        [AllowAnonymous]
        public ActionResult GetPaymentLink([FromQuery(Name = "token")] string token)
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                SqlParameter[] param = {
                    new SqlParameter("@token",token),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"select TOP 1 * from paymentlink where token=@token", param);
                ViewBag.msg = "";
                ViewBag.invnumber = lst[0]["token"];
                ViewBag.invdate = lst[0]["date"];
                ViewBag.invperson = lst[0]["name"];
                ViewBag.invamountusd = lst[0]["amountusd"];
                ViewBag.invamountmyr = lst[0]["amountmyr"];
                ViewBag.description = lst[0]["note"];
                ViewBag.email = lst[0]["email"];

            }
            else
            {
                ViewBag.msg = "";
            }
            return View("paymentlink");
        }
        [HttpGet("payresponse")]
        [AllowAnonymous]
        public ActionResult GetPaymentResponse([FromQuery(Name = "token")] string token)
        {
            if (Request.QueryString.ToString().Length > 0)
            {
                SqlParameter[] param = {
                    new SqlParameter("@token",token),
                };
                var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"select TOP 1 * from paymentlink where token=@token", param);
                ViewBag.msg = "Thank you for processing your payment, We will Update you once payment is Confirmed.";
                ViewBag.invnumber = lst[0]["token"];
                ViewBag.invdate = lst[0]["date"];
                ViewBag.invperson = lst[0]["name"];
                ViewBag.invamountusd = lst[0]["amountusd"];
                ViewBag.invamountmyr = lst[0]["amountmyr"];
                ViewBag.description = lst[0]["note"];
                ViewBag.email = lst[0]["email"];

            }
            else
            {
                ViewBag.msg = "";
            }
            return View("paymentlink");
        }
        [HttpPost("pay")]
        [AllowAnonymous]
        public IActionResult payRHB([FromForm]FormModel model)
        {
            SqlParameter[] param = {
                new SqlParameter("@token",model.token.ToString()),
            };
            var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.VShop, 
                @"select TOP 1 * from paymentlink where token=@token", param);
            try
            {
                string TransactionID = lst[0]["token"] + "_" + DateTime.Now.ToUniversalTime().Ticks.ToString();
                string Merchant = appSettings.Value.Merchant;
                string Access = appSettings.Value.Access;
                string SECURE_SECRET = appSettings.Value.SecureSecret;
                System.Collections.SortedList transactionData = new System.Collections.SortedList(new VPCStringComparer());
                string BaseString = "https://migs.mastercard.com.au/vpcpay";
                string queryString = "";
                string AqueryString = "";

                transactionData.Add("vpc_Command", "pay");
                transactionData.Add("vpc_Version", "1");
                transactionData.Add("vpc_Merchant", Merchant);
                transactionData.Add("vpc_AccessCode", Access);

                transactionData.Add("vpc_MerchTxnRef", TransactionID);
                transactionData.Add("vpc_OrderInfo", TransactionID); //user id
                transactionData.Add("vpc_Amount", Convert.ToInt32(Convert.ToDouble(lst[0]["amountmyr"]) * 100));
                transactionData.Add("vpc_ReturnURL", "https://api.the-v.net/payresponse?invoice=" + TransactionID);
                transactionData.Add("vpc_Locale", "en");

                foreach (System.Collections.DictionaryEntry item in transactionData)
                {
                    queryString += System.Web.HttpUtility.UrlEncode(item.Key.ToString()) + "=" + item.Value.ToString() + "&";
                    AqueryString += System.Web.HttpUtility.UrlEncode(item.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }
                string signature = "";
                if (SECURE_SECRET.Length > 0)
                {
                    queryString = queryString.Remove(queryString.Length - 1, 1);
                    // create the signature and add it to the query string
                    signature = CreateSHA256Signature(queryString);// Helper.CreateMD5Signature(rawHashData);
                    AqueryString += "vpc_SecureHash=" + signature + "&vpc_SecureHashType=SHA256";
                }
                return Redirect(BaseString + "?" + AqueryString);
            }
            catch (Exception ex)
            {
                ViewBag.msg = "RHB Error for Token: " + model.token.ToString() + "<br/>" + ex.ToString();
                return View("paymentlink");
            }
            
        }


        private class VPCStringComparer : IComparer
        {
            public int Compare(Object a, Object b)
            {
                if (a == b) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                string sa = a as string;
                string sb = b as string;
                System.Globalization.CompareInfo myComparer = System.Globalization.CompareInfo.GetCompareInfo("en-US");
                if (sa != null && sb != null)
                {
                    return myComparer.Compare(sa, sb, System.Globalization.CompareOptions.Ordinal);
                }
                throw new ArgumentException("a and b should be strings.");
            }
        }

        private string CreateSHA256Signature(String sb)
        {
            string SECURE_SECRET = appSettings.Value.SecureSecret;
            byte[] convertedHash = new byte[SECURE_SECRET.Length / 2];
            for (int i = 0; i < SECURE_SECRET.Length / 2; i++)
            {
                convertedHash[i] = (byte)Int32.Parse(SECURE_SECRET.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            // Create secureHash on string
            string hexHash = "";
            using (HMACSHA256 hasher = new HMACSHA256(convertedHash))
            {
                byte[] hashValue = hasher.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                foreach (byte b in hashValue)
                {
                    hexHash += b.ToString("X2");
                }
            }
            return hexHash;
        }
#endregion


    }
    public class FormModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string password2 { get; set; }
        public string irid { get; set; }
        public string returnurl { get; set; }
        public string token { get; set; }

        public string qrcode{ get; set; }
        public string hash{ get; set; }
    }
}