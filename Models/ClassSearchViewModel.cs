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
using log4net;

namespace WDAIIP.WEB.Models
{
    #region ClassSearchViewModel
    [Serializable]
    public class ClassSearchViewModel
    {
        public ClassSearchViewModel()
        {
            this.Form = new ClassSearchFormModel();
        }

        /// <summary>查詢條件 FormModel</summary>
        public ClassSearchFormModel Form { get; set; }

        /// <summary>查詢結果清單(for 產投) Grid1Model</summary>
        public IList<ClassSearchGrid1Model> Grid1 { get; set; }

        /// <summary>查詢結果清單(for 分署自辦在職) Grid2Model</summary>
        public IList<ClassSearchGrid2Model> Grid2 { get; set; }

        /// <summary>查詢結果清單(for 區域產業據點) Grid3Model</summary>
        public IList<ClassSearchGrid3Model> Grid3 { get; set; }

        /// <summary>明細頁，(內含 detail1 為產投明細、detail2 為在職進修明細)</summary>
        public ClassSearchDetailModel Detail { get; set; }

        /// <summary>課程比一比</summary>
        public ClassSearchCompareModel Compare { get; set; }

        /// <summary>產投課程查詢條件開訓日期
        /// 開訓日期起：抓系統日前一個月的資料（因報名結束日會設成大於開訓日）</summary>
        public IList<SelectListItem> YEAR0_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                //年度 //int syear = DateTime.Now.Year - 1911; //當年度
                //int syear = DateTime.Now.AddMonths(-1).Year - 1911; //取前一個月所在的年份
                DateTime now = DateTime.Now;
                //設定起始年
                int syear = now.Month <= 6 ? now.AddYears(-1).Year - 1911 : now.Year - 1911;
                //當年度直到mon >= n月後才有下1年度 'Year(Now) - 1911 + 1
                int eyear = now.AddYears(1).Year - 1911;
                //2019-01-30 修改問題7：開訓日期區間預設選取的邏輯為
                for (int i = syear; i <= eyear; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }
                return list;
            }
        }

        /// <summary>年度(前五年) 清單來源</summary>
        public IList<SelectListItem> YEAR1_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                //年度
                //int syear = DateTime.Now.Year - 1911; //當年度
                //int syear = DateTime.Now.AddMonths(-1).Year - 1911; //取前一個月所在的年份
                DateTime now = DateTime.Now;
                //設定起始年
                int syear = (now.Month <= 6) ? now.AddYears(-1).Year - 1911 : now.Year - 1911;
                //當年度直到mon >= n月後才有下1年度 'Year(Now) - 1911 + 1
                int eyear = now.AddYears(1).Year - 1911;

                for (int i = syear; i <= eyear; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }
                return list;
            }
        }

        /// <summary>
        /// 年度(今明年) 清單來源
        /// </summary>
        public IList<SelectListItem> YEAR2_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();

                //年度
                int year = DateTime.Now.Year - 1911;
                for (int i = 0; i < 2; i++)
                {
                    list.Add(new SelectListItem { Value = (year).ToString(), Text = (year).ToString() });
                    year++;
                }

                return list;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<SelectListItem> YEAR3_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                DateTime now = DateTime.Now;
                int sYear = 97;
                int eYear = now.Year - 1911 + 1;

                for (int i = sYear; i <= eYear; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }

                return list;
            }
        }

        /// <summary> 歷史課程查詢-在職進修 訓練期間年份下拉選項（查近20年） </summary>
        public IList<SelectListItem> YEAR4_list
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                DateTime now = DateTime.Now;
                int nowYear = now.Year - 1911;

                for (int i = nowYear - 20; i <= nowYear; i++)
                {
                    list.Add(new SelectListItem { Value = (i).ToString(), Text = (i).ToString() });
                }

                if (DateTime.Now.Month >= 6)
                {
                    list.Add(new SelectListItem { Value = (nowYear + 1).ToString(), Text = (nowYear + 1).ToString() });
                }

                return list;
            }
        }

        /// <summary> 月份 清單來源 </summary>
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

        /// <summary>縣市 清單來源</summary>
        public IList<SelectListItem> CTID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>縣市 清單來源</summary>
        public IList<CheckBoxListItem> CTID_CHK_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();
                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }

        /// <summary>上課時間(可複選)</summary>
        public IList<CheckBoxListItem> CLASSTIME_CHK_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetClassTimeCodeList();
                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }

        /// <summary> 鄉鎮市區 清單來源 </summary>
        public IList<SelectListItem> ZIPCODE_list => MyCommonUtil.ConvertSelItems(
            string.IsNullOrEmpty(Form.CTID) ? new List<KeyMapModel>() : new MyKeyMapDAO().GetCountyZipList(Form.CTID)
            );

        /// <summary> 上課時段(可複選) 清單來源 </summary>
        public IList<CheckBoxListItem> TPERIOD28S_list
        {
            get
            {
                //"上午、下午、晚上"
                var list = new List<KeyMapModel>
                {
                    new KeyMapModel { CODE = "1", TEXT = "上午" },
                    new KeyMapModel { CODE = "2", TEXT = "下午" },
                    new KeyMapModel { CODE = "3", TEXT = "晚上" }
                };
                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }
        /// <summary> 星期 清單來源 </summary>
        public IList<CheckBoxListItem> WEEKS_list
        {
            get
            {
                string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
                var list = new List<KeyMapModel>();
                for (int i = 0; i < 7; i++)
                {
                    list.Add(new KeyMapModel
                    {
                        CODE = i.ToString(),
                        TEXT = $"星期{dayNames[i]}"
                    });
                }

                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }
        /// <summary>
        /// 輔助計畫別 清單來源
        /// </summary>
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

        /// <summary>依訓練職能別,六大職能別 清單來源</summary>
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
        /// 縮小單位範圍清單來源
        /// </summary>
        public IList<SelectListItem> COMIDNO_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                //if (!string.IsNullOrEmpty(this.Form.SCHOOLNAME)),{,TODO: 取得單位縮小範圍清單,}
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 是否提供上課位置距離 清單
        /// </summary>
        public IList<SelectListItem> ProvideLocation_list
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
        /// 查詢計畫類型 清單來源
        /// 2018-12-26 add區域產業據點(tplanid=70)
        /// </summary>
        public IList<SelectListItem> HisPlanType_list
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

        /// <summary> 「辦理方式」搜尋條件，並區分實體課程、混成課程。 清單 </summary>
        public IList<SelectListItem> DISTANCE_N_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "", "不拘"},
                    { "3","實體課程"},
                    { "2","混成課程"}

                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary> 輔導考證 清單 </summary>
        public IList<SelectListItem> TGOVEXAM_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "", "不拘"},
                    { "Y","是"},
                    { "N","否"}

                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary> 包含已截止報名課程(Y:是  N:否) </summary>
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

        /// <summary>
        /// MEMBER ID
        /// </summary>
        public decimal? MemSN { get; set; }

        /// <summary> 檢核參數 </summary>
        /// <param name="httpContext"></param>
        public static void CheckArgument(HttpContextBase httpContext)
        {
            NameValueCollection parms = httpContext.Request.Params;

            foreach (var key in parms.AllKeys)
            {
                if (key.Contains("OCID"))
                {
                    string value = parms[key];
                    Regex patterns = new Regex("^[0-9]+$");
                    if (!patterns.IsMatch(value))
                    {
                        var s_err1 = string.Format("#ClassSearchViewModel.OCID:[{0}]", value);
                        throw new ArgumentException(s_err1);
                    }
                }

                if (key.Contains("SELECTIS"))
                {
                    string value = parms[key];
                    value = value.ToUpper();
                    Regex patterns = new Regex("^(TRUE,FALSE|FALSE)$");
                    if (!patterns.IsMatch(value))
                    {
                        var s_err1 = string.Format("#ClassSearchViewModel.SELECTIS:[{0}]", value);
                        throw new ArgumentException(s_err1);
                    }
                }
            }
        }

        /// <summary>取得目前(DB)系統時間</summary>
        public DateTime GetSysDateNow()
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            DateTime nowTime = dao.GetSysDateNow();
            return nowTime;
        }

        /// <summary>「訓練班別」、「課程名稱」有期別則顯示期別</summary>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public string GetSystemConfig(string ItemName)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var itemValue = dao.GetSystemConfig(ItemName);
            return itemValue;
        }

        /// <summary>遠距教學</summary>
        /// <param name="s_DA"></param>
        /// <returns></returns>
        public string GetDISTANCEN(string s_DA)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var itemValue = dao.GetDISTANCEN(s_DA);
            return itemValue;
        }

        /// <summary>回傳電話與行動</summary>
        /// <param name="s_PHONE"></param>
        /// <param name="s_MOBILE"></param>
        /// <returns></returns>
        public string GetCPHONEMOBILE(string s_PHONE, string s_MOBILE)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var itemValue = dao.GetCPHONEMOBILE(s_PHONE, s_MOBILE);
            return itemValue;
        }

    }
    #endregion

    #region ClassSearchFormModel
    [Serializable]
    public class ClassSearchFormModel : PagingResultsViewModel
    {

        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 政策性課程專區-Policy course area(Y/N)
        /// </summary>
        public string IsPolicy { get; set; }

        /// <summary>
        /// 測試用flag-70:區域據點1(Y:測試)
        /// </summary>
        public string IsTestT701 { get; set; }

        /// <summary>
        /// new
        /// </summary>
        public ClassSearchFormModel()
        {
            this.ORDERBY = "Distance1";
            this.ORDERDESC = "ASC";
        }

        /// <summary>
        /// 是否為第一次進入此功能 (Y 是, N 否)
        /// </summary>
        public string IsFirst { get; set; }

        /// <summary>
        /// 是否已查詢(Y 是, N 否)
        /// </summary>
        public string IsSearched { get; set; }

        /// <summary>
        /// 查詢類別("1":產業人才投資方案、"2":在職進修訓練、"5":區域產業據點)
        /// </summary>
        [Display(Name = "類別")]
        public string PlanType { get; set; }

        //---產投使用條件---
        /// <summary>
        /// 課程代碼
        /// </summary>
        [Display(Name = "課程代碼")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "課程代碼 必須為數字！")]
        public Int64? OCID { get; set; }

        /// <summary>
        /// 開訓日期區間("0":近一個月、"1":日期區間、"2":今日開放報名)
        /// </summary>
        [Display(Name = "開訓日期區間")]
        public string CASETYPE { get; set; }

        /// <summary>
        /// 師資名稱
        /// </summary>
        [Display(Name = "師資名稱")]
        public string TEACHCNAME { get; set; }

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
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位名稱")]
        public string SCHOOLNAME { get; set; }

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

        /// <summary>上課時段,註：此參數已不需使用</summary>
        [Display(Name = "上課時段(可複選)")]
        public string TPERIOD28S { get; set; }
        /// <summary>上課時段(checkboxlist顯示用)</summary>
        public string[] TPERIOD28S_SHOW
        {
            get
            {
                return this.TPERIOD28S != null ? this.TPERIOD28S.Replace("'", "").Split(',') : new string[0];
            }
            set
            {
                if (value != null)
                {
                    this.TPERIOD28S = MyCommonUtil.ConvertToWhereInValues(value.ToList());
                }
            }
        }
        /// <summary>組上課時段條件SQL</summary>
        public string TPERIOD28S_Sql { get; set; }

        /// <summary>上課時間,註：此參數已不需使用</summary>
        [Display(Name = "上課時間(可複選)")]
        public string WEEKS { get; set; }
        /// <summary>上課時間(checkboxlist顯示用)</summary>
        public string[] WEEKS_SHOW
        {
            get
            {
                //return this.WEEKS.Split(',');
                return this.WEEKS != null ? this.WEEKS.Replace("'", "").Split(',') : new string[0];
            }
            set
            {
                if (value != null)
                {
                    this.WEEKS = MyCommonUtil.ConvertToWhereInValues(value.ToList());
                    //string wk = string.Join(",",value); //this.WEEKS = wk;
                }
            }
        }
        /// <summary>組星期條件SQL</summary>
        public string WEEKS_Sql { get; set; }

        /// <summary>輔助計畫別</summary>
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

        /// <summary>依訓練職能別,六大職能</summary>
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
                string strName = "";

                MyKeyMapDAO dao = new MyKeyMapDAO();
                if (this.JOBTMID.HasValue)
                {
                    strName = dao.GetJobName(this.JOBTMID.Value);
                }

                return strName;
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

        public string TMIDNAME
        {
            get
            {
                string result = "";
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetTmidList();
                if (list != null && list.Count > 0)
                {
                    var tmp = list.Where(m => m.CODE == Convert.ToString(this.TMID)).ToList();
                    if (tmp.Count > 0)
                        result = tmp.First().TEXT;
                }
                return result;
            }
        }

        /// <summary>
        /// 業別名稱
        /// </summary>
        public string TRAINNAME
        {
            get
            {
                string strName = "";

                MyKeyMapDAO dao = new MyKeyMapDAO();
                if (this.TMID.HasValue)
                {
                    strName = dao.GetTrainName(this.TMID.Value);
                }

                return strName;
            }
        }

        /// <summary>辦理方式</summary>
        [Display(Name = "辦理方式")]
        public string DISTANCE { get; set; } = "";

        /// <summary>
        /// 是否輔導考證("Y":是、"N":否)
        /// </summary>
        [Display(Name = "輔導考證")]
        public string TGOVEXAM { get; set; } = "";

        //---在職進修訓練---
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
                return this.DISTID != null ? this.DISTID.Replace("'", "").Split(',') : new string[0];
            }
            set
            {
                if (value != null)
                {
                    //this.DISTID = MyCommonUtil.ConvertToWhereInValues(value.ToList());
                    var item = value.ToList();
                    if (!(item.Count == 1 && "".Equals((string)item[0])))
                    {
                        this.DISTID = MyCommonUtil.ConvertToWhereInValues(value.ToList());
                    }
                }
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
                return !string.IsNullOrEmpty(this.CTID_KEY) ? this.CTID_KEY.Split(',') : new string[0];
            }

            set
            {
                if (value != null)
                {
                    var item = value.ToList();
                    if (!(item.Count == 1 && "".Equals((string)item[0])))
                    {
                        this.CTID_KEY = string.Join(",", item);
                    }
                }
            }
        }

        //---共用---
        /// <summary> 關鍵字搜尋 </summary>
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

        /// <summary> 課程名稱 </summary>
        [Display(Name = "課程名稱檢索")]
        public string CLASSCNAME { get; set; }

        /// <summary> 課程名稱+期別 </summary>
        [Display(Name = "課程名稱檢索")]
        public string CLASSCNAME2 { get; set; }

        /// <summary> 職訓專長能力 </summary>
        [Display(Name = "職訓專長能力")]
        public string ABILITYS { get; set; }

        public IList<string> ABILITYS_list
        {
            get
            {
                IList<string> result = new List<string>();
                if (this.ABILITYS != null && !string.IsNullOrEmpty(this.ABILITYS.Trim()))
                {
                    result = this.ABILITYS.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return result;
            }
        }

        /// <summary>
        /// 開訓日期_起年/訓練日期_起年(西元年)
        /// </summary>
        public string STDATE_YEAR
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(this.STDATE_YEAR_SHOW)) { return result; }
                int i_yshow = -1;
                if (!int.TryParse(this.STDATE_YEAR_SHOW, out i_yshow))
                {
                    logger.WarnFormat("#ClassSearchFormModel,STDATE_YEAR,STDATE_YEAR_SHOW:{0}", this.STDATE_YEAR_SHOW);
                    return result;
                }
                return (i_yshow + 1911).ToString();
            }
        }

        /// <summary>
        /// 開訓日期_起年/訓練日期_起年(民國年)
        /// </summary>
        public string STDATE_YEAR_SHOW { get; set; }

        /// <summary> 開訓日期_起月/訓練日期_起月 </summary>
        public string STDATE_MON { get; set; }

        /// <summary> 開訓日期_迄年/訓練日期_迄年(西元年) </summary>
        public string FTDATE_YEAR
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(this.FTDATE_YEAR_SHOW)) { return result; }
                int i_yshow = -1;
                if (!int.TryParse(this.FTDATE_YEAR_SHOW, out i_yshow))
                {
                    logger.WarnFormat("#ClassSearchFormModel,FTDATE_YEAR,FTDATE_YEAR_SHOW:{0}", this.FTDATE_YEAR_SHOW);
                    return result;
                }
                return (i_yshow + 1911).ToString();
            }
        }

        /// <summary> 開訓日期_迄年/訓練日期_迄年(民國年) </summary>
        public string FTDATE_YEAR_SHOW { get; set; }

        /// <summary> 開訓日期_迄月/訓練日期_迄月 </summary>
        public string FTDATE_MON { get; set; }

        /// <summary> 開訓日期/訓練日期(yyyyMMdd) </summary>
        public string STDATE { get; set; }

        /// <summary> 開訓日期/訓練日期(yyyyMMdd) </summary>
        public string FTDATE { get; set; }

        /// <summary> 提供上課位置距離("Y":是、"N":否) </summary>
        [Display(Name = "提供上課位置距離")]
        public string ProvideLocation { get; set; } = "N";

        /// <summary>SELOCD28 </summary>
        public string SELOCD28 { get; set; }

        /// <summary> 是否為首頁查詢("Y":是、"N":否) </summary>
        public string IsHomeSearch { get; set; }

        /// <summary> 是否為歷史課程查詢 (null:否 / "Y":是) </summary>
        public string IsHisSearch { get; set; }

        /// <summary> 包含已截止報名課程 </summary>
        [Display(Name = "包含已截止報名課程")]
        public string IsContainsOverEnter { get; set; }

        /// <summary> 會員代碼(e_member.mem_sn) </summary>
        public decimal? MemSN { get; set; }

        /// <summary> 身分證號 </summary>
        public string MEM_ACID { get; set; }

        /// <summary> 排序 </summary>
        public string ORDERBY { get; set; }

        /// <summary> 排序別 </summary>
        public string ORDERDESC { get; set; }
    }
    #endregion

    #region ClassSearchGrid1Model
    /// <summary> 產投課程清單（tplanid=28） </summary>
    [Serializable]
    public class ClassSearchGrid1Model
    {
        /// <summary> 選取Checkbox </summary>
        public bool SELECTIS { get; set; }

        /// <summary> 開班流水號 </summary>
        public Int64? OCID { get; set; }

        /// <summary> 班別中文名稱 </summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 班別中文名稱+期別 </summary>
        public string CLASSCNAME2 { get; set; }

        /// <summary> 班別英文名稱 </summary>
        public string CLASSENGNAME { get; set; }

        /// <summary> 課程內容(大綱) </summary>
        public string CONTENT { get; set; }

        /// <summary> 計畫代碼 (ref:id_plan.planid) </summary>
        public Int64? PLANID { get; set; }

        /// <summary> 廠商(機構)統一編號 </summary>
        public string COMIDNO { get; set; }

        /// <summary> 開班計畫主檔序號 (ref:plan_planinfo.seqno) </summary>
        public Int64? SEQNO { get; set; }

        /// <summary> 訓練人數 </summary>
        public Int64? TNUM { get; set; }

        /// <summary> 訓練時數 </summary>
        public Int64? THOURS { get; set; }

        /// <summary> 上課地址學科場地名稱 </summary>
        public string CTNAME1 { get; set; }

        /// <summary> 上課地址術科場地名稱 </summary>
        public string CTNAME2 { get; set; }

        /// <summary> 學科場地名稱 </summary>
        public double? Distance1 { get; set; }

        /// <summary> 術科場地名稱  </summary>
        public double? Distance2 { get; set; }

        /// <summary> 坐標-X </summary>
        public double? REAL_TWD97_X { get; set; }
        /// <summary> 坐標-Y</summary>
        public double? REAL_TWD97_Y { get; set; }

        /// <summary> 遠距教學 </summary>
        public string DISTANCE { get; set; }

        /// <summary> 遠距教學-顯示 </summary>
        public string DISTANCE_N
        {
            get
            {
                var s_DA = this.DISTANCE ?? "";
                //var s_DA_N = s_DA.Equals("1") ? "(整班為遠距教學)" : s_DA.Equals("2") ? "(部分課程為遠距/實體教學)" : s_DA.Equals("3") ? "(整班為實體教學)" : "";
                var s_DA_N = s_DA.Equals("1") ? "(遠距課程)" : s_DA.Equals("2") ? "(混成課程)" : s_DA.Equals("3") ? "(實體課程)" : "";
                return s_DA_N;
            }
        }

        /// <summary> 是否結訓 </summary>
        public string ISCLOSED { get; set; }

        /// <summary> 開訓日期 </summary>
        public DateTime? STDATE { get; set; }

        /// <summary> 結訓日期 </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary> 開訓日期 (民國年 yyy/MM/dd) </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary> 結訓日期 (民國年 yyy/MM/dd) </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        /// <summary> 報名起日期時間 </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary> 報名訖日期時間 </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary> 經費來源-政府負擔 </summary>
        public string DEFGOVCOST { get; set; }

        /// <summary> 經費來源-學員負擔 </summary>
        public string DEFSTDCOST { get; set; }

        /// <summary> 經費來源-政府負擔 </summary>
        public double? DEFGOVCOST1 { get; set; }

        /// <summary> 經費來源-學員負擔 </summary>
        public double? DEFSTDCOST1 { get; set; }

        /// <summary> 報名繳費方式中文 </summary>
        public string ENTERSUPPLYSTYLE { get; set; }

        /// <summary> 招生狀態 </summary>
        public Int64? ADMISSIONS { get; set; }

        /// <summary> 廠商(機構)名稱 </summary>
        public string ORGNAME { get; set; }

        /// <summary>上課地址-縣市代碼</summary>
        public Int16? CTID { get; set; }

        /// <summary>上課地址-縣市</summary>
        public string CTNAME { get; set; }

        /// <summary>上課地址</summary>
        public string TADDRESS { get; set; }

        /// <summary> 原班別中文名稱 </summary>
        public string OLDCLASSCNAME { get; set; }

        /// <summary> 計畫程式代碼(1：產投/2：自辦在職/5：區域據點) </summary>
        public string PLANTYPE { get; set; }

        /// <summary> 上課距離 </summary>
        public Decimal? Distance { get; set; }

        /// <summary> 訓練性質中文描述 </summary>
        public string TPROPERTYID_TEXT { get; set; }


    }
    #endregion

    #region ClassSearchGrid2Model
    /// <summary> 分署自辦在職清單（tplanid=06）</summary>
    [Serializable]
    public class ClassSearchGrid2Model
    {
        /// <summary>
        /// 選取Checkbox
        /// </summary>
        public bool SELECTIS { get; set; }

        /// <summary>
        /// 報名起日期
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名迄日期
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 2019-01-30 add 上課時間
        /// </summary>
        public string NOTE3 { get; set; }

        /// <summary>
        /// 報名起日 (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string SENTERDATE_FULL_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日 (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string FENTERDATE_FULL_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.FENTERDATE); }
        }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 開訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        /// <summary> 課程代碼 </summary>
        public Int64? OCID { get; set; }

        /// <summary> 課程名稱+期別 </summary>
        public string TRAINCLASS { get; set; }

        /// <summary> 期別 </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public Int64? THOURS { get; set; }

        /// <summary>
        /// 訓練時段代碼
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TPERIODNAME { get; set; }

        /// <summary>
        /// 訓練計畫名稱 
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區中心名稱
        /// </summary>
        public string TRAINCENTER { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string TRAINPLACE { get; set; }

        /// <summary>
        /// 受訓資格學歷名稱
        /// </summary>
        public string TRAINEDGE { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TRAINTIME { get; set; }

        /// <summary>
        /// 廠商(機構)名稱
        /// </summary>
        public string TRAINORG { get; set; }

        /// <summary>
        /// 通俗職類代碼
        /// </summary>
        public string CJOBNO { get; set; }

        /// <summary>
        /// 通俗職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 上課距離
        /// </summary>
        public Decimal? Distance { get; set; }
    }
    #endregion

    #region ClassSearchGrid3Model
    /// <summary> 區域產業據點（tplanid=70） </summary>
    [Serializable]
    public class ClassSearchGrid3Model
    {
        /// <summary>
        /// 選取Checkbox
        /// </summary>
        public bool SELECTIS { get; set; }

        /// <summary>
        /// 報名起日期
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名迄日期
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 報名起日 (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string SENTERDATE_FULL_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日 (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string FENTERDATE_FULL_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.FENTERDATE); }
        }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 開訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary> 課程名稱+期別 </summary>
        public string TRAINCLASS { get; set; }

        /// <summary> 期別 </summary>
        public string CYCLTYPE { get; set; }

        /// <summary> 訓練時數 </summary>
        public Int64? THOURS { get; set; }

        /// <summary>
        /// 訓練時段代碼
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TPERIODNAME { get; set; }

        /// <summary>
        /// 訓練計畫名稱 
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區中心名稱
        /// </summary>
        public string TRAINCENTER { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string TRAINPLACE { get; set; }

        /// <summary>
        /// 受訓資格學歷名稱
        /// </summary>
        public string TRAINEDGE { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TRAINTIME { get; set; }

        /// <summary>
        /// 廠商(機構)名稱
        /// </summary>
        public string TRAINORG { get; set; }

        /// <summary>
        /// 通俗職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 上課距離
        /// </summary>
        public Decimal? Distance { get; set; }
    }
    #endregion

    #region ClassSearchDetailModel
    /// <summary>
    /// 課程明細使用 (detail1:為產投明細、detail2:為在職進修訓練使用)
    /// </summary>
    [Serializable]
    public class ClassSearchDetailModel
    {
        /// <summary>
        /// 預設建構子
        /// </summary>
        public ClassSearchDetailModel()
        {
            this.detail1 = new ClassSearchDetail1Model();
            this.detail2 = new ClassSearchDetail2Model();
        }

        /// <summary>
        /// 訓練計劃類型("1":產投、"2":在職、"5":區域產業據點)
        /// </summary>
        public string PlanType { get; set; }

        /// <summary>
        /// 是否提供上課距離("Y":是、"N":否)
        /// </summary>
        public string ProvideLocation { get; set; }

        /// <summary>開班流水號</summary>
        public Int64? OCID { get; set; }

        /// <summary>明細資料(for 產投)</summary>
        public ClassSearchDetail1Model detail1 { get; set; }

        /// <summary>明細資料(for 在職)</summary>
        public ClassSearchDetail2Model detail2 { get; set; }

        /// <summary>明細資料(for 區域產業據點)</summary>
        public ClassSearchDetail3Model detail3 { get; set; }

        //返回上一頁要導去的Controller //public string ControllerName { get; set; }

        //返回上一頁要導向的ActionName //public string ActionName { get; set; }
    }
    #endregion

    #region ClassSearchDetail1Model

    /// <summary>明細資料 for 產投</summary>
    [Serializable]
    public class ClassSearchDetail1Model
    {
        /// <summary>
        /// 預設建構子
        /// </summary>
        public ClassSearchDetail1Model()
        {
            this.TrainDesc = new List<ClassSearchTranDetail2Model>();
            this.TeacherInfo = new List<TblTEACH_TEACHERINFO>();
            this.ViewRecGrid = new List<ClassViewRecordGridModel>();
            this.AbilityGrid = new List<TblPLAN_ABILITY>();
        }

        /// <summary>系統端時間（可否報名的日期依據）</summary>
        public string SERVERTIME { get; set; }
        /// <summary>計畫代碼 (ref: key_plan.tplanid)</summary>
        public string TPLANID { get; set; }
        /// <summary>計畫代碼</summary>
        public Int64? PLANID { get; set; }
        /// <summary>廠商(機構)統一編號</summary>
        public string COMIDNO { get; set; }
        /// <summary>計畫主檔序號</summary>
        public Int64? SEQNO { get; set; }
        /// <summary>開班流水號</summary>
        public Int64? OCID { get; set; }
        /// <summary> 班別中文名稱 </summary>
        public string CLASSCNAME { get; set; }
        /// <summary> 班別中文名稱+期別 </summary>
        public string CLASSCNAME2 { get; set; }
        /// <summary>班別英文名稱</summary>
        public string CLASSENGNAME { get; set; }
        /// <summary>遠距教學資訊
        /// 1."申請整班為遠距教學", 2."申請部分課程為遠距教學,3.申請整班為實體教學/無遠距教學</summary>
        public string DISTANCE { get; set; }
        /// <summary>訓練人數</summary>
        public Int64? TNUM { get; set; }
        /// <summary>訓練時數</summary>
        public Int64? THOURS { get; set; }
        /// <summary>報名起日期</summary>
        public DateTime? SENTERDATE { get; set; }
        /// <summary>報名訖日期</summary>
        public DateTime? FENTERDATE { get; set; }
        /// <summary>開訓日期</summary>
        public DateTime? STDATE { get; set; }
        /// <summary>結訓日期</summary>
        public DateTime? FTDATE { get; set; }
        /// <summary>是否結訓</summary>
        public string ISCLOSED { get; set; }
        /// <summary>業務關係代碼</summary>
        public string RID { get; set; }
        /// <summary>訓練地點</summary>
        public string TADDRESS { get; set; }
        /// <summary>訓練地點郵遞區號</summary>
        public Int64? ZIP1 { get; set; }
        /// <summary>訓練地點郵遞區號後兩碼</summary>
        public string ZIP2 { get; set; }
        /// <summary>課程狀態</summary>
        public string CLASSOPEN { get; set; }
        /// <summary>訓練性質(0:職前、1:在職、其他:委託訓練)</summary>
        public string TPROPERTY { get; set; }
        /// <summary>上課時間</summary>
        public string WEEKSTIME { get; set; }
        /// <summary>政府負擔費用</summary>
        public Decimal? DEFGOVCOST { get; set; }
        /// <summary>學員負擔費用</summary>
        public Decimal? DEFSTDCOST { get; set; }
        /// <summary>訓練計畫名稱</summary>
        public string PLANNAME { get; set; }
        /// <summary>訓練時段名稱</summary>
        public string HOURRANNAME { get; set; }
        /// <summary>學歷名稱</summary>
        public string NAME { get; set; }
        /// <summary>受訓資格-年齡起</summary>
        public Int64? CAPAGE1 { get; set; }
        /// <summary>受訓資格-年齡迄</summary>
        public Int64? CAPAGE2 { get; set; }
        /// <summary>是否為學分班</summary>
        public string POINTYN { get; set; }
        /// <summary>學分數</summary>
        public Int64? CREDPOINT { get; set; }
        /// <summary>受訓資格-性別</summary>
        public string CAPSEX { get; set; }
        /// <summary>受訓資格-兵役</summary>
        public string CAPMILITARY { get; set; }
        /// <summary>廠商(機構)名稱</summary>
        public string ORGNAME { get; set; }
        /// <summary>保險證號</summary>
        public string ACTNO { get; set; }
        /// <summary>訓練期限名稱</summary>
        public string TRAINEXPNAME { get; set; }
        /// <summary>縣市代碼</summary>
        public Int64? CTID { get; set; }
        /// <summary>縣市名稱</summary>
        public string CTNAME1 { get; set; }
        /// <summary>郵遞區號名稱</summary>
        public string ZIPNAME { get; set; }
        /// <summary>課程內容(大綱)</summary>
        public string CONTENT { get; set; }
        /// <summary>廠商(機構)地址</summary>
        public string ADDRESS { get; set; }
        /// <summary>班級聯絡人</summary>
        public string PCONTACTNAME { get; set; }
        /// <summary>廠商(機構)聯絡人</summary>
        public string CONTACTNAME { get; set; }
        /// <summary>班級聯絡人電話</summary>
        public string CONTACTPHONE { get; set; }
        public string CONTACTMOBILE { get; set; }
        /// <summary>廠商(機構)聯絡電話</summary>
        public string PHONE { get; set; }
        /// <summary>受訓資格-其他一</summary>
        public string CAPOTHER1 { get; set; }
        /// <summary>受訓資格-其他二</summary>
        public string CAPOTHER2 { get; set; }
        /// <summary>受訓資格-其他三</summary>
        public string CAPOTHER3 { get; set; }
        /// <summary>機構別2</summary>
        public string ORGKIND2 { get; set; }
        /// <summary>報名繳費方式</summary>
        public Int64? ENTERSUPPLYSTYLE { get; set; }
        /// <summary>招訓方式-招訓對象</summary>
        public string RECRUIT { get; set; }
        /// <summary>遴選方式</summary>
        public string SELMETHOD { get; set; }
        /// <summary>是否為iCAP課程</summary>
        public string ISiCAPCOUR { get; set; }
        /// <summary>課程相關說明</summary>
        public string iCAPCOURDESC { get; set; }
        /// <summary>iCAP標章證號</summary>
        public string iCAPNUM { get; set; }
        /// <summary>iCAP標章有效期</summary>
        public DateTime? iCAPMARKDATE { get; set; }
        /// <summary>學歷資格</summary>
        public string CAPALL { get; set; }
        /// <summary>是否輔導學員參加政府機關辦理相關證照考試或技能檢定</summary>
        public string TGOVEXAM { get; set; }
        /// <summary>本課程屬環境部淨零綠領人才培育課程</summary>
        public string ENVZEROTRAIN { get; set; }
        /// <summary>政府機關名稱</summary>
        public string GOVAGENAME { get; set; }
        /// <summary>證照或檢定名稱</summary>
        public string TGOVEXAMNAME { get; set; }
        /// <summary>備註(舊程式註記:有對應欄位。沒有使用此欄。)</summary>
        public string MEMO8 { get; set; }
        /// <summary>訓練方式說明</summary>
        public string TRAINMODE { get; set; }
        /// <summary>上課地址學科場地地址1(郵遞區號5碼)</summary>
        public string ADDRESS21 { get; set; }
        /// <summary>上課地址學科場地地址2(郵遞區號5碼)</summary>
        public string ADDRESS22 { get; set; }
        /// <summary>上課地址術科場地地址1(郵遞區號5碼)</summary>
        public string ADDRESS31 { get; set; }
        /// <summary>上課地址術科場地地址2(郵遞區號5碼)</summary>
        public string ADDRESS32 { get; set; }
        /// <summary>上課地址學科場地地址1(郵遞區號3碼)</summary>
        public string HIDADDRESS21 { get; set; }
        /// <summary>上課地址學科場地地址2(郵遞區號3碼)</summary>
        public string HIDADDRESS22 { get; set; }
        /// <summary>上課地址術科場地地址1(郵遞區號3碼)</summary>
        public string HIDADDRESS31 { get; set; }
        /// <summary>上課地址術科場地地址2(郵遞區號3碼)</summary>
        public string HIDADDRESS32 { get; set; }
        /// <summary>上課地址學科場地-縣市</summary>
        public string CTNAME2 { get; set; }
        /// <summary>上課地址術科場地-縣市</summary>
        public string CTNAME3 { get; set; }
        /// <summary>機構別計畫名稱</summary>
        public string ORGPLANNAME { get; set; }
        /// <summary>學科場地距離</summary>
        public double? Distance1 { get; set; }
        /// <summary>術科場地距離</summary>
        public double? Distance2 { get; set; }
        /// <summary>報名人數(扣除e網審核失敗)</summary>
        public Int64? EnterCount { get; set; }
        /// <summary>  報名鈕狀態(1:反灰、2:隱藏、3:系統提示訊息 及 HOME_NEWS3 控制是否開放報名(反灰)) 如果_btnstatus已經有值了 </summary>
        public string BtnStatus
        {
            get
            {
                string result = "";
                TblTB_CONTENT item = new MyKeyMapDAO().GetCtrlItemSet("009");
                if (item != null) { result = (!string.IsNullOrEmpty(item.C_CTRLITEM) ? item.C_CTRLITEM : ""); }
                //系統提示訊息及HOME_NEWS3權限最大
                string showMsg = (new WDAIIPWEBDAO()).StopEnterTempMsg();
                //3.反灰
                if (!string.IsNullOrEmpty(showMsg)) { result = "3"; }
                return result;
            }
        }
        /// <summary> 顯示提示訊息 </summary>
        public string ShowMsg
        {
            get
            {
                string result = "";
                TblTB_CONTENT item = new MyKeyMapDAO().GetCtrlItemSet("009");
                if (item != null) { result = MyHelperUtil.ChgBreakLine(item.C_CONTENT1); }
                //系統提示訊息及HOME_NEWS3 權限最大
                string Msg = (new WDAIIPWEBDAO()).StopEnterTempMsg();
                if (!string.IsNullOrEmpty(Msg)) { result = Msg.Replace("\n", "<br/>"); }
                return result;
            }
        }
        /// <summary>教學方式1：講授教學法（運用敘述或講演的方式，傳遞教材知識的一種教學方法，提供相關教材或講義）</summary>
        public string TMETHODC01 { get; set; }
        /// <summary>教學方式2：討論教學法（指團體成員齊聚一起，經由說、聽和觀察的過程，彼此溝通意見，由講師帶領達成教學目標）</summary>
        public string TMETHODC02 { get; set; }
        /// <summary>教學方式3：演練教學法（由講師的帶領下透過設備或教材，進行練習、表現和實作，親自解說示範的技能或程序的一種教學方法）</summary>
        public string TMETHODC03 { get; set; }
        /// <summary>教學方式99：其他教學方法</summary>
        public string TMETHODC99 { get; set; }
        /// <summary>其他教學方法說明</summary>
        public string TMETHODOTH { get; set; }
        /// <summary>教學方式</summary>
        public string TMETHOD_TEXT
        {
            get
            {
                string rtn = string.Empty;
                rtn = (string.IsNullOrEmpty(this.TMETHODC01) ? "" : this.getTMethodDesc("01"));
                rtn += (string.IsNullOrEmpty(this.TMETHODC02) ? "" : this.getTMethodDesc("02"));
                rtn += (string.IsNullOrEmpty(this.TMETHODC03) ? "" : this.getTMethodDesc("03"));
                rtn += (string.IsNullOrEmpty(this.TMETHODC99) ? "" : this.getTMethodDesc("99", this.TMETHODOTH));
                if (!string.IsNullOrEmpty(rtn))
                {
                    rtn = "<ul style='padding-left:18px;'>" + rtn + "</ul>";
                }
                return rtn;
            }
        }
        /// <summary> '技檢訓練時數 '符合技能檢定訓練時數 EHOURS</summary>
        public double? DESC_EHOURS
        {
            get
            {
                if (this.TrainDesc == null || this.TrainDesc.Count == 0) { return null; }
                double? rst1 = 0;
                foreach (var iteam1 in TrainDesc)
                {
                    rst1 += iteam1.EHOUR ?? 0;
                }
                return (rst1.HasValue && rst1.Value > 0 ? rst1 : null);
            }
        }

        /// <summary>訓練費用說明</summary>
        public string TOTALCOST_TEXT { get; set; }
        /// <summary>瀏覽次數 (2019-02-14 問題18 add)</summary>
        public Int64? BROWSECNT { get; set; }
        /// <summary>課程訓練內容_v2</summary>
        public IList<ClassSearchTranDetail2Model> TrainDesc { get; set; }
        //public IList<TblPLAN_TRAINDESC> TrainDesc { get; set; }
        /// <summary>師資</summary>
        public IList<TblTEACH_TEACHERINFO> TeacherInfo { get; set; }
        /// <summary>課程瀏覽記錄</summary>
        public IList<ClassViewRecordGridModel> ViewRecGrid { get; set; }
        /// <summary>專長能力標籤-PLAN_ABILITY</summary>
        public IList<TblPLAN_ABILITY> AbilityGrid { get; set; }
        /// <summary>取得教學方式中文描述</summary>
        /// <param name="tMethod">教學方式代碼</param>
        /// <param name="tMethodOth">教學方式其他說明</param>
        /// <returns></returns>
        private string getTMethodDesc(string tMethod, string tMethodOth = "")
        {
            string desc = string.Empty;

            switch (tMethod)
            {
                case "01":
                    desc = "講授教學法（運用敘述或講演的方式，傳遞教材知識的一種教學方法，提供相關教材或講義）";
                    break;
                case "02":
                    desc = "討論教學法（指團體成員齊聚一起，經由說、聽和觀察的過程，彼此溝通意見，由講師帶領達成教學目標）";
                    break;
                case "03":
                    desc = "演練教學法（由講師的帶領下透過設備或教材，進行練習、表現和實作，親自解說示範的技能或程序的一種教學方法）";
                    break;
                case "99":
                    desc = "其他教學方法：" + tMethodOth;
                    break;
            }

            if (string.IsNullOrEmpty(desc)) return string.Empty;

            return string.Format("<li style='width:100%;padding:0px;list-style-type: decimal;background-color:#ffffff'>{0}</li>", desc);
        }
    }
    #endregion

    #region ClassSearchDetail2Model
    /// <summary> for 在職</summary>
    public class ClassSearchDetail2Model
    {
        /// <summary>
        /// 預設建構子
        /// </summary>
        public ClassSearchDetail2Model()
        {
            this.TrainDesc = new List<TblPLAN_TRAINDESC>();
            this.ViewRecGrid = new List<ClassViewRecordGridModel>();
            this.AbilityGrid = new List<TblPLAN_ABILITY>();
        }

        /// <summary>大計畫代碼(ref:KEY_PLAN.TPLANID)</summary>
        public string TPLANID { get; set; }
        /// <summary>計畫代碼</summary>
        public Int64? PLANID { get; set; }
        /// <summary>廠商統一編號</summary>
        public string COMIDNO { get; set; }
        /// <summary>計畫主檔序號</summary>
        public Int64? SEQNO { get; set; }
        /// <summary>課程代碼</summary>
        public Int64? OCID { get; set; }
        /// <summary> 課程名稱 </summary>
        public string CLASSCNAME { get; set; }
        /// <summary> 課程名稱+期別</summary>
        public string CLASSCNAME2 { get; set; }
        /// <summary> 訓練單位 </summary>
        public string ORGNAME { get; set; }
        /// <summary> 訓練單位ID </summary>
        public Int64? ORGID { get; set; }
        /// <summary> 訓練地點(位置) ZIP </summary>
        public string TADDRESSZIP { get; set; }
        /// <summary> 訓練地點(位置) 地址 </summary>
        public string TADDRESS { get; set; }
        /// <summary> 聯絡電話 </summary>
        public string CONTACTPHONE { get; set; }
        public string CONTACTMOBILE { get; set; }
        /// <summary> 聯絡窗口 </summary>
        public string CONTACTNAME { get; set; }
        /// <summary>訓練性質ID</summary>
        public Int64? TPROPERTYID { get; set; }
        /// <summary>訓練性質名稱</summary>
        public string TPROPERTY { get; set; }
        /// <summary>訓練學員數</summary>
        public Int64? TNUM { get; set; }
        /// <summary>通俗職類代碼</summary>
        public string CJOB_NO { get; set; }
        /// <summary>通俗職類</summary>
        public string CJOB_NAME { get; set; }
        /// <summary>訓練費用-學員</summary>
        public Int64? DEFSTDCOST { get; set; }
        /// <summary>訓練費用-政府負擔</summary>
        public Int64? DEFGOVCOST { get; set; }
        /// <summary>訓練時數</summary>
        public Int64? THOURS { get; set; }
        /// <summary>訓練時段</summary>
        public string HOURRANNAME { get; set; }
        /// <summary>訓練日期-起日</summary>
        public DateTime? STDATE { get; set; }
        /// <summary>訓練日期-迄日</summary>
        public DateTime? FTDATE { get; set; }
        /// <summary>報名日期-起日</summary>
        public DateTime? SENTERDATE { get; set; }
        /// <summary>報名日期-迄日</summary>
        public DateTime? FENTERDATE { get; set; }
        /// <summary>目前報名人數</summary>
        public Int64? STUD_ENTERTYPE { get; set; }
        /// <summary>甄試日期</summary>
        public DateTime? EXAMDATE { get; set; }
        /// <summary>甄試日期時段-EXAMPERIOD-KEY_EXAMPERIOD</summary>
        public string EPNAME { get; set; }
        /// <summary>適用就保非自願離職免試入訓</summary>
        public string INVEXEM { get; set; }
        /// <summary>就保非自願離職免試入訓百分比</summary>
        public string ENTERPOINT { get; set; }
        /// <summary>訓練職類</summary>
        public string TRAINNAME { get; set; }
        /// <summary>訓練概要</summary>
        public string PURTECH { get; set; }
        /// <summary>參訓資格-性別</summary>
        public string CAPSEX { get; set; }
        /// <summary>參訓資格-年齡</summary>
        public string CAPAGE { get; set; }
        /// <summary>參訓資格-學歷</summary>
        public string CAPEDU { get; set; }
        /// <summary>參訓資格-兵役</summary>
        public string CAPMILITARY { get; set; }
        /// <summary>參訓資格-其他條件1</summary>
        public string CAPOTHER1 { get; set; }
        /// <summary>參訓資格-其他條件2</summary>
        public string CAPOTHER2 { get; set; }
        /// <summary>參訓資格-其他條件3</summary>
        public string CAPOTHER3 { get; set; }
        /// <summary>錄訓方式-持推介單</summary>
        public string GETTRAIN1 { get; set; }
        /// <summary>錄訓方式-自行報名</summary>
        public string GETTRAIN2 { get; set; }
        /// <summary>錄訓方式-甄試方式</summary>
        public string GETTRAIN3 { get; set; }
        /// <summary>錄訓方式-權益說明 (固定內容)</summary>
        public string EQUITY { get; set; }
        /// <summary>錄訓方式-其他說明</summary>
        public string GETTRAIN4 { get; set; }
        /// <summary>錄訓方式-備註</summary>
        public string NOTE { get; set; }
        /// <summary>其他說明2</summary>
        public string NOTE3 { get; set; }
        /// <summary>轄區中心名稱</summary>
        public string UNITNAME { get; set; }
        /// <summary>上課距離</summary>
        public Decimal? Distance { get; set; }
        /// <summary>瀏覽次數 (2019-02-14 問題18 add)</summary>
        public Int64? BROWSECNT { get; set; }
        /// <summary> 顯示提示訊息-暫停報名 </summary>
        public string ShowMsg4
        {
            get
            {
                string result = "";
                //TblTB_CONTENT item = new MyKeyMapDAO().GetCtrlItemSet("009");
                //if (item != null) { result = MyHelperUtil.ChgBreakLine(item.C_CONTENT1); }
                //系統提示訊息 HOME_NEWS4
                string Msg = (new WDAIIPWEBDAO()).StopEnterTempMsg4();
                if (!string.IsNullOrEmpty(Msg)) { result = Msg.Replace("\n", "<br/>"); }
                return result;
            }
        }
        /// <summary>課程訓練內容</summary>
        public IList<TblPLAN_TRAINDESC> TrainDesc { get; set; }
        /// <summary> 課程瀏覽記錄 </summary>
        public IList<ClassViewRecordGridModel> ViewRecGrid { get; set; }
        /// <summary>專長能力標籤-PLAN_ABILITY</summary>
        public IList<TblPLAN_ABILITY> AbilityGrid { get; set; }
    }
    #endregion

    #region ClassSearchDetail3Model
    /// <summary>
    /// for 區域產業據點
    /// </summary>
    public class ClassSearchDetail3Model
    {
        /// <summary>
        /// 預設建構子
        /// </summary>
        public ClassSearchDetail3Model()
        {
            this.TrainDesc = new List<TblPLAN_TRAINDESC>();
            this.ViewRecGrid = new List<ClassViewRecordGridModel>();
            this.AbilityGrid = new List<TblPLAN_ABILITY>();
        }

        /// <summary>大計畫代碼(ref:KEY_PLAN.TPLANID)</summary>
        public string TPLANID { get; set; }

        /// <summary>計畫代碼</summary>
        public Int64? PLANID { get; set; }

        /// <summary>廠商統一編號</summary>
        public string COMIDNO { get; set; }

        /// <summary>計畫主檔序號</summary>
        public Int64? SEQNO { get; set; }

        /// <summary>課程代碼</summary>
        public Int64? OCID { get; set; }

        /// <summary> 課程名稱</summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 課程名稱+期別</summary>
        public string CLASSCNAME2 { get; set; }

        /// <summary>訓練單位</summary>
        public string ORGNAME { get; set; }

        /// <summary>訓練單位ID</summary>
        public Int64? ORGID { get; set; }

        /// <summary>訓練地點(位置) ZIP</summary>
        public string TADDRESSZIP { get; set; }

        /// <summary>訓練地點(位置) 地址</summary>
        public string TADDRESS { get; set; }

        /// <summary>聯絡電話</summary>
        public string CONTACTPHONE { get; set; }
        public string CONTACTMOBILE { get; set; }

        /// <summary>聯絡窗口</summary>
        public string CONTACTNAME { get; set; }

        /// <summary>訓練性質ID</summary>
        public Int64? TPROPERTYID { get; set; }

        /// <summary>訓練性質名稱</summary>
        public string TPROPERTY { get; set; }

        /// <summary>訓練學員數</summary>
        public Int64? TNUM { get; set; }

        /// <summary>通俗職類代碼</summary>
        public string CJOB_NO { get; set; }

        /// <summary>通俗職類</summary>
        public string CJOB_NAME { get; set; }

        /// <summary>訓練費用-學員</summary>
        public Int64? DEFSTDCOST { get; set; }

        /// <summary>訓練費用-政府負擔</summary>
        public Int64? DEFGOVCOST { get; set; }

        /// <summary>訓練時數</summary>
        public Int64? THOURS { get; set; }

        /// <summary>訓練時段</summary>
        public string HOURRANNAME { get; set; }

        /// <summary>訓練日期-起日</summary>
        public DateTime? STDATE { get; set; }

        /// <summary>訓練日期-迄日</summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>報名日期-起日</summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>報名日期-迄日</summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>目前報名人數</summary>
        public Int64? STUD_ENTERTYPE { get; set; }

        /// <summary>甄試日期</summary>
        public DateTime? EXAMDATE { get; set; }

        /// <summary>適用就保非自願離職免試入訓</summary>
        public string INVEXEM { get; set; }

        /// <summary>就保非自願離職免試入訓百分比</summary>
        public string ENTERPOINT { get; set; }

        /// <summary>訓練職類</summary>
        public string TRAINNAME { get; set; }

        /// <summary>訓練概要</summary>
        public string PURTECH { get; set; }

        /// <summary>參訓資格-性別</summary>
        public string CAPSEX { get; set; }

        /// <summary>參訓資格-年齡</summary>
        public string CAPAGE { get; set; }

        /// <summary>參訓資格-學歷</summary>
        public string CAPEDU { get; set; }

        /// <summary>參訓資格-兵役</summary>
        public string CAPMILITARY { get; set; }

        /// <summary>參訓資格-其他條件1</summary>
        public string CAPOTHER1 { get; set; }

        /// <summary>參訓資格-其他條件2</summary>
        public string CAPOTHER2 { get; set; }

        /// <summary>參訓資格-其他條件3</summary>
        public string CAPOTHER3 { get; set; }

        /// <summary>錄訓方式-持推介單</summary>
        public string GETTRAIN1 { get; set; }

        /// <summary>錄訓方式-自行報名</summary>
        public string GETTRAIN2 { get; set; }

        /// <summary>錄訓方式-甄試方式</summary>
        public string GETTRAIN3 { get; set; }

        /// <summary>錄訓方式-權益說明 (固定內容)</summary>
        public string EQUITY { get; set; }

        /// <summary>       /// 錄訓方式-其他說明</summary>
        public string GETTRAIN4 { get; set; }

        /// <summary>錄訓方式-備註</summary>
        public string NOTE { get; set; }

        /// <summary>其他說明2</summary>
        public string NOTE3 { get; set; }

        /// <summary>轄區中心名稱</summary>
        public string UNITNAME { get; set; }

        /// <summary>上課距離</summary>
        public Decimal? Distance { get; set; }

        /// <summary>瀏覽次數 (2019-02-14 問題18 add)</summary>
        public Int64? BROWSECNT { get; set; }

        /// <summary> 顯示提示訊息-暫停報名 </summary>
        public string ShowMsg4
        {
            get
            {
                string result = "";
                //TblTB_CONTENT item = new MyKeyMapDAO().GetCtrlItemSet("009");
                //if (item != null) { result = MyHelperUtil.ChgBreakLine(item.C_CONTENT1); }
                //系統提示訊息 HOME_NEWS4
                string Msg = (new WDAIIPWEBDAO()).StopEnterTempMsg4();
                if (!string.IsNullOrEmpty(Msg)) { result = Msg.Replace("\n", "<br/>"); }
                return result;
            }
        }

        /// <summary>課程訓練內容</summary>
        public IList<TblPLAN_TRAINDESC> TrainDesc { get; set; }

        /// <summary>課程瀏覽記錄</summary>
        public IList<ClassViewRecordGridModel> ViewRecGrid { get; set; }

        /// <summary>專長能力標籤-PLAN_ABILITY</summary>
        public IList<TblPLAN_ABILITY> AbilityGrid { get; set; }

    }
    #endregion

    #region ClassSearchCompareModel
    /// <summary>
    /// 課程比一比功能 (IList Detail1:為產投使用、IList Detail2:在職進修訓練使用)
    /// </summary>
    [Serializable]
    public class ClassSearchCompareModel
    {
        public ClassSearchCompareModel()
        {
            this.Detail1 = new List<ClassSearchDetail1Model>();
            this.Detail2 = new List<ClassSearchDetail2Model>();
        }

        /// <summary> 計畫類型 </summary>
        public string PlanType { get; set; }

        /// <summary> 是否提供上課位置距離 </summary>
        public string ProvideLocation { get; set; }

        /// <summary> 課程明細清單(for 產投) </summary>
        public IList<ClassSearchDetail1Model> Detail1 { get; set; }

        /// <summary> 課程明細清單(for 在職進修訓練) </summary>
        public IList<ClassSearchDetail2Model> Detail2 { get; set; }
    }
    #endregion

    #region ClassViewRecordModel
    /// <summary> 課程瀏覽記錄 </summary>
    [Serializable]
    public class ClassViewRecordGridModel : TblTB_VIEWRECORD
    {
        /// <summary> 課程名稱 </summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 課程名稱+期別 </summary>
        public string CLASSCNAME2 { get; set; }
    }
    #endregion
}