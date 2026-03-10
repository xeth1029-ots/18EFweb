using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Commons.Filter;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using log4net;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    public class LoginController : Controller
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LoginController()
        {
        }

        // GET: Login
        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            LoginViewModel viewModel = new LoginViewModel();
            Session.RemoveAll();
            Session.Clear();
            viewModel.Form.UserNo = "";
            viewModel.Form.UserPwd = "";

            return View(viewModel);
        }

        /// <summary>
        /// 使用者按下登入按鈕
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginFormModel form)
        {
            ActionResult rtn;
            try
            {
                eYVTRmngService service = new eYVTRmngService();

                // 檢查驗證碼及輸入欄位
                this.InputValidate(form);

                // 登入帳密檢核, 並取得使用者帳號及權限角色清單資料
                LoginUserInfo userInfo = service.LoginValidate(form.UserNo, form.UserPwd);

                userInfo.NetID = "1";   /* 使用者登入區域, 有點 hard-code ... */
                userInfo.LoginIP = HttpContext.Request.UserHostAddress;


                // 登入失敗, 丟出錯誤訊息
                if (!userInfo.LoginSuccess)
                {
                    // 寫入 登入失敗記錄
                    //service.RecordLogin(userInfo);

                    throw new LoginExceptions(userInfo.LoginErrMessage);
                }

                // 寫入 登入成功記錄
                //service.RecordLogin(userInfo);

                // 將登入者資訊保存在 SessionModel 中
                SessionModel sm = SessionModel.Get();
                sm.UserInfo = userInfo;

                // 依據當前 Role 帶入(更新)對應的的功能選單
                //new ClamService().GetUserRoleFuncs(sm.UserInfo);
                sm.RoleFuncs = service.GetUserRoleFuncs(sm.UserInfo);

                LoginViewModel model = new LoginViewModel();
                model.Form = form;
                if (userInfo.ChangePwdRequired)
                {
                    rtn = RedirectToAction("Index", "ChangePwd");
                }
                else
                {
                    rtn = RedirectToAction("Index", "Home");
                    //rtn = View("Index", model);
                }
            }
            catch (LoginExceptions ex)
            {
                LOG.Info("Login(" + form.UserNo + ") Failed from " + Request.UserHostAddress + ": " + ex.Message);

                // 清除不想要 Cache POST data 的欄位
                ModelState.Remove("form.ValidateCode");
                ModelState.Remove("form.UserPwd");

                LoginViewModel model = new LoginViewModel();
                model.Form.UserNo = form.UserNo;
                model.ErrorMessage = ex.Message;
                rtn = View("Index", model);
            }

            return rtn;
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
            SessionModel.Get().LoginValidateCode = vCode;

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
            string vCode = SessionModel.Get().LoginValidateCode;

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

        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Index", "Login");
        }

        /// <summary>
        /// 自然人憑證 HiLocalServer 測試頁
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult CertTest()
        {
            return View();
        }

        /// <summary>
        /// 解碼使用者密碼(測試用, 上線前要停用)
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        //public ActionResult DCUPWD(string userNo)
        //{
        //    if(string.IsNullOrEmpty(userNo) || !Request.IsLocal )
        //    {
        //        return HttpNotFound();
        //    }
        //    else
        //    {
        //        ClamDAO dao = new ClamDAO();
        //        ClamUser user = dao.GetUser(userNo);
        //        if (user != null)
        //        {
        //            RSACSP.RSACSP rsa = new RSACSP.RSACSP();
        //            string plainPwd = rsa.Utl_Decrypt(user.PWD);
        //            return Content(string.Format("{0} / {1}", userNo, plainPwd));
        //        }
        //        else
        //        {
        //            return Content(string.Format("User '{0}' not found", userNo));
        //        }
        //    }
        //}

        /// <summary>
        /// 重設使用者密碼(測試用, 上線前要停用)
        /// </summary>
        /// <param name="userNo"></param>
        /// <param name="uPwd"></param>
        /// <returns></returns>
        //public ActionResult RESETUPWD(string userNo, string uPwd)
        //{
        //    if (string.IsNullOrEmpty(userNo) || string.IsNullOrEmpty(uPwd) || !Request.IsLocal)
        //    {
        //        return HttpNotFound();
        //    }
        //    else
        //    {
        //        ClamDAO dao = new ClamDAO();
        //        ClamUser user = dao.GetUser(userNo);
        //        if (user != null)
        //        {
        //            RSACSP.RSACSP rsa = new RSACSP.RSACSP();


        //            TblCLAMDBURM pwdModel = new TblCLAMDBURM
        //            {
        //                PWD = rsa.Utl_Encrypt(uPwd),
        //                MODUSERID = "system",
        //                MODTIME = MyHelperUtil.DateTimeToLongTwString(DateTime.Now)
        //            };
        //            TblCLAMDBURM where = new TblCLAMDBURM
        //            {
        //                USERNO = user.USERNO
        //            };

        //            dao.Update<TblCLAMDBURM>(pwdModel, where);

        //            return Content(string.Format("User '{0}' PWD has reseted", userNo));
        //        }
        //        else
        //        {
        //            return Content(string.Format("User '{0}' not found", userNo));
        //        }
        //    }
        //}

        // 檢核輸入欄位
        private void InputValidate(LoginFormModel form)
        {
            if (string.IsNullOrEmpty(form.UserNo) || string.IsNullOrEmpty(form.UserPwd))
            {
                LoginExceptions ex = new LoginExceptions("請輸入 您的帳號及密碼 !!");
                throw ex;
            }
            if (string.IsNullOrEmpty(form.ValidateCode))
            {
                LoginExceptions ex = new LoginExceptions("驗證碼輸入不正確!");
                throw ex;
            }
            if (!form.ValidateCode.Equals(SessionModel.Get().LoginValidateCode))
            {
                LoginExceptions ex = new LoginExceptions("驗證碼輸入不正確!!");
                throw ex;
            }
        }

    }
}