using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace ReptileDemo.Infrastructure
{
    public class HttpHelper
    {
        public static HttpClient HttpClient { get; } = new HttpClient();

        public static string GetHtmlByUrl(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Accept = "application/json, text/javascript, */*; q=0.01";
                webRequest.CookieContainer=new CookieContainer();
                webRequest.Method = "Post";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.UseDefaultCredentials = true;
                var task = webRequest.GetResponseAsync();
                var webResponse = task.Result;
                var stream = webResponse.GetResponseStream();
                var streamReader = new StreamReader(stream, Encoding.UTF8);
                return streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
