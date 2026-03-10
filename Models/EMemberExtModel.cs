using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{

    /// <summary>
    /// 學員資料(含補充)-Ext Convert_E_Member
    /// </summary>
    public class EMemberExtModel : TblE_MEMBER
    {
        /// <summary>
        /// 出生日期 (max stud_studentinfo.birth)
        /// </summary>
        public DateTime? SBIRTHDAY { get; set; }

        /// <summary>
        /// 會員出生日 (西元年 yyyy/MM/dd)
        /// </summary>
        public string MEM_BIRTH_AD
        {
            get { return MyHelperUtil.DateTimeToLongString(this.MEM_BIRTH); }
        }

        /// <summary>
        /// 會員出生日 (民國年 yyy/MM/dd)
        /// </summary>
        public string MEM_BIRTH_TW
        {
            get { return MyHelperUtil.DateTimeToLongTwString(this.MEM_BIRTH); }
        }

        /// <summary>
        /// 身心障礙者
        /// </summary>
        public string HANDTYPEYN { get; set; }

        /// <summary>
        /// 身心障礙者-(1:舊制/2:新制)
        /// </summary>
        public string HANDTYPENUM { get; set; }
    }
}