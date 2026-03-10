using WDAIIP.WEB.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models
{
    public class eYVTRmngUser : TblSYS_USER
    {
        /// <summary>
        /// 最後變更密碼的時間(民國年格式字串: yyyMMddHHMMss)
        /// </summary>
        [NotDBField]
        public string LAST_PW_CHG_DT { get; set; }

        /// <summary>
        /// 最後一次登入時間(民國年格式字串: yyyMMddHHMMss)
        /// </summary>
        [NotDBField]
        public string LAST_LOGIN_DT { get; set; }

        /// <summary>
        /// 使用者所屬單位名稱
        /// </summary>
        [NotDBField]
        public string UNIT_NAME { get; set; }
    }
}