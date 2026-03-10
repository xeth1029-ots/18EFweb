using WDAIIP.WEB.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models
{
    public class eYVTRmngRoleFunc : TblE_FUN
    {
        /// <summary>
        /// 項次(已排序)
        /// </summary>
        public int RNUM { get; set; }

        /// <summary>
        /// 類別: 1.選單TreeNode(沒有 action path), 0.選單功能連結項目 (有 action path)
        /// </summary>
        [NotDBField]
        public string NODE { get; set; }
    }
}