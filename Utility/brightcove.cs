using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net.Http;
using RestSharp;

namespace myvapi.Utility
{
    public static class brightcove
    {
        public static string brightcove_token()
        {
            var client = new RestClient("https://oauth.brightcove.com/v4/access_token?grant_type=client_credentials");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "Basic TEST");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AlwaysMultipartFormData = true;
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        public static string brightcove_images(string id, string token)
        {
            var client = new RestClient("https://cms.api.brightcove.com/v1/accounts/3745659807001/videos/"+ id +"/images");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "Bearer "+ token);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        public static string brightcove_create_video(string data, string token){
            var client = new RestClient("https://cms.api.brightcove.com/v1/accounts/3745659807001/videos");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "Bearer "+ token);
            request.AddHeader("Content-Type", "text/plain");
            request.AddParameter("application/x-www-form-urlencoded,text/plain", data,  ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        public static string brightcove_upload_video(string video_id,string data, string token)
        {
            var client = new RestClient("https://ingest.api.brightcove.com/v1/accounts/3745659807001/videos/"+ video_id + "/ingest-requests");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "Bearer "+ token);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Content-Type", "text/plain");
            request.AddParameter("application/json,text/plain", data,  ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }
        
    }
}
