using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using Turbo.Commons;
using Turbo.Crypto;
using log4net;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>最新消息分類共用</summary>
    public abstract class ContentController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 取得類別項目代碼
        /// </summary>
        /// <returns></returns>
        public abstract string GetFunID();

        /// <summary>
        /// 取得子類別項目代碼
        /// </summary>
        /// <returns></returns>
        public abstract string GetSubFunID();

        // GET: Content
        /// <summary>
        /// 查詢頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            SessionModel sm = SessionModel.Get();
            ContentViewModel model = new ContentViewModel();

            ContentFormModel form = model.Form;
            ActionResult rtn = View("Index", model);

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu01;

            //設定最新消息明細頁導回依據
            sm.NewsIndexController = @Url.Action("Index");

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 依輸入條件進行查詢
                string funID = this.GetFunID();
                string subFunID = this.GetSubFunID();
                TblTB_CONTENT where = new TblTB_CONTENT { FUNID = funID, SUB_FUNID = subFunID };
                model.Grid = dao.QueryContent(where);

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            return rtn;
        }

        /// <summary>
        /// 查詢結果清單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ContentFormModel form)
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            SessionModel sm = SessionModel.Get();
            ContentViewModel model = new ContentViewModel();
            model.Form = form;

            ActionResult rtn = View("Index", model);

            sm.NewsIndexController = @Url.Action("Index");

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 依輸入條件進行查詢
                string funID = this.GetFunID();
                string subFunID = this.GetSubFunID();
                TblTB_CONTENT where = new TblTB_CONTENT { FUNID = funID, SUB_FUNID = subFunID };
                model.Grid = dao.QueryContent(where);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_NewsGridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_NewsGridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            return rtn;
        }

        /// <summary>
        /// 查詢明細資料-GET
        /// </summary>
        /// <param name="seqno"></param>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Detail(Int64? seqno)
        {
            if (!seqno.HasValue)
            {
                LOG.Error("ContentController DownloadFile seqno is null");
                return new HttpStatusCodeResult(404);
                //return new HttpStatusCodeResult(403);
                //throw new ArgumentNullException("seqno ");
            }

            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            SessionModel sm = SessionModel.Get();
            ContentViewModel model = new ContentViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            // 查詢明細資料
            string funID = this.GetFunID();
            string subFunID = this.GetSubFunID();
            TblTB_CONTENT where = new TblTB_CONTENT { FUNID = funID, SUB_FUNID = subFunID, SEQNO = seqno.Value };
            model.Detail = dao.GetContentDetail(where);

            if (model.Detail == null)
            {
                sm.LastResultMessage = "查無資料";

                if (!string.IsNullOrEmpty(sm.NewsIndexController)
                    && (Url.Action("Index", "Home")).Equals(sm.NewsIndexController))
                {
                    sm.RedirectByHttpMethod = "GET";
                }

                sm.RedirectUrlAfterBlock = sm.NewsIndexController;
            }

            return View("Detail", model);
        }

        /// <summary>查詢明細資料-GET</summary>
        /// <returns></returns>
        public ActionResult Detailv2()
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }
            string str_SEQ = Request.QueryString["SEQ"]; //空白-str_SEQ-異常
            if (string.IsNullOrEmpty(str_SEQ)) { return base.SetPageNotFound(); }
            string str_WXR = Request.QueryString["WXR"]; //空白-str_WXR-異常
            if (string.IsNullOrEmpty(str_WXR)) { return base.SetPageNotFound(); }
            AesTk aesTk = new AesTk();
            int i_WXR = -1;
            bool fg_WXR_ok = false;
            try
            {
                str_WXR = aesTk.Decrypt(str_WXR);
                //str_WXR = int.Parse(aesTk.Decrypt(str_WXR)).ToString();
                //str_WXR = aesTk.Encrypt(str_WXR);
                fg_WXR_ok = int.TryParse(str_WXR, out i_WXR);
            }
            catch (Exception)
            {
                //有異常訊息直接清空
                str_WXR = "";
            }
            //解析後 str_WXR 為空白-異常
            if (string.IsNullOrEmpty(str_WXR)) { return base.SetPageNotFound(); }
            //解析後 str_WXR 為非數字-異常
            if (!fg_WXR_ok) { return base.SetPageNotFound(); }
            //解析後 str_WXR 不等於 str_SEQ-異常
            if (!aesTk.CreateHash(str_WXR).Equals(str_SEQ)) { return base.SetPageNotFound(); }
            int i_seqno = Convert.ToInt32(str_WXR.Replace(".0", "")); /*非數字的異常*/
            //if (!int.TryParse(str_WXR, out i_seqno)) { return base.SetPageNotFound(); }

            SessionModel sm = SessionModel.Get();
            ContentViewModel model = new ContentViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            // 查詢明細資料
            string funID = this.GetFunID();
            string subFunID = this.GetSubFunID();
            TblTB_CONTENT where = new TblTB_CONTENT { FUNID = funID, SUB_FUNID = subFunID, SEQNO = i_seqno };
            model.Detail = dao.GetContentDetail(where);

            if (model.Detail == null)
            {
                sm.LastResultMessage = "查無資料";
                //if (!string.IsNullOrEmpty(sm.NewsIndexController)
                //    && (Url.Action("Index", "Home")).Equals(sm.NewsIndexController))
                //{
                //    sm.RedirectByHttpMethod = "GET";
                //}
                //sm.RedirectUrlAfterBlock = sm.NewsIndexController;
                return base.SetPageNotFound();
            }

            return View("Detail", model);
        }

        //[HttpPost]
        /// <summary> 下載附件 </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public ActionResult DownloadFile(string fileid, string fileid2)
        {
            string filePath = "";
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            if ("POST".Equals(Request.HttpMethod))
            {
                #region "POST-1"
                //o you should be able to call HttpContext.Request.ContentType. //var x_ContentType = Request.ContentType;

                decimal i_fileid = -1;
                bool fg_fileid_ok = (fileid != null) ? decimal.TryParse(fileid, out i_fileid) : false;
                if (string.IsNullOrEmpty(fileid) || string.IsNullOrEmpty(fileid2))
                {
                    LOG.Warn("##POST.ContentController DownloadFile fileid is null");
                    return base.SetPageNotFound();
                }
                else if (!fg_fileid_ok)
                {
                    LOG.Warn(string.Concat("##POST.ContentController DownloadFile i_fileid = -1", fileid));
                    return base.SetPageNotFound();
                    //return Content(""); //null; new HttpStatusCodeResult(404);// return new HttpStatusCodeResult(404); // return new HttpStatusCodeResult(403);
                }

                try
                {
                    string it_fileid2 = MyCommonUtil.DecodeString(fileid2);
                    string it_fileIDymd2 = string.Concat(fileid, DateTime.Now.ToString("yyyyMMddHH"));
                    if (it_fileIDymd2 != it_fileid2) { return HttpNotFound(); }

                }
                catch (Exception ex)
                {
                    LOG.Warn(string.Concat(ex.Message, ", #POST.ContentController-", fileid, "-", fileid2, "-"), ex);
                    return HttpNotFound();
                }
                TblTB_FILE where = new TblTB_FILE { FILEID = i_fileid };
                TblTB_FILE data = dao.GetFile(where);

                if (data != null)
                {
                    // 更新下載次數
                    data.DLCOUNT = (data.DLCOUNT.HasValue ? data.DLCOUNT : 0) + 1;
                    dao.UpdateFileDLCount(data);

                    // 將指定的檔案，下載到用戶端
                    //string filePath = Server.MapPath(Url.Content(ConfigModel.UploadTempPath + "/NEWS/" + data.FILE_PHYNAME));  //取得檔案存放位置(實體路徑)
                    filePath = Server.MapPath(ConfigModel.UploadTempPath + "/NEWS/" + data.FILE_PHYNAME);  //取得檔案存放位置(實體路徑)
                    LOG.Debug("#ContentController DownloadFile filepath1=" + filePath);
                    LOG.Debug("#ContentController DownloadFile filepath2=" + Server.MapPath(Url.Content(ConfigModel.UploadTempPath + "/NEWS/" + data.FILE_PHYNAME)));

                    if (System.IO.File.Exists(filePath))
                    {
                        TempData["fileid"] = fileid;
                        TempData["fileid2"] = fileid2;
                        /*byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string fileName = data.FILE_ORINAME; //下載的預設檔案名稱
                        TempData["file"] = File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                        return Content(""); */
                    }
                    else
                    {
                        //檔案已不存在的處理方法
                        //TempData["message"] = "檔案已不存在！";
                        //return RedirectToAction("Detail",new { seqno = seqno });
                        string str_LOGError = "##檔案已不存在！";
                        str_LOGError += string.Concat(",filepath1=", filePath);
                        str_LOGError += string.Concat(",filepath2 = ", ConfigModel.UploadTempPath, "/NEWS/", data.FILE_PHYNAME);
                        LOG.Error(str_LOGError);
                        return Content("檔案已不存在！");
                    }
                }
                else
                {
                    byte[] fileBytes = null;
                    string fileName = "";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                #endregion
            }
            else if ("GET".Equals(Request.HttpMethod))
            {
                #region "GET-1"
                if (TempData == null)
                {
                    LOG.Warn("##GET.ContentController DownloadFile TempData == null");
                    return base.SetPageNotFound();
                }
                else if (!TempData.ContainsKey("fileid") || !TempData.ContainsKey("fileid2"))
                {
                    LOG.Warn("##GET.ContentController DownloadFile !TempData.ContainsKey(\"fileid\")");
                    return base.SetPageNotFound();
                }
                //string str_fileid = TempData["fileid"] as string;
                string str_fileid = Convert.ToString(TempData["fileid"]);
                string str_fileid2 = Convert.ToString(TempData["fileid2"]);
                //if (string.IsNullOrEmpty(fileid) || string.IsNullOrEmpty(fileid2)) { return HttpNotFound(); }
                if (string.IsNullOrEmpty(str_fileid)) { return new HttpStatusCodeResult(404); }
                if (str_fileid.Length == 0) { return new HttpStatusCodeResult(404); }
                decimal i_fileid = -1;
                decimal? id = decimal.TryParse(str_fileid, out i_fileid) ? i_fileid : (decimal?)null;
                if (!id.HasValue)
                {
                    LOG.Warn("##GET.ContentController DownloadFile !id.HasValue");
                    return base.SetPageNotFound();
                    //return Content(""); //null;new HttpStatusCodeResult(404); //return new HttpStatusCodeResult(403);
                }

                try
                {
                    string it_fileid2 = MyCommonUtil.DecodeString(str_fileid2);
                    string it_fileIDymd2 = string.Concat(str_fileid, DateTime.Now.ToString("yyyyMMddHH"));
                    if (it_fileIDymd2 != it_fileid2) { return HttpNotFound(); }
                }
                catch (Exception ex)
                {
                    LOG.Warn(string.Concat(ex.Message, ", #POST.ContentController-", fileid, "-", fileid2, "-"), ex);
                    return HttpNotFound();
                }

                TempData["fileid"] = "";
                TempData["fileid2"] = "";
                TblTB_FILE where = new TblTB_FILE { FILEID = id };
                TblTB_FILE data = dao.GetFile(where);

                if (data == null) { return null; }

                filePath = Server.MapPath(ConfigModel.UploadTempPath + "/NEWS/" + data.FILE_PHYNAME);  //取得檔案存放位置(實體路徑)
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                string fileName = data.FILE_ORINAME; //下載的預設檔案名稱
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                #endregion

            }

            return Content(""); //null;
        }

    }
}