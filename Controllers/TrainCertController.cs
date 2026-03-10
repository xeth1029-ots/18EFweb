using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using Omu.ValueInjecter;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using Turbo.Commons;
using WDAIIP.WEB.Commons;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WDAIIP.WEB.Controllers
{
    public class TrainCertController : BaseController
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private static string strSID = ConfigModel.SSOSystemID;

        // GET: TrainCert
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();

            TrainCertViewModel model = new TrainCertViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu01;

            if (!string.IsNullOrEmpty(sm.LastResultMessage)) sm.LastResultMessage = "";

            if (sm.IsLogin)
            {
                TrainCertFormModel form = new TrainCertFormModel();

                return Index(form);
            }
            return View("Index", model);
        }
        
        /// <summary>查詢結果</summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(TrainCertFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            ActionResult RtnAction = null;
            TrainCertViewModel model = new TrainCertViewModel();
            model.Form = form;
             
            //todo 檢核登入
            if (sm == null || string.IsNullOrEmpty(sm.RID) || string.IsNullOrEmpty(sm.SID) || string.IsNullOrEmpty(sm.ACID) || string.IsNullOrEmpty(sm.Birthday))
            {
                logger.Info("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);

                RtnAction = RedirectToAction("Login", "Member");

                return RtnAction;
            }

            form.IDNO = sm.ACID;
            form.BIRTHDAY = sm.Birthday;
            model.Form.InjectFrom(form);

            ModelState.Clear();

            RtnAction = View("Index", model);

            // 欄位檢核 OK, 處理查詢
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            if (form.useCache > 0 || ModelState.IsValid)
            {
                //ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 依輸入條件進行查詢
                model.Grid = dao.QueryStudTrainCert(model.Form);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    RtnAction = Json(pagingModel);
                    model.Form.PagingInfo = dao.PaginationInfo;
                }
                else
                {
                    //RtnAction = View("SearchResults", model);
                    RtnAction = View("Index", model);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            //model.Form = form;
            //byte[] byarr = ObjectToByteArray(model);
            //Session["LastModel"] = Convert.ToBase64String(byarr);
            //Session["rid"] = dao.ResultID;
            return RtnAction;
            //throw new NotImplementedException();
        }
    }
}