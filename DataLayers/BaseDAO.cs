using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.DataLayers
{
    /// <summary>
    /// 原本 BaseDAO 的內容, 已拉到 Turbo.DataLayer.RowBaseDAO,
    /// 保留這個class以維持原有程式相容性
    /// </summary>
    public class BaseDAO : RowBaseDAO
    {
        /// <summary>
        /// 以預設的 SqlMap config 連接資料庫
        /// </summary>
        public BaseDAO(): base()
        {
            base.PageSize = ConfigModel.DefaultPageSize;
        }

        /// <summary>
        /// 以指定的 SqlMap config 連接資料庫
        /// </summary>
        /// <param name="sqlMapConfig"></param>
        public BaseDAO(string sqlMapConfig) : base(sqlMapConfig)
        {
            base.PageSize = ConfigModel.DefaultPageSize;
        }

    }
}