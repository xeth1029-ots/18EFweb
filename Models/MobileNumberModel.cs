using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 用來表示一組完整的 手機號碼, 
    /// 例如: 0911-111111 可轉換為 MobileNumberModel { AreaCode=0911, Number=111111}
    /// </summary>
    public class MobileNumberModel
    {
        /// <summary>
        /// 前4碼
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// 後6碼
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 將 MobileNumberModel 物件以格式化字串輸出, 例: 0911-111111
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
        private static string pattern1 = "([0-9]*)-([0-9]*)";          /* 內容格式為: 0911-111111 */
        private static Regex rgx1 = new Regex(pattern1, RegexOptions.Compiled);
        #endregion

        /// <summary>
        /// 將完整格式的電話號碼字串, 轉成 MobileNumberModel 物件回傳,  
        /// 例如: 02-27769993 可轉換為 MobileNumberModel { AreaCode=02, Number=27769993},
        /// 若 Parse 失敗, MobileNumberModel.Number 會填入傳入的 mobileNumber 字串 
        /// </summary>
        /// <param name="mobileNumber">完整格式的電話號碼字串, 如:0911-111111</param>
        /// <returns>MobileNumberModel 物件</returns>
        public static MobileNumberModel Parse(string mobileNumber)
        {
            MobileNumberModel MobileNumberModel = new MobileNumberModel();
            if (string.IsNullOrEmpty(mobileNumber))
            {
                return MobileNumberModel;
            }

            //聯絡人電話(日)
            MatchCollection matchs;
            if ((matchs = rgx1.Matches(mobileNumber)) != null && matchs.Count > 0)
            {
                MobileNumberModel.AreaCode = matchs[0].Groups[1].Value;   //前4碼
                MobileNumberModel.Number = matchs[0].Groups[2].Value;   //後6碼
            }
            else
            {
                MobileNumberModel.Number = mobileNumber;
            }

            return MobileNumberModel;
        }

    }
}