using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using log4net;
using Newtonsoft.Json;
using WDAIIP.WEB.Services;
using System.Collections;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 產投報名 ThreadPool 運作狀態資訊 監控
    /// </summary>
    public class SignUpMonitorController : Controller
    {
        //protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: SignUpMonitor
        public ActionResult Index()
        {
            // 返回報名處理狀態統計頁面

            // 主機IP
            ViewBag.Host = Server.MachineName;

            // 啟動 統計資訊 背景更新 Thread
            (new ClassSignUpMonitor()).StartRefresher(ViewBag.Host);

            // 最大報名背景處理個數 (Thread Pool 容量)
            ViewBag.ProcessEnterWorkerMaxThreads = ConfigModel.ProcessEnterWorkerMaxThreads;

            // 報名處理排隊等待最長時間(秒)
            ViewBag.ProcessEnterWorkerWaitTimeout = ConfigModel.ProcessEnterWorkerWaitTimeout;

            return View();
        }

        /// <summary>
        /// 設計供AJAX呼叫用, 以 JSON 格式回傳產投課程報名處理狀態統計, <seealso cref="ClassSignUpStatistic"/>
        /// </summary>
        /// <returns></returns>
        public ActionResult Statistics()
        {
            // 以 JSON 格式回傳 60分鐘統計循環計數器(每1分鐘為1級距)

            ClassSignUpStatistic[] statistics = ClassSignUpStatistic.GetStatistics();
            ClassSignUpStatistic[] localStatistics = new ClassSignUpStatistic[statistics.Length];
            int Current = DateTime.Now.Minute;
            for (int i = 0; i < statistics.Length; i++)
            {
                if (statistics[i] != null)
                {
                    localStatistics[i] = statistics[i].Clone();
                    if (localStatistics[i].IsCurrent.HasValue && localStatistics[i].IsCurrent.Value) Current = i;
                }
                else
                {
                    localStatistics[i] = new ClassSignUpStatistic();
                }
            }

            Hashtable result = new Hashtable { ["current"] = Current, ["data"] = localStatistics };
            string str = JsonConvert.SerializeObject(result);
            return Content(str, "application/json");
        }

        /// <summary>
        /// 清空/重設 產投課程報名處理狀態統計, 以重新統計
        /// </summary>
        /// <returns></returns>
        public ActionResult Reset()
        {
            string host = Server.MachineName;
            ClassSignUpStatistic.ResetStatistics(host);
            return Content("done");
        }

    }
}