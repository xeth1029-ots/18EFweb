using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    public class StudOnlineViewModel
    {
        public StudOnlineViewModel()
        {
            this.Form = new StudOnlineFormModel();

            this.Detail = new StudOnlineDetailModel() { IsNew = false };
        }

        public StudOnlineFormModel Form { get; set; }
        public IList<StudOnlineGridModel> Grid { get; set; }
        public IList<StudOnlineGrid2Model> Grid2 { get; set; }
        public StudOnlineDetailModel Detail { get; set; }

        public void Valid(ModelStateDictionary modelState)
        {
            if (!string.IsNullOrEmpty(this.Detail.CONTACTPHONE)) { this.Detail.CONTACTPHONE = this.Detail.CONTACTPHONE.Trim(); }
            //if (!string.IsNullOrEmpty(this.Detail.ZIPCODE1)) { this.Detail.ZIPCODE1 = this.Detail.ZIPCODE1.Trim(); }
            if (!string.IsNullOrEmpty(this.Detail.ZIPCODE1_2W)) { this.Detail.ZIPCODE1_2W = this.Detail.ZIPCODE1_2W.Trim(); }
            if (!string.IsNullOrEmpty(this.Detail.ZIPCODE1_6W)) { this.Detail.ZIPCODE1_6W = this.Detail.ZIPCODE1_6W.Trim(); }
            //if (!string.IsNullOrEmpty(this.Detail. ZIPCODE1_N)) { this.Detail.ZIPCODE1_N = this.Detail.ZIPCODE1_N.Trim(); }
            if (!string.IsNullOrEmpty(this.Detail.ADDRESS)) { this.Detail.ADDRESS = this.Detail.ADDRESS.Trim(); }
            if (!string.IsNullOrEmpty(this.Detail.ISAGREE)) { this.Detail.ISAGREE = this.Detail.ISAGREE.Trim(); }

            if (string.IsNullOrEmpty(this.Detail.CONTACTPHONE)) { modelState.AddModelError("", "請輸入連絡電話"); }
            if (string.IsNullOrEmpty(this.Detail.ADDRESS)) { modelState.AddModelError("", "請輸入通訊地址"); }
            if (string.IsNullOrEmpty(this.Detail.ISAGREE) || this.Detail.ISAGREE != "Y") { modelState.AddModelError("", "請勾選已閱讀並同意此份文件內容!"); }
        }
    }

    public class StudOnlineFormModel : PagingResultsViewModel
    {
        public string IDNO { get; set; }
        public string BIRTHDAY_STR { get; set; }
        public bool CANVIEW { get; set; }
        public string SOCID { get; set; }
        public string OCID { get; set; }
        public string SID { get; set; }
        public string ELNO { get; set; }
    }

    public class StudOnlineGridModel
    {
        public long? OCID { get; set; }
        public long? SOCID { get; set; }
        /// <summary>學員流水號 ID (yyyymmddhhmmss+兩碼流水號)</summary>
        public string SID { get; set; }
        //public string OCID_enc { get { return ConfigModel.Encodelong(OCID); } }
        //public string SOCID_enc { get { return ConfigModel.Encodelong(SOCID); } }
        //public string SID_enc { get { return ConfigModel.EncodeString(SID); } }
        /// <summary>中文姓名</summary>
        public string CNAME { get; set; }
        public decimal THOURS { get; set; }
        public decimal TNUM { get; set; }
        /// <summary>開訓日期</summary>
        public DateTime? STDATE { get; set; }
        /// <summary>結訓日期</summary>
        public DateTime? FTDATE { get; set; }
        public string ORGNAME { get; set; }
        public string PLANNAME { get; set; }
        /// <summary>班別中文名稱</summary>
        public string CLASSCNAME2 { get; set; }
        /// <summary>身份證字號</summary>
        public string IDNO { get; set; }
        public DateTime? BIRTHDAY { get; set; }
        public string DISTID { get; set; }
        public string DISTNAME { get; set; }
        public string YEARS { get; set; }
        public string YEARS_ROC { get; set; }
        public decimal? STUDSTATUS { get; set; }
        public string STUDSTATUS_N { get; set; }

        /// <summary>開訓日期 (西元年yyyy/MM/dd)</summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>開訓日期 (民國年yyy/MM/dd)</summary>
        public string STDATE_TW
        {
            get { return (STDATE.HasValue ? MyHelperUtil.DateTimeToTwString(STDATE.Value) : ""); }
        }

        /// <summary>結訓日期 (西元年yyyy/MM/dd)</summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>結訓日期 (民國年yyy/MM/dd)</summary>
        public string FTDATE_TW
        {
            get { return (FTDATE.HasValue ? MyHelperUtil.DateTimeToTwString(FTDATE.Value) : ""); }
        }

        /// <summary>系統日</summary>
        public DateTime? ATODAY { get; set; }

        /// <summary>系統日 (西元年yyyy/MM/dd)</summary>
        public string ATODAY_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.ATODAY); }
        }

        /// <summary>
        /// 受訓期間
        /// </summary>
        public string TROUND
        {
            get
            {
                return string.Concat(this.STDATE_TW, "<br>|<br>", this.FTDATE_TW);
            }
        }
    }

    public class StudOnlineGrid2Model : TblKEY_ELFORM
    {
        public long? OCID { get; set; }
        public long? SOCID { get; set; }
        /// <summary>學員流水號 ID (yyyymmddhhmmss+兩碼流水號)</summary>
        public string SID { get; set; }
        public string SEND_N { get; set; }
        public string COMIDNO { get; set; }
        /// <summary>中文姓名</summary>
        public string CNAME { get; set; }
        public long? THOURS { get; set; }
        public long? TNUM { get; set; }
        /// <summary>開訓日期</summary>
        public DateTime? STDATE { get; set; }
        /// <summary>結訓日期</summary>
        public DateTime? FTDATE { get; set; }
        public string ORGNAME { get; set; }
        public string PLANNAME { get; set; }
        /// <summary>班別中文名稱</summary>
        public string CLASSCNAME2 { get; set; }
        /// <summary>身份證字號</summary>
        public string IDNO { get; set; }
        public DateTime? BIRTHDAY { get; set; }
        public string DISTID { get; set; }
        public string DISTNAME { get; set; }

        public int? STUDSTATUS { get; set; }
        public string STUDSTATUS_N { get; set; }

        /// <summary>開訓日期 (西元年yyyy/MM/dd)</summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>開訓日期 (民國年yyy/MM/dd)</summary>
        public string STDATE_TW
        {
            get { return (STDATE.HasValue ? MyHelperUtil.DateTimeToTwString(STDATE.Value) : ""); }
        }

        /// <summary>結訓日期 (西元年yyyy/MM/dd)</summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>結訓日期 (民國年yyy/MM/dd)</summary>
        public string FTDATE_TW
        {
            get { return (FTDATE.HasValue ? MyHelperUtil.DateTimeToTwString(FTDATE.Value) : ""); }
        }

        /// <summary>系統日</summary>
        public DateTime? ATODAY { get; set; }

        /// <summary>系統日 (西元年yyyy/MM/dd)</summary>
        public string ATODAY_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.ATODAY); }
        }
        /// <summary>受訓期間</summary>
        public string TROUND
        {
            get
            {
                return string.Concat(this.STDATE_TW, "<br>|<br>", this.FTDATE_TW);
            }
        }
        public long? CSELNO { get; set; }
        public string P1_LINK { get; set; }
        public string FILEPATH1 { get; set; }
        public string SIGNDACCT { get; set; }
        public DateTime? SIGNDATE { get; set; }
        public string SENDACCT { get; set; }
        public DateTime? SENDDATE { get; set; }
    }

    public class StudOnlineDetailModel : TblSTUD_ENTERTEMP4
    {
        /// <summary>是否為新增模式</summary>
        [NotDBField]
        public bool IsNew { get; set; }

    }

    public class StudOnlineReportModel
    {
        public string TPLANID { get; set; }
        public long? OCID { get; set; }
        public string ORGKINDGW { get; set; }
        public string RID { get; set; }
        public long? SOCID { get; set; }
        public string SID { get; set; }
        public string YEARS { get; set; }
        public string YEARS_ROC { get; set; }
        public string RTE { get; set; }
        public string ELNO { get; set; }

        /// <summary>產生報表名稱</summary>
        public string PRINTFILENAME1
        {
            get
            {
                //下載空白文件:T "下載簽署文件:D
                string rst = null;
                switch (ELNO)
                {
                    case "1":
                        //補助學員參訓契約書 SD_14_021G3/SD_14_021W3/OJTSD1421
                        if (RTE == "T")
                        {
                            rst = ("G".Equals(this.ORGKINDGW) ? "OJTSD1421G3" : "OJTSD1421W3");
                        }
                        else if (RTE == "D")
                        {
                            rst = ("G".Equals(this.ORGKINDGW) ? "OJTSD1421G3S" : "OJTSD1421W3S");
                        }
                        break;
                    case "2":
                        //學員基本資料表 //學員資料表 SD_14_005_2021 /OJTSD1405B1
                        if (RTE == "T")
                        {
                            rst = "OJTSD1405B1";
                        }
                        else if (RTE == "D")
                        {
                            rst = "OJTSD1405B1S";
                        }
                        break;
                    case "3":
                        //學員補助申請書 SD_14_013_2018G/SD_14_013_2018W /OJTSD140138
                        if (RTE == "T")
                        {
                            rst = ("G".Equals(this.ORGKINDGW) ? "OJTSD140138G" : "OJTSD140138W");
                        }
                        else if (RTE == "D")
                        {
                            rst = ("G".Equals(this.ORGKINDGW) ? "OJTSD140138GS" : "OJTSD140138WS");
                        }
                        break;
                }
                return rst;
            }
        }
    }
}
