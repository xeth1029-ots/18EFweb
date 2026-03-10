using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    public class ChangePwdViewModel
    {
        public ChangePwdViewModel()
        {
            this.Form = new ChangePwdFormModel();
        }

        public ChangePwdFormModel Form { get; set; }
    }

    public class ChangePwdFormModel
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string UserNo { get; set; }

        /// <summary>
        /// 舊密碼
        /// </summary>
        public string UserPwd { get; set; }

        /// <summary>
        /// 新密碼
        /// </summary>
        public string UserPwdNew { get; set; }

        /// <summary>
        /// 確認新密碼
        /// </summary>
        public string UserPwdNewChk { get; set; }

        /// <summary>
        /// 錯誤訊息1
        /// </summary>
        public string MESSAGE1 { get; set; }

        /// <summary>
        /// 錯誤訊息2
        /// </summary>
        public string MESSAGE2 { get; set; }

        /// <summary>
        /// 錯誤訊息3
        /// </summary>
        public string MESSAGE3 { get; set; }

        /// <summary>
        /// 是否符合輸入規則
        /// </summary>
        public bool IsValid { get; set; }
    }
}