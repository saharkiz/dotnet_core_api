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
using RestSharp;
//sudo dotnet publish --configuration Release
namespace myvapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [NonController]
    public class PromoController : Controller
    {
        private readonly IOptions<MySettingsModel> appSettings; 
        public PromoController(IOptions<MySettingsModel> app)  
        {  
            appSettings = app;  
        } 
        [HttpGet("ir")]
        [AllowAnonymous]
        public ActionResult GetIR([FromQuery(Name = "irid")] string irid)
        {
            return Ok(SqlHelper.getIR(irid));
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Get([FromQuery(Name = "msg")] string msg)
        {
            ViewBag.msg = msg;
            ViewBag.Name = "PROMO TICKET PURCHASE";
            ViewBag.Entry = "none";
            ViewBag.EntryCode = true;
            ViewBag.EntryShow = false;
            ViewBag.EntryResult = false;
            ViewBag.Code = "block";
            return View("promo");
        }
        [HttpPost("checkcode")]
        [AllowAnonymous]
        public ActionResult checkcode([FromForm]PromoModel model)
        {
            ViewBag.EntryResult = false;
            ViewBag.EntryCode = false;
            if (model.inputPromoCode.Length >0)
            {
                 SqlParameter[] param = {
                    new SqlParameter("@promoCode",model.inputPromoCode.ToString()),
                };
                var promoTable = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Promo,
                "select PromoCode,price,EventName from t_VCON_Promo where PromoCode=@promoCode and Used < Quantity", param);

                if (promoTable.Count > 0)
                {
                    ViewBag.Name = promoTable[0]["EventName"].ToString();
                    var isValid = promoTable[0]["PromoCode"].ToString();
                    if (isValid.Length > 0)
                    {
                        ViewBag.PriceUSD = Convert.ToDouble(promoTable[0]["price"]).ToString("F2");
                        Double Myr = Convert.ToDouble(getExchange()) * Convert.ToDouble(promoTable[0]["price"]);
                        ViewBag.PriceMYR = Myr.ToString("F2");
                        ViewBag.PromoCode = model.inputPromoCode.ToString();
                        ViewBag.Entry = "flex";
                        ViewBag.EntryShow = true;
                        ViewBag.Code = "none";
                        return View("promo");
                    }
                    else
                    {
                        return Redirect("/promo?msg=Promo Code Invalid");
                    }
                }
                else
                {
                    return Redirect("/promo?msg=Promo Code Not Valid");
                }
            }
            else
            {
                return Redirect("/promo?msg=Promo Code Empty");
            }
        }
        [HttpPost("ticket")]
        [AllowAnonymous]
        public ActionResult ticket([FromForm]PromoModel model)
        {
            if (model.inputIrid.Length == 8)
            {
                if (checkDuplicate(model.inputEmail, model.inputIrid, model.inputPromoCodeEnter))
                {
                    return Redirect("/promo?msg=You cannot purchase another Ticket. Please Try Again.");
                }
                string scode = promoCode(model.inputPromoCodeEnter);
                string TransactionID = scode + "_" + DateTime.Now.ToUniversalTime().Ticks.ToString();
                SqlParameter[] param = {
                    new SqlParameter("@EVENTATS", scode),
                    new SqlParameter("@CustomerID","1"),
                    new SqlParameter("@FullName",model.inputFirstName + " " + model.inputLastName),
                    new SqlParameter("@Email",model.inputEmail),
                    new SqlParameter("@IRID",model.inputIrid),
                    new SqlParameter("@amount",getPrice(model.inputPromoCodeEnter)),
                    new SqlParameter("@ref", TransactionID),
                    new SqlParameter("@promocode",model.inputPromoCodeEnter.ToUpper()),
                    new SqlParameter("@tamount",model.inputAmount),
                    new SqlParameter("@nticket","1"),
                    new SqlParameter("@country",model.inputCountry),
                    new SqlParameter("@contact",model.inputEmergencyNumber),
                    new SqlParameter("@team",model.inputTeam),
                    new SqlParameter("@passport",model.inputPassportNumber ?? ""),
                };
                SqlHelper.ExecuteStatement(appSettings.Value.Promo,
                @"insert into t_ticketPromoT(event,CustomerID,FullName,status,
                                            Email,IRID,createdon,amount,ref,promocode,
                                            totalAmount,noticket,country,contact,team,passportNum) 
                                    values 
                                            (@EVENTATS,@CustomerID,@FullName,'',
                                            @Email,@IRID, GETDATE(),@amount,@ref,@promocode,
                                            @tamount,@nticket,@country,@contact,@team,@passport)", param);
                

                return Redirect("/promo/pay?transactionid="+TransactionID);
            }
            else{
                return Redirect("/promo?msg=Invalid IR ID. Please Try Again.");
            }
        }
        [HttpGet("pay")]
        [AllowAnonymous]
        public ActionResult pay([FromQuery(Name = "transactionid")] string transactionID)
        {
            
            ViewBag.EntryShow = false;
            ViewBag.EntryResult = false;
            SqlParameter[] param = {
                new SqlParameter("@token",transactionID),
            };
            var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Promo, 
                @"select TOP 1 * from t_ticketPromoT where ref=@token", param);
            try
            {
                string TransactionID = transactionID;
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
                transactionData.Add("vpc_Amount", Convert.ToInt32(Convert.ToDouble(lst[0]["totalAmount"]) * 100));
                transactionData.Add("vpc_ReturnURL", "https://promo.the-v.net/promo/result?invoice=" + TransactionID);
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
                //return View("promo");
            }
            catch (Exception)
            {
                return Redirect("/promo?msg=Invalid Bank Transaction. Please Try Again.");
            }
        }
        [HttpGet("result")]
        [AllowAnonymous]
        public ActionResult result([FromQuery(Name = "invoice")] string invoice, 
                                    [FromQuery(Name = "vpc_OrderInfo")] string vpc_OrderInfo, 
                                    [FromQuery(Name = "vpc_TxnResponseCode")] string vpc_TxnResponseCode,
                                    [FromQuery(Name = "vpc_Message")] string vpc_Message)
        {
            ViewBag.Entry = "none";
            ViewBag.Invoice = invoice;
            ViewBag.Today = DateTime.Now.ToString();
            ViewBag.EntryShow = false;
            ViewBag.EntryResult = true;
            ViewBag.EntryCode = false;

            try{
                string transactionID = vpc_OrderInfo.Length > 0 ? vpc_OrderInfo : "Unknown";
                string txnResponseCode = vpc_TxnResponseCode.Length > 0 ? vpc_TxnResponseCode : "Unknown";
                ViewBag.Invoice = transactionID;
                if (txnResponseCode.Equals("0"))
                {
                    ViewBag.Success = "";
                    ViewBag.Fail = "hidden";

                        SqlParameter[] param = {
                            new SqlParameter("@token",transactionID),
                        };
                        var lst = SqlHelper.ExecuteStatementDataTable(appSettings.Value.Promo, 
                            @"select TOP 1 * from t_ticketPromoT where ref=@token", param);

                        UpdateStatus(transactionID);
                        IncrementCode(lst[0]["promocode"].ToString());

                                ViewBag.EventName = promoName(lst[0]["promocode"].ToString());
                                ViewBag.Irid = lst[0]["IRID"].ToString();
                                ViewBag.MyName = lst[0]["FullName"].ToString();
                                ViewBag.Email = lst[0]["Email"].ToString();
                                string bodycontent = System.IO.File.ReadAllText(Environment.CurrentDirectory + "/Views/email/promo_invoice.html");
                                bodycontent = bodycontent.Replace("$$$_Name_$$$",lst[0]["FullName"].ToString());

                                bodycontent = bodycontent.Replace("$$$_Trans_$$$",transactionID.ToString());
                                bodycontent = bodycontent.Replace("$$$_Resp_$$$",txnResponseCode.ToString());
                                bodycontent = bodycontent.Replace("$$$_Amount_$$$",lst[0]["totalAmount"].ToString());
                                bodycontent = bodycontent.Replace("$$$_Amountm_$$$",lst[0]["amount"].ToString());
                                bodycontent = bodycontent.Replace("$$$_Irid_$$$",lst[0]["IRID"].ToString());
                                bodycontent = bodycontent.Replace("$$$_Email_$$$",lst[0]["Email"].ToString());
                                bodycontent = bodycontent.Replace("$$$_Event_$$$",promoName(lst[0]["event"].ToString()));
                                bodycontent = bodycontent.Replace("$$$_OrderDate_$$$",lst[0]["createdon"].ToString());
                                
                                MailAddress fromEmail = new MailAddress("vbox@the-v.net", "The-V");  
                                MailMessage message = new MailMessage();
                                message.From = fromEmail;
                                message.To.Add(lst[0]["Email"].ToString().ToLower());
                                message.Bcc.Add("aresh.saharkhiz@the-v.net");
                                message.Bcc.Add("davoud@the-v.net");
                                message.Subject = "The-V Promo Invoice";
                                message.Body = bodycontent;
                                message.IsBodyHtml = true;
                                
                                SmtpClient client = new SmtpClient();
                                client.UseDefaultCredentials = true;
                                client.Port = appSettings.Value.SMTPPort;
                                client.Host = appSettings.Value.SMTPServer;
                                client.EnableSsl = true;
                                try{
                                    client.Send(message);
                                }
                                catch (Exception)
                                {
                                }
                }
                else
                {
                    ViewBag.Success = "hidden";
                    ViewBag.Fail = "";
                    ViewBag.Error = vpc_Message;
                }
            }
            catch(Exception){
                ViewBag.Success = "hidden";
                ViewBag.Fail = "";
                ViewBag.Error = "Invalid Payment Gateway";
            }
            return View("promo");
        }

#region private funcs
        private void IncrementCode(string enteredcode)
        {
            SqlParameter[] param = {
                new SqlParameter("@promoCode",enteredcode),
            };
            var promo = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Promo,
            "Update t_VCON_Promo set Used=Used+1 where PromoCode=@promoCode", param);
        }
        private void UpdateStatus(string transactionID)
        {
            SqlParameter[] paramser = {
                                new SqlParameter("@token",transactionID),
                            };
                            SqlHelper.ExecuteStatement(appSettings.Value.Promo, 
                                @"UPDATE t_ticketPromoT set status='SUCCESS', UpdatedOn=getdate() where ref=@token", paramser);
        }
        private String promoName(string enteredcode){
            SqlParameter[] param = {
                new SqlParameter("@promoCode",enteredcode),
            };
            var promo = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Promo,
            "select EventName from t_VCON_Promo where PromoCode=@promoCode", param);
            return promo;
        }
        private String promoCode(string enteredcode){
            SqlParameter[] param = {
                new SqlParameter("@promoCode",enteredcode),
            };
            var promo = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Promo,
            "select EventCode from t_VCON_Promo where PromoCode=@promoCode", param);
            return promo;
        }
        private Double getPrice(string promocode){
            SqlParameter[] param = {
                new SqlParameter("@promoCode",promocode),
            };
            var promoPrice = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Promo,
            "select price from t_VCON_Promo where PromoCode=@promoCode and Used < Quantity", param);
            return Convert.ToDouble(promoPrice);
        }
        private Double getExchange(){
            SqlParameter[] param = {
            };
            var promoRate = SqlHelper.ExecuteStatementReturnString(appSettings.Value.VShop,
            "SELECT rate FROM [vs_exchange_rate] where currency='MYR'", param);
            return Convert.ToDouble(promoRate);
        }
        private bool checkDuplicate(string email,string irid, string enteredcode){
            SqlParameter[] param = {
                new SqlParameter("@EVENTATS",enteredcode),
                new SqlParameter("@Email",email),
                new SqlParameter("@Irid",irid),
            };
            var purchaseIR = SqlHelper.ExecuteStatementReturnString(appSettings.Value.Promo,
            "select isnull(irid,'') as irid from t_ticketPromoT where (email=@Email OR irid=@Irid) and event=@EVENTATS and status='SUCCESS'", param);
            if (purchaseIR != "")
            {
                return true;	
            }
            else{
                return false;
            }
        }
#endregion

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
    }
    public class PromoModel
    {
        public string inputFirstName { get; set; }
        public string inputLastName { get; set; }
        public string inputIrid { get; set; }
        public string inputEmail { get; set; }
        public string inputCountry { get; set; }
        public string inputTeam { get; set; }
        public string inputEmergencyNumber { get; set; }
        public string inputPassportNumber { get; set; }
        public string inputPromoCodeEnter { get; set; }
        public string inputAmount { get; set; }
        public string inputAmountUs { get; set; }

        public string inputPromoCode{ get; set; }
    }
}