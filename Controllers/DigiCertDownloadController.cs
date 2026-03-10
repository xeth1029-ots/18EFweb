using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    public class DigiCertDownloadController : LoginBaseController
    {
        //BaseController
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: DigiCertDownload
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            //增加檢核E_Menber MEM_SN(序號)
            if (sm == null || !sm.IsLogin || sm.MemSN == null)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            var dpform = new DigiCertDLPageFormModel
            {
                TRC_MSN = sm.MemSN,//E_Menber MEM_SN(序號)
                CNAME = sm.UserName,
                IDNO = sm.ACID
            };

            var model = new DigiCertViewModel { DPForm = dpform };

            //return View("Index", model);
            rtn = Index(model.DPForm);

            return rtn;
        }

        [HttpPost]
        public ActionResult Index(DigiCertDLPageFormModel dpform)
        {
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            //增加檢核E_Menber MEM_SN(序號)
            if (sm == null || !sm.IsLogin || sm.MemSN == null)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            //return RedirectToAction("Detail", "DigiCertOnline");

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            var wdata = new TblSTUD_DIGICERTAPPLY { IDNO = sm.ACID };
            var oDCaplist = dao.GetRowList(wdata);
            if (oDCaplist == null || oDCaplist.Count == 0)
            {
                LOG.Warn("oDCaplist == null || oDCaplist.Count==0!" + Request.UserHostAddress + ": 會員(mem_id=" + sm.UserID);
                return RedirectToAction("Detail", "DigiCertOnline");
            }

            if (dpform == null) { dpform = new DigiCertDLPageFormModel(); }

            dpform.TRC_MSN = sm.MemSN;//E_Menber MEM_SN(序號)
            dpform.CNAME = sm.UserName;
            dpform.IDNO = sm.ACID;

            var model = new DigiCertViewModel { DPForm = dpform };

            rtn = View("Index", model);

            if (dpform.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                // 設定查詢分頁資訊
                dao.SetPageInfo(dpform.rid, dpform.p);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                model.Grid2 = dao.QueryDigiCertApply(dpform);

                if (!string.IsNullOrEmpty(dpform.rid) && dpform.useCache == 0)
                {
                    // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(dpform, dao, "Index");
            }

            model.DPForm = dpform;
            byte[] byarr = MyCommonUtil.ObjectToByteArray(model);
            Session["LastModel"] = Convert.ToBase64String(byarr);
            Session["rid"] = dao.ResultID;
            return rtn;
        }




    }
}