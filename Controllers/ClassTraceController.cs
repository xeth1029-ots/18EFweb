using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using Turbo.DataLayer;
using WDAIIP.WEB.DataLayers;
using Turbo.Commons;
using log4net;
using WDAIIP.WEB.Commons.Filter;

namespace WDAIIP.WEB.Controllers
{
    public class ClassTraceController : LoginBaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: ClassTrace
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ClassTraceViewModel model = new ClassTraceViewModel();
            ActionResult rtn = null;

            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                LOG.Info("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);

                rtn = RedirectToAction("Login", "Member");
            }
            else
            {
                model.Form.TRC_MSN = sm.MemSN;
                rtn = Index(model.Form);
            }
            return rtn;
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ClassTraceFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            ClassTraceViewModel model = new ClassTraceViewModel();
            model.Form = form;

            ActionResult rtn = View("Index", model);

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                model.Grid = dao.QueryClassTrace(form);

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //// 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            model.Form = form;
            byte[] byarr = MyCommonUtil.ObjectToByteArray(model);
            Session["LastModel"] = Convert.ToBase64String(byarr);
            Session["rid"] = dao.ResultID;
            return rtn;
        }

        /// <summary>
        /// 最後一次查詢結果
        /// </summary>
        /// <returns></returns>
        public ActionResult LastResult()
        {
            if (Session["LastModel"] == null) { return base.SetPageNotFound(); }
            string sessData = (string)Session["LastModel"] ?? null;
            if (sessData == null) { return base.SetPageNotFound(); }

            ClassTraceViewModel model = new ClassTraceViewModel();
            BaseDAO dao = new BaseDAO();
            if (sessData != null)
            {
                var data = Convert.FromBase64String(sessData);
                if (data != null)
                {
                    if (Session["rid"] == null) { return base.SetPageNotFound(); }
                    string rid = Session["rid"].ToString();
                    dao.ResultID = rid;
                    model = (ClassTraceViewModel)MyCommonUtil.ByteArrayToObject(data);
                }
                else
                {
                    model = new ClassTraceViewModel();
                    model.Form = new ClassTraceFormModel();
                    base.SetPagingParams(model.Form, dao, "Index");
                    model.Form.action = Url.Action("Index");
                }
            }

            return View("Index", model);
        }

        /// <summary>
        /// 刪除課程收藏
        /// </summary>
        /// <param name="TRCSN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(Int64? TRCSN)
        {
            if (TRCSN == null)
            {
                throw new ArgumentNullException("TRCSN");
            }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = new RedirectResult(Url.Action("Index"));

            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                LOG.Info("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);

                rtn = RedirectToAction("Login", "Member");
            }
            else
            {
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                TblE_CLSTRACE param = new TblE_CLSTRACE { TRC_SN = TRCSN, };

                dao.UpdateClassTrace("D", param);

                sm.LastResultMessage = "課程收藏移除成功！";
            }
            return rtn;
        }

        /// <summary>
        /// 分享/取消分享課程收藏
        /// </summary>
        /// <param name="Type">Y:分享、N:取消分享</param>
        /// <param name="TRCSN2">課程收藏流水號</param>
        /// <param name="OCID2">課程代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Share(string Type, Int64? TRCSN2, Int64? OCID2)
        {
            if (string.IsNullOrEmpty(Type) || (Type != "Y" && Type != "N"))
            {
                throw new ArgumentNullException("Type");
            }
            if (TRCSN2 == null)
            {
                throw new ArgumentNullException("TRCSN2");
            }
            if (OCID2 == null)
            {
                throw new ArgumentNullException("OCID2");
            }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = new RedirectResult(Url.Action("Index"));

            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                LOG.Info("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);

                rtn = RedirectToAction("Login", "Member");
            }
            else
            {
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                TblE_CLSTRACE param = new TblE_CLSTRACE
                {
                    TRC_SN = TRCSN2,
                    ISSHARE = Type,
                    TRC_OCID = OCID2
                };

                dao.UpdateClassTrace("S", param);

                if (Type == "Y")
                    sm.LastResultMessage = "課程分享成功！";
                else
                    sm.LastResultMessage = "課程已取消分享。";
            }
            return rtn;
        }

        /// <summary> 多筆課程刪除 </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MultiDelClassTrace(ClassTraceViewModel model)
        {
            //ClassTraceViewModel model / ClassTraceFormModel form
            //ClassSearchViewModel.CheckArgument(this.HttpContext);
            ClassTraceViewModel.CheckArgument(this.HttpContext);

            SessionModel sm = SessionModel.Get();
            if (sm.UserID == null)
            {
                return RedirectToAction("Login", "Member");
            }
            ActionResult rtn = LastResult(); //2019-02-11 fix 問題9：修改點選完「課程收藏」後，都會跳到查詢頁一會，等按掉alert又跳回查詢清單頁，然後此時就又會跳出查詢清單頁一進入時的alert一次問題

            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                LOG.Info("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);
                rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            if (model.Grid == null)
            {
                //return RedirectToAction("Login", "Member");
                sm.LastErrorMessage = "刪除選取課程清單失敗。";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace", new { useCache = 1 });
                return rtn;
            }

            //classlist = model.Grid.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
            IList<Int64?> classlist = new List<Int64?>();
            classlist = model.Grid.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
            if (classlist == null || classlist.Count == 0)
            {
                sm.LastResultMessage = "請至少勾選一項";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace", new { useCache = 1 });
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace");
                //rtn = new RedirectResult(Url.Action("Index", "ClassTrace"));
                return rtn;
            }

            try
            {
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
                //批次加入課程收藏
                dao.DelClassTrace(classlist, sm);

                sm.LastResultMessage = "已刪除選取課程清單";
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace", new { });
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace");
                rtn = new RedirectResult(Url.Action("Index", "ClassTrace"));
            }
            catch (Exception ex)
            {
                LOG.Error("ClassTraceController MultiDelClassTrace failed: " + ex.Message, ex);
                //throw new Exception("ClassTraceController MultiDelClassTrace failed:" + ex.Message, ex);
                sm.LastErrorMessage = "刪除選取課程清單失敗。";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace", new { useCache = 1 });
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace");
                //rtn = new RedirectResult(Url.Action("Index", "ClassTrace"));
                //return rtn;
            }
            return rtn;
        }

    }
}