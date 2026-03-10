using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            this.Form = new LoginFormModel();
        }

        public LoginFormModel Form { get; set; }

        /// <summary>
        /// 登入失敗的錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 登入表單 Model
    /// </summary>
    public class LoginFormModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string UserNo { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        public string UserPwd { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        public string ValidateCode { get; set; }
    }


    /// <summary>
    /// 角色群組選擇表單 Model
    /// </summary>
    public class RoleFormModel
    {
        /// <summary>
        /// 使用者選擇角色的檢定類別代碼
        /// </summary>
        public string ExamKind { get; set; }

        /// <summary>
        /// 使用者選擇角色的代碼
        /// </summary>
        public string Role { get; set; }
    }
}