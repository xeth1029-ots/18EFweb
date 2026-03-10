using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 完整地址 的 View/Data Model
    /// </summary>
    public class AddressModel
    {
        /// <summary>
        /// 3碼郵遞區號
        /// </summary>
        public string Zip { get; set; }
        public string ZipText { get; set; }

        /// <summary>
        /// 縣市
        /// </summary>
        public string City { get; set; }

        public string CityText { get; set; }

        /// <summary>
        /// 鄉鎮市區
        /// </summary>
        public string Town { get; set; }

        public string TownText { get; set; }

        /// <summary>
        /// 路段,號碼 等
        /// </summary>
        public string Address { get; set; }
    }
}