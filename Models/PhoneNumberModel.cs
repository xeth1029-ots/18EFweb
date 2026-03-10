using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 用來表示一組完整的 市話, 
    /// 例如: 02-27769993#102 可轉換為 PhoneNumberModel { AreaCode=02, Number=27769993, Ext=102 }
    /// </summary>
    public class PhoneNumberModel
    {
        /// <summary>
        /// 區碼
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 分機
        /// </summary>
        public string Ext { get; set; }


        /// <summary>
        /// 將 PhoneNumberModel 物件以格式化字串輸出, 例: 02-27769993#102, 02-27769993, 27769993#102
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.AreaCode) && !string.IsNullOrEmpty(this.Number) && !string.IsNullOrEmpty(this.Ext))
            {
                return string.Format("{0}-{1}#{2}", this.AreaCode, this.Number, this.Ext);
            }
            else if (!string.IsNullOrEmpty(this.AreaCode) && !string.IsNullOrEmpty(this.Number) )
            {
                return string.Format("{0}-{1}", this.AreaCode, this.Number);
            }
            else if ( !string.IsNullOrEmpty(this.Number) && !string.IsNullOrEmpty(this.Ext))
            {
                return string.Format("{0}#{1}", this.Number, this.Ext);
            }
            else if ( !string.IsNullOrEmpty(this.Number) )
            {
                return this.Number;
            }
            else
            {
                return string.Empty;
            }
            
        }

        #region Parse() 會引用的 static 變數宣告
        private static string pattern1 = "([0-9]*)-([0-9]*[-]*[0-9]*)[#]*([0-9]*)";  /* 內容格式為: 02-12345678#123, 02-1234-5678#123 */
        private static string pattern2 = "([0-9]*)-([0-9]*[-]*[0-9]*)[#]*";          /* 內容格式為: 02-12345678, 02-1234-5678 */
        private static string pattern3 = "([0-9]*[-]*[0-9]*)#([0-9]*)";           /* 內容格式為: 12345678#123, 1234-5678#123 */
        private static Regex rgx1 = new Regex(pattern1, RegexOptions.Compiled);
        private static Regex rgx2 = new Regex(pattern2, RegexOptions.Compiled);
        private static Regex rgx3 = new Regex(pattern3, RegexOptions.Compiled);
        #endregion

        /// <summary>
        /// 將完整格式的電話號碼字串, 轉成 PhoneNumberModel 物件回傳,  
        /// 例如: 02-27769993#102 可轉換為 PhoneNumberModel { AreaCode=02, Number=27769993, Ext=102 },
        /// 若 Parse 失敗, PhoneNumberModel.Number 會填入傳入的 phoneNumber 字串 
        /// </summary>
        /// <param name="phoneNumber">完整格式的電話號碼字串, 如:02-27769993#102</param>
        /// <returns>PhoneNumberModel 物件</returns>
        public static PhoneNumberModel Parse(string phoneNumber)
        {
            PhoneNumberModel phoneNumberModel = new PhoneNumberModel();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return phoneNumberModel;
            }

            //聯絡人電話(日)
            MatchCollection matchs;
            if ((matchs = rgx1.Matches(phoneNumber)) != null && matchs.Count > 0)
            {
                phoneNumberModel.AreaCode = matchs[0].Groups[1].Value;   //區碼
                phoneNumberModel.Number = matchs[0].Groups[2].Value;   //電話
                phoneNumberModel.Ext = matchs[0].Groups[3].Value;   //分機
            }
            else if ((matchs = rgx2.Matches(phoneNumber)) != null && matchs.Count > 0)
            {
                phoneNumberModel.AreaCode = matchs[0].Groups[1].Value;   //區碼
                phoneNumberModel.Number = matchs[0].Groups[2].Value;   //電話
            }
            else if ((matchs = rgx3.Matches(phoneNumber)) != null && matchs.Count > 0)
            {
                phoneNumberModel.Number = matchs[0].Groups[1].Value;   //電話
                phoneNumberModel.Ext = matchs[0].Groups[2].Value;   //分機
            }
            else
            {
                phoneNumberModel.Number = phoneNumber;
            }

            return phoneNumberModel;
        }

    }
}