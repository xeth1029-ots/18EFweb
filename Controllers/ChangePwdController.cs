using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Controllers
{
    public class ChangePwdController : Controller
    {
        // GET: ChangePwd
        [HttpGet]
        public ActionResult Index()
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();

            if (sm.UserInfo == null || sm.UserInfo.AppUser == null || string.IsNullOrEmpty(sm.UserInfo.AppUser.UsrID))
            {
                rtn = RedirectToAction("Index", "Login", new { area = "" });
            }
            else
            {
                ChangePwdViewModel model = new ChangePwdViewModel();

                model.Form.UserName = sm.UserInfo.AppUser.UsrName;
                model.Form.UserNo = sm.UserInfo.UserNo;

                rtn = View(model);
            }

            return rtn;
        }

        /// <summary>
        /// 儲存變更密碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ChangePwdFormModel form)
        {
            ActionResult rtn = null;
            ChangePwdViewModel model = new ChangePwdViewModel();
            eYVTRmngDAO dao = new eYVTRmngDAO();
            SessionModel sm = SessionModel.Get();

            try
            {
                model.Form = form;
                model.Form.UserName = sm.UserInfo.AppUser.UsrName;
                model.Form.UserNo = sm.UserInfo.UserNo;

                var user = sm.UserInfo.AppUser;

                // 檢核輸入欄位
                this.InputValidate(form);

                if (!form.IsValid)
                {
                    throw new Exception("密碼變更資訊不符規則");
                }

                //變更密碼
                dao.UpdateUsrPwd(user.UsrID, form.UserPwdNew);

                sm.LastResultMessage = "密碼變更成功，請於下次使用新密碼登入系統!";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "Home");

                rtn = View(model);
            }
            catch (Exception ex)
            {
                if (form.IsValid)
                {
                    sm.LastErrorMessage = "變更失敗";
                }

                rtn = View(model);
            }

            return rtn;
        }

        /// <summary>
        /// 檢核輸入欄位
        /// </summary>
        /// <param name="form"></param>
        private void InputValidate(ChangePwdFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            AppUserInfo user = sm.UserInfo.AppUser;
            string msg1 = "";
            string msg2 = "";
            string msg3 = "";

            form.IsValid = false;

            if (string.IsNullOrWhiteSpace(form.UserPwd))
            {
                msg1 = "請輸入舊密碼!";
            }
            else if (!form.UserPwd.Equals(user.UsrPwd))
            {
                msg1 = "舊密碼輸入錯誤!";
            }
            else if (form.UserPwd.Equals(form.UserPwdNew))
            {
                msg1 = "新密碼不得與現在密碼設定相同!";
            }

            if (string.IsNullOrWhiteSpace(form.UserPwdNew))
            {
                msg2 = "請輸入新密碼!";
            }
            else if (form.UserPwdNew.Length < 12 || form.UserPwdNew.Length > 16)
            {
                msg2 = "新密碼長度應為12~16碼，且需至少為英數字混合!";
            }
            else if (!MyCommonUtil.ChkPwdFmt(form.UserPwdNew))
            {
                msg2 = "新密碼長度應為12~16碼，且需至少為英數字混合!";
            }

            if (string.IsNullOrWhiteSpace(form.UserPwdNewChk))
            {
                msg3 = "請輸入確認新密碼!";
            }
            else if (!string.IsNullOrWhiteSpace(form.UserPwdNew) && !form.UserPwdNew.Equals(form.UserPwdNewChk))
            {
                msg3 = "新密碼與確認新密碼輸入的內容不同!";
            }

            form.MESSAGE1 = msg1;
            form.MESSAGE2 = msg2;
            form.MESSAGE3 = msg3;

            if (string.IsNullOrEmpty(msg1) && string.IsNullOrEmpty(msg2) && string.IsNullOrEmpty(msg3))
            {
                form.IsValid = true;
            }
        }
    }
}