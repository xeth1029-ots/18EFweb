using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    #region HomeViewModel
    /// <summary>
    /// 首頁 View Model
    /// </summary>
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            this.Form = new HomeFormModel();
        }

        /// <summary>
        /// 系統公告訊息
        /// </summary>
        public NoticeDetailModel Notice { get; set; }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public HomeFormModel Form { get; set; }

        /// <summary>
        /// 首頁大Banner資訊
        /// </summary>
        public BannerGridModel BigBanner { get; set; }

        /// <summary>
        /// 首頁大Banner清單資訊
        /// </summary>
        public IList<BannerGridModel> BigBannerGrid { get; set; }

        /// <summary>
        /// 首頁Banner清單資訊
        /// </summary>
        public IList<BannerGridModel> BannerGrid { get; set; }

        /// <summary>焦點新聞清單 GridModel</summary>
        public IList<TopContentGridModel> TopNewsGrid { get; set; }

        /// <summary>計畫公告清單 GridModel</summary>
        public IList<TopContentGridModel> TopPlanGrid { get; set; }

        /// <summary>最多分享課程清單 GridModel</summary>
        public IList<TopShareClassGridModel> TopShareClassGrid { get; set; }
        
        /// <summary>政策性課程專區-Policy course area</summary>
        public IList<PolicyClassGridModel> PolicyClassGrid { get; set; }

        /// <summary>歷史政策性課程-Historical policy course</summary>
        public IList<PolicyClassGridModel> PolicyClassHisGrid { get; set; }

        /// <summary>宣導影片</summary>
        public TopContentGridModel Film { get; set; }

        /// <summary>查詢計畫類型 清單來源</summary>
        public IList<SelectListItem> PlanType_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "2","分署自辦在職訓練" },{ "1","產業人才投資方案" },{ "5","區域產業據點" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 上課地點(產投-縣市) 清單來源
        /// </summary>
        public IList<SelectListItem> CTID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 上課地點(分署)
        /// </summary>
        public IList<SelectListItem> DISTID_list
        {
            get
            {
                //etraining直接寫死
                var dictionary = new Dictionary<string, string>
                {
                    {"001","北基宜花金馬分署" },
                    {"003","桃竹苗分署" },
                    {"004","中彰投分署" },
                    {"005","雲嘉南分署" },
                    {"006","高屏澎東分署" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }
    }
    #endregion

    #region HomeFormModel
    public class HomeFormModel : ClassSearchFormModel
    {

    }
    #endregion

    #region BannerGridModel
    public class BannerGridModel : TblTB_BANNER
    {

    }
    #endregion

    #region TopContentGridModel
    public class TopContentGridModel : TblTB_CONTENT
    {
        /// <summary>
        /// 標題(擷取顯示部份資訊)
        /// </summary>
        public string C_TITLE_TEXT
        {
            get
            {
                string result = this.C_TITLE;

                if (!string.IsNullOrEmpty(this.C_TITLE))
                {
                    if (this.C_TITLE.Length >= 25)
                    {
                        result = this.C_TITLE.Substring(0, 25) + "...";
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 上架日(西元年 yyyy/MM/dd)
        /// </summary>
        public string C_SDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.C_SDATE); }
        }

        /// <summary>
        /// 上架日 (西元年 yyy/MM/dd)
        /// </summary>
        public string C_SDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.C_SDATE); }
        }
    }
    #endregion

    #region TopShareClassGridModel
    public class TopShareClassGridModel
    {
        /// <summary>
        /// 訓練計畫代碼(ref:ID_PLAN.TPLANID)
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public decimal? OCID { get; set; }

        /// <summary>
        /// 班別中文名稱
        /// </summary>
        public string CLASSCNAME { get; set; }
    }
    #endregion

    #region PolicyClassGridModel
    public class PolicyClassGridModel
    {
        /// <summary>
        /// 訓練計畫代碼(ref:ID_PLAN.TPLANID)
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public decimal? OCID { get; set; }

        /// <summary> 班別中文名稱 </summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 班別中文名稱+期別 </summary>
        public string CLASSCNAME2 { get; set; }
    }
    #endregion

    #region NewClassGridModel
    public class NewClassGridModel : TblCLASS_CLASSINFO
    {
        /// <summary>
        /// 課程名稱(擷取顯示部份資訊)
        /// </summary>
        public string CLASSCNAME_TEXT
        {
            get
            {
                string result = this.CLASSCNAME;

                if (!string.IsNullOrEmpty(this.CLASSCNAME))
                {
                    if (this.CLASSCNAME.Length >= 25)
                    {
                        result = this.CLASSCNAME.Substring(0, 25) + "...";
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 學科訓練地點
        /// </summary>
        public string CTNAME1 { get; set; }

        /// <summary>
        /// 術科訓練地點
        /// </summary>
        public string CTNAME2 { get; set; }

        /// <summary>
        /// 開訓日(西元年 yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日(西元年 yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>
        /// 報名日期起日(西元年 yyyy/MM/dd HH24:mi:ss)
        /// </summary>
        //public string SENTERDATE_AD
        //{
        //    get { return MyHelperUtil.DateTimeToADFormatLongString(this.SENTERDATE); }
        //}
        public string SENTERDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名日期迄日(西元年 yyyy/MM/dd HH24:mi:ss)
        /// </summary>
        public string FENTERDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FENTERDATE); }
        }

        /// <summary>
        /// 政府負擔
        /// </summary>
        public string DEFGOVCOST { get; set; }

        /// <summary>
        /// 學員負擔
        /// </summary>
        public string DEFSTDCOST { get; set; }

        /// <summary>
        /// 報名繳費方式
        /// </summary>
        public string ENTERSUPPLYSTYLE { get; set; }


    }
    #endregion

    #region NoticeDetailModel
    public class NoticeDetailModel : TblTB_CONTENT
    {

    }
    #endregion
}