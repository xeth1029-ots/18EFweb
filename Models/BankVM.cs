using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>金融機構</summary>
    public class BankVM
    {
        /// <summary> 下拉選項顯示文字 </summary>
        public string Text { get; set; }

        /// <summary> 銀行代碼(CODE) </summary>
        public string BnakCode { get; set; }

        /// <summary> 銀行名稱(DESCR) </summary>
        public string BankName { get; set; }

        /// <summary> 分行代碼(CODE) </summary>
        public string BranchCode { get; set; }

        /// <summary> 分行名稱(DESCR) </summary>
        public string BranchName { get; set; }

        /// <summary> 分行-銀行代碼(APK1) </summary>
        public string APK1 { get; set; }       

    }
}
