using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using log4net;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 用來表示一組完整的 電子郵件, 
    /// 例如: name@turbotech.com.tw 可轉換為 EmailModel { User=name, Domain=turbotech.com.tw }
    /// </summary>
    public class EmailModel
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool isValid = false;

        /// <summary>
        /// 電子郵件的使用者姓名(在@之前)部份
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 電子郵件的網址名稱(在@之後的Domain Name)部份
        /// </summary>
        public string Domain { get; set; }


        /// <summary>
        /// 將 EmailModel 物件以格式化字串輸出, 例: name@turbotech.com.tw
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}@{1}", this.UserName, this.Domain);
        }

        #region Parse() 會引用的 static 變數宣告
        private static string pattern1 = @"^(\w+([-+.']\w+)*)@(\w+([-.]\w+)*\.\w+([-.]\w+)*)$";  /* 內容格式為: user@turbotech.com.tw */
        private static Regex rgx1 = new Regex(pattern1, RegexOptions.Compiled);
        #endregion

        /// <summary>
        /// 將完整格式的電話號碼字串, 轉成 PhoneNumberModel 物件回傳,  
        /// 例如: 02-27769993#102 可轉換為 PhoneNumberModel { AreaCode=02, Number=27769993, Ext=102 },
        /// 若 Parse 失敗, PhoneNumberModel.Number 會填入傳入的 phoneNumber 字串 
        /// </summary>
        /// <param name="phoneNumber">完整格式的電話號碼字串, 如:02-27769993#102</param>
        /// <returns>PhoneNumberModel 物件</returns>
        public static EmailModel Parse(string email)
        {
            EmailModel emailModel = new EmailModel();
            if (string.IsNullOrEmpty(email))
            {
                return emailModel;
            }

            MatchCollection matchs;
            if ((matchs = rgx1.Matches(email)) != null && matchs.Count > 0)
            {
                // 比對成功, 為有效的 Email
                emailModel.isValid = true;

                //debug
                for (int i = 0; i < matchs.Count; i++)
                {
                    Match match = matchs[i];
                    for (int k = 0; k < match.Groups.Count; k++)
                    {
                        Capture group = match.Groups[k];
                        logger.Debug(string.Format("Parse: matchs[{0}].Groups[{1}].Value=", i, k, group.Value));
                    }
                }
                emailModel.UserName = matchs[0].Groups[1].Value;  
                emailModel.Domain = matchs[0].Groups[3].Value;   
            }

            return emailModel;
        }

        /// <summary>
        /// 是否為合法的 Email Address 格式
        /// </summary>
        public bool IsValid
        {
            get { return isValid; }
        }

    }
}