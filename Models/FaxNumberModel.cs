using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 用來表示一組完整的 傳真, 
    /// 例如: 02-27769993 可轉換為 FaxNumberModel { AreaCode=02, Number=27769993}
    /// </summary>
    public class FaxNumberModel
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
        /// 將 FaxNumberModel 物件以格式化字串輸出, 例: 02-27769993, 27769993
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.AreaCode) && !string.IsNullOrEmpty(this.Number))
            {
                return string.Format("{0}-{1}", this.AreaCode, this.Number);
            }
            else if (!string.IsNullOrEmpty(this.Number))
            {
                return this.Number;
            }
            else
            {
                return string.Empty;
            }

        }

        #region Parse() 會引用的 static 變數宣告
        private static string pattern1 = "([0-9]*)-([0-9]*[-]*[0-9]*)[#]*";          /* 內容格式為: 02-12345678, 02-1234-5678 */
        private static Regex rgx1 = new Regex(pattern1, RegexOptions.Compiled);
        #endregion

        /// <summary>
        /// 將完整格式的電話號碼字串, 轉成 FaxNumberModel 物件回傳,  
        /// 例如: 02-27769993 可轉換為 FaxNumberModel { AreaCode=02, Number=27769993},
        /// 若 Parse 失敗, FaxNumberModel.Number 會填入傳入的 faxNumber 字串 
        /// </summary>
        /// <param name="faxNumber">完整格式的電話號碼字串, 如:02-27769993</param>
        /// <returns>FaxNumberModel 物件</returns>
        public static FaxNumberModel Parse(string faxNumber)
        {
            FaxNumberModel FaxNumberModel = new FaxNumberModel();
            if (string.IsNullOrEmpty(faxNumber))
            {
                return FaxNumberModel;
            }

            //聯絡人電話(日)
            MatchCollection matchs;
            if ((matchs = rgx1.Matches(faxNumber)) != null && matchs.Count > 0)
            {
                FaxNumberModel.AreaCode = matchs[0].Groups[1].Value;   //區碼
                FaxNumberModel.Number = matchs[0].Groups[2].Value;   //電話
            }
            else
            {
                FaxNumberModel.Number = faxNumber;
            }

            return FaxNumberModel;
        }

    }
}