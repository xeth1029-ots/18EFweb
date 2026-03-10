using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 產業人才投資計畫招訓簡章 Model
    /// </summary>
    public class WGReportModel
    {
        public WGReportModel() { }

        /// <summary> 列印表單代碼（ireport）</summary>
        public string PRINTFILENAME
        {
            get
            {
                string rtn = "";
                //string printG = "SD024ONG"; string printW = "SD024ONW";
                rtn = ("G".Equals(this.ORGKINDGW) ? "SD024ONG" : "SD024ONW");
                return rtn;
            }
        }

        /// <summary> 計畫代碼 </summary>
        public Int64? PLANID { get; set; }

        /// <summary> 班級代碼</summary>
        public Int64? OCID { get; set; }

        /// <summary> 計畫單位關係代碼 </summary>
        public string RID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PCSVALUE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PCSV { get; set; }

        /// <summary>
        /// 班級名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 開訓日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 開訓日(西元年yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日(西元年yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>
        /// 機構名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 計畫資料異動日
        /// </summary>
        public DateTime? PLANMODIFYDATE { get; set; }

        /// <summary>
        /// 班級資料異動日
        /// </summary>
        public DateTime? CLASSMODIFYDATE { get; set; }

        /// <summary>
        /// 計畫資料異動時間（ssmmHH） + 班級資料異動時間（ssmmHH）
        /// </summary>
        public string PMD
        {
            get
            {
                string rtn = "";

                if (this.PLANMODIFYDATE.HasValue)
                {
                    rtn = this.PLANMODIFYDATE.Value.ToString("ssmmHH");
                }

                if (this.CLASSMODIFYDATE.HasValue)
                {
                    rtn += this.CLASSMODIFYDATE.Value.ToString("ssmmHH");
                }

                return rtn;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ORGKINDGW { get; set; }
    }
}