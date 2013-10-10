using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;

namespace BalanceTracker.Http
{
    public class HttpRepository<TSession> where TSession : HttpSession
    {
        public HttpRepository(TSession session)
        {
            Session = session;
        }

        protected TSession Session { get; set; }

        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36";

        public HttpWebRequest GetHttpRequest(HttpMethod method, Uri uri, NameValueCollection parameters = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method.ToString().ToUpper();
            request.CookieContainer = Session.Cookies;

            if (method == HttpMethod.Post && parameters != null && parameters.Count > 0)
            {
                var formData = GetFormData(parameters);
                var postStream = request.GetRequestStream();
                using (var writer = new StreamWriter(postStream))
                {
                    writer.Write(formData);
                }

                request.ContentType = "application/x-www-form-urlencoded";
            }


            request.UserAgent = UserAgent;
            request.AllowAutoRedirect = true;
            return request;
        }

        public string GetResponseBody(HttpMethod method, Uri uri, NameValueCollection parameters = null)
        {
            var request = GetHttpRequest(method, uri, parameters);

            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetFormData(NameValueCollection parameters)
        {
            string result = null;

            foreach (var key in parameters.AllKeys)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += "&";
                }

                result += string.Format("{0}={1}", System.Web.HttpUtility.UrlEncode(key), 
                                        System.Web.HttpUtility.UrlEncode(parameters[key]));
            }

            return result;
        }

        protected string GetString(Uri uri)
        {
            return GetResponseBody(HttpMethod.Get, uri);
        }
    }
}