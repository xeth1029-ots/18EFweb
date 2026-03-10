using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 線上說明 Data Model
    /// </summary>
    public class OnlineHelpModel
    {
        /// <summary>
        /// 是否發生錯誤
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 說明檔標題
        /// </summary>
        public string HelpHeader { get; set; }

        /// <summary>
        /// 說明檔案內容
        /// </summary>
        public string HelpContent { get; set; }

    }
}