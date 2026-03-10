using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 地圖顯示區域
    /// </summary>
    [Serializable]
    public class MapSearchBounds
    {
        /// <summary>
        /// X座標左邊界(最小值)
        /// </summary>
        public double left { get; set; }
        /// <summary>
        /// X座標右邊界(最大值)
        /// </summary>
        public double right { get; set; }
        /// <summary>
        /// Y座標上邊界(最大值)
        /// </summary>
        public double top { get; set; }
        /// <summary>
        /// Y座標下邊界(最小值)
        /// </summary>
        public double bottom { get; set; }
    }
}