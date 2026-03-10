using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models
{
    [Serializable]
    public class PagingViewModel
    {
        public PagingViewModel() { }

        public string Result { get; set; }

        public PaginationInfo PagingInfo { get; set; }

        public string Rid { get; set; }

        /// <summary>
        /// 是否要重新設定 rid，前端換頁用。
        /// </summary>
        public bool IsResetRid { get; set; }

    }

}