using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Turbo.Crypto;

namespace WDAIIP.WEB.Commons
{
    /// <summary> 用來處理全域 Unhandled Exception 的 logging 及 routing 的類 </summary>
    public class ExceptionHandler
    {
        protected ILog LOG = LogManager.GetLogger(typeof(ExceptionHandler));

        private Controller ErrorPageController = null;
        private string ControllerName = "ErrorPage";
        private string DefaultAction = "Index";
        //const string cst_from_emailaddress = "from_emailaddress";//'web.config
        public string cst_EmailtoMe = "amuting@gmail.com";//錯誤信給我

        public ExceptionHandler(Controller controller)
        {
            ErrorPageController = controller;
            ControllerName = controller.GetType().Name;
        }

        public static bool ValidateServerCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //using System.Net.Security;
            //using System.Security.Cryptography.X509Certificates;
            return true;
        }

        /// <summary>
        /// 取得錯誤相關資訊-lastError
        /// </summary>
        /// <param name="lastError"></param>
        /// <returns></returns>
        public string GetErrorMsg(Exception lastError)
        {
            string sMailBody = string.Empty;
            string str_LocalAddr = MyCommonUtil.GetLocalAddr("2");
            sMailBody += string.Format("時間：{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")); //存入時間
            sMailBody += string.Format("LocalAddr：{0}\n", str_LocalAddr);
            if (lastError != null) { sMailBody += string.Format("LastError：{0}\n", lastError.ToString()); }
            //bool isHttpException = false;
            if (lastError == null) { lastError = new System.ArgumentNullException("沒有Exception資訊"); }
            return sMailBody;
        }

        /// <summary>
        /// 取得錯誤相關資訊-context/lastError
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lastError"></param>
        /// <returns></returns>
        public string GetErrorMsg(HttpContext context, Exception lastError)
        {
            if (context == null) { return string.Empty; }
            if (lastError == null) { return string.Empty; }
            if (lastError != null)
            {
                //System.Web.HttpResponse.set_StatusCode(Int32 value) 傳送 HTTP 標頭後，伺服器無法設定狀態
                string str_error1a = "傳送 HTTP 標頭後，伺服器無法設定狀態";
                string str_error1b = "System.Web.HttpResponse.set_StatusCode(Int32 value)";
                if (lastError.ToString().Contains(str_error1a) && lastError.ToString().Contains(str_error1b)) { return string.Empty; }

                //LastError：System.Web.HttpException(0x80004005): 具有潛在危險 Request.Path 的值已從用戶端 (:) 偵測到。
                //於 System.Web.HttpRequest.ValidateInputIfRequiredByConfig()
                //於 System.Web.HttpApplication.PipelineStepManager.ValidateHelper(HttpContext context)
                string[] ar_error2b = { "具有潛在危險", "Request.Path", "System.Web.HttpRequest.ValidateInputIfRequiredByConfig()", "System.Web.HttpApplication.PipelineStepManager.ValidateHelper(HttpContext context)" };
                bool flag_error2b = true;
                foreach (string str_V in ar_error2b) { if (!lastError.ToString().Contains(str_V)) { flag_error2b = false; break; } }
                if (flag_error2b) { return string.Empty; }
            }

            //HttpContext context
            //Exception lastError = context.Server.GetLastError();
            string sMailBody = string.Empty;
            string str_UserHostIp = MyCommonUtil.GetIpAddress(context);
            //ToString("yyyy-MM-dd HH:mm:ss")
            //sMailBody += string.Format("時間：{0}\n", DateTime.Now.ToString()); //存入時間
            sMailBody += string.Format("時間：{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")); //存入時間
            sMailBody += string.Format("MachineName：{0}\n", context.Server.MachineName);
            sMailBody += string.Format("UserAgent：{0}\n", context.Request.UserAgent);
            sMailBody += string.Format("IpAddress：{0}\n", str_UserHostIp);
            //sMailBody += string.Format("UserHostAddress：{0}\n", context.Request.UserHostAddress);
            sMailBody += string.Format("UserHostName：{0}\n", context.Request.UserHostName);//UserHostAddress
            sMailBody += string.Format("Url：{0}\n", context.Request.Url);
            sMailBody += string.Format("RawUrl：{0}\n", context.Request.RawUrl);
            sMailBody += string.Format("LastError：{0}\n", lastError.ToString());
            //sMailBody += string.Format("StackTrace：{0}\n", lastError.StackTrace.ToString());
            //bool isHttpException = false;
            //if (lastError == null) { lastError = new System.ArgumentNullException("沒有Exception資訊"); }

            int i_statusCode = 500;
            //isHttpException = true;
            if (lastError.GetType() == typeof(HttpException)) { i_statusCode = ((HttpException)lastError).GetHttpCode(); }
            if (i_statusCode == 404) { return string.Empty; }
            /*
            string[] s_aRawUrlRtnEmpty = { "robots.txt", "apple-touch-icon", "bootstrap.css.map", "browserconfig.xml", "ads.txt", ".php" };
            foreach (string s_v1 in s_aRawUrlRtnEmpty) { if (context.Request.RawUrl.Contains(s_v1)) { return string.Empty; } }
             */

            HttpContextWrapper contextWrapper = new HttpContextWrapper(context);
            RouteData thisRouteData = RouteTable.Routes.GetRouteData(contextWrapper);
            if (thisRouteData == null) { sMailBody = string.Empty; return sMailBody; }

            string thisArea = (string)thisRouteData.DataTokens["area"];
            string thisController = (string)thisRouteData.Values["controller"];
            string thisAction = (string)thisRouteData.Values["action"];
            thisArea = (thisArea != null) ? "~/" + thisArea : "~";
            string exMessage = lastError.GetType().FullName + ":\n" + lastError.Message;
            string s_LOGError = string.Format("LOG_Error:\n {0}/ {1}/ {2}: {3}: {4}:\nexMessage: \n{5}\n", thisArea, thisController, thisAction, str_UserHostIp, i_statusCode, exMessage);
            sMailBody += s_LOGError;
            //sMailBody += "--ServerVariables:\n" + MyCommonUtil.GetHTTP_HOST(context);
            return sMailBody;
        }

        /// <summary> 寄送錯誤信件 </summary>
        /// <param name="sMailBody"></param>
        /// <param name="strToEmail"></param>
        public void SendMailTest(string sMailBody, string strToEmail)
        {
            if (string.IsNullOrEmpty(strToEmail) || string.IsNullOrEmpty(sMailBody)) { return; }

            int iMaxCanMailCount = MyCommonUtil.cst_iMaxCanMailCount;//'DEFALUT: (每天)最大寄信量
            string ugMaxCanMailCount = MyCommonUtil.Utl_GetConfigSet(MyCommonUtil.cst_MaxCanMailCount);//'MaxCanMailCount:7
            if (!string.IsNullOrEmpty(ugMaxCanMailCount))
            {
                bool fg_ok1 = int.TryParse(ugMaxCanMailCount, out iMaxCanMailCount);
                if (!fg_ok1) { return; }
            }
            int iSendMailCount = MyCommonUtil.SendMailCount();//目前寄信總數量
                                                              //'strErrmsg &= TIMS.GetErrorMsg(Me) '取得錯誤資訊寫入
                                                              //'置換換行符號 'strErrmsg = Replace(strErrmsg, vbCrLf, "<br>" & vbCrLf)
                                                              //'已達今日最大寄信數量，停止寄EMAIL '0:無限寄信／有限寄信 Return;//Exit Sub
            if (iMaxCanMailCount != 0 && iSendMailCount > iMaxCanMailCount) { return; }

            string from_emailaddress = ConfigurationManager.AppSettings["from_emailaddress"];
            const string cst_Subject = "OJT-狀況提醒";
            const string strFromName = "系統自動發信";
            string strFromEmail = "ojt@msa.wda.gov.tw";
            if (!string.IsNullOrEmpty(from_emailaddress)) { strFromEmail = from_emailaddress; }
            const string strToName = "系統自動收信";
            //string strToEmail = "";
            //string strSubject = "";

            string strHtmlBody = "";
            strHtmlBody += String.Format("寄件日期：{0}\n", DateTime.Now.ToString("yyyy-MM-dd"));
            strHtmlBody += String.Format("時間：{0}\n", DateTime.Now);
            strHtmlBody += String.Format("寄件數量：{0}, {1}\n", iSendMailCount, iMaxCanMailCount);
            strHtmlBody += sMailBody;

            strHtmlBody = strHtmlBody.Replace("\n", "<br>");

            //AesTk aesTk = new AesTk();
            //string strHtmlBodyEnc = aesTk.Encrypt(strHtmlBody);
            //WebRequest物件如何忽略憑證問題
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
            //TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072
            string strResult = "";
            string s_LOGError = "";
            bool flag_mail_error = false;
            //https://wltims.wda.gov.tw/GetJobMail3/Service1.asmx
            //https://ojfile119.ejob.gov.tw/GetJobMail3/Service1.asmx
            SendMailws3.Service1 m = new SendMailws3.Service1();
            try
            {
                strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strHtmlBody, "");
            }
            catch (Exception ex)
            {
                flag_mail_error = true;
                s_LOGError = string.Format("SendMailTest({0}):\n sMailBody:\n{1}\n ex.Message:\n{2}\n ex.StackTrace:\n{3}\n", strToEmail, sMailBody, ex.Message, ex.StackTrace);
                LOG.Error(s_LOGError);
                //throw;
            }
            if (flag_mail_error)
            {
                AesTk aesTk = new AesTk();
                string s_LOGErrorEnc = string.IsNullOrEmpty(s_LOGError) ? "" : aesTk.Encrypt(s_LOGError);
                string strHtmlBodyEnc = string.IsNullOrEmpty(strHtmlBody) ? "" : aesTk.Encrypt(strHtmlBody);
                if (!string.IsNullOrEmpty(s_LOGErrorEnc))
                {
                    string strEncBody = string.Format("s_LOGError: \n{0}\n<br> ", s_LOGErrorEnc);
                    strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strEncBody, "");
                }
                if (!string.IsNullOrEmpty(strHtmlBodyEnc))
                {
                    string strEncBody2 = string.Format("strHtmlBody:\n{0}\n<br> ", strHtmlBodyEnc);
                    strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strEncBody2, "");
                }
            }
        }

        public void RouteErrorPage(HttpContext context, Exception lastError)
        {
            bool isHttpException = false;
            if (lastError == null)
            {
                lastError = new System.ArgumentNullException("沒有Exception資訊");
            }

            int i_statusCode = 500;
            if (lastError.GetType() == typeof(HttpException))
            {
                isHttpException = true;
                i_statusCode = ((HttpException)lastError).GetHttpCode();
            }
            //else { }
            // Not an HTTP related error so this is a problem in our code, set status to
            // 500 (internal server error)
            //i_statusCode = 500;
            //statusCode = 404;

            try
            {
                // keep lastError to Session, that ErrorPageController can read it
                //2018-12-25 fix資安 Struts REST Plugin Remote Code Execution
                //add if
                if (context.Session != null) { context.Session["LastException"] = lastError; }
            }
            catch (Exception e)
            {
                //exMessage = String.Concat(lastError.GetType().FullName , ": ", lastError.Message);
                LOG.Error(String.Concat("Session 存取異常: ", e.Message), e);
            }

            string str_UserHostIp = MyCommonUtil.GetIpAddress(context);
            // logging current RouteData and lastError
            HttpContextWrapper contextWrapper = new HttpContextWrapper(context);
            RouteData thisRouteData = RouteTable.Routes.GetRouteData(contextWrapper);
            string s_LOGError = string.Empty;
            if (thisRouteData == null)
            {
                s_LOGError = string.Format("thisRouteData == null; {0}: {1}: {2}\n", str_UserHostIp, context.Request.UserHostAddress, i_statusCode);
                LOG.Error(s_LOGError);
                LOG.Error(">>", lastError);
                return;
            }

            string thisArea = (string)thisRouteData.DataTokens["area"];
            string thisController = (string)thisRouteData.Values["controller"];
            string thisAction = (string)thisRouteData.Values["action"];
            thisArea = (thisArea != null) ? "~/" + thisArea : "~";

            //Exception / ] ERROR
            string exMessage = lastError.GetType().FullName + ": " + lastError.Message;
            s_LOGError = string.Format(": {0}/ {1}/ {2}: {3}: {4}: {5}: {6}\n", thisArea, thisController, thisAction, str_UserHostIp, context.Request.UserHostAddress, i_statusCode, exMessage);
            LOG.Error(s_LOGError);
            LOG.Error(">>", lastError);

            if (!isHttpException)
            {
                // pass exception to ErrorsPageController
                RouteData newRouteData = new RouteData();
                newRouteData.Values.Add("controller", ControllerName);
                newRouteData.Values.Add("action", DefaultAction);
                newRouteData.Values.Add("statusCode", i_statusCode);
                newRouteData.Values.Add("exMessage", exMessage);
                newRouteData.Values.Add("isAjaxRequet", contextWrapper.Request.IsAjaxRequest());

                IController controller = this.ErrorPageController;
                RequestContext requestContext = new RequestContext(contextWrapper, newRouteData);
                controller.Execute(requestContext);
            }
            else
            {
                //2018-12-25 fix資安 Struts REST Plugin Remote Code Execution
                //（eric協助debug）當.net偵到有異常的錯誤時改一律回傳404
                if (context.Response.IsClientConnected && context.Response.StatusCode != 404)
                {
                    //連接-不為404-改為404
                    context.Response.StatusCode = 404;
                }
            }

            //是否仍然連接到伺服器-未連接-清除資訊
            if (!context.Response.IsClientConnected) { context.Response.Clear(); return; }
            //連接-輸出強制中斷
            context.Response.End();
        }


    }
}