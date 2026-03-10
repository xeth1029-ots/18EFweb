using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models
{
    #region ClassMatchListViewModel
    public class ClassMatchListViewModel
    {
        public ClassMatchListViewModel()
        {
            this.Form = new ClassMatchListFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public ClassMatchListFormModel Form { get; set; }

        /// <summary>
        /// 產業人才投資方案-速配課程結果列
        /// </summary>
        public IList<ClassMatchGrid1Model> Grid1 { get; set; }

        /// <summary>
        /// 自辦在職進修-速配課程結果列
        /// </summary>
        public IList<ClassMatchGrid2Model> Grid2 { get; set; }
    }
    #endregion

    #region ClassMatchListFormModel
    public class ClassMatchListFormModel : PagingResultsViewModel
    {
        public ClassMatchListFormModel()
        {
            this.ProvideLocation = "N";
        }

        /// <summary>
        /// 計畫類型(1.產業人才投資方案 2.自辦在職進修)
        /// </summary>
        public string PlanType { get; set; }

        /// <summary>
        /// 策略性產業別(產投用)
        /// </summary>
        public string TMID { get; set; }

        /// <summary>
        /// 課程地點所屬縣市別(產投用)
        /// </summary>
        public string CTID { get; set; }

        /// <summary>
        /// 分署別(在職用)
        /// </summary>
        public string DISTID { get; set; }

        /// <summary>
        /// 通俗職類別(在職用 ref:class_classinfo.cjob_unkey)
        /// </summary>
        public string CJOBUNKEY { get; set; }

        /// <summary>
        /// 是否公開顯示距離
        /// </summary>
        public string ProvideLocation { get; set; }
    }

    public class ClassMatchGrid1Model : ClassSearchGrid1Model
    {

    }

    public class ClassMatchGrid2Model : ClassSearchGrid2Model
    {

    }
    #endregion
}