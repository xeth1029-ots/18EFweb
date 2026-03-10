using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    public class HomeNews3ExtModel : TblHOME_NEWS3
    {
        /// <summary>
        /// 
        /// </summary>
        public string MODIFYNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string STOPDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STOPSDATE); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string STOPSDATE_TWFULL {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.STOPSDATE); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string STOPEDATE_TWFULL {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.STOPEDATE); }
        }


    }
}