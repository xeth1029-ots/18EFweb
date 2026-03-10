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
    public abstract class DownloadController : BaseController
    {
        public abstract string GetKindID();

        public abstract string GetPlanID();

        /// <summary> 查詢 </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            string str_IX = Request.QueryString["IX"]; //目前只能是2：計畫表單下載 
            if (!string.IsNullOrEmpty($"{str_IX}") && !str_IX.Equals("2")) { return base.SetPageNotFound(); }
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }
            string s_classLoader = Request.QueryString["class.module.classLoader.class.name.bytes[99999]"];
            if (!string.IsNullOrEmpty(s_classLoader)) { return base.SetPageNotFound(); }
            ViewBag.MENUNM2 = str_IX == "2" ? " 計畫表單下載" : "計畫表單";

            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            DownloadViewModel model = new DownloadViewModel();
            DownloadFormModel form = model.Form;

            string t_KindID = GetKindID(); form.KINDID = t_KindID;
            string t_PlanID = GetPlanID(); form.PLANID = t_PlanID;

            //= check form.PLANID =
            int i_PlanID = -1;
            bool flag_int_OK = string.IsNullOrEmpty(t_PlanID) ? false : int.TryParse(t_PlanID, out i_PlanID);
            form.PLANID = flag_int_OK ? t_PlanID : null; //if (flag_int_OK) { form.PLANID = t_PlanID; } else { form.PLANID = null; }

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu06;

            //針對「計畫表單」頁面，進行額外的處理作業
            //if (!flag_int_OK && !string.IsNullOrEmpty(t_KindID) && t_KindID == "1") { return base.SetPageNotFound(); }
            //if (string.IsNullOrEmpty(t_KindID)) { return base.SetPageNotFound(); }
            if (!string.IsNullOrEmpty(t_KindID))
            {
                if (t_KindID.Trim().Equals("1"))
                {
                    //查詢常見問題類別清單(啟用中)
                    model.DownloadType_list = dao.QueryDownloadPlan();
                    model.DLPlanName = string.Empty;

                    if (model.DownloadType_list != null && model.DownloadType_list.Count > 0)
                    {
                        var item = model.DownloadType_list[0];
                        model.Form.PLANID = item.CODE;
                        model.DLPlanName = item.TEXT;
                    }

                    //dao = new WDAIIPWEBDAO();
                }
            }

            //設定查詢分頁資訊
            dao.SetPageInfo(model.Form.rid, model.Form.p);

            //依輸入條件進行查詢
            model.Grid = dao.QueryDownload(form);

            //設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model.Form, dao, "Index");
            //base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");

            return View(model);
        }

        /// <summary> 查詢結果清單 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(DownloadFormModel form)
        {
            if (form == null) { return base.SetPageNotFound(); }
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }
            string s_classLoader = Request.QueryString["class.module.classLoader.class.name.bytes[99999]"];
            if (!string.IsNullOrEmpty(s_classLoader)) { return base.SetPageNotFound(); }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            DownloadViewModel model = new DownloadViewModel();
            model.Form = form;
            ActionResult rtn = View(model);

            //針對「計畫表單」頁面，進行額外的處理作業
            string t_KindID = form.KINDID;

            //= check form.PLANID =
            string t_PlanID = string.IsNullOrEmpty(form.PLANID) ? "" : form.PLANID.Trim() ?? "";
            int i_PlanID = -1;
            bool flag_int_OK = string.IsNullOrEmpty(t_PlanID) ? false : int.TryParse(t_PlanID, out i_PlanID);
            //if (!flag_int_OK) { return base.SetPageNotFound(); }
            form.PLANID = flag_int_OK ? t_PlanID : null;

            //if (!flag_int_OK && !string.IsNullOrEmpty(t_KindID) && t_KindID == "1") { return base.SetPageNotFound(); }
            //if (string.IsNullOrEmpty(t_KindID)) { return base.SetPageNotFound(); }
            if (!string.IsNullOrEmpty(t_KindID))
            {
                if (t_KindID.Trim().Equals("1"))
                {
                    model.DownloadType_list = dao.QueryDownloadPlan();

                    if (flag_int_OK && !"99".Equals(t_PlanID))
                    {
                        model.DLPlanName = model.DownloadType_list[i_PlanID - 1].TEXT;
                    }
                    else
                    {
                        model.DLPlanName = "全部資料";
                    }
                }
            }

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                //dao = new WDAIIPWEBDAO();

                //設定查詢分頁資訊
                dao.SetPageInfo(model.Form.rid, model.Form.p);

                //= check form.PLANID =
                if (!string.IsNullOrEmpty(form.PLANID))
                {
                    if (form.PLANID.Trim().Equals("99")) { form.PLANID = "0"; }
                    //if (!flag_int_OK) { i_PlanID = -1; form.PLANID = i_PlanID.ToString(); }
                }

                //依輸入條件進行查詢
                model.Grid = dao.QueryDownload(form);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_DownloadGridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_DownloadGridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                //設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(model.Form, dao, "Index");
                //base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");
            }

            return rtn;
        }

        /// <summary>檔案下載</summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DownloadFile(string dlid, string fileSeq)
        {

            if (dlid == null || fileSeq == null) { return base.SetPageNotFound(); }
            Decimal? idlid = MyCommonUtil.get_Decimal_null(dlid);
            Int64? ifileSeq = MyCommonUtil.get_Int64_null(fileSeq);
            if (!idlid.HasValue || !ifileSeq.HasValue) { return base.SetPageNotFound(); }

            TblTB_DLFILE where = new TblTB_DLFILE { DLID = idlid };

            //取得指定條件的DownloadDetailModel
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            DownloadDetailModel detail = new DownloadDetailModel();
            detail = dao.GetDownload(where);

            if (detail != null)
            {
                //更新檔案點擊次數
                int hits = (detail.DLCOUNT.HasValue ? detail.DLCOUNT.Value : 0);
                hits += 1;
                detail.DLCOUNT = hits;
                dao.UpdateDownloadHits(detail);

                string myFileTitle = detail.DLTITLE;
                string myFileName = "";
                string myFileExt = "";

                Int64 mySeq = ifileSeq.Value;
                if (mySeq == 1)
                {
                    myFileName = detail.FILE1_NAME;
                    myFileExt = detail.FILE1_TYPE;
                }
                else if (mySeq == 2)
                {
                    myFileName = detail.FILE2_NAME;
                    myFileExt = detail.FILE2_TYPE;
                }

                //將指定的檔案，下載到用戶端
                string fileSavePath1 = Url.Content(ConfigModel.UploadTempPath + "/DLFILE/" + myFileName);  //取得檔案存放(虛擬)路徑
                string fileSavePath2 = Server.MapPath(fileSavePath1);                                      //("虛擬路徑"轉"實體路徑")

                if (System.IO.File.Exists(fileSavePath2))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileSavePath2);
                    string fileName = myFileTitle + myFileExt;                                             //下載的預設檔案名稱
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    //檔案已不存在的處理方法
                    TempData["message"] = "檔案已不存在！";
                    if (detail.KINDID.Equals("1"))
                        return RedirectToAction("Index", "PlanDownload");
                    else
                        return RedirectToAction("Index", "OtherDownload");
                }
            }
            else
            {
                #region (目前不使用)

                //(1)
                //byte[] fileBytes = null;
                //string fileName = "";
                //return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                //(2)
                //return new EmptyResult();

                #endregion

                return RedirectToAction("Index", "Home");
            }
        }
    }
}