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
    public class ClassMapSchViewModel
    {

        public ClassMapSchViewModel()
        {
            this.Form = new ClassMapSchFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public ClassMapSchFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果清單(for 產投) Grid1Model
        /// </summary>
        public IList<ClassMapSchGrid1Model> Grid1 { get; set; }



        /// <summary>
        /// 產投課程查詢條件開訓日期
        /// 開訓日期起：抓系統日前一個月的資料（因報名結束日會設成大於開訓日）
        /// </summary>
        public IList<SelectListItem> YEAR0_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                //年度
                //int syear = DateTime.Now.Year - 1911; //當年度
                //int syear = DateTime.Now.AddMonths(-1).Year - 1911; //取前一個月所在的年份
                DateTime now = DateTime.Now;
                int syear = now.Year - 1911;
                int eyear = now.Year - 1911;
                //2019-01-30 修改問題7：開訓日期區間預設選取的邏輯為
                /*
                 當日介於１~６月為上半年：查詢區間預設->前一年12月 ~ 當年6月
                 當日介於７～１２月為下半年：查詢區間預設->當年6月 ~ 當年１２月
                 */
                //設定起始年
                if (now.Month <= 6) { syear = now.AddYears(-1).Year - 1911; }
                //當年度直到mon >= n月後才有下1年度 'Year(Now) - 1911 + 1
                if (now.Month >= 6) { eyear += 1; }

                for (int i = syear; i <= eyear; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }
                return list;
            }
        }

        /// <summary>
        /// 年度(前五年) 清單來源
        /// </summary>
        public IList<SelectListItem> YEAR1_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                //年度
                //int syear = DateTime.Now.Year - 1911; //當年度
                //int syear = DateTime.Now.AddMonths(-1).Year - 1911; //取前一個月所在的年份
                DateTime now = DateTime.Now;
                int syear = now.Year - 1911;
                int eyear = now.Year - 1911;

                //設定起始年
                if (now.Month <= 6) { syear = now.AddYears(-1).Year - 1911; }
                //當年度直到mon >= n月後才有下1年度 'Year(Now) - 1911 + 1
                if (now.Month >= 6) { eyear += 1; }

                for (int i = syear; i <= eyear; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }
                return list;
            }
        }



        /// <summary>
        /// 月份 清單來源
        /// </summary>
        public IList<SelectListItem> MON_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();

                //月份
                for (int i = 1; i < 13; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }

                return list;
            }
        }

        /// <summary> 縣市 清單來源 </summary>
        public IList<CheckBoxListItem> CTID_CHK_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();
                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }

        /// <summary>
        /// 鄉鎮市區 清單來源
        /// </summary>
        /// <returns></returns>
        public IList<SelectListItem> ZIPCODE_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                if (!string.IsNullOrEmpty(this.Form.CTID))
                    list = dao.GetCountyZipList(this.Form.CTID);
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>輔助計畫別 清單來源</summary>
        public IList<SelectListItem> ORGKIND_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"G", "產業人才投資計畫"},
                    {"W", "提升勞工自主學習計畫" }
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary> 訓練職能別,六大職能別 清單來源 </summary>
        public IList<SelectListItem> CLASSCATE_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetClassCateList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 訓練職類 項目清單 TMID,BUSNAME 清單來源
        /// </summary>
        public IList<SelectListItem> TMIDBUS_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetTMIDBUSList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 訓練職類 項目清單 TMID,JOBNAME
        /// </summary>
        public IList<SelectListItem> TMIDJOB_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                if (this.Form.PARENT1.HasValue)
                    list = dao.GetTMIDJOBList(this.Form.PARENT1.Value);
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 訓練業別（第一層） 清單來源
        /// </summary>
        public IList<SelectListItem> TMIDLv1_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetTMIDItemLv1List();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 訓練業別（第二層） 清單來源
        /// </summary>
        public IList<SelectListItem> TMIDLv2_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                if (this.Form.JOBTMID.HasValue)
                    list = dao.GetTMIDItemLv2List(this.Form.JOBTMID.Value);
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 策略性產業 清單來源
        /// </summary>
        public IList<SelectListItem> TMID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetTmidList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

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
        /// 上課地點(分署)
        /// </summary>
        public IList<CheckBoxListItem> DISTID_list
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
                return MyCommonUtil.ConvertCheckBoxItems(dictionary);
            }
        }



        /// <summary>
        /// 包含已截止報名課程(Y:是  N:否)
        /// </summary>
        public IList<SelectListItem> ContainsOverEnter_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "Y","是"},
                    { "N","否" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }


    }

    [Serializable]
    public class ClassMapSchFormModel
    {
        /// <summary>
        /// new
        /// </summary>
        public ClassMapSchFormModel()
        {
            this.ORDERBY = "CTID";
            this.ORDERDESC = "ASC";
        }

        /// <summary>
        /// 查詢類別("1":產業人才投資方案、"2":在職進修訓練、"5":區域產業據點)
        /// </summary>
        [Display(Name = "類別")]
        public string PlanType { get; set; }

        ///--------------------------產投使用條件--------------------------
        /// <summary>
        /// 課程代碼
        /// </summary>
        [Display(Name = "課程代碼")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "課程代碼 必須為數字！")]
        public Int64? OCID { get; set; }



        /// <summary>
        /// 縣市代碼
        /// </summary>
        [Display(Name = "縣市代碼")]
        public string CTID { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string CityName
        {
            get
            {
                string strName = "";
                MyKeyMapDAO KMdao = new MyKeyMapDAO();
                if (!string.IsNullOrEmpty(this.CTID))
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
        public string ZipName
        {
            get
            {
                string strName = "";
                MyKeyMapDAO KMdao = new MyKeyMapDAO();
                if (!string.IsNullOrEmpty(this.ZIPCODE))
                    strName = KMdao.GetZipName(this.ZIPCODE);
                return strName;
            }
        }

        /// <summary>
        /// 單位統一編號
        /// </summary>
        public string COMIDNO { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string OrgName
        {
            get
            {
                string strName = "";
                MyKeyMapDAO KMdao = new MyKeyMapDAO();
                if (!string.IsNullOrEmpty(this.COMIDNO))
                    strName = KMdao.GetOrgName(this.COMIDNO);
                return strName;
            }
        }

        /// <summary>
        /// 輔助計畫別
        /// </summary>
        [Display(Name = "依輔助計畫別")]
        public string ORGKIND { get; set; }

        /// <summary>
        /// 產投-計畫別中文
        /// </summary>
        public string ORGKINDNAME
        {
            get
            {
                switch (this.ORGKIND)
                {
                    case "G":
                        return "產業人才投資計畫"; //break;
                    case "W":
                        return "提升勞工自主學習計畫"; //break;
                    default:
                        return "";
                }
            }
        }

        /// <summary>訓練職能別,六大職能別</summary>
        [Display(Name = "依訓練職能別")]
        public string CLASSCATE { get; set; }
        public string CLASSCATENAME
        {
            get
            {
                string result = "";
                if (!string.IsNullOrEmpty(this.CLASSCATE) && this.CLASSCATE.Length > 2) { this.CLASSCATE = null; }
                if (string.IsNullOrEmpty(this.CLASSCATE)) { return result; }
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetClassCateList();
                if (list != null && list.Count > 0)
                {
                    var tmp = list.Where(m => m.CODE == this.CLASSCATE).ToList();
                    if (tmp != null && tmp.Count > 0) { result = tmp.First().TEXT; }
                }
                return result;
            }
        }

        /// <summary>訓練職類 項目清單 TMID,BUSNAME</summary>
        [Display(Name = "訓練職類大項")]
        public Int64? PARENT1 { get; set; }

        /// <summary>訓練職類 項目清單 TMID,JOBNAME</summary>
        [Display(Name = "訓練職類中項")]
        public Int64? PARENT2 { get; set; }


        public string PARENT1NAME
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();

                return this.PARENT1.HasValue ? dao.GetBusName(this.PARENT1.Value) : "";
            }
        }

        public string PARENT2NAME
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();

                return this.PARENT2.HasValue ? dao.GetJobName2(this.PARENT2.Value) : "";
            }
        }


        /// <summary>
        /// 訓練業別-第二層-職類課程
        /// </summary>
        [Display(Name = "職類課程")]
        public Int64? JOBTMID { get; set; }

        /// <summary>
        /// 職類課程名稱
        /// </summary>
        public string JOBNAME
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();

                return this.JOBTMID.HasValue ? dao.GetJobName(this.JOBTMID.Value) : "";
            }
        }

        /// <summary>
        /// 通俗職類代碼-通俗職業代碼
        /// </summary>
        [Display(Name = "通俗職業代碼")]
        public string CJOBNO { get; set; }

        /// <summary>
        /// 通俗職類名稱-通俗職業代碼名稱
        /// </summary>
        [Display(Name = "通俗職業代碼名稱")]
        public string CJOBNAME
        {
            get
            {
                string result = "";
                //通俗職業代碼-2019-項目清單
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetCJOBList();
                if (list != null && list.Count > 0)
                {
                    var tmp = list.Where(m => m.CODE == Convert.ToString(this.CJOBNO)).ToList();
                    if (tmp.Count > 0)
                        result = tmp.First().TEXT;
                }
                return result;
            }
        }

        /// <summary>
        /// 訓練業別-第三層-業別
        /// </summary>
        [Display(Name = "訓練業別")]
        public Int64? TMID { get; set; }

        /// <summary>
        /// 業別名稱
        /// </summary>
        public string TRAINNAME
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();

                return this.TMID.HasValue ? dao.GetTrainName(this.TMID.Value) : "";
            }
        }

        /// <summary>
        /// 是否輔導考證("Y":是、"N":否)
        /// </summary>
        [Display(Name = "輔導考證")]
        public string TGOVEXAM { get; set; } = "";

        //--------------------------在職進修訓練--------------------------
        /// <summary>
        /// 上課地點
        /// </summary>
        [Display(Name = "上課地點")]
        public string DISTID { get; set; }

        /// <summary>
        /// 分署別(checkboxlist顯示用)
        /// </summary>
        public string[] DISTID_SHOW
        {
            get
            {
                //回傳一個長度為0的字串陣列
                if (this.DISTID == null) { return new string[0]; }
                return this.DISTID.Replace("'", "").Split(',');
            }
            set
            {
                if (value == null) { return; }

                var item = value.ToList();
                if (item.Count == 1 && "".Equals((string)item[0])) { return; }

                this.DISTID = MyCommonUtil.ConvertToWhereInValues(value.ToList());
            }
        }

        /// <summary>依訓練職能別, 六大職能別 string[]</summary>
        public string[] CLASSCATE_SHOW
        {
            get
            {
                //回傳一個長度為0的字串陣列
                if (this.CLASSCATE == null) { return new string[0]; }
                return this.CLASSCATE.Replace("'", "").Split(',');
            }
            set
            {
                if (value == null) { return; }

                var item = value.ToList();
                if (item.Count == 1 && "".Equals((string)item[0])) { return; }

                this.CLASSCATE = MyCommonUtil.ConvertToWhereInValues(value.ToList());
            }
        }

        /// <summary>
        /// 輔助計畫別 string[] ORGKIND_list
        /// </summary>
        public string[] ORGKIND_SHOW
        {
            get
            {
                //回傳一個長度為0的字串陣列
                if (this.ORGKIND == null) { return new string[0]; }
                return this.ORGKIND.Replace("'", "").Split(',');
            }
            set
            {
                if (value == null) { return; }

                var item = value.ToList();
                if (item.Count == 1 && "".Equals((string)item[0])) { return; }

                this.ORGKIND = MyCommonUtil.ConvertToWhereInValues(value.ToList());
            }
        }

        /// <summary>
        /// 訓練單位
        /// </summary>
        [Display(Name = "訓練單位")]
        public string DISTNAME { get; set; }

        /// <summary>
        /// 上課地點(縣市別複選)
        /// </summary>
        public string CTID_KEY { get; set; }

        /// <summary>
        /// 上課地點(縣市別複選-checkboxlistfor顯示用)
        /// </summary>
        public string[] CTID_SHOW
        {
            get
            {
                //回傳一個長度為0的字串陣列
                if (this.CTID_KEY == null) { return new string[0]; }
                //return this.CTID_KEY.Replace("'", "").Split(',');
                return this.CTID_KEY.Split(',');
            }

            set
            {
                if (value == null) { return; }

                var item = value.ToList();
                if (item.Count == 1 && "".Equals((string)item[0])) { return; }

                this.CTID_KEY = string.Join(",", item);
            }
        }

        //--------------------------共用--------------------------

        /// <summary>包含已截止報名課程</summary>
        [Display(Name = "包含已截止報名課程")]
        public bool ContainsOverEnter { get; set; }

        /// <summary>包含已截止報名課程 Y/N</summary>
        [Display(Name = "包含已截止報名課程")]
        public string hidCoursesClosed { get; set; }

        /// <summary>查詢條件</summary>
        [Display(Name = "查詢條件")]
        public string hidQryConditionsd { get; set; }

        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        [Display(Name = "關鍵字搜尋")]
        public string KEYWORDS { get; set; }

        public IList<string> KEYWORDS_list
        {
            get
            {
                IList<string> result = new List<string>();
                if (this.KEYWORDS != null)
                {
                    if (!string.IsNullOrEmpty(this.KEYWORDS.Trim()))
                        result = this.KEYWORDS.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return result;
            }
        }

        /// <summary>
        /// 課程名稱
        /// </summary>
        [Display(Name = "課程名稱檢索")]
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 開訓日期_起年/訓練日期_起年(西元年)
        /// </summary>
        public string STDATE_YEAR
        {
            get
            {
                string result = "";
                if (!string.IsNullOrEmpty(this.STDATE_YEAR_SHOW))
                {
                    result = (Convert.ToInt32(this.STDATE_YEAR_SHOW) + 1911).ToString();
                }
                return result;
            }
        }

        /// <summary>
        /// 開訓日期_起年/訓練日期_起年(民國年)
        /// </summary>
        public string STDATE_YEAR_SHOW { get; set; }

        /// <summary>
        /// 開訓日期_起月/訓練日期_起月
        /// </summary>
        public string STDATE_MON { get; set; }

        /// <summary>
        /// 開訓日期_迄年/訓練日期_迄年(西元年)
        /// </summary>
        public string FTDATE_YEAR
        {
            get
            {
                string result = "";
                if (!string.IsNullOrEmpty(this.FTDATE_YEAR_SHOW))
                {
                    result = (Convert.ToInt32(this.FTDATE_YEAR_SHOW) + 1911).ToString();
                }
                return result;
            }
        }

        /// <summary>
        /// 開訓日期_迄年/訓練日期_迄年(民國年)
        /// </summary>
        public string FTDATE_YEAR_SHOW { get; set; }

        /// <summary>
        /// 開訓日期_迄月/訓練日期_迄月
        /// </summary>
        public string FTDATE_MON { get; set; }

        /// <summary>
        /// 開訓日期/訓練日期(yyyyMMdd)
        /// </summary>

        public string STDATE
        {
            get
            {
                //public string STDATE { get; set; }
                string rtn = string.Empty;
                if (string.IsNullOrEmpty(this.STDATE_YEAR)) { return rtn; }
                if (string.IsNullOrEmpty(this.STDATE_MON)) { return rtn; }
                rtn = string.Format("{0}/{1}/01 00:00:00", this.STDATE_YEAR, this.STDATE_MON.PadLeft(2, '0'));
                return rtn;
            }
        }

        /// <summary>
        /// 開訓日期/訓練日期(yyyyMMdd)
        /// </summary>
        public string FTDATE
        {
            get
            {
                //public string FTDATE { get; set; }
                string rtn = string.Empty;
                if (string.IsNullOrEmpty(this.FTDATE_YEAR)) { return rtn; }
                if (string.IsNullOrEmpty(this.FTDATE_MON)) { return rtn; }
                var vFTDATE_MON2 = (Convert.ToInt32(this.FTDATE_MON) + 1).ToString();
                var vFTDATE_YEAR2 = this.FTDATE_YEAR;
                if (Convert.ToInt32(vFTDATE_MON2) > 12)
                {
                    vFTDATE_MON2 = "1";
                    vFTDATE_YEAR2 = (Convert.ToInt32(vFTDATE_YEAR2) + 1).ToString();
                }
                rtn = string.Format("{0}/{1}/01 00:00:00", vFTDATE_YEAR2, vFTDATE_MON2.PadLeft(2, '0'));
                return rtn;
            }
        }

        /// <summary>
        /// 提供上課位置距離("Y":是、"N":否)
        /// </summary>
        [Display(Name = "提供上課位置距離")]
        public string ProvideLocation { get; set; } = "N";

        /// <summary>
        /// 是否為首頁查詢("Y":是、"N":否)
        /// </summary>
        public string IsHomeSearch { get; set; }

        /// <summary>
        /// 是否為歷史課程查詢 (null:否 / "Y":是)
        /// </summary>
        public string IsHisSearch { get; set; }

        /// <summary>
        /// 包含已截止報名課程
        /// </summary>
        [Display(Name = "包含已截止報名課程")]
        public string IsContainsOverEnter { get; set; }

        /// <summary>
        /// 會員代碼(e_member.mem_sn)
        /// </summary>
        public decimal? MemSN { get; set; }

        /// <summary>
        /// 身分證號
        /// </summary>
        public string MEM_ACID { get; set; }

        public string hidJobBounds { get; set; }
        public string hidSearchBounds { get; set; }
        public string hidSearchLoc { get; set; }
        public string hidOpenNotSearch { get; set; }
        public string liSDM { get; set; }

        /// <summary> 排序 </summary>
        public string ORDERBY { get; set; }

        /// <summary> 排序別 </summary>
        public string ORDERDESC { get; set; }

    }

    //依 ClassSearchGrid1Model
    public class ClassMapSchGrid1Model : ClassSearchGrid1Model { }

}