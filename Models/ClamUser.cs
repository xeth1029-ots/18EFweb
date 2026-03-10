using System;
using System.Collections.Generic;
using System.Web;
using eYVTR_mng_n.Models.Entities;
using Turbo.DataLayer;

namespace eYVTR_mng_n.Models
{
    public class ClamUser: TblCLAMDBURM
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