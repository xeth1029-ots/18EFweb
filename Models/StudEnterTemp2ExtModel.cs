using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    public class StudEnterTemp2ExtModel : TblSTUD_ENTERTEMP2
    {
        /// <summary>
        /// e網報名課程代碼集合(xx,xx,xx...)
        /// </summary>
        public string E_OCIDS { get; set; }

        /// <summary>
        /// tims網路報名課程代碼集合（含現場報名）
        /// </summary>
        public string TIMS_OCIDS { get; set; }

        /// <summary>
        /// 是否有網路報名資料
        /// </summary>
        public string HasEnterTemp { get; set; }

    }
}