using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 青年網站後台-系統登入使用者資訊
    /// </summary>
    public class AppUserInfo
    {
        ///<summary>底層系統架構的系統使用者資訊物件</summary>
        LoginUserInfo _BaseUser = null;

        /// <summary>預設建構子</summary>
        public AppUserInfo() { }

        /// <summary>預設建構子</summary>
        /// <param name="baseUser"></param>
        public AppUserInfo(LoginUserInfo baseUser)
        {
            _BaseUser = baseUser;
        }

        /// <summary>
        /// 帳號 
        /// </summary>
        public string UsrID { get; set; }

        /// <summary>
        /// 單位基礎序號 ref:org_organize.org_id
        /// </summary>
        public Int64? UsrOrg { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        public string UsrPwd { get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UsrName { get; set; }

        /// <summary>
        /// 聯絡電話
        /// </summary>
        public string UsrTel { get; set; }

        /// <summary>
        /// E-mail
        /// </summary>
        public string UsrEmail { get; set; }

        /// <summary>
        /// 網站帳號是否使用 Y/N
        /// </summary>
        public string UsrEUsed { get; set; }

        /// <summary>
        /// 網站帳號停用日期
        /// </summary>
        public DateTime? UsrEStopDate { get; set; }

        /// <summary>
        /// 群組代碼
        /// </summary>
        public Int64? UsrGrpID { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? UsrCDate { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string UsrCUser { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public DateTime? UsrMDate { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public string UsrMUser { get; set; }

        /// <summary>
        /// 使用者登入時間
        /// </summary>
        public string UsrLoginDate { get; set; }
        
    }
}