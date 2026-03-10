using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 擴充 TblCLASS_CLASSINFO 
    /// </summary>
    public class ClassClassInfoExtModel : TblCLASS_CLASSINFO
    {
        /// <summary>
        /// 
        /// </summary>
        public ClassClassInfoExtModel() { }

        /// <summary>
        /// 大計畫代碼
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary> 班級名稱 </summary>
        public string CLASSNAME { get; set; }

        /// <summary> 班級名稱（含期別） </summary>
        public string CLASSNAME2 { get; set; }

        /// <summary> 班級名稱（含期別） </summary>
        public string CLASSCNAME2 { get; set; }

        /// <summary>
        /// 報名起日 (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string SENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日 (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string FENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.FENTERDATE); }
        }

        /// <summary>
        /// 訓練起日 (西元年 yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>
        /// 訓練起日（民國年 yyy/MM/dd）
        /// </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary>
        /// 報名迄日 (西元年 yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>
        /// 報名迄日(民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        /// <summary>
        /// 經費來源-政府負擔金額
        /// </summary>
        public string CLSDEFGOVCOST { get; set; }

        /// <summary>
        /// 總費用
        /// </summary>
        public string CLSTTLCOST { get; set; }

        /// <summary>
        /// 訓練職類代碼
        /// </summary>
        public string TRAINID { get; set; }

        /// <summary>
        /// 訓練職類名稱
        /// </summary>
        public string TRAINNAME { get; set; }

        /// <summary>
        /// 單位代碼 (ref:org_orginfo)
        /// </summary>
        public decimal? ORGID { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 會員身分證
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 受訓資格-年齡起
        /// </summary>
        public int? CAPAGE1 { get; set; }

        /// <summary>
        /// 受訓資格-年齡迄
        /// </summary>
        public int? CAPAGE2 { get; set; }

        /// <summary>
        /// 受訓資格-學歷代碼 (ref Key_Degree.DEGREEID)
        /// </summary>
        public string CAPDEGREE { get; set; }

        /// <summary>
        /// 受訓資格-學歷中文描述
        /// </summary>
        public string CAPDEGREE_TEXT { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PLANYEAR { get; set; }

    }
}