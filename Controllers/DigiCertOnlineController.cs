using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    public class DigiCertOnlineController : LoginBaseController
    {
        //BaseController //LoginBaseController
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: DigiCertOnline
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            DigiCertViewModel model = new DigiCertViewModel();
            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            //model.Detail.CNAME = "CNAME";
            return View(model); //return View("Detail", model); //return View(model); //return View();
        }

        [HttpPost]
        public ActionResult Index(DigiCertViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            //DigiCertViewModel model = new DigiCertViewModel();
            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            //登入後才可執行
            if (sm == null || !sm.IsLogin || !sm.MemSN.HasValue)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                var rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            //DigiCertViewModel model = new DigiCertViewModel();
            //var logmsg = string.Concat("#Index:", ",model.Form.PURID:", model.Form.PURID, ",model.Form.UAGID:", model.Form.UAGID); //LOG.Debug(logmsg);
            var form1 = get_form1_data(sm, model.Form);
            //logmsg = string.Concat("#Index:", ",form.PURID:", form1.PURID, ",form.UAGID:", form1.UAGID); //LOG.Debug(logmsg);

            model.Form = form1;
            return View("Detail", model); // View();
        }

        DigiCertFormModel get_form1_data(SessionModel sm, DigiCertFormModel form2)
        {
            if (sm == null || !sm.IsLogin || !sm.MemSN.HasValue) { return form2; }

            //DigiCertViewModel model = new DigiCertViewModel();
            DigiCertFormModel form1 = new DigiCertFormModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            if (!string.IsNullOrEmpty(sm.DCANO) && !string.IsNullOrEmpty(sm.DCASENO))
            {
                var wdc1 = new TblSTUD_DIGICERTAPPLY() { DCANO = long.Parse(sm.DCANO), DCASENO = sm.DCASENO };
                var dc1 = dao.GetRow(wdc1);
                if (dc1 != null)
                {
                    form1.DCANO = dc1.DCANO;
                    form1.DCASENO = dc1.DCASENO;
                    form1.IDNO = dc1.IDNO;
                    form1.CNAME = dc1.CNAME;
                    form1.PURID = dc1.PURID;
                    form1.UAGID = dc1.UAGID;
                    form1.EMAIL = dc1.EMAIL;
                    form1.EMVCODE = dc1.EMVCODE;
                    form1.GUID1 = dc1.GUID1;
                    form1.APPLNACCT = dc1.APPLNACCT;
                    form1.APPLNDATE = dc1.APPLNDATE;
                    form1.MODIFYACCT = dc1.MODIFYACCT;
                    form1.MODIFYDATE = dc1.MODIFYDATE;
                    return form1;
                }
            }

            var idno = sm.ACID;
            DateTime birth = DateTime.Parse(sm.Birthday);
            if (form2 == null) { form2 = new DigiCertFormModel(); }
            var aNow = new MyKeyMapDAO().GetSysDateNow();
            //查詢會員基本資料
            TblE_MEMBER mem = dao.GetEMemberByIDNO(idno, birth);
            MyKeyMapDAO kdao = new MyKeyMapDAO();
            if (string.IsNullOrEmpty(form2.DCASENO)) form2.DCASENO = kdao.GET_DCASENO_20();
            //申請人姓名
            form2.CNAME = sm.UserName;
            //form1.BIRTHDAY = sm.Birthday; //身分證編號
            form2.IDNO = sm.ACID;
            //申請用途//請選擇 //申請證明要提供的使用單位//請選擇 //電子郵件
            if (string.IsNullOrEmpty(form2.EMAIL))
            {
                form2.EMAIL = string.IsNullOrEmpty(sm.EMAIL) ? mem.MEM_EMAIL : sm.EMAIL;
            }
            //電子郵件驗證碼 //圖型驗證碼 //請輸入下方圖片中文字 //產生驗證碼 撥放 //申請日期
            form2.APPLNDATE = aNow;
            form2.APPLNACCT = sm.MemSN.ToString();
            return form2;
        }

        public ActionResult Detail()
        {
            //登入後才可執行
            SessionModel sm = SessionModel.Get();
            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            //登入後才可執行
            if (sm == null || !sm.IsLogin || !sm.MemSN.HasValue)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                var rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            DigiCertViewModel model = new DigiCertViewModel();
            var form1 = get_form1_data(sm, model.Form);
            model.Form = form1;
            return View("Detail", model); // View();
        }

        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCode()
        {
            Turbo.Commons.ValidateCode vc = new Turbo.Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(4);
            SessionModel.Get().DigiValidateCode = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCodeAudio()
        {
            string vCode = SessionModel.Get().DigiValidateCode;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                Turbo.Commons.ValidateCode vc = new Turbo.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

        // 檢核輸入欄位
        private string InputValidate(DigiCertFormModel form)
        {
            var VCode = SessionModel.Get().DigiValidateCode;

            if (string.IsNullOrEmpty(VCode))
            {
                return "驗證碼不正確!"; // throw ex;
            }
            if (string.IsNullOrEmpty(form.ValidateCode))
            {
                return "驗證碼輸入不正確!"; // throw ex;
            }
            else if (!form.ValidateCode.Equals(SessionModel.Get().DigiValidateCode))
            {
                return string.Concat("驗證碼輸入不正確!!", form.ValidateCode); // throw ex;
            }
            else if (form.ValidateCode.Equals(SessionModel.Get().DigiValidateCode))
            {
                Turbo.Commons.ValidateCode vc = new Turbo.Commons.ValidateCode();
                string vCode = vc.CreateValidateCode(4);
                SessionModel.Get().DigiValidateCode = vCode;
            }
            return null; // true;
        }

        /// <summary>Guid.NewGuid().ToString()</summary>
        /// <returns></returns>
        static string GetGUID1()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        void Savedata1(DigiCertFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            if (!string.IsNullOrEmpty(sm.DCANO) && !string.IsNullOrEmpty(sm.DCASENO))
            {
                //資料重複不再新增
                var wdc1 = new TblSTUD_DIGICERTAPPLY() { DCANO = long.Parse(sm.DCANO), DCASENO = sm.DCASENO };
                var dc1 = dao.GetRow(wdc1);
                if (dc1 != null) { return; }
            }

            //string g1 = GetGUID1();
            string g1 = dao.CheckGetGUID1N();
            long iDCANO = 0;
            if (!form.DCANO.HasValue)
            {
                iDCANO = dao.GetNewId("STUD_DIGICERTAPPLY_DCANO_SEQ,STUD_DIGICERTAPPLY,DCANO").Value; //dao.GetAutoNum()
                form.DCANO = iDCANO;
                form.GUID1 = GetGUID1();
            }
            if (iDCANO == 0 || !form.DCANO.HasValue)
            {
                //資料有誤直接離開
                ArgumentNullException ex = new ArgumentNullException("iDCANO");
                throw ex;
            }

            //STUD_DIGICERTAPPLY // 資料維護 /*sm.SessionID, sm.ACID,*/
            var oDCAdata = new TblSTUD_DIGICERTAPPLY()
            {
                DCANO = form.DCANO,
                DCASENO = form.DCASENO,
                IDNO = form.IDNO,
                CNAME = form.CNAME,
                PURID = form.PURID,
                UAGID = form.UAGID,
                EMAIL = form.EMAIL,
                EMVCODE = form.EMVCODE,
                GUID1 = form.GUID1,
                APPLNACCT = sm.RID,
                APPLNDATE = aNow,
                MODIFYACCT = sm.ACID,
                MODIFYDATE = aNow
            };
            dao.Insert(oDCAdata);

            sm.DCANO = iDCANO.ToString();//案件流水ID
            sm.DCASENO = form.DCASENO;//案件編號
        }

        [HttpPost]
        public ActionResult Save(DigiCertViewModel model)
        {
            //return View(); //ActionResult rtn = null;
            //ActionResult rtn = View("Index", model);
            //ActionResult rtn = View("Detail", model);
            ActionResult rtn = null; //Index(model);

            SessionModel sm = SessionModel.Get();
            //增加檢核E_Menber MEM_SN(序號)
            if (sm == null || !sm.IsLogin || sm.MemSN == null)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            //rtn = RedirectToAction("Detail", model);
            //model.Form.APPLNDATE = aNow;
            DigiCertFormModel form = model.Form;
            //var logmsg = string.Concat("#Save:", ",form.PURID:", form.PURID, ",form.UAGID:", form.UAGID);
            //LOG.Debug(logmsg);
            try
            {
                // 表單欄位檢核
                model.Valid(ModelState);

                var strErr = this.InputValidate(form);

                if (!string.IsNullOrEmpty(strErr)) { ModelState.AddModelError("", strErr); };

                if (ModelState.IsValid)
                {
                    ModelState.Clear();

                    Savedata1(form);

                    sm.LastResultMessage = "線上申請,課程勾稽";

                    sm.RedirectUrlAfterBlock = "";

                    rtn = RedirectToAction("Index2", "DigiCertOnline");

                    return rtn;
                }
            }
            catch (Exception ex)
            {
                //string s_ErrorMessage = string.Concat("儲存失敗，資料異常 或 資料庫異常，請重試!!<br>", "請再試一次，造成您不便之處，還請見諒。<br>", "(若持續出現此問題，請聯絡系統管理者)!!!");
                var s_ErrorMessage = ex.Message;
                sm.LastErrorMessage = s_ErrorMessage;
                //sm.RedirectUrlAfterBlock = Url.Action("Detail", "DigiCertOnline");
                LOG.Error(string.Concat("#DigiCertOnlineController Save ex:", ex.Message, "\n", s_ErrorMessage), ex);
                throw ex;
            }

            rtn = Index(model);
            return rtn;

        }

        public ActionResult Index2()
        {
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;
            long iDCANO = -1;
            bool fg_DCANO_NG = string.IsNullOrEmpty(sm.DCANO); //資料為空資訊有誤
            if (!fg_DCANO_NG) { iDCANO = long.Parse(sm.DCANO); }

            //增加檢核E_Menber MEM_SN(序號)
            if (sm == null || !sm.IsLogin || sm.MemSN == null || fg_DCANO_NG)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            DigiCertViewModel model = new DigiCertViewModel();
            model.PForm.TRC_MSN = sm.MemSN;
            model.PForm.DCANO = iDCANO;//案件流水ID
            model.PForm.DCASENO = sm.DCASENO;
            model.PForm.CNAME = sm.UserName;
            model.PForm.IDNO = sm.ACID;

            var wg = new TblSTUD_DIGICERTCLASS { DCANO = iDCANO };
            var dao = new WDAIIPWEBDAO();
            var glist = dao.GetRowList(wg);
            if (glist != null) { model.PForm.CLASSCNT = glist.Count; }

            rtn = Index2(model.PForm);
            return rtn;
        }

        [HttpPost]
        public ActionResult Index2(DigiCertPageFormModel pform)
        {
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;
            long iDCANO = -1;
            bool fg_DCANO_NG = string.IsNullOrEmpty(sm.DCANO); //資料為空資訊有誤
            if (!fg_DCANO_NG) { iDCANO = long.Parse(sm.DCANO); }

            //增加檢核E_Menber MEM_SN(序號)
            if (sm == null || !sm.IsLogin || sm.MemSN == null || fg_DCANO_NG)
            {
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                rtn = RedirectToAction("Login", "Member");
                return rtn;
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            var wdata = new TblSTUD_DIGICERTAPPLY { DCANO = iDCANO, DCASENO = sm.DCASENO };
            var oDCAP = dao.GetRow(wdata);
            if (oDCAP == null)
            {
                LOG.Warn("oDCAP == null: " + Request.UserHostAddress + ": 會員(mem_id=" + sm.UserID);
                return RedirectToAction("Login", "Member");
            }
            else if (oDCAP != null && oDCAP.IDNO != sm.ACID)
            {
                LOG.Warn("oDCAP.IDNO!=sm.ACID: " + Request.UserHostAddress + ": 會員(mem_id=" + sm.UserID);
                return RedirectToAction("Login", "Member");
            }

            pform.DCANO = iDCANO;//案件流水ID
            //pform.DCASENO = sm.DCASENO;//案件編號 //pform.IDNO = oDCAP.IDNO;
            pform.TRC_MSN = sm.MemSN;
            pform.DCASENO = sm.DCASENO;//案件編號
            pform.CNAME = sm.UserName;
            pform.IDNO = sm.ACID;

            var model = new DigiCertViewModel();
            model.PForm = pform;
            rtn = View("Index2", model); //ActionResult rtn = View("Index2", model);

            if (pform.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                // 設定查詢分頁資訊
                dao.SetPageInfo(pform.rid, pform.p);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                model.Grid = dao.QueryDigiCertClass(pform);

                if (!string.IsNullOrEmpty(pform.rid) && pform.useCache == 0)
                {
                    // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(pform, dao, "Index2");
            }

            model.PForm = pform;
            byte[] byarr = MyCommonUtil.ObjectToByteArray(model);
            Session["LastModel"] = Convert.ToBase64String(byarr);
            Session["rid"] = dao.ResultID;
            return rtn;
        }

        /// <summary>最後一次查詢結果</summary>
        /// <returns></returns>
        public ActionResult LastResult()
        {
            if (Session["LastModel"] == null) { return base.SetPageNotFound(); }
            string sessData = (string)Session["LastModel"] ?? null;
            if (sessData == null) { return base.SetPageNotFound(); }

            var s_ViewNM_index = "Index2";
            DigiCertViewModel model = new DigiCertViewModel();
            BaseDAO dao = new BaseDAO();
            if (sessData != null)
            {
                var data = Convert.FromBase64String(sessData);
                if (data != null)
                {
                    if (Session["rid"] == null) { return base.SetPageNotFound(); }
                    string rid = Session["rid"].ToString();
                    dao.ResultID = rid;
                    model = (DigiCertViewModel)MyCommonUtil.ByteArrayToObject(data);
                }
                else
                {
                    model = new DigiCertViewModel();
                    model.PForm = new DigiCertPageFormModel();
                    base.SetPagingParams(model.PForm, dao, s_ViewNM_index);
                    model.PForm.action = Url.Action(s_ViewNM_index);
                }
            }

            return View(s_ViewNM_index, model);
        }


        /// <summary> 多筆課程勾選-確認勾選 </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MultiAddClass(DigiCertViewModel model)
        {
            DigiCertViewModel.CheckArgument(this.HttpContext);

            SessionModel sm = SessionModel.Get();
            if (sm == null || !sm.IsLogin || sm.UserID == null || sm.MemSN == null)
            {
                //增加檢核E_Menber MEM_SN(序號)
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                return RedirectToAction("Login", "Member");
            }
            ActionResult rtn = LastResult(); //2019-02-11 fix 問題9：修改點選完「課程收藏」後，都會跳到查詢頁一會，等按掉alert又跳回查詢清單頁，然後此時就又會跳出查詢清單頁一進入時的alert一次問題

            if (model.Grid == null)
            {
                //return RedirectToAction("Login", "Member");
                sm.LastErrorMessage = "選取課程清單失敗。";
                sm.RedirectUrlAfterBlock = Url.Action("Index2", "DigiCertOnline", new { useCache = 1 });
                return rtn;
            }

            //classlist = model.Grid.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
            IList<Int64?> classlist = model.Grid.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
            if (classlist == null || classlist.Count == 0)
            {
                sm.LastResultMessage = "請至少勾選一項";
                sm.RedirectUrlAfterBlock = Url.Action("Index2", "DigiCertOnline", new { useCache = 1 });
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace");
                //rtn = new RedirectResult(Url.Action("Index", "ClassTrace"));
                return rtn;
            }
            IList<Int64?> classlistdel = model.Grid.Where(m => m.SELECTIS == false).Select(m => m.OCID).ToList();

            try
            {
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                //批次刪除課程
                dao.DelDigiCertClass(classlistdel, sm);
                //批次加入課程
                dao.AddDigiCertClass(classlist, sm);

                sm.LastResultMessage = "已選取課程清單";
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace", new { });
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace");
                rtn = new RedirectResult(Url.Action("Index2", "DigiCertOnline"));
            }
            catch (Exception ex)
            {
                LOG.Error("DigiCertOnlineController MultiAddClass failed: " + ex.Message, ex);
                //throw new Exception("ClassTraceController MultiDelClassTrace failed:" + ex.Message, ex);
                sm.LastErrorMessage = "選取課程清單失敗。";
                sm.RedirectUrlAfterBlock = Url.Action("Index2", "DigiCertOnline", new { useCache = 1 });
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassTrace");
                //rtn = new RedirectResult(Url.Action("Index", "ClassTrace"));
                //return rtn;
            }
            return rtn;
        }

        /// <summary>寄送EMAIL</summary>
        /// <param name="owdc1"></param>
        /// <param name="SNStext"></param>
        void DigiSendEmail2(TblSTUD_DIGICERTAPPLY owdc1, string SNStext)
        {
            //測試環境不寄信
            string s_Env = ConfigModel.WebEnvironment ?? "";
            if (s_Env == "test" || !string.IsNullOrEmpty(s_Env)) { owdc1.EMAIL = (new ExceptionHandler(new ErrorPageController())).cst_EmailtoMe; }
            string s_TestEmail = ConfigModel.WebTestEmail ?? "";
            if (!string.IsNullOrEmpty(s_TestEmail) && s_TestEmail.IndexOf(';') > -1) { s_TestEmail = s_TestEmail.Replace(';', ','); }
            bool fg2 = (!string.IsNullOrEmpty(s_TestEmail) && s_TestEmail.IndexOf(",") > -1);//多筆
            bool fg3 = (!string.IsNullOrEmpty(s_TestEmail));//單筆

            //2.寄email
            const string cst_Subject = "「在職訓練網」在職訓練課程數位結訓證明線上申辦案件：受理通知";
            const string strFromName = "系統自動發信";
            string strFromEmail = "ojt@msa.wda.gov.tw";
            var strToName = "在職訓學員"; //sm.UserName;
            var strToEmail = owdc1.EMAIL;
            var strHtmlBody = SNStext;
            //WebRequest物件如何忽略憑證問題
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ExceptionHandler.ValidateServerCertificate);
            //TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072
            string strResult = "";
            string s_LOGError = "";
            string s_Err1 = "";
            //bool flag_mail_error = false;
            //發信失敗? //https://wltims.wda.gov.tw/GetJobMail3/Service1.asmx //https://ojfile119.ejob.gov.tw/GetJobMail3/Service1.asmx
            SendMailws3.Service1 m = new SendMailws3.Service1();

            string ee1 = "";
            try
            {
                var testSubject = string.Concat(cst_Subject, "(TEST)");
                if (fg2 && !string.IsNullOrEmpty(s_TestEmail))
                {
                    foreach (var vEmail in s_TestEmail.Split(','))
                    {
                        if (!string.IsNullOrEmpty(vEmail))
                        {
                            ee1 = vEmail;
                            strResult = m.SendMailT(strFromName, strFromEmail, strToName, vEmail, testSubject, strHtmlBody, "");
                        }
                    }
                }
                else if (fg3 && !string.IsNullOrEmpty(s_TestEmail))
                {
                    ee1 = s_TestEmail;
                    strResult = m.SendMailT(strFromName, strFromEmail, strToName, s_TestEmail, testSubject, strHtmlBody, "");
                }
                else
                {
                    ee1 = strToEmail;
                    strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strHtmlBody, "");
                }
            }
            catch (Exception ex)
            {
                s_Err1 = ex.Message;
                //flag_mail_error = true;
                s_LOGError = string.Format("#DigiSendEmail2({0}):\n sMailBody:\n{1}\n ex.Message:\n{2}\n", ee1, strHtmlBody, ex.Message);
                LOG.Error(s_LOGError, ex);
                bLOG.WarnFormat("發信失敗，請確認您的Email是否正確![{0}]{1} ", ee1, s_Err1);
                throw ex;
            }
        }

        /// <summary>點送出後，不需審核，系統寄發受理通知信</summary>
        /// <param name="owdc1"></param>
        void DigiCertSendMail2(TblSTUD_DIGICERTAPPLY owdc1)
        {
            if (owdc1 == null) { return; }

            DateTime aNow = (new MyKeyMapDAO()).GetSysDateNow();
            //var ojtweba1 = "<strong><a href=\"https://ojt.wda.gov.tw/\" target=\"_blank\" title=\"[另開新視窗]前往在職訓練網\"><span class=\"sr-only\">(另開新視窗)</span>「在職訓練網」</a></strong>";
            var ojtweba1 = "https://ojt.wda.gov.tw";
            //SNStext += "「在職訓練網」email驗證通知，請完成操作";
            var rdY = (int.Parse(owdc1.APPLNDATE.Value.ToString("yyyy")) - 1911).ToString();
            var rdM = owdc1.APPLNDATE.Value.ToString("MM"); // 113年08月20日
            var rdd = owdc1.APPLNDATE.Value.ToString("dd");
            var rOC1 = string.Concat(rdY, "年", rdM, "月", rdd, "日");
            var ee1 = owdc1.EMAIL;

            var SNStext = "";
            SNStext += string.Concat(owdc1.CNAME, "君 您好：<br/><br/>");
            SNStext += string.Concat("您於【在職訓練網】提出在職訓練課程數位結訓證明線上申請案件，已於", rOC1, "受理完成<br/><br/>");
            SNStext += string.Concat("案件編號為：", owdc1.DCASENO, "。<br/><br/>");
            SNStext += string.Concat("您可隨時至在職訓練網 ( ", ojtweba1, " ) 下載您的「數位結訓證明」檔案。<br/><br/>");
            SNStext += string.Concat("＊此為系統自動發送信件，請勿直接回信。", aNow.ToString("yyyy-MM-dd HH:mm:ss"), "<br/>");

            //bool flag_mail_error = false;
            try
            {
                //2.寄email
                DigiSendEmail2(owdc1, SNStext);
                //strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strHtmlBody, "");
            }
            catch (Exception ex)
            {
                //flag_mail_error = true;
                LOG.Error(ex.Message, ex); //throw;
            }

            //strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strHtmlBody, "");
        }

        [HttpPost]
        public ActionResult Savedata2(string xDCANO, string xDCASENO)
        {
            DigiCertViewModel.CheckArgument(this.HttpContext);
            SessionModel sm = SessionModel.Get();
            if (sm == null || !sm.IsLogin || sm.UserID == null || sm.MemSN == null)
            {
                //增加檢核E_Menber MEM_SN(序號)
                LOG.Info(string.Concat("Login Failed from ", Request.UserHostAddress, ": 未登入會員(mem_id=", sm.UserID));
                return RedirectToAction("Login", "Member");
            }

            long iDCANO = long.Parse(xDCANO);
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            var aNow = new MyKeyMapDAO().GetSysDateNow();

            var wdc1 = new TblSTUD_DIGICERTAPPLY { DCANO = iDCANO, DCASENO = xDCASENO, IDNO = sm.ACID };
            var owdc1 = dao.GetRow(wdc1);
            if (owdc1 == null)
            {
                sm.DCANO = string.Empty;
                sm.DCASENO = string.Empty;
                sm.LastResultMessage = "線上申請案有誤，請重新申請！";
                return RedirectToAction("Index", "Home");
            }
            var wSdcc1 = new TblSTUD_DIGICERTCLASS { DCANO = iDCANO };
            var Sdcc1 = dao.GetRowList(wSdcc1);
            if (Sdcc1 == null || Sdcc1.Count == 0)
            {
                //sm.DCANO = string.Empty; //sm.DCASENO = string.Empty;
                sm.LastResultMessage = "線上申請案有誤，請先確認勾選，再按申請送出！";
                return RedirectToAction("Index2", "DigiCertOnline");
            }

            //產出一個數位結訓證明檔案，並給予一個驗證檢核碼 //系統要紀錄相關資訊，可供後端管理者查詢申請與下載紀錄
            var dc1 = new TblSTUD_DIGICERTAPPLY { SENDACCT = sm.ACID, SENDDATE = aNow };
            dao.Update(dc1, wdc1);

            var owdc2 = dao.GetRow(wdc1);
            if (owdc2 == null)
            {
                sm.DCANO = string.Empty;
                sm.DCASENO = string.Empty;
                sm.LastResultMessage = "線上申請案有誤，請重新申請!!";
                return RedirectToAction("Index", "Home");
            }

            //點送出後，不需審核，系統寄發受理通知信
            DigiCertSendMail2(owdc2);

            sm.DCANO = string.Empty;
            sm.DCASENO = string.Empty;
            sm.LastResultMessage = "線上申請案已送出！";
            return RedirectToAction("Index", "Home");
        }

    }
}