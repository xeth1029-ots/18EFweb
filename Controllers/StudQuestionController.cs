using log4net;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using Turbo.Commons;
using WDAIIP.WEB.Commons;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WDAIIP.WEB.Controllers
{
    public class StudQuestionController : BaseController
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string strSID = ConfigModel.SSOSystemID;

        const string cstFtDate1 = "2012/02/01";
        const string cstFtDate2 = "2012/09/01";
        const string cstEndDate1 = "2013/01/01";

        // GET: StudQuestion
        /// <summary>
        /// 查詢結果頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            StudQuestionViewModel model = new StudQuestionViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu01;

            if (!string.IsNullOrEmpty(sm.LastResultMessage)) sm.LastResultMessage = "";

            if (sm.IsLogin)
            {
                StudQuestionFormModel form = new StudQuestionFormModel();
                //form.QueType "1":意見調查表 "2":訓後動態調查表
                form.QueType = "1";
                return Index(form);
            }
            return View("Index", model);
        }

        /// <summary>序列化</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) { return null; };
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 查詢結果
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        // [SecurityFilter]
        [HttpPost]
        public ActionResult Index(StudQuestionFormModel form)
        {
            //if (!StudQuestionFormModel.CheckArgument(this.HttpContext))
            //{
            //    return new HttpStatusCodeResult(403);
            //}

            SessionModel sm = SessionModel.Get();
            ActionResult RtnAction = null;
            StudQuestionViewModel model = new StudQuestionViewModel();
            model.Form = form;

            //if ("Y".Equals(ConfigModel.LoginTest))
            //{
            //    測試登入 測訓後動態調查表用
            //    sm.ACID = "X123123123";
            //    sm.Birthday = "yyyy/MM/dd";
            //    sm.SID = null;//1
            //    sm.RID = null;//1
            //    sm.ACID = null;
            //    sm.Birthday = null;
            //}

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
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 依輸入條件進行查詢
                //form.QueType : 1:aFac /2: aFin /3: aTrain /4: aTion
                //aFac-參訓學員意見調查表/aFin-參訓學員訓後動態調查表/aTrain-受訓期間意見調查表/aTion-期末學員滿意度調查表
                model.Grid = dao.QueryStudQuestion(model.Form);

                // 檢核調查表是否可填寫
                //form.QueType : 1:aFac /2: aFin /3: aTrain /4: aTion
                //aFac-參訓學員意見調查表/aFin-參訓學員訓後動態調查表/aTrain-受訓期間意見調查表/aTion-期末學員滿意度調查表
                this.procSearchResult(form.QueType, model.Grid);

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
        }

        /// <summary>參訓學員意見調查表-明細</summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyFac(long? ocid, long? socid)
        {
            SessionModel sm = SessionModel.Get();
            StudQuestionViewModel view = new StudQuestionViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            try
            {
                if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
                if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }
            }
            catch (Exception ex)
            {
                logger.Warn("#StudQuestionController ModifyFac (404) error:" + ex.Message);
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }

            //檢核是否為正確的學員資料
            bool blChkStud = dao.chkStudent(ocid.Value, socid.Value);
            if (!blChkStud)
            {
                sm.LastResultMessage = "請登入學員資料";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion");
                return Redirect(sm.RedirectUrlAfterBlock);
            }

            //查詢取得經費資訊
            FacDetailModel costInfo = dao.getFacCost(ocid.Value, socid.Value);

            //查詢意見調查表填寫資訊
            FacDetailModel detail = dao.getFac(socid.Value);

            detail = dao.getFac(socid.Value);

            bool isNew = (detail == null) ? true : false;

            if (detail == null) { detail = new FacDetailModel(); }

            detail.IsNew = isNew;
            detail.OCID = ocid;
            detail.SOCID = socid;
            detail.YEARS = costInfo.YEARS;
            detail.ORGPLANNAME = costInfo.ORGPLANNAME;

            view.FacDetail = detail;

            return View("FacDetail", view);
        }

        /// <summary> 參訓學員訓後動態調查表-明細 </summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyFin(long? ocid, long? socid)
        {
            SessionModel sm = SessionModel.Get();
            StudQuestionViewModel view = new StudQuestionViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            if (!ocid.HasValue)
            {
                throw new ArgumentNullException("ocid", "參數 ocid 不可為空值");
            }

            if (!socid.HasValue)
            {
                throw new ArgumentNullException("socid", "參數 socid 不可為空值");
            }

            //檢核是否為正確的學員資料
            bool blChkStud = dao.chkStudent(ocid.Value, socid.Value);
            if (!blChkStud)
            {
                sm.LastResultMessage = "請登入學員資料";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion");
                return Redirect(sm.RedirectUrlAfterBlock);
            }

            //查詢取得計畫資訊
            FinDetailModel planInfo = dao.getFinPlan(ocid.Value, socid.Value);

            //查詢訓後動態調查表資訊
            view.FinDetail = dao.getFin(socid.Value);

            if (view.FinDetail == null)
            {
                view.FinDetail = new FinDetailModel();
                view.FinDetail.OCID = ocid;
            }
            else
            {
                view.FinDetail.IsNew = false;
                view.FinDetail.OCID = ocid;

                string q1_4Sub = "";
                if ("Y".Equals(view.FinDetail.Q1A))
                {
                    q1_4Sub = "1";
                }
                else if ("Y".Equals(view.FinDetail.Q1B))
                {
                    q1_4Sub = "2";
                }
                else if ("Y".Equals(view.FinDetail.Q1C))
                {
                    q1_4Sub = "3";
                }
                view.FinDetail.Q1_4Sub = q1_4Sub;
            }

            view.FinDetail.SOCID = socid;
            view.FinDetail.ORGPLANNAME = planInfo.ORGPLANNAME;

            return View("FinDetail", view);
        }

        /// <summary> 受訓期間意見調查表</summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyTrain(long? ocid, long? socid)
        {
            if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
            if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }

            //ModifyFac-參訓學員意見調查表/ModifyFin-參訓學員訓後動態調查表/ModifyTrain-受訓期間意見調查表/ModifyTion-期末學員滿意度調查表
            SessionModel sm = SessionModel.Get();
            StudQuestionViewModel view = new StudQuestionViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            // 檢核是否為正確的學員資料
            bool blChkStud = dao.chkStudent(ocid.Value, socid.Value);
            if (!blChkStud)
            {
                sm.LastResultMessage = "請登入學員資料";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion");
                return Redirect(sm.RedirectUrlAfterBlock);
            }

            //查詢取得資訊
            TrainDetailModel planInfo = dao.getTrainPlan(ocid.Value, socid.Value);

            //查詢 受訓期間意見調查表
            TrainDetailModel detail = dao.getTrain(socid.Value);

            bool isNew = (detail == null) ? true : false;

            if (detail == null) { detail = new TrainDetailModel(); }

            detail.IsNew = isNew;
            detail.OCID = ocid;
            detail.SOCID = socid;
            detail.STUDENTID = planInfo.STUDENTID;
            detail.PLANNAME = planInfo.PLANNAME;
            detail.RID = planInfo.RID;
            detail.TYPE = 3; //1.系統登打 2.電話訪查 3.報名網寫入

            //string s_log1 = "##ModifyTrain";
            //s_log1 += string.Format("\n. detail.RID ={0}", detail.RID);
            //s_log1 += string.Format("\n. planInfo.RID={0}", planInfo.RID);
            //logger.Debug(s_log1);

            view.TrainDetail = detail;

            return View("TrainDetail", view);
        }

        /// <summary> 期末學員滿意度調查表</summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyTion(long? ocid, long? socid)
        {
            if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
            if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }

            //ModifyFac-參訓學員意見調查表/ModifyFin-參訓學員訓後動態調查表/ModifyTrain-受訓期間意見調查表/ModifyTion-期末學員滿意度調查表
            SessionModel sm = SessionModel.Get();
            StudQuestionViewModel view = new StudQuestionViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            // 檢核是否為正確的學員資料
            bool blChkStud = dao.chkStudent(ocid.Value, socid.Value);
            if (!blChkStud)
            {
                sm.LastResultMessage = "請登入學員資料";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion");
                return Redirect(sm.RedirectUrlAfterBlock);
            }

            //查詢取得資訊
            TionDetailModel planInfo = dao.getTionPlan(ocid.Value, socid.Value);

            //查詢 期末學員滿意度調查表
            TionDetailModel detail = dao.getTion(ocid.Value, planInfo.STUDENTID);

            bool isNew = (detail == null) ? true : false;

            if (detail == null) { detail = new TionDetailModel(); }

            detail.IsNew = isNew;
            detail.OCID = ocid;
            detail.STUDENTID = planInfo.STUDENTID;
            detail.STUDID = planInfo.STUDENTID;
            detail.SOCID = planInfo.SOCID;
            detail.PLANNAME = planInfo.PLANNAME;
            detail.QID = planInfo.QID;
            detail.QNAME = planInfo.QNAME;
            detail.QNOTE = planInfo.QNOTE;

            view.TionDetail = detail;

            return View("TionDetail", view);
        }

        /// <summary> 儲存-Fac-參訓學員意見調查表 </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveFac(StudQuestionViewModel view)
        {
            ActionResult result = null;
            SessionModel sm = SessionModel.Get();
            if (!sm.IsLogin)
            {
                string errMsg1 = "登入資訊有誤，請重新登入！";
                Exception ex = new Exception(errMsg1);
                sm.LastErrorMessage = errMsg1;
                //result = View("FacDetail", view);
                throw ex;
            }
            if (sm == null || string.IsNullOrEmpty(sm.RID) || string.IsNullOrEmpty(sm.SID) || string.IsNullOrEmpty(sm.ACID) || string.IsNullOrEmpty(sm.Birthday))
            {
                logger.Info(string.Concat("Login Failed from ", Request.UserHostAddress));

                result = RedirectToAction("Login", "Member");
                //string retMsg = "儲存失敗，登入資訊有誤，請重新登入，再試一次!!";
                //sm.LastErrorMessage = retMsg; //"儲存失敗，請洽系統管理者！";
                return result;
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            FacDetailModel detail = null;
            Int64? ocid = view.FacDetail.OCID;
            decimal? socid = view.FacDetail.SOCID;

            if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
            if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }

            ModelState.Clear();
            this.FacFormValidate(view.FacDetail);

            if (!ModelState.IsValid)
            {
                result = View("FacDetail", view);
                return result;
            }

            try
            {
                //查詢調查表資料是否存在
                detail = dao.getFac(view.FacDetail.SOCID.Value);

                //view.FacDetail.DB_ACTION = (!view.FacDetail.IsNew ? "UPDATE" : "CREATE");
                view.FacDetail.DB_ACTION = (detail == null ? "CREATE" : "UPDATE");

                dao.SaveFacData("StudQuestion", view.FacDetail);

                sm.LastResultMessage = "儲存成功";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion", new { useCache = 2 });

                //異動儲存成功停在編輯頁
                view.FacDetail.IsNew = false;
                //result = RedirectToAction("ModifyFac", new { ocid = ocid, socid = socid });
                result = View("FacDetail", view);
            }
            catch (Exception ex)
            {
                logger.Error("#StudQuestionController SaveFac error:" + ex.Message, ex);
                sm.LastErrorMessage = "儲存失敗，請洽系統管理者！(SQC1)";
                result = View("FacDetail", view);
            }
            return result;
        }

        /// <summary> 儲存-Fin-參訓學員訓後動態調查表 </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveFin(StudQuestionViewModel view)
        {
            ActionResult result = null;
            SessionModel sm = SessionModel.Get();
            if (!sm.IsLogin)
            {
                string errMsg1 = "登入資訊有誤，請重新登入！";
                Exception ex = new Exception(errMsg1);
                sm.LastErrorMessage = errMsg1;
                //result = View("FacDetail", view);
                throw ex;
            }
            if (sm == null || string.IsNullOrEmpty(sm.RID) || string.IsNullOrEmpty(sm.SID) || string.IsNullOrEmpty(sm.ACID) || string.IsNullOrEmpty(sm.Birthday))
            {
                logger.Info(string.Concat("Login Failed from ", Request.UserHostAddress));

                result = RedirectToAction("Login", "Member");
                //string retMsg = "儲存失敗，登入資訊有誤，請重新登入，再試一次!!";
                //sm.LastErrorMessage = retMsg; //"儲存失敗，請洽系統管理者！";
                return result;
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            FinDetailModel detail = null;
            Int64? ocid = view.FinDetail.OCID;
            decimal? socid = view.FinDetail.SOCID;
            if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
            if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }

            ModelState.Clear();
            //表單檢核
            this.FinFormValidate(view.FinDetail);

            if (!ModelState.IsValid)
            {
                result = View("FinDetail", view);
                return result;
            }

            try
            {
                //例外處理
                if (!string.IsNullOrEmpty(view.FinDetail.Q1_4Sub))
                {
                    view.FinDetail.Q1A = ("1".Equals(view.FinDetail.Q1_4Sub) ? "Y" : "");
                    view.FinDetail.Q1B = ("2".Equals(view.FinDetail.Q1_4Sub) ? "Y" : "");
                    view.FinDetail.Q1C = ("3".Equals(view.FinDetail.Q1_4Sub) ? "Y" : "");
                }
                else
                {
                    view.FinDetail.Q1A = "";
                    view.FinDetail.Q1B = "";
                    view.FinDetail.Q1C = "";
                }

                //查詢調查表資料是否存在
                detail = dao.getFin(view.FinDetail.SOCID.Value);

                //view.FinDetail.DB_ACTION = (!view.FinDetail.IsNew ? "UPDATE" : "CREATE");
                view.FinDetail.DB_ACTION = (detail == null ? "CREATE" : "UPDATE");

                dao.SaveFinData("StudQuestion", view.FinDetail);

                sm.LastResultMessage = "儲存成功";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion", new { useCache = 2 });

                //異動儲存成功先停在編輯頁
                view.FinDetail.IsNew = false;
                //result = RedirectToAction("ModifyFin", new { ocid = ocid, socid = socid });
                result = View("FinDetail", view);
            }
            catch (Exception ex)
            {
                logger.Error("StudQuestionController SaveFin error:" + ex.Message, ex);
                sm.LastErrorMessage = "儲存失敗，請洽系統管理者！(SQC2)";
                result = View("FinDetail", view);
            }
            return result;
        }

        /// <summary> 儲存-Train-受訓期間意見調查表 </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveTrain(StudQuestionViewModel view)
        {
            TrainDetailModel v_detail = view.TrainDetail ?? new TrainDetailModel();
            long? ocid = v_detail.OCID;
            long? socid = v_detail.SOCID;
            //string studid = v_detail.STUDENTID;
            if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
            if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }
            //if (string.IsNullOrEmpty(studid)) { throw new ArgumentNullException("studid", "參數 studid 不可為空值"); }

            ActionResult result = null;
            SessionModel sm = SessionModel.Get();
            if (!sm.IsLogin)
            {
                string errMsg1 = "登入資訊有誤，請重新登入！";
                Exception ex = new Exception(errMsg1);
                sm.LastErrorMessage = errMsg1;
                //result = View("FacDetail", view);
                throw ex;
            }
            if (sm == null || string.IsNullOrEmpty(sm.RID) || string.IsNullOrEmpty(sm.SID) || string.IsNullOrEmpty(sm.ACID) || string.IsNullOrEmpty(sm.Birthday))
            {
                logger.Info(string.Concat("Login Failed from ", Request.UserHostAddress));

                result = RedirectToAction("Login", "Member");
                //string retMsg = "儲存失敗，登入資訊有誤，請重新登入，再試一次!!";
                //sm.LastErrorMessage = retMsg; //"儲存失敗，請洽系統管理者！";
                return result;
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ModelState.Clear();

            //表單檢核
            this.TrainFormValidate(v_detail);

            if (!ModelState.IsValid)
            {
                result = View("TrainDetail", view);
                return result;
            }

            try
            {
                //例外處理
                DateTime now = DateTime.Now;

                if (!v_detail.FILLFORMDATE.HasValue) { v_detail.FILLFORMDATE = Convert.ToDateTime(now.ToString("yyyy/MM/dd")); }

                //查詢調查表資料是否存在 STUD_QUESTRAINING
                TrainDetailModel db_detail = dao.getTrain(socid.Value);

                //view.FinDetail.DB_ACTION = (!view.FinDetail.IsNew ? "UPDATE" : "CREATE");
                v_detail.DB_ACTION = (db_detail == null ? "CREATE" : "UPDATE");

                dao.SaveTrainData("StudQuestion", v_detail);

                sm.LastResultMessage = "儲存成功";

                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion", new { useCache = 2 });

                //異動儲存成功先停在編輯頁
                view.TrainDetail.IsNew = false;

                //result = RedirectToAction("ModifyFin", new { ocid = ocid, socid = socid });
                result = View("TrainDetail", view);
            }
            catch (Exception ex)
            {
                logger.Error("StudQuestionController SaveTrain error:" + ex.Message, ex);

                sm.LastErrorMessage = "儲存失敗，請洽系統管理者！(SQC3)";

                result = View("TrainDetail", view);
            }
            return result;
        }

        /// <summary> 儲存-Tion-期末學員滿意度調查表 </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveTion(StudQuestionViewModel view)
        {
            TionDetailModel v_detail = view.TionDetail ?? new TionDetailModel();
            long? ocid = v_detail.OCID;
            long? socid = v_detail.SOCID;
            string studid = v_detail.STUDID;
            if (!ocid.HasValue) { throw new ArgumentNullException("ocid", "參數 ocid 不可為空值"); }
            if (!socid.HasValue) { throw new ArgumentNullException("socid", "參數 socid 不可為空值"); }
            if (string.IsNullOrEmpty(studid)) { throw new ArgumentNullException("studid", "參數 studid 不可為空值"); }

            ActionResult result = null;
            SessionModel sm = SessionModel.Get();
            if (!sm.IsLogin)
            {
                string errMsg1 = "登入資訊有誤，請重新登入！";
                Exception ex = new Exception(errMsg1);
                sm.LastErrorMessage = errMsg1;
                //result = View("FacDetail", view);
                throw ex;
            }
            if (sm == null || string.IsNullOrEmpty(sm.RID) || string.IsNullOrEmpty(sm.SID) || string.IsNullOrEmpty(sm.ACID) || string.IsNullOrEmpty(sm.Birthday))
            {
                logger.Info(string.Concat("Login Failed from ", Request.UserHostAddress));

                result = RedirectToAction("Login", "Member");
                //string retMsg = "儲存失敗，登入資訊有誤，請重新登入，再試一次!!";
                //sm.LastErrorMessage = retMsg; //"儲存失敗，請洽系統管理者！";
                return result;
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ModelState.Clear();

            //表單檢核
            this.TionFormValidate(v_detail);

            if (!ModelState.IsValid)
            {
                result = View("TionDetail", view);
                return result;
            }

            try
            {
                //例外處理
                DateTime now = DateTime.Now;
                if (!v_detail.FILLFORMDATE.HasValue) { v_detail.FILLFORMDATE = Convert.ToDateTime(now.ToString("yyyy/MM/dd")); }

                //查詢調查表資料是否存在 STUD_QUESTIONARY
                TionDetailModel db_detail = dao.getTion(v_detail.OCID.Value, v_detail.STUDID);

                //view.FinDetail.DB_ACTION = (!view.FinDetail.IsNew ? "UPDATE" : "CREATE");
                v_detail.DB_ACTION = (db_detail == null ? "CREATE" : "UPDATE");

                dao.SaveTionData("StudQuestion", v_detail);

                sm.LastResultMessage = "儲存成功";

                sm.RedirectUrlAfterBlock = Url.Action("Index", "StudQuestion", new { useCache = 2 });

                //異動儲存成功先停在編輯頁
                view.TionDetail.IsNew = false;

                //result = RedirectToAction("ModifyFin", new { ocid = ocid, socid = socid });
                result = View("TionDetail", view);
            }
            catch (Exception ex)
            {
                logger.Error("StudQuestionController SaveTion error:" + ex.Message, ex);
                sm.LastErrorMessage = "儲存失敗，請洽系統管理者！(SQC4)";
                result = View("TionDetail", view);
            }
            return result;
        }

        /// <summary> 處理查詢結果清單（依查詢調查表類型）</summary>
        /// <param name="queType"></param>
        /// <param name="rows"></param>
        public void procSearchResult(string queType, IList<StudQuestionGridModel> rows)
        {
            if (rows == null) { return; }

            foreach (StudQuestionGridModel item in rows)
            {
                procSearchResult2_row(queType, item);
            }
        }

        /// <summary> 處理查詢結果清單 ROW 每筆規則設定 </summary>
        /// <param name="queType"></param>
        /// <param name="item"></param>
        void procSearchResult2_row(string queType, StudQuestionGridModel item)
        {
            string socidchk = item.SOCIDCHK; //學號,填寫狀況1:有填,0:未填寫
            string[] socidAry = socidchk.Split(',');

            if (socidchk == null || "0,0".Equals(socidchk))
            {
                //註記找不到socid
                item.FILLSTATUS = "0";
            }
            else if (socidAry != null && socidAry.Length > 1)
            {
                if ("9".Equals(socidAry[1]))
                {
                    return; //強制可填寫 
                }
                else if ("8".Equals(socidAry[1]))
                {
                    item.FILLSTATUS = "1"; //註記已填寫過調查表
                    return; //強制可填寫 
                }
                else if (!"0".Equals(socidAry[1]))
                {
                    //註記已填寫過調查表
                    item.FILLSTATUS = "1";
                }
            }
            //測試狀況下，全部都可以填寫測試 CANWRITEMSG
            if (ConfigModel.TurboTestLocal) { return; }

            //調查表類型 1意見調查表 2.訓後調查表 aFac - 1.參訓學員意見調查表 / aFin - 2.參訓學員訓後動態調查表 / aTrain - 3.受訓期間意見調查表 / aTion - 4.期末學員滿意度調查表
            //aFac - 參訓學員意見調查表 / aFin - 參訓學員訓後動態調查表 / aTrain - 受訓期間意見調查表 / aTion - 期末學員滿意度調查表
            //todo 檢核是否為可填寫時段
            //"1":意見調查表 "2":訓後動態調查表
            switch (queType)
            {
                case "1":
                    //意見調查表 //需為訓後30天內才可填寫 //2018則改為訓後21天內可填寫，從第22天起即不可再填
                    string s_WRITEMSG1 = string.Format("該班結訓日期為 {0} \n填寫日期區間為 {1} ~ {2} (結訓日後21天)", item.FTDATE_AD, item.WRDATE1_AD, item.WRDATE2_AD);
                    //非填寫時間，註解說明
                    if (!"Y".Equals(item.CANWRITE)) { item.CANWRITEMSG = s_WRITEMSG1; }
                    //if ("N".Equals(item.CANWRITE)) { item.CANWRITEMSG = "本班調查表已結束填寫"; }
                    break;

                case "2":
                    //訓後動態調查表 //1.結訓日為cst_wwFTDate1 之後的班 //2.結訓日在cst_wwFTDate2 之前的班 //3.啟動日為cst_wwEndDate1
                    if ((new TimeSpan(Convert.ToDateTime(item.FTDATE_AD).Ticks - Convert.ToDateTime(cstFtDate1).Ticks).Days >= 0)
                        && (new TimeSpan(Convert.ToDateTime(cstFtDate2).Ticks - Convert.ToDateTime(item.FTDATE_AD).Ticks).Days > 0)
                        && (new TimeSpan(Convert.ToDateTime(cstEndDate1).Ticks - Convert.ToDateTime(item.ATODAY_AD).Ticks).Days > 0))
                    {
                        item.QUEVER = "1"; //第一版
                        //20120830 使用舊規則(特殊)
                        if (new TimeSpan(Convert.ToDateTime(item.ATODAY_AD).Ticks - Convert.ToDateTime(item.WRDATE1_AD).Ticks).Days >= 0)
                        {
                            //符合 可填寫日期條件 填寫時間
                            item.CANWRITE = "Y";
                        }
                        else
                        {
                            string s_WRITEMSG2a = string.Format("該班結訓日期為 {0} \n填寫日期為結訓後3個月才可開放填寫\n即 {1} 以後", item.FTDATE_AD, item.WRDATE1_AD);
                            //非填寫時間，註解說明
                            item.CANWRITE = "N";
                            item.CANWRITEMSG = s_WRITEMSG2a;
                        }
                    }
                    else
                    {
                        item.QUEVER = "2"; //第二版
                        string s_WRITEMSG2b = string.Format("該班結訓日期為 {0} \n填寫日期區間為 {1} ~ {2} (結訓日後3個月，填寫1個月)", item.FTDATE_AD, item.WRDATE1_AD, item.WRDATE2_AD);
                        //非填寫時間，註解說明
                        if (!"Y".Equals(item.CANWRITE)) { item.CANWRITEMSG = s_WRITEMSG2b; }
                    }
                    break;

                case "3":
                    //非填寫時間，註解說明
                    string s_WRITEMSG3 = string.Format("該班結訓日期為 {0} \n填寫日期區間為 {1} ~ {2} (結訓日後21天)", item.FTDATE_AD, item.WRDATE1_AD, item.WRDATE2_AD);
                    if (!"Y".Equals(item.CANWRITE)) { item.CANWRITEMSG = s_WRITEMSG3; }
                    break;

                case "4":
                    //非填寫時間，註解說明
                    string s_WRITEMSG4 = string.Format("該班結訓日期為 {0} \n填寫日期區間為 {1} ~ {2} (結訓日後1個月)", item.FTDATE_AD, item.WRDATE1_AD, item.WRDATE2_AD);
                    if (!"Y".Equals(item.CANWRITE)) { item.CANWRITEMSG = s_WRITEMSG4; }
                    break;
            }
        }

        /// <summary>
        /// 表單檢核-Fac-參訓學員意見調查表/意見調查表
        /// </summary>
        /// <param name="model"></param>
        public void FacFormValidate(FacDetailModel model)
        {
            IList<string> msgs = new List<string>();
            string msg = string.Empty;

            ModelState.Remove("FacCheck");

            /*學員基本資料*/
            if (!(model.S11_CHECKED || model.S12_CHECKED || model.S13_CHECKED || model.S14_CHECKED || model.S15_CHECKED || model.S16_CHECKED))
            {
                msgs.Add("學員基本資料：(一)參加產投方案動機（可複選）");
            }

            if (!model.S2.HasValue)
            {
                msgs.Add("學員基本資料：(二)是否為第1次參加產業人才投資方案課程");
            }

            if (!model.S3.HasValue)
            {
                msgs.Add("學員基本資料：(三)服務單位員工人數");
            }

            /*第一部份*/
            if (!(model.A1_1_CHECKED || model.A1_2_CHECKED || model.A1_3_CHECKED
                || model.A1_4_CHECKED || model.A1_5_CHECKED || model.A1_6_CHECKED
                || model.A1_7_CHECKED || model.A1_8_CHECKED || model.A1_9_CHECKED))
            {
                msgs.Add("第一部份：(一)您獲得本次課程的訊息來源（可複選）");
            }

            if (!model.A2.HasValue)
            {
                msgs.Add("第一部份：(二)參加本次課程的主要原因");
            }

            if (!model.A3.HasValue)
            {
                msgs.Add("第一部份：(三)選擇本訓練單位的主要原因");
            }

            if (!model.A4.HasValue)
            {
                msgs.Add("第一部份：(四)沒有參加本方案訓練之前，每年參加訓練支出的費用");
            }

            if (!model.A5.HasValue)
            {
                msgs.Add("第一部份：(五)如果沒有補助訓練費用，你每年願意自費參加訓練課程的金額");
            }

            if (!model.A6.HasValue)
            {
                msgs.Add("第一部份：(六)您認為本次課程的訓練費用是否合理");
            }

            if (!model.A7.HasValue)
            {
                msgs.Add("第一部份：(七)結訓後對於工作的規劃");
            }

            /*第二部份*/
            if (!model.B11.HasValue)
            {
                msgs.Add("第二部份：(一)訓練課程1.課程內容符合期望");
            }

            if (!model.B12.HasValue)
            {
                msgs.Add("第二部份：(一)訓練課程2.課程難易安排適當");
            }

            if (!model.B13.HasValue)
            {
                msgs.Add("第二部份：(一)訓練課程3.課程總時數適當");
            }

            if (!model.B14.HasValue)
            {
                msgs.Add("第二部份：(一)訓練課程4.課程符合實務需求");
            }

            if (!model.B15.HasValue)
            {
                msgs.Add("第二部份：(一)訓練課程5.課程符合產業發展趨勢");
            }

            if (!model.B21.HasValue)
            {
                msgs.Add("第二部份：(二)講師1.滿意講師的教學態度");
            }

            if (!model.B22.HasValue)
            {
                msgs.Add("第二部份：(二)講師2.滿意講師的教學方法");
            }

            if (!model.B23.HasValue)
            {
                msgs.Add("第二部份：(二)講師3.滿意講師的課程專業度");
            }

            if (!model.B31.HasValue)
            {
                msgs.Add("第二部份：(三)教材1.對於訓練教材感到滿意");
            }

            if (!model.B32.HasValue)
            {
                msgs.Add("第二部份：(三)教材2.訓練教材能夠輔助課程學習");
            }

            if (!model.B41.HasValue)
            {
                msgs.Add("第二部份：(四)訓練環境1.您對於訓練場地感到滿意");
            }

            if (!model.B42.HasValue)
            {
                msgs.Add("第二部份：(四)訓練環境2.您對於訓練設備感到滿意");
            }

            if (!model.B43.HasValue)
            {
                msgs.Add("第二部份：(四)訓練環境3.您認為實作設備的數量適當");
            }

            if (!model.B44.HasValue)
            {
                msgs.Add("第二部份：(四)訓練環境4.您認為實作設備新穎");
            }

            if (!model.B51.HasValue)
            {
                msgs.Add("第二部份：(五)訓練評量:訓練評量（如：訓後測驗、專題報告、作品展示等）能促進學習效果");
            }

            if (!model.B61.HasValue)
            {
                msgs.Add("第二部份：(六)立即學習效果1.您認為在訓練課程中，課程內容能讓您專注");
            }

            if (!model.B62.HasValue)
            {
                msgs.Add("第二部份：(六)立即學習效果2.您在完成訓練後，已充份學習訓練課程所教授知識或技能");
            }

            if (!model.B63.HasValue)
            {
                msgs.Add("第二部份：(六)立即學習效果3.您在完成訓練後，有學習到新的知識或技能");
            }

            if (!model.B71.HasValue)
            {
                msgs.Add("第二部份：(七)整體意見1.您對於訓練單位的課程安排與授課情形感到滿意");
            }

            if (!model.B72.HasValue)
            {
                msgs.Add("第二部份：(七)整體意見2.您對於訓練單位的行政服務感到滿意");
            }

            if (!model.B73.HasValue)
            {
                msgs.Add("第二部份：(七)整體意見3.您對於產業人才投資方案感到滿意");
            }

            if (!model.B74.HasValue)
            {
                msgs.Add("第二部份：(七)整體意見4.您認為完成本訓練課程對於目前或未來工作有幫助");
            }

            /*第三部份*/
            if (!model.C11.HasValue)
            {
                msgs.Add("第三部份：(一)若本訓練課程沒有補助，是否會全額自費參訓");
            }

            if (!string.IsNullOrEmpty(model.C21_NOTE) && model.C21_NOTE.Length > model.C21_NOTE_MaxLength)
            {
                string msgsC21 = string.Format("第三部份：(二)其他建議 長度超過系統範圍(限制:{0}字)/使用者輸入:{1}字", model.C21_NOTE_MaxLength, model.C21_NOTE.Length);
                msgs.Add(msgsC21);
            }

            foreach (string itm in msgs)
            {
                msg += itm.ToString() + "\n";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                msg = (string.IsNullOrEmpty(msg) ? "" : "請確認下列答案\n" + msg);
                ModelState.AddModelError("FacCheck", msg);
            }
        }

        /// <summary>
        /// 表單檢核-Fin-參訓學員訓後動態調查表/訓後動態調查表
        /// </summary>
        /// <param name="model"></param>
        public void FinFormValidate(FinDetailModel model)
        {
            IList<string> msgs = new List<string>();
            string msg = string.Empty;

            ModelState.Remove("FinCheck");

            if (!model.Q1.HasValue)
            {
                msgs.Add("請選擇(一)請問您目前的就業狀況為何");
            }
            else if (model.Q1 == 4)
            {
                if (string.IsNullOrEmpty(model.Q1_4Sub))
                {
                    msgs.Add("選擇(一)目前的就業狀況為 4.創業 時，請繼續選擇（a.b.或c.）");
                }
            }

            if (!model.Q2.HasValue)
            {
                msgs.Add("請選擇(二)請問您的薪資於結訓後有提升嗎");
            }

            if (!model.Q3.HasValue)
            {
                msgs.Add("請選擇(三)請問您擔任的職位有變化嗎");
            }

            if (!model.Q4.HasValue)
            {
                msgs.Add("請選擇(四)請問您對目前工作的滿意度是否有變化");
            }

            if (!model.Q5.HasValue)
            {
                msgs.Add("請選擇(五)請問您目前工作內容是否與本次參訓課程有相關");
            }

            if (string.IsNullOrEmpty(model.Q8))
            {
                msgs.Add("請選擇(六)請問您是否有繼續參與本方案的意願");
            }

            if (!(model.Q7MR1_CHECKED || model.Q7MR2_CHECKED || model.Q7MR3_CHECKED || model.Q7MR4_CHECKED))
            {
                msgs.Add("請選擇(七)結訓後是否有與下列人員保持聯絡?(可複選)");
            }
            else if (model.Q7MR4_CHECKED && (model.Q7MR1_CHECKED || model.Q7MR2_CHECKED || model.Q7MR3_CHECKED))
            {
                msgs.Add("(七)結訓後是否有與下列人員保持聯絡，已選擇無，但又選擇其它答案，邏輯異常");
            }

            if (string.IsNullOrEmpty(model.Q211))
            {
                msgs.Add("請選擇 二、訓練成效 (一)1.答案");
            }

            if (string.IsNullOrEmpty(model.Q212))
            {
                msgs.Add("請選擇 二、訓練成效 (一)2.答案");
            }

            if (string.IsNullOrEmpty(model.Q213))
            {
                msgs.Add("請選擇 二、訓練成效 (一)3.答案");
            }

            if (string.IsNullOrEmpty(model.Q214))
            {
                msgs.Add("請選擇 二、訓練成效 (一)4.答案");
            }

            if (string.IsNullOrEmpty(model.Q215))
            {
                msgs.Add("請選擇 二、訓練成效 (一)5.答案");
            }

            if (string.IsNullOrEmpty(model.Q216))
            {
                msgs.Add("請選擇 二、訓練成效 (一)6.答案");
            }

            if (string.IsNullOrEmpty(model.Q217))
            {
                msgs.Add("請選擇 二、訓練成效 (一)7.答案");
            }

            if (string.IsNullOrEmpty(model.Q218))
            {
                msgs.Add("請選擇 二、訓練成效 (一)8.答案");
            }

            if (string.IsNullOrEmpty(model.Q221))
            {
                msgs.Add("請選擇 二、訓練成效 (二)1.答案");
            }

            if (string.IsNullOrEmpty(model.Q222))
            {
                msgs.Add("請選擇 二、訓練成效 (二)2.答案");
            }

            if (string.IsNullOrEmpty(model.Q223))
            {
                msgs.Add("請選擇 二、訓練成效 (二)3.答案");
            }

            if (string.IsNullOrEmpty(model.Q224))
            {
                msgs.Add("請選擇 二、訓練成效 (二)4.答案");
            }

            if (string.IsNullOrEmpty(model.Q225))
            {
                msgs.Add("請選擇 二、訓練成效 (二)5.答案");
            }

            if (string.IsNullOrEmpty(model.Q226))
            {
                msgs.Add("請選擇 二、訓練成效 (二)6.答案");
            }

            foreach (string itm in msgs)
            {
                msg += itm.ToString() + "\n";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                ModelState.AddModelError("FinCheck", msg);
            }
        }

        /// <summary>
        /// 表單檢核-Train-受訓期間意見調查表
        /// </summary>
        /// <param name="model"></param>
        public void TrainFormValidate(TrainDetailModel model)
        {
            ModelState.Remove("TrainCheck");

            IList<string> msgs = new List<string>();
            string msg = string.Empty;

            if (!model.Q1_1.HasValue) { msgs.Add("請選擇 一、課程安排(1.)"); }
            if (!model.Q1_2.HasValue) { msgs.Add("請選擇 一、課程安排(2.)"); }
            if (!model.Q1_3.HasValue) { msgs.Add("請選擇 一、課程安排(3.)"); }

            if (!model.Q2_1.HasValue) { msgs.Add("請選擇 二、師資、助教及教學(1.)"); }
            if (!model.Q2_2.HasValue) { msgs.Add("請選擇 二、師資、助教及教學(2.)"); }
            if (!model.Q2_3.HasValue) { msgs.Add("請選擇 二、師資、助教及教學(3.)"); }
            //if (!model.Q2_4.HasValue) { msgs.Add("請選擇 第二部份：師資與教學(4.)"); }

            if (!model.Q3_1.HasValue) { msgs.Add("請選擇 三、設備和教材(1.)"); }
            if (!model.Q3_2.HasValue) { msgs.Add("請選擇 三、設備和教材(2.)"); }
            if (!model.Q3_3.HasValue) { msgs.Add("請選擇 三、設備和教材(3.)"); }

            if (!model.Q4_1.HasValue) { msgs.Add("請選擇 四、行政措施(1.)"); }
            if (!model.Q4_2.HasValue) { msgs.Add("請選擇 四、行政措施(2.)"); }
            if (!model.Q4_3.HasValue) { msgs.Add("請選擇 四、行政措施(3.)"); }
            if (!model.Q4_4.HasValue) { msgs.Add("請選擇 四、行政措施(4.)"); }

            foreach (string itm in msgs)
            {
                msg += itm.ToString() + "\n";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                ModelState.AddModelError("TrainCheck", msg);
            }
        }

        /// <summary>
        /// 表單檢核-Tion-期末學員滿意度調查表
        /// </summary>
        /// <param name="model"></param>
        public void TionFormValidate(TionDetailModel model)
        {
            ModelState.Remove("TionCheck");

            IList<string> msgs = new List<string>();
            string msg = string.Empty;

            if (!model.Q1_1.HasValue) { msgs.Add("請選擇 第一部份：課程與教材(1.)"); }
            if (!model.Q1_2.HasValue) { msgs.Add("請選擇 第一部份：課程與教材(2.)"); }
            if (!model.Q1_3.HasValue) { msgs.Add("請選擇 第一部份：課程與教材(3.)"); }

            if (!model.Q2_1.HasValue) { msgs.Add("請選擇 第二部份：師資與教學(1.)"); }
            if (!model.Q2_2.HasValue) { msgs.Add("請選擇 第二部份：師資與教學(2.)"); }
            if (!model.Q2_3.HasValue) { msgs.Add("請選擇 第二部份：師資與教學(3.)"); }
            if (!model.Q2_4.HasValue) { msgs.Add("請選擇 第二部份：師資與教學(4.)"); }
            if (!model.Q2_5.HasValue) { msgs.Add("請選擇 第二部份：師資與教學(5.)"); }

            if (!model.Q3_1.HasValue) { msgs.Add("請選擇 第三部份：學習環境與行政支援(1.)"); }
            if (!model.Q3_2.HasValue) { msgs.Add("請選擇 第三部份：學習環境與行政支援(2.)"); }
            if (!model.Q3_3.HasValue) { msgs.Add("請選擇 第三部份：學習環境與行政支援(3.)"); }

            if (!model.Q4_1.HasValue) { msgs.Add("請選擇 第四部份：學習效果(1.)"); }
            if (!model.Q4_2.HasValue) { msgs.Add("請選擇 第四部份：學習效果(2.)"); }
            if (!model.Q4_3.HasValue) { msgs.Add("請選擇 第四部份：學習效果(3.)"); }
            if (!model.Q4_4.HasValue) { msgs.Add("請選擇 第四部份：學習效果(4.)"); }
            if (!model.Q4_5.HasValue) { msgs.Add("請選擇 第四部份：學習效果(5.)"); }
            if (!model.Q4_6.HasValue) { msgs.Add("請選擇 第四部份：學習效果(6.)"); }

            if (!model.Q5_1.HasValue) { msgs.Add("請選擇 第五部份：職訓與工作(1.)"); }
            if (!model.Q5_2.HasValue) { msgs.Add("請選擇 第五部份：職訓與工作(2.)"); }
            if (!model.Q5_3.HasValue) { msgs.Add("請選擇 第五部份：職訓與工作(3.)"); }
            if (!model.Q5_4.HasValue) { msgs.Add("請選擇 第五部份：職訓與工作(4.)"); }

            foreach (string itm in msgs)
            {
                msg += itm.ToString() + "\n";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                ModelState.AddModelError("TionCheck", msg);
            }
        }

    }
}