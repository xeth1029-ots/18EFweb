using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Turbo.Commons;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Controllers;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;

namespace WDAIIP.WEB
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected static readonly ILog LOG = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start(object source, EventArgs e)
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch
                      (new System.IO.FileInfo(Server.MapPath("~/log4net.config")));

            LOG.Info("#Application_Start");

            //隱藏網站所使用的 ASP.NET MVC 版本資訊
            MvcHandler.DisableMvcResponseHeader = true;

            // 加入自定義的 View/PartialView Location 設定
            ExtendedRazorViewEngine engine = ExtendedRazorViewEngine.Instance();
            // 報表模組-客制化報表結果顯示用的 PartialViews
            engine.AddPartialViewLocationFormat("~/Views/Report/Custom/{0}.cshtml");
            engine.Register();

            // 設置OpenCvSharpExtern.dll路徑到環境變量( for OpenCv(OCR影像辨識的影像前處理套件) )
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string x64Path = Path.Combine(basePath, "bin", "dll", "x64");
            //string x86Path = Path.Combine(basePath, "bin", "dll", "x86");
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $";{x64Path}");
            //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $";{x86Path}");
            //var x64Pth = Path.Combine(basePath, "bin", "x64");
            //var x86Pth = Path.Combine(basePath, "bin", "x86");
            //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $";{x64Pth}");
            //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $";{x86Pth}");
            //string dllPath = AppDomain.CurrentDomain.BaseDirectory;
            //string currentPath = Environment.GetEnvironmentVariable("PATH");
            //Environment.SetEnvironmentVariable("PATH", currentPath + ";" + dllPath);

            //防止 ClickJacking https://stackoverflow.com/questions/20254303/mvc-5-prevents-access-to-content-via-iframe
            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

            AreaRegistration.RegisterAllAreas();

            GlobalFilters.Filters.Add(new StopServiceFilterAttribute());

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // 主機 IP
            string host = Server.MachineName;

            // 啟動 產投報名監控統計資訊 背景更新 Thread
            (new WDAIIP.WEB.Services.ClassSignUpMonitor()).StartRefresher(host);

            // 檢核資料庫連線狀態
            bool flag_error = Chk_dao_connection();
            if (flag_error) { return; }

            // 重設 產投報名 Thread Pool 排隊控制檔
            ILog logger = LogManager.GetLogger("ProcessEnterWorker");
            ClassSignUpCtl signUpCtl = new ClassSignUpCtl(host);
            // 重設產投排隊計數器
            TblSYS_SIGNUP_CTL ctl = signUpCtl.GetSignUpCtl();
            signUpCtl.ResetSignUpCtl();
            logger.Info("#Application_Start: 重設產投排隊計數器, 目前計數器狀態: " + Newtonsoft.Json.JsonConvert.SerializeObject(ctl));
            //try{} catch (Exception ex){LOG.Error("#Application_Start: " + ex.Message, ex);}
        }

        protected void Application_Error(object source, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Exception lastError = Server.GetLastError();
            // handled the exception and route to error page   
            ExceptionHandler handler = new ExceptionHandler(new ErrorPageController());
            string sMailBody = handler.GetErrorMsg(this.Context, lastError);
            if (!string.IsNullOrEmpty(sMailBody))
            {
                LOG.Error("#sMailBody: " + sMailBody);
                try
                {
                    handler.SendMailTest(sMailBody, handler.cst_EmailtoMe);
                }
                catch (Exception ex)
                {
                    LOG.Error("#ErrorPageController: " + sMailBody, ex);
                }
                LOG.Error(string.Concat("#Application_Error: ", sMailBody), lastError);
            }
            Server.ClearError();

            handler.RouteErrorPage(this.Context, lastError);

            //Response.StatusCode = 404;
            //Response.End();
        }

        /// <summary> request header info </summary>
        /// <param name="s_vk1"></param>
        /// <returns></returns>
        string Get_HeaderValue_info(string s_vk1)
        {
            //string s_vk1 = "X-Scan-Memo"; //string s_memo = string.Empty;
            string s_rst = string.Empty;
            if (s_vk1.Equals("-1"))
            {
                foreach (string s_k1 in Request.Headers.AllKeys)
                {
                    string[] s_amlist1 = Request.Headers.GetValues(s_k1);
                    if (s_amlist1 != null)
                    {
                        string s_rst2 = string.Empty;
                        foreach (string s_v1 in s_amlist1)
                        {
                            s_rst2 += string.Concat((!string.IsNullOrEmpty(s_rst2)) ? ";" : "", s_v1);
                        }
                        s_rst += string.Concat(((!string.IsNullOrEmpty(s_rst)) ? "\n" : ""), "[", s_k1, "]=", s_rst2);

                    }
                }
            }
            else
            {
                StringComparison comp1 = StringComparison.OrdinalIgnoreCase;
                foreach (string s_k1 in Request.Headers.AllKeys)
                {
                    if (s_k1.IndexOf(s_vk1, comp1) > -1)
                    {
                        string[] s_amlist1 = Request.Headers.GetValues(s_k1);
                        if (s_amlist1 != null)
                        {
                            foreach (string s_v1 in s_amlist1)
                            {
                                s_rst += string.Concat((!string.IsNullOrEmpty(s_rst)) ? ";" : "", s_v1);
                            }
                        }
                    }
                }
            }
            //if (Request.Headers.AllKeys.Any(k => k.Equals(s_vk1))) { }
            return s_rst;
        }

        /// <summary>Insecure Deployment: HTTP Request Smuggling(11621.11622)</summary>
        /// <returns></returns>
        bool Chk_HK1()
        {
            string cst_k1 = "X-Scan-Memo";
            string s_rst = Get_HeaderValue_info(cst_k1); //bool flag_hk1 = false;
            if (!string.IsNullOrEmpty(s_rst) && s_rst.Length > 0)
            {
                StringComparison comp1 = StringComparison.OrdinalIgnoreCase;
                if (s_rst.IndexOf(@"Engine=""Http+Request+Smuggling""", comp1) > -1)
                {
                    LOG.Error(string.Format("##Application_BeginRequest [1][chk_HK1]:{0}:{1}", cst_k1, s_rst));
                    return true;// flag_hk1; flag_hk1 = true;
                }
                else if (s_rst.IndexOf(@"Engine=""Server+Side+Prototype+Pollution""", comp1) > -1)
                {
                    //Engine=""Server+Side+Prototype+Pollution""
                    LOG.Error(string.Format("##Application_BeginRequest [2][chk_HK1]:{0}:{1}", cst_k1, s_rst));
                    return true;// flag_hk1; flag_hk1 = true;
                }
                else if (s_rst.IndexOf(@"Category=""Crawl""", comp1) > -1 && s_rst.IndexOf(@"SessionType=""Crawl""", comp1) > -1 && s_rst.IndexOf(@"CrawlType=""AJAXInclude""", comp1) > -1)
                {
                    //Engine=""Server+Side+Prototype+Pollution""
                    LOG.Error(string.Format("##Application_BeginRequest [3][chk_HK1]:{0}:{1}", cst_k1, s_rst));
                    return true;// flag_hk1; flag_hk1 = true;
                }
                else
                {
                    LOG.Debug(string.Format("##Application_BeginRequest [m][chk_HK1]:{0}:{1}", cst_k1, s_rst));
                }
                //else { LOG.Info(string.Format("##Application_BeginRequest [chk_HK1]:{0}:{1}", cst_k1, s_rst)); }
            }
            return false;//flag_hk1;
        }

        bool Chk_HK2()
        {
            //bool flag_hk2 = false;
            string s_Hostall = ConfigurationManager.AppSettings["WebFmHost1"];
            if (s_Hostall == null || string.IsNullOrEmpty(s_Hostall) || s_Hostall.Length <= 1) { return false; }

            string cst_k1 = "Host";
            string s_rst = Get_HeaderValue_info(cst_k1);
            StringComparison comp1 = StringComparison.OrdinalIgnoreCase;
            if (!string.IsNullOrEmpty(s_rst) && s_rst.Length > 0)
            {
                //Host,查無資料是攻擊
                if (s_Hostall.IndexOf(s_rst, comp1) == -1)
                {
                    LOG.Error(string.Format("##Application_BeginRequest [chk_HK2]:{0}:{1}", cst_k1, s_rst));
                    return true;//flag_hk2 = true;
                }
            }
            return false;//flag_hk2;
        }

        /// <summary> 檢核資料庫連線狀態 </summary>
        bool Chk_dao_connection()
        {
            MyKeyMapDAO kdao = new MyKeyMapDAO();
            bool flag_error = false;
            try
            {
                DateTime sNow = kdao.GetSysDateNow();
            }
            catch (Exception ex)
            {
                flag_error = true;
                LOG.Error(ex.Message, ex);
            }
            //if (flag_error)
            //{
            //    string StopUrl = ConfigModel.StopUrl;
            //    var resp = Response;
            //    Response.Redirect(StopUrl);
            //    Response.End();
            //}
            return flag_error;
        }

        /// <summary>每次頁面的請求都會觸發</summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(object source, EventArgs e)
        {
            //A6 Security Misconfiguration
            //A6:2017(OWASP Top 10 Application Security Risks, A6: 2017)
            //this.Response.Headers["X-Content-Type-Options"] = "nosniff";

            //string s_ck = Get_HeaderValue_info("-1");
            //LOG.Debug(string.Format("##Application_BeginRequest -1={0} ", s_ck));
            //string s_ck = Get_HeaderValue_info("COOKIE");
            //LOG.Debug(string.Format("##Application_BeginRequest COOKIE={0} ", s_ck));
            //bool flag_hk1 = false;
            //嚴重風險HTTP Request Smuggling的解決方法。
            //Insecure Deployment: HTTP Request Smuggling(11621.11622)
            //資安特殊狀況控制;
            if (Chk_HK1() || Chk_HK2())
            {
                //Response.AppendHeader("Cache-Control", "private");
                //Response.Cache.AppendCacheExtension("no-cache, no-store, must-revalidate");
                //是否仍然連接到伺服器-未連接-清除資訊
                if (Response.IsClientConnected && Response.StatusCode != 404) { Response.StatusCode = 404; }
                if (!Response.IsClientConnected) { Response.Clear(); return; }
                Response.End();
                return;
            }
        }

        //protected void Application_PreSendRequestHeaders(object source, EventArgs e)
        //{
        //    應用程式每次傳送響應時刪除Server標頭
        //    Response.Headers.Remove("Server");
        //}

    }
}