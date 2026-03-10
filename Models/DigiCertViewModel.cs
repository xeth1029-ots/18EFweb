using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.Services;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace WDAIIP.WEB.Models
{
    #region DigiCertViewModel
    [Serializable]
    public class DigiCertViewModel
    {
        public DigiCertViewModel()
        {
            this.Form = new DigiCertFormModel();
            this.PForm = new DigiCertPageFormModel();
            this.DPForm = new DigiCertDLPageFormModel();
            this.Detail = new DigiCertDetailModel();
        }

        /// <summary>DigiCertFormModel</summary>
        public DigiCertFormModel Form { get; set; }

        public DigiCertPageFormModel PForm { get; set; }

        public DigiCertDLPageFormModel DPForm { get; set; }

        /// <summary>DigiCertDetailModel</summary>
        public DigiCertDetailModel Detail { get; set; }

        /// <summary>查詢結果</summary>
        public IList<DigiCertGridModel> Grid { get; set; }

        /// <summary>查詢結果</summary>
        public IList<DigiCertApplyGridModel> Grid2 { get; set; }


        /// <summary>申請用途 清單來源</summary>
        public IList<SelectListItem> PURPOSE_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetPURPOSExList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 使用單位 清單來源
        /// </summary>
        public IList<SelectListItem> USAGEUNIT_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetUSAGEUNITxList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public void Valid(ModelStateDictionary modelState)
        {
            //throw new NotImplementedException(); //string sDate = string.Empty; //bool blCheckIDNO = true;
            string msg = string.Empty;

            #region 調整欄位值(trim, 全型半型轉換...)
            this.Form.CNAME = (!string.IsNullOrEmpty(this.Form.CNAME)) ? HttpUtility.HtmlDecode(this.Form.CNAME.Trim()) : "";
            this.Form.IDNO = (!string.IsNullOrEmpty(this.Form.IDNO)) ? this.Form.IDNO.Trim() : "";
            this.Form.PURID = (!string.IsNullOrEmpty(this.Form.PURID)) ? this.Form.PURID.Trim() : "";
            this.Form.UAGID = (!string.IsNullOrEmpty(this.Form.UAGID)) ? this.Form.UAGID.Trim() : "";
            this.Form.EMAIL = (!string.IsNullOrEmpty(this.Form.EMAIL)) ? this.Form.EMAIL.Trim() : "";
            this.Form.EMVCODE = (!string.IsNullOrEmpty(this.Form.EMVCODE)) ? this.Form.EMVCODE.Trim() : "";
            this.Form.ValidateCode = (!string.IsNullOrEmpty(this.Form.ValidateCode)) ? this.Form.ValidateCode.Trim() : "";
            //相關輸入欄位字母全型轉半型
            this.Detail.IDNO = MyHelperUtil.ChangeIDNO(this.Detail.IDNO);
            #endregion

            #region 資料欄位檢核
            if (string.IsNullOrWhiteSpace(this.Form.CNAME)) modelState.AddModelError("", "請輸入姓名");
            if (string.IsNullOrWhiteSpace(this.Form.IDNO)) modelState.AddModelError("", "請輸入身分證號");
            if (string.IsNullOrWhiteSpace(this.Form.PURID)) modelState.AddModelError("", "請選擇 申請用途");
            if (string.IsNullOrWhiteSpace(this.Form.UAGID)) modelState.AddModelError("", "請選擇 申請證明要提供的使用單位");
            if (string.IsNullOrWhiteSpace(this.Form.EMAIL)) modelState.AddModelError("", "請輸入電子郵件");
            if (string.IsNullOrWhiteSpace(this.Form.EMVCODE)) modelState.AddModelError("", "請輸入電子郵件驗證碼");
            if (string.IsNullOrWhiteSpace(this.Form.ValidateCode)) modelState.AddModelError("", "請輸入 圖形驗證碼");
            #endregion

            if (!string.IsNullOrWhiteSpace(this.Form.EMVCODE))
            {
                var dao = new WDAIIPWEBDAO();
                var dc1 = this.Form.DCASENO;
                var evc1 = this.Form.EMVCODE;
                var em1 = this.Form.EMAIL;
                // 取得資料(案號資料)
                //var wECode1 = dao.GetRow(new TblE_EMAILCODE { DCASENO = dc1, EMVCODE = evc1, CHECKS = "0" });
                var wECode1 = dao.GetRow(new TblE_EMAILCODE { DCASENO = dc1, EMVCODE = evc1 });
                if (wECode1 == null)
                {
                    modelState.AddModelError("", string.Concat("電子郵件驗證碼有誤! ", evc1));
                    return;
                }

                //if (!string.IsNullOrEmpty(oECode1.EMAIL) && oECode1.EMAIL != em1) {
                //    modelState.AddModelError("", "電子郵件驗證碼與電子郵件資料不符!");
                //    return;
                //}

                if (wECode1 != null)
                {
                    DateTime aNow = (new MyKeyMapDAO()).GetSysDateNow();
                    var oECode1 = new TblE_EMAILCODE { CHECKS = "1", E_UDATE = aNow };
                    dao.Update(oECode1, wECode1);
                }
            }

            //string errmsg_ZIPCODE2 = MyCommonUtil.CHK_ZIPCODE(this.Detail.ZIPCODE2, this.Detail.ZIPCODE2_2W, "戶籍地址(縣市)", true);
            //if (!string.IsNullOrEmpty(errmsg_ZIPCODE2)) { modelState.AddModelError("ZIPCODE2", errmsg_ZIPCODE2); }
            //else if (!string.IsNullOrEmpty(this.Detail.ZIPCODE2))
            //{
            //    string s_zipname = (new MyKeyMapDAO()).GetZipName(this.Detail.ZIPCODE2);
            //    if (string.IsNullOrEmpty(s_zipname)) { modelState.AddModelError("ZIPCODE2", string.Concat("戶籍地址(縣市)", "郵遞區號前3碼", "鍵詞範圍有誤!")); }
            //}
        }

        public static void CheckArgument(HttpContextBase httpContext)
        {
            string cst_str_s1 = string.Empty;
            NameValueCollection parms = httpContext.Request.Params;

            foreach (var key in parms.AllKeys)
            {
                cst_str_s1 = "TRCSN";
                if (key.Contains(cst_str_s1))
                {
                    string value = parms[key];
                    //if (value.Contains(',')) { value = value.Replace(',', '0'); }
                    Regex patterns = new Regex("^[0-9]+$");
                    if (!patterns.IsMatch(value))
                    {
                        throw new ArgumentException("DigiCertViewModel.." + cst_str_s1);
                    }
                }

                cst_str_s1 = "OCID";
                if (key.Contains(cst_str_s1))
                {
                    string value = parms[key]; //string value = "," + parms[key];
                    //if (value.Contains(',')) { value = value.Replace(',', '0'); }
                    Regex patterns = new Regex("^[0-9]+$");
                    if (!patterns.IsMatch(value))
                    {
                        throw new ArgumentException("DigiCertViewModel.." + cst_str_s1);
                    }
                }

                cst_str_s1 = "SELECTIS";
                if (key.Contains(cst_str_s1))
                {
                    //string value = "," + parms[key].ToUpper();
                    //Regex patterns = new Regex("^(,TRUE,FALSE|,FALSE,FALSE|,FALSE)+$");
                    string value = parms[key].ToUpper();
                    Regex patterns = new Regex("^(TRUE,FALSE|FALSE)+$");
                    if (!patterns.IsMatch(value))
                    {
                        throw new ArgumentException("DigiCertViewModel.." + cst_str_s1);
                    }
                }
            }

        }
    }
    #endregion



    #region DigiCertFormModel
    [Serializable]
    public class DigiCertFormModel : TblSTUD_DIGICERTAPPLY
    {
        public DigiCertFormModel() { }

        /// <summary>
        /// 將 DateTime 轉換為 民國年日期字串(YYY/MM/dd), 若date為Null則回傳 null,
        /// delimiter 參數用來指定民國年的日期的分隔字元, 預設是 '/'
        /// </summary>
        public static string TransToDateRoc(DateTime? date, string delimiterY = "年", string delimiterM = "月", string delimiter = "日")
        {
            if (date == null || !date.HasValue) { return null; }
            int twYear = (date.Value.Year > 1911) ? date.Value.Year - 1911 : date.Value.Year;
            return string.Concat(twYear, delimiterY, date.Value.ToString("MM"), delimiterM, date.Value.ToString("dd"), delimiter);
        }

        /// <summary>申請日期</summary>
        [Display(Name = "申請日期")]
        public string APPLNDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.APPLNDATE); }
        }

        /// <summary>申請日期</summary>
        [Display(Name = "申請日期")]
        public string APPLNDATE_ROC
        {
            get { return TransToDateRoc(this.APPLNDATE); }
        }

        /// <summary>驗證碼</summary>
        [Display(Name = "圖形驗證碼")]
        public string ValidateCode { get; set; }

        /// <summary>證書狀態</summary>
        [Display(Name = "證書狀態")]
        public string CERTSTATUS
        {
            get
            {
                return string.IsNullOrEmpty(this.GUID1) ? "" : (string.IsNullOrEmpty(this.DCASENO) ? "無效" : "有效");
            }
        }
        ///// <summary>驗證碼</summary>
        //[Display(Name = "驗證碼")]
        //public string HashValidateCode { get; set; }

        /// <summary>身分證字號</summary>
        [Display(Name = "身分證編號")]
        public string IDNO_MK
        {
            get
            {
                //return (new MyKeyMapDAO()).GET_IDNO_MK(this.IDNO);
                if (string.IsNullOrEmpty(this.IDNO)) { return ""; }
                if (this.IDNO.Length < 10) { return "*******"; }
                return string.Concat(IDNO.Substring(0, 3), "******", IDNO.Substring(9, 1));
            }
        }
        /// <summary>姓名</summary>
        [Display(Name = "申請人姓名")]
        public string CNAME_MK
        {
            get
            {
                //return (new MyKeyMapDAO()).GET_CNAME_MK(this.CNAME);
                if (string.IsNullOrEmpty(this.CNAME)) { return ""; }
                if (this.IDNO.Length < 3) { return "＊＊＊"; }
                return string.Concat(CNAME.Substring(0, 1), "＊＊");
            }
        }
    }

    [Serializable]
    public class DigiCertPageFormModel : PagingResultsViewModel
    {
        public DigiCertPageFormModel() { }
        public decimal? TRC_MSN { get; set; }
        public long? DCANO { get; set; }
        public string DCASENO { get; set; }
        public string CNAME { get; set; }
        public string IDNO { get; internal set; }
        public int? CLASSCNT { get; set; }
    }

    [Serializable]
    public class DigiCertDLPageFormModel : PagingResultsViewModel
    {
        public DigiCertDLPageFormModel() { }
        public decimal? TRC_MSN { get; set; }
        public string CNAME { get; set; }
        public string IDNO { get; internal set; }
    }
    #endregion

    #region DigiCertDetailModel
    [Serializable]
    public class DigiCertDetailModel : TblSTUD_DIGICERTAPPLY
    {
        public DigiCertDetailModel() { }
    }
    #endregion

    #region DigiCertGridModel
    [Serializable]
    public class DigiCertGridModel
    {
        /// <summary>選取Checkbox</summary>
        public bool SELECTIS { get; set; }
        /// <summary>開班流水號</summary>
        public long? OCID { get; set; }
        public long? SOCID { get; set; }
        public string TPLANID { get; set; }
        /// <summary>班級名稱+期別</summary>
        public string CLASSCNAME { get; set; }
        public string DISTID { get; set; }
        public string DISTNAME { get; set; }
        public string YEARS { get; set; }
        public string ROC_YEAR { get; set; }
        public string PLANNAME { get; set; }
        public string ORGNAME { get; set; }
        public int? THOURS { get; set; }
        public DateTime? STDATE { get; set; }
        public DateTime? FTDATE { get; set; }
        public short? STUDSTATUS { get; set; }
        public string STUDSTATUS_N { get; set; }
        public string ISCHECK { get; set; }
        public int? CREDITPOINTS { get; set; }
    }
    #endregion

    [Serializable]
    public class DigiCertApplyGridModel : TblSTUD_DIGICERTAPPLY
    {
        /// <summary>選取Checkbox</summary>
        public bool SELECTIS { get; set; }
        public string ISCHECK { get; set; }
    }

    [Serializable]
    public class DigiCertRptModel
    {
        /// <summary>報表名稱</summary>
        public string PRINTFILENAME
        {
            get
            {
                return (RPT == "D" ? "OJTSD14031D" : "OJTSD14031C");
            }
        }
        /// <summary>報表種類 C:封面, D:資料</summary>
        public string RPT { get; set; }
        public long? DCANO { get; set; }
        public string DCASENO { get; set; }
        public string EMVCODE { get; set; }
    }

}