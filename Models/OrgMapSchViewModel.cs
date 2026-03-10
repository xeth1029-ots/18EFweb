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
using System.Collections.Specialized;
using System.Text.RegularExpressions;


namespace WDAIIP.WEB.Models
{
    [Serializable]
    public class OrgMapSchViewModel
    {
        public OrgMapSchViewModel()
        {
            this.Form = new OrgMapSchFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public OrgMapSchFormModel Form { get; set; }


        /// <summary>
        /// 查詢結果清單(for 產投) Grid1Model
        /// </summary>
        public IList<OrgMapSchGrid1Model> Grid1 { get; set; }

        /// <summary>
        /// 查詢計畫類型 清單來源
        /// 2018-12-26 add區域產業據點(tplanid=70)
        /// </summary>
        public IList<SelectListItem> PlanType_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "2","分署自辦在職訓練" },
                    { "1","產業人才投資方案" },
                    { "5","區域產業據點" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 縣市 清單來源
        /// </summary>
        public IList<SelectListItem> CTID_SEL_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        ///  鄉鎮市區 清單來源
        /// </summary>
        public IList<SelectListItem> ZIPCODE_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                if (string.IsNullOrEmpty(this.Form.CTID)) { return new List<SelectListItem>(); }
                list = dao.GetCountyZipList(this.Form.CTID);
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

    }


    [Serializable]
    public class OrgMapSchFormModel
    {
        public OrgMapSchFormModel()
        {
            this.ORDERBY = "CTID";
            this.ORDERDESC = "ASC";
        }

        /// <summary>
        /// 縣市代碼
        /// </summary>
        [Display(Name = "縣市代碼")]
        public string CTID { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        [Display(Name = "縣市名稱")]
        public string CityName
        {
            get
            {
                string strName = "";
                MyKeyMapDAO KMdao = new MyKeyMapDAO();
                if (string.IsNullOrEmpty(this.CTID)) { return ""; }
                strName = KMdao.GetCityName(this.CTID);
                return strName;
            }
        }


        /// <summary>
        /// 鄉鎮市區代碼
        /// </summary>
        [Display(Name = "鄉鎮市區代碼")]
        public string ZIPCODE { get; set; }

        /// <summary>
        /// 鄉鎮市區名稱
        /// </summary>
        [Display(Name = "鄉鎮市區名稱")]
        public string ZipName
        {
            get
            {
                string strName = "";
                MyKeyMapDAO KMdao = new MyKeyMapDAO();
                if (string.IsNullOrEmpty(this.ZIPCODE)) { return ""; }
                strName = KMdao.GetZipName(this.ZIPCODE);
                return strName;
            }
        }

        /// <summary>
        /// 分署 Y
        /// </summary>
        [Display(Name = "分署")]
        public bool IsBranchOffice { get; set; }
        public string BranchOffice { get; set; }

        /// <summary>
        /// 訓練單位 Y
        /// </summary>
        [Display(Name = "訓練單位")]
        public bool IsTrainingOrg { get; set; }

        public string TrainingOrg { get; set; }

        //public string hidJobBounds { get; set; }
        //public string hidSearchBounds { get; set; }
        //public string hidSearchLoc { get; set; }
        //public string hidOpenNotSearch { get; set; }

        public string hidStationID { get; set; }
        public string hidCityID { get; set; }

        public string liSDM { get; set; }

        /// <summary> 排序 </summary>
        public string ORDERBY { get; set; }

        /// <summary> 排序別 </summary>
        public string ORDERDESC { get; set; }

    }

    [Serializable]
    public class OrgMapSchGrid1Model
    {
        /// <summary>機構流水號</summary>
        public Int64? TB_ORGID { get; set; }
        public Int64? TB_RSID { get; set; }
        public string TB_RID { get; set; }
        public string TB_TYPE { get; set; }

        /// <summary>TB_ORGNAME</summary> 
        public string TB_ORGNAME { get; set; }

        /// <summary>COMIDNO</summary> 
        public string COMIDNO { get; set; }
        /// <summary>ORGKIND</summary> 
        public string ORGKIND { get; set; }
        /// <summary>DISTID</summary> 
        public string DISTID { get; set; }
        /// <summary>PLANID</summary> 
        public Int32? PLANID { get; set; }
        /// <summary>ORGLEVEL</summary> 
        public Int32? ORGLEVEL { get; set; }
        /// <summary>REAL_TWD97_X</summary> 
        public double? REAL_TWD97_X { get; set; }
        /// <summary>REAL_TWD97_Y</summary> 
        public double? REAL_TWD97_Y { get; set; }
        /// <summary>CTID</summary> 
        public Int32? CTID { get; set; }
        /// <summary>CTNAME</summary> 
        public string CTNAME { get; set; }
        /// <summary>ZNAME</summary> 
        public string ZNAME { get; set; }
        /// <summary>ZIPNAME</summary> 
        public string ZIPNAME { get; set; }
        /// <summary>ZIPCODE</summary> 
        public Int32? ZIPCODE { get; set; }

        /// <summary>ZIPCODE6W</summary> 
        public string ZIPCODE6W { get; set; }
        /// <summary>ADDRESS</summary> 
        public string ADDRESS { get; set; }

        /// <summary>TB_ADDRESS1</summary> 
        public string TB_ADDRESS1
        {
            get
            {
                if (string.IsNullOrEmpty(this.ZIPNAME)) { return ""; }
                if (string.IsNullOrEmpty(this.ADDRESS)) { return ""; }
                return string.Format("{0}{1}", this.ZIPNAME, this.ADDRESS);
            }
        }

        /// <summary>PHONE</summary> 
        public string PHONE { get; set; }
        /// <summary>CONTACTEMAIL</summary> 
        public string CONTACTEMAIL { get; set; }
        /// <summary>CONTACTCELLPHONE</summary> 
        public string CONTACTCELLPHONE { get; set; }
        /// <summary>MODIFYDATE</summary> 
        public DateTime? MODIFYDATE { get; set; }

    }

}