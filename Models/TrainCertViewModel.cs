using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    #region TrainCertViewModel

    /// <summary> TrainCert 下載訓練證明 ViewModel </summary>
    [Serializable]
    public class TrainCertViewModel
    {
        /// <summary> 建構子 </summary>
        public TrainCertViewModel()
        {
            this.Form = new TrainCertFormModel();
        }

        /// <summary>查詢條件</summary>
        public TrainCertFormModel Form { get; set; }

        /// <summary>查詢結果清單</summary>
        public IList<TrainCertGridModel> Grid { get; set; }
    }

    #endregion

    #region TrainCertFormModel
    [Serializable]
    public class TrainCertFormModel : PagingResultsViewModel
    {
        /// <summary> 身分證字號 </summary>
        public string IDNO { get; set; }

        /// <summary> 生日 YYYY/MM/DD </summary>
        public string BIRTHDAY { get; set; }

        public static bool CheckArgument(HttpContextBase context)
        {
            NameValueCollection parms = context.Request.Params;
            bool result = true;
            try
            {
                var props = new string[] { "IDNO", "BIRTHDAY" };

                var match = (from a in props
                             from b in parms.AllKeys
                             where b.Contains(a)
                             select b).ToList();

                if (match == null || match.Count == 0)
                {
                    throw new ArgumentException("TrainCertFormModel");
                }

                foreach (var key in parms.AllKeys)
                {
                    if (key.Contains("QueType"))
                    {
                        string value = parms[key];
                        if (value != "!" && value != "2")
                        {
                            throw new ArgumentException("StudQuestionViewModel.QueType");
                        }
                    }
                }

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
    }
    #endregion

    #region TrainCertGridModel
    /// <summary>查詢結果清單</summary>
    [Serializable]
    public class TrainCertGridModel
    {
        /// <summary> 班級代碼OCID </summary>
        public Int64? OCID { get; set; }

        /// <summary> 班級類別代碼</summary>
        public Int64? CLSID { get; set; }

        /// <summary> 班級學員序號 </summary>
        public Int64? SOCID { get; set; }

        /// <summary>format(cs.MODIFYDATE,'ssmmdd') TMD</summary>
        public string TMD { get; set; }

        /// <summary> IDNO </summary>
        public string IDNO { get; set; }

        /// <summary> 生日 yyyy/MM/dd </summary>
        public string BIRTHDAY { get; set; }

        /// <summary> 姓名 </summary>
        public string STDNAME { get; set; }
        /// <summary> 學號 </summary>
        public string STUDID2 { get; set; }

        /// <summary> 系統日期時間 </summary>
        public DateTime? ATODAY { get; set; }

        /// <summary> 系統日 yyyy/MM/dd </summary>
        public string ATODAY_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.ATODAY); }
        }
        /// <summary> 機構名 </summary>
        public string ORGNAME { get; set; }
        /// <summary> 轄區分署名 </summary>
        public string DISTNAME { get; set; }

        /// <summary> 學員狀態代碼 (Default 1 ； 1.在訓 2.離訓 3.退訓 4.續訓 5.結訓)</summary>
        public Decimal? STUDSTATUS { get; set; }

        /// <summary> 狀態 </summary>
        public string STUDSTATUS2 { get; set; }

        /// <summary> 班別中文名稱 </summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 期別 </summary>
        public string CYCLTYPE { get; set; }

        /// <summary> 班別名稱 </summary>
        public string CLASSCNAME2 { get; set; }

        /// <summary> 計畫年度 yyyy </summary>
        public string PLANYEAR { get; set; }

        /// <summary> 計畫年度 (民國年 yyy) </summary>
        public string PLANYEAR_TW
        {
            get
            {
                string rtn = string.Empty;
                if (string.IsNullOrEmpty(PLANYEAR)) return rtn;

                int i_yr = 0;
                if (!int.TryParse(this.PLANYEAR, out i_yr)) return rtn;

                rtn = string.Concat("", i_yr > 1911 ? i_yr - 1911 : i_yr);
                return rtn;
            }
        }

        /// <summary> 開訓日期  </summary>
        public DateTime? STDATE { get; set; }

        /// <summary> 開訓日期 (西元年 yyyy/MM/dd) </summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary> 結訓日期 </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary> 結訓日期 (西元年 yyyy/MM/dd) </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

    }
    #endregion

    //RTE, TMD, SOCID, OCID, CLSID, STUDSTATUS
    public class TrainCertReportModel
    {
        public TrainCertReportModel() { }
        public string PRINTFILENAME
        {
            get { return "ojt_inclass"; }
        }

        public string IDNO { get; set; }
        public string SID { get; set; }

        /// <summary> RTE</summary>
        public string RTE { get; set; }

        /// <summary>format(cs.MODIFYDATE,'ssmmdd') TMD</summary>
        public string TMD { get; set; }

        /// <summary> 班級學員代碼</summary>
        public Int64? SOCID { get; set; }

        /// <summary> 班級代碼</summary>
        public Int64? OCID { get; set; }

        /// <summary> 班級類別代碼</summary>
        public Int64? CLSID { get; set; }

        /// <summary> 班級學員狀態代碼</summary>
        public Decimal? STUDSTATUS { get; set; }

    }


}