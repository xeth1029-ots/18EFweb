using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using log4net;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 共用 線上說明文件顯示的 Controller
    /// </summary>
    public class OnlineHelpController : WDAIIP.WEB.Controllers.BaseController
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string OnlineHelpBasePath = System.Web.HttpContext.Current.Server.MapPath("~/OnlineHelp/");

        /// <summary>
        /// 依傳入的 actionPath 返回對應的線上說明文件內容 Partial View
        /// </summary>
        /// <param name="helpPath"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string helpPath)
        {
            // 判斷 helpPath 是否包含 Application Path, 若有先移除
            HttpRequest request = System.Web.HttpContext.Current.Request;
            string applicationPath = request.ApplicationPath;

            // remove ApplicationPath
            int p = helpPath.IndexOf(applicationPath);
            if (p > -1)
            {
                helpPath = helpPath.Substring(p + applicationPath.Length);
            }

            string helpDoc = OnlineHelpBasePath + helpPath + ".htm";
            logger.Debug("helpPath: " + helpPath + ", helpDoc=" + helpDoc);

            OnlineHelpModel model = new OnlineHelpModel();

            FileInfo file = new FileInfo(helpDoc);
            if (file.Exists)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(helpDoc))
                    {
                        String line = sr.ReadToEnd();
                        model.HasError = false;
                        model.HelpContent = line;
                    }
                }
                catch (Exception ex)
                {
                    model.HasError = true;
                    model.ErrorMessage = "讀取說明文件檔案發生錯誤: " + ex.Message;
                    logger.Error(model.ErrorMessage, ex);
                }
            }
            else
            {
                model.HasError = true;
                model.ErrorMessage = string.Format("找不到關於 [{0}] 的線上說明文件 !", helpPath);
            }

            return PartialView("_OnlineHelp", model);
        }
	}
}