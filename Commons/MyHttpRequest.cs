using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Web;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 發出 HTTP Request 並取回結果內容的工具類
    /// </summary>
    public class MyHttpRequest
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string MyUserAgent = "Coach.Taiwanjobs.HttpRequest";
        private HttpWebRequest httpRequest;
        private int requestTimeout = 100000;        // default set to 10 seconds
        private int statusCode = 0;
        private string StatusDescription = "";

        /// <summary>
        /// HttpRequest 的建構子，HTTP request timeout 預設為10秒
        /// </summary>
        public MyHttpRequest()
        {
        }

        /// <summary>
        /// 指定 HTTP request timeout 毫秒數的 HttpRequest 的建構子
        /// </summary>
        /// <param name="requestTimeout"></param>
        public MyHttpRequest(int requestTimeout)
        {
            this.requestTimeout = requestTimeout;
        }

        public int getStatusCode()
        {
            return statusCode;
        }

        public string getStatusDescription()
        {
            return StatusDescription;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP Get Request，並回傳一個內容字串，
        /// </summary>
        /// <param name="requestUrl">本次 Request 的目的 url</param>
        /// <param name="parms">各項 Http Get 參數</param>
        /// <returns></returns>
        public string Get(string requestUrl, Hashtable parms)
        {
            return request("GET", requestUrl, parms);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP Get Request，並回傳一個內容字串，
        /// </summary>
        /// <param name="requestUrl">本次 Request 的目的 url</param>
        /// <param name="parms">QueryString 格式的參數字串, 例: A=12&B=34</param>
        /// <returns></returns>
        public string Get(string requestUrl, string parms)
        {
            return request("GET", requestUrl, parms);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP POST Request，並回傳一個內容字串，
        /// <para>POST Content-Type: application/x-www-form-urlencoded</para>
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="parms">各項 Http Post 參數</param>
        /// <returns></returns>
        public string Post(string requestUrl, Hashtable parms)
        {
            return request("POST", requestUrl, parms);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP POST Request，並回傳一個內容字串，
        /// <para>POST Content-Type: application/x-www-form-urlencoded</para>
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="parms">QueryString 格式的參數字串, 例: A=12&B=34</param>
        /// <returns></returns>
        public string Post(string requestUrl, string parms)
        {
            return request("POST", requestUrl, parms);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP POST Request，並回傳一個內容字串，
        /// <para>POST Content-Type: application/json</para>
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public string PostJson(string requestUrl, string jsonData)
        {
            return request("POST", requestUrl, "", true, jsonData);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP POST Request，並回傳一個內容字串，
        /// <para>POST Content-Type: application/json</para>
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public string PostJson(string requestUrl, Hashtable parms)
        {
            string jsonData = JsonConvert.SerializeObject(parms);
            return request("POST", requestUrl, "", true, jsonData);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP Get/Post Request，並回傳一個內容字串，
        /// </summary>
        /// <param name="requestMethod">本次 Request 所要採用的 Method 字串: GET/POST</param>
        /// <param name="requestUrl">本次 Request 的目的 url</param>
        /// <param name="postJson">
        /// 本次 Request 的ContentType，除了 json 之外, 其餘一律為 application/x-www-form-urlencoded
        /// </param>
        /// <param name="formParms">各項 Http Get/Post 參數</param>
        /// <param name="jsonData">當 postJson 時, 直接傳入要 POST 的 JSON 內容字串</param>
        /// <returns></returns>
        private string request(string requestMethod, string requestUrl, Hashtable formParms, bool postJson = false, string jsonData = null)
        {
            // 將各個參數組成 QueryString / Post Body
            string reqParms = "";
            if (formParms != null)
            {
                foreach (string key in (formParms.Keys))
                {
                    if (!"".Equals(reqParms))
                    {
                        reqParms += "&";
                    }
                    reqParms += key + "=" + HttpUtility.UrlEncode((string)formParms[key], Encoding.UTF8);
                }
            }

            return request(requestMethod, requestUrl, reqParms, postJson, jsonData);
        }

        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP Get/Post Request，並回傳一個內容字串，
        /// </summary>
        /// <param name="requestMethod">本次 Request 所要採用的 Method 字串: GET/POST</param>
        /// <param name="requestUrl">本次 Request 的目的 url</param>
        /// <param name="reqParms">各項 Http Get/Post 參數</param>
        /// <param name="postJson">
        /// 本次 Request 的ContentType，除了 json 之外, 其餘一律為 application/x-www-form-urlencoded
        /// </param>
        /// <param name="jsonData">當 postJson 時, 直接傳入要 POST 的 JSON 內容字串</param>
        /// <returns></returns>
        private string request(string requestMethod, string requestUrl, string reqParms, bool postJson = false, string jsonData = null)
        {

            LOG.Info(requestMethod + " " + requestUrl);
            LOG.Info("postJson: " + postJson);
            LOG.Info("reqParms: " + reqParms);
            LOG.Info("jsonData: " + jsonData);

            if (requestUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // .NET Framework 4.0, 4.5 以上的程式支援 TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                LOG.Info("TLS 1.2 Enabled");

                // 忽略 SSL 憑證有效性檢查
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }


            // 依不同的 HTTP method 採用不同傳參數的方式
            if ("POST".Equals(requestMethod, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    httpRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                    httpRequest.Method = requestMethod.ToUpper();
                    httpRequest.UserAgent = MyUserAgent;
                    httpRequest.Timeout = requestTimeout;

                    httpRequest.Credentials = CredentialCache.DefaultCredentials;

                    if (!postJson)
                    {
                        httpRequest.ContentType = "application/x-www-form-urlencoded; charset=" + Encoding.UTF8.ToString();

                        byte[] postBytes = new ASCIIEncoding().GetBytes(reqParms);
                        httpRequest.ContentLength = postBytes.Length;

                        // add post data to request
                        Stream postStream = httpRequest.GetRequestStream();
                        postStream.Write(postBytes, 0, postBytes.Length);
                        postStream.Flush();
                        postStream.Close();
                    }
                    else
                    {
                        httpRequest.ContentType = "application/json; charset=" + Encoding.UTF8.ToString();

                        byte[] postBytes = new ASCIIEncoding().GetBytes(jsonData);
                        httpRequest.ContentLength = postBytes.Length;

                        // add post data to request
                        Stream postStream = httpRequest.GetRequestStream();
                        postStream.Write(postBytes, 0, postBytes.Length);
                        postStream.Flush();
                        postStream.Close();
                    }

                }
                catch (Exception ex)
                {
                    string err = "指定的 requestUrl '" + requestUrl + "' 不合法或不支援";
                    LOG.Error(err + ", " + ex.Message);
                    throw new Exception(err, ex);
                }
            }
            else
            {
                // default method: GET
                string reqGetUrl = requestUrl + "?" + reqParms;
                try
                {
                    httpRequest = (HttpWebRequest)WebRequest.Create(reqGetUrl);
                    httpRequest.UserAgent = MyUserAgent;
                    httpRequest.Timeout = requestTimeout;

                    httpRequest.Credentials = CredentialCache.DefaultCredentials;
                }
                catch (Exception ex)
                {
                    string err = "指定的 requestUrl '" + reqGetUrl + "' 不合法或不支援";
                    LOG.Error(err + ", " + ex.Message);
                    throw new Exception(err, ex);
                }
            }

            // 等待及取得 response 內容
            string resData = null;
            try
            {
                using (WebResponse wr = httpRequest.GetResponse())
                {
                    // 200 OK
                    statusCode = (int)((HttpWebResponse)wr).StatusCode;
                    StatusDescription = ((HttpWebResponse)wr).StatusDescription;

                    using (StreamReader sr = new StreamReader(wr.GetResponseStream(), Encoding.UTF8))
                    {
                        resData = sr.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                // others
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    if (httpResponse != null)
                    {
                        statusCode = (int)httpResponse.StatusCode;
                        StatusDescription = httpResponse.StatusDescription;
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                            resData = streamReader.ReadToEnd();
                    }
                    else
                    {
                        statusCode = 500;
                        StatusDescription = "Internal Error";
                    }
                }
                LOG.Error("httpRequest.GetResponse: " + e.Message, e);
            }

            LOG.Info("response status: " + statusCode + " " + StatusDescription);
            LOG.Info("response data: " + resData);
            return resData;
        }

    }
}
