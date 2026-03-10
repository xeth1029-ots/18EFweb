using Turbo.DataLayer;
using WDAIIP.WEB.DataLayers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Turbo.Commons;
using WDAIIP.WEB.Commons.Filter;
using System.Text.RegularExpressions;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WDAIIP.WEB.Controllers
{
    /// <summary> 有權限控管的共用 Controller 基底類, 不需要 登入/授權 才能使用的功能, 一律繼承這個基底類 </summary>
    public class BaseController : Controller
    {
        //using log4net;
        protected static readonly ILog bLOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string CACHE_FORM_ACTION = "Index";

        private static bool StressTestModeEnabled = ConfigModel.StressTestMode;

        /// <summary>
        /// 設定分頁元件所用到的參數:
        /// pagingModel: 查詢用的 FormModel, 
        /// dao: 執行查詢的DAO, 
        /// action: 分頁連結的 action 名稱
        /// </summary>
        /// <param name="pagingModel"></param>
        /// <param name="dao"></param>
        /// <param name="action"></param>
        protected void SetPagingParams(PagingResultsViewModel pagingModel, BaseDAO dao, string action)
        {
            SetPagingParams(pagingModel, dao, action, "");
        }

        /// <summary>
        /// 設定分頁元件所用到的參數(指定客制化的分頁資料顯示 callback function):
        /// pagingModel: 查詢用的 FormModel, 
        /// dao: 執行查詢的DAO, 
        /// action: 分頁連結的 action 名稱, 
        /// callback: 分頁資料AJAX載入後會呼叫的 js callback function
        /// </summary>
        /// <param name="pagingModel">查詢用的 FormModel</param>
        /// <param name="dao">執行查詢的DAO</param>
        /// <param name="action">分頁連結的 action</param>
        /// <param name="callback">分頁資料AJAX載入後會呼叫的 js callback function</param>
        protected void SetPagingParams(PagingResultsViewModel pagingModel, BaseDAO dao, string action, string callback)
        {
            pagingModel.PagingInfo = dao.PaginationInfo;
            pagingModel.rid = dao.ResultID;
            pagingModel.action = Url.Action(action);
            pagingModel.ajaxLoadPageCallback = callback;
        }

        /// <summary> 每個 action 被執行前會觸發這個 event </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string action = ControllerContextHelper.GetAction(filterContext);
            string controller = ControllerContext.RouteData.Values["controller"].ToString();

            string sv_http_referer = Request.ServerVariables["http_referer"];

            // 將任何的 Model Binding Exception 加到一般的 ModelError 中
            // 以便讓這些 Exception 有機會顯示出來
            var modelState = filterContext.Controller.ViewData.ModelState;
            if (!modelState.IsValid)
            {
                var modelStateErrors = modelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors);

                List<string> exceptions = new List<string>();
                foreach (var item in modelStateErrors)
                {
                    if (item.Exception != null)
                    {
                        bLOG.Error("; action: " + action + "; ModelError: " + item.Exception.Message);
                        exceptions.Add(item.Exception.Message);
                    }
                }
                if (exceptions.Count > 0)
                {
                    modelState.AddModelError("Exception", string.Join("<br/>\n", exceptions.ToArray()));
                }
            }

            var req = filterContext.RequestContext.HttpContext.Request;
            System.Collections.Specialized.NameValueCollection parms = null;
            try
            {
                parms = req.Params;
            }
            catch (Exception ex)
            {
                bLOG.Error(string.Concat("##parms = req.Params: ", ex.Message), ex);
                Utl_StatusCode404();
                return; //throw;
            }

            //var parms = req.Params;
            if (req.HttpMethod == "POST" && !req.IsAjaxRequest())
            {
                // 判斷是否為會員登入
                if (controller.ToUpper() == "MEMBER" && (action.ToUpper() == "MINFORECEIVER" || action.ToUpper() == "LOGINSUCCESS"))
                {
                    if (string.IsNullOrEmpty(parms["SID"]) && string.IsNullOrEmpty(parms["Sid"]))
                    {
                        //Response.AppendHeader("Cache-Control", "private");
                        //Response.Cache.AppendCacheExtension("no-cache, no-store, must-revalidate");
                        //Response.StatusCode = 404;
                        //Response.End();
                        bLOG.Warn("##IsNullOrEmpty(SID) Response.StatusCode=404");
                        Utl_StatusCode404();
                        return; //throw;
                    }
                }
                else if (parms["__SECURITY_FORM_ID__"] == null || !MyCommonUtil.CompareSecurityCode(parms["__SECURITY_FORM_ID__"]))
                {
                    if (!StressTestModeEnabled)
                    {
                        // 20181212, Eric, 在壓力測試模式下, 略過此項檢核
                        // 加強資安前端於表單 submit 時動態加入欄位,
                        // 避免機器人程式送出 post request.
                        bLOG.Warn("##!StressTestModeEnabled Response.StatusCode=404");
                        Utl_StatusCode404();
                        return; //throw;
                        //throw new ArgumentException("__SECURITY_FORM_ID__");
                    }
                }
            }
            //secure
            Regex patternsR = new Regex("(--|1=1|timeout|sleep |shutdown)");
            foreach (var key in parms.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    string value = parms[key];
                    if (string.IsNullOrEmpty(value)) { continue; } //SKIP
                    if (req.HttpMethod == "POST" && req.IsAjaxRequest() && controller.ToUpper() == "ENTERTYPE")
                    {
                        string patternSkip = "multipart/form-data; boundary=--";
                        Match matchSkip = Regex.Match(value, patternSkip);
                        if (matchSkip.Success) { continue; } //SKIP
                    }
                    if (patternsR.IsMatch(value) || patternsR.IsMatch(key))
                    {
                        bLOG.Warn(string.Concat("##Regex Response.StatusCode=404 K:", key, ".(", patternsR.IsMatch(key), "),V:", value, ".(", patternsR.IsMatch(value), ")."));
                        Utl_StatusCode404();
                        return; //throw; // throw new ArgumentException("BaseController.OnActionExecuting");
                    }
                }
                else
                {
                    bLOG.Warn("##IsNullOrEmpty(key) Response.StatusCode=404");
                    Utl_StatusCode404();
                    return; //throw;
                }
            }

            // FormModelCacheFilter OnActionExecuting 
            if (CACHE_FORM_ACTION.Equals(action))
            {
                (new FormModelCacheFilter()).OnActionExecuting(filterContext);
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        /// <summary> 每個 action 被執行後會觸發這個 event </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string action = ControllerContextHelper.GetAction(filterContext);

            if (CACHE_FORM_ACTION.Equals(action))
            {
                (new FormModelCacheFilter()).OnActionExecuted(filterContext);
            }
            else
            {
                base.OnActionExecuted(filterContext);
            }
        }

        /// <summary> response httpstatus 404 + 空白頁 取代 return new HttpStatusCodeResult(404) </summary>
        /// <returns></returns>
        protected ActionResult SetPageNotFound()
        {
            //Response.Flush();
            //Response.Clear();
            bLOG.Warn("##SetPageNotFound Response.StatusCode=404");
            Utl_StatusCode404();
            return Content("");
        }

        /// <summary> StatusCode = 404 </summary>
        public void Utl_StatusCode404()
        {
            bLOG.Error("##Utl_StatusCode404");
            if (!Response.IsClientConnected)
            {
                //是否仍然連接到伺服器-未連接-清除資訊
                Response.Clear();
                return;
            }
            Response.Clear();
            Response.BufferOutput = true;
            if (Response.StatusCode != 404) { Response.StatusCode = 404; }
            Response.End();
            return;

            //Response.AppendHeader("Cache-Control", "private");
            //Response.Cache.SetCacheability(HttpCacheability.Private);
            //Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            //Response.AppendHeader("Pragma", "no-cache");
            //Response.AppendHeader("Expires", "0");
            //Response.StatusCode = 404;
            //Response.Status = "404 Not Found";
            //Response.End();
        }

        #region LogError
        /// <summary>在系統日誌內記錄一筆「正常訊息」</summary>
        /// <param name="message">錯誤訊息</param>
        public void LogInfo(string message)
        {
            if (string.IsNullOrEmpty(message)) { return; }

            if (bLOG != null) bLOG.Info(message);
            else throw new Exception("未建立系統日誌物件。");

        }

        /// <summary>在系統日誌內記錄一筆「錯誤」</summary>
        /// <param name="message">錯誤訊息</param>
        public void LogError(string message)
        {
            if (string.IsNullOrEmpty(message)) { return; }

            if (bLOG != null) bLOG.Error(message);
            else throw new Exception("未建立系統日誌物件。");
        }

        /// <summary>在系統日誌內記錄一筆「錯誤」</summary>
        /// <param name="ex">異常例外</param>
        public void LogError(Exception ex)
        {
            if (ex == null) { return; }

            if (bLOG != null) bLOG.Error(ex.Message, ex);
            else throw new Exception("未建立系統日誌物件。");
        }

        /// <summary>在系統日誌內記錄一筆「錯誤」</summary>
        /// <param name="exMsg"></param>
        /// <param name="ex"></param>
        public void LogError(string exMsg, Exception ex)
        {
            if (ex == null) { return; }

            if (bLOG != null) bLOG.Error(exMsg, ex);
            else throw new Exception("未建立系統日誌物件。");
        }
        #endregion        

        // GET: /Base/HandleUnknownAction (BaseController)
        /// <summary> 實作 HandleUnknownAction 功能，以避免 404 的發生 </summary>
        /// <param name="actionName"></param>
        protected override void HandleUnknownAction(string actionName)
        {
            if (bLOG != null) bLOG.Error(string.Concat("##HandleUnknownAction(string actionName):", actionName));
            SetPageNotFound();
            //base.HandleUnknownAction(actionName);
        }

        // GET: /Base/IncompleteAction (BaseController)
        /// <summary> This action is to illustrate exception handling </summary>
        /// <returns></returns>
        public ActionResult IncompleteAction()
        {
            throw new NotImplementedException("This Action is not yet complete");
        }
    }

    //using System;//using System.Security.Cryptography;//using System.Web;

    public static class NonceGenerator
    {
        public static string GenerateNonce()
        {
            var nonceBytes = new byte[32]; // 32 位元組的 nonce
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }
            return Convert.ToBase64String(nonceBytes);
        }
        public static string GenerateRandomStr(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }
            return sb.ToString();
        }
    }

    public class NonceModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                HttpContext.Current.Items["ScriptNonce"] = NonceGenerator.GenerateNonce();
            };
        }
        public void Dispose() { }
    }

}
