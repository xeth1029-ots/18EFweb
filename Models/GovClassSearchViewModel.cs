using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    #region GovClassSearchViewModel
    public class GovClassSearchViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public GovClassSearchViewModel()
        {
            this.Form = new GovClassSearchFormModel();
        }

        /// <summary>
        /// 查詢條件 
        /// </summary>
        public GovClassSearchFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果列
        /// </summary>
        public IList<GovClassSearchGridModel> Grid { get; set; }

        /// <summary>
        /// 明細頁
        /// </summary>
        public GovClassSearchDetailModel Detail { get; set; }

        /// <summary>
        /// 訓練單位選項
        /// </summary>
        public IEnumerable<CheckBoxListItem> ORGID_list
        {
            get
            {
                //etraining直接寫死
                //var dictionary = new Dictionary<string, string>
                //{
                //    {"x","內政部營建署" },
                //    {"04191945","內政部中部辦公室（營建業務）" },
                //    {"52755393","內政部國土測繪中心" },
                //    {"27541255","經濟部工業局" },
                //    {"69102402","經濟部智慧財產局（智慧財產培訓學院）" },
                //    {"23212200","經濟部商業司" },
                //    {"03722109","經濟部國貿局" },
                //    {"04179436","經濟部中小企業處" },
                //    {"58815101","文化部" },
                //    {"13535082","客家委員會" },
                //    {"33508909","交通部公路總局公路人員訓練所" },
                //    {"x","行政院衛生署" },
                //    {"76766095","行政院環境保護署（訓練所）" },
                //    {"23124692","行政院農業委員會（輔導處）" },
                //    {"23165380","行政院經濟建設委員會" },
                //    {"83851414","行政院農業委員會漁業署" },
                //    {"20082444","行政院原住民族委員會" },
                //    {"PCC","行政院公共工程委員會" },
                //    {"52014146","衛生福利部國民健康署" },
                //    {"43669853","退輔會職訓中心" },
                //    {"40401902","宜蘭縣政府勞工處" },
                //    {"nt660660","南投縣政府" },
                //    {"31009316","臺北市職能發展學院" },
                //    {"29329756","新北市政府職業訓練中心" },
                //    {"02370","新竹市政府" },
                //    {"92088214","高雄市政府勞工局訓練就業中心" },
                //    {"41384841","財團法人台灣金融研訓院" },
                //    {"80778299","財團法人國家實驗研究院國家高速網路與計算中心" },
                //    {"05076416","財團法人資訊工業策進會" },
                //    {"02750963","財團法人工業技術研究院" },
                //    {"15648033","財團法人國家實驗研究院" },
                //    {"11084792","財團法人中華顧問工程司" },
                //    {"17597502","財團法人台灣網路資訊中心1" },
                //    {"03702716","財團法人中華民國對外貿易發展協會1" },
                //    {"01013947","財團法人台灣博物館文教基金會" },
                //    {"14875622","財團法人台灣建築中心" },
                //    {"78322626","財團法人印刷工業技術研究中心" },
                //    {"01891206","財團法人紡織產業綜合研究所" },
                //    {"17602060","財團法人中央畜產會" },
                //    {"01030642","財團法人台灣手工業推廣中心" },
                //    {"01107297","財團法人中衛發展中心" },
                //    {"77217652","財團法人鞋類暨運動休閒科技研發中心" }
                //};

                //return MyCommonUtil.ConvertCheckBoxItems(dictionary);


                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(WDAIIP.WEB.Commons.StaticCodeMap.CodeMap.GOVORGID);

                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }

        /// <summary>
        /// 訓練期間-年度項目
        /// </summary>
        public IEnumerable<SelectListItem> YEAR_list
        {
            get
            {
                //產生年度下拉選項清單
                List<SelectListItem> listYears = new List<SelectListItem>();
                DateTime dNow = System.DateTime.Now;

                int nowYear = dNow.Year;
                int nowMonth = dNow.Month;

                //年度下拉顯示自系統年度往後推1年
                for (int y = nowYear; y <= (nowYear + 1); y++)
                {
                    listYears.Add(new SelectListItem { Text = (y - 1911).ToString(), Value = y.ToString() });
                }

                return listYears;
            }
        }

        /// <summary>
        /// 訓練期間-月份項目
        /// </summary>
        public IEnumerable<SelectListItem> MON_list
        {
            get
            {
                List<SelectListItem> listMonths = new List<SelectListItem>();
                for (int m = 1; m <= 12; m++)
                {
                    listMonths.Add(new SelectListItem { Text = m.ToString("00"), Value = m.ToString("00") });
                }
                
                return listMonths;
            }
        }

        public void Valid(ModelStateDictionary modelState)
        {
            DateTime sdate = DateTime.Now;
            DateTime edate = DateTime.Now;
            bool blFlag = false;

            if (!string.IsNullOrEmpty(this.Form.STDATE))
            {
                if (!DateTime.TryParse(this.Form.STDATE, out sdate))
                {
                    blFlag = true;
                    modelState.AddModelError("", "訓練期間起日格式錯誤");
                }
            }

            if (!string.IsNullOrEmpty(this.Form.FTDATE))
            {
                if (!DateTime.TryParse(this.Form.FTDATE, out edate))
                {
                    blFlag = true;
                    modelState.AddModelError("", "訓練期間迄日格式錯誤");
                }
            }

            if (!blFlag && !string.IsNullOrEmpty(this.Form.STDATE) && !string.IsNullOrEmpty(this.Form.FTDATE))
            {
                if (DateTime.Compare(sdate,edate) > 0)
                {
                    modelState.AddModelError("", "訓練期間起年月 不得大於 訓練期間迄年月");
                }
            }
        }
    }
    #endregion

    #region GovClassSearchFormModel
    public class GovClassSearchFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 課程關鍵字
        /// </summary>
        [Display(Name = "課程關鍵字")]
        public string CLASSCNAME { get; set; }

        [Display(Name = "訓練單位")]
        public string ORGIDITEM { get; set; }

        /// <summary>
        /// 訓練單位
        /// </summary>
        [Display(Name = "訓練單位")]
        public string[] ORGIDITEM_SHOW
        {
            get
            {
                if (this.ORGIDITEM != null)
                {
                    return this.ORGIDITEM.Replace("'", "").Split(',');
                }
                else
                {
                    return new string[0];
                }
            }
            set
            {
                if (value != null)
                {
                    this.ORGIDITEM = MyCommonUtil.ConvertToWhereInValues(value.ToList());
                }
            }
        }

        /// <summary>
        /// 訓練單位名稱
        /// </summary>
        public string ORGIDITEM_TEXT
        {
           get
            {
                string rtn = string.Empty;
                string[] orgIDAry = { };

                if (!string.IsNullOrEmpty(this.ORGIDITEM))
                {
                    orgIDAry = this.ORGIDITEM.Replace("'", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    MyKeyMapDAO dao = new MyKeyMapDAO();
                    IList<KeyMapModel> list = dao.GetCodeMapList(WDAIIP.WEB.Commons.StaticCodeMap.CodeMap.GOVORGID);

                    foreach (string item in orgIDAry)
                    {
                        if (!string.IsNullOrEmpty(rtn)) rtn += "、";
                        foreach (var item2 in list)
                        {
                            if (item.Equals(item2.CODE))
                            {
                                rtn += item2.TEXT;
                                break;
                            }
                        }
                    }
                }
                return rtn;
            }
        }

        /// <summary>
        /// 開訓日期 (yyyyMMdd)
        /// </summary>
        public string STDATE
        {
            get
            {
                string rtn = string.Empty;
                if (!string.IsNullOrEmpty(this.STDATE_YEAR) && !string.IsNullOrEmpty(this.STDATE_MON))
                {
                    rtn = this.STDATE_YEAR + "/" + this.STDATE_MON.PadLeft(2, '0') + "/01 00:00:00";
                }

                return rtn;
            }
        }

        /// <summary>
        /// 結訓日期 (yyyyMMdd)
        /// </summary>
        public string FTDATE {
            get
            {
                string rtn = string.Empty;
                if (!string.IsNullOrEmpty(this.FTDATE_YEAR) && !string.IsNullOrEmpty(this.FTDATE_MON))
                {
                    rtn = this.FTDATE_YEAR + "/" + this.FTDATE_MON.PadLeft(2, '0') + "/01 00:00:00";
                }

                return rtn;
            }
        }

        /// <summary>
        /// 開訓日期_起年/訓練日期_起年
        /// </summary>
        public string STDATE_YEAR { get; set;}

        /// <summary>
        /// 開訓日期_起月/訓練日期_起月
        /// </summary>
        public string STDATE_MON { get; set; }

        /// <summary>
        /// 開訓日期_迄年/訓練日期_迄年
        /// </summary>
        public string FTDATE_YEAR { get; set; }

        /// <summary>
        /// 開訓日期_迄月/訓練日期_迄月
        /// </summary>
        public string FTDATE_MON { get; set; }

        /// <summary>
        /// 訓練迄日（組合的參數內容）
        /// </summary>
        public string STDATEBIND { get { return (this.STDATE_YEAR != null && this.STDATE_MON != null) ? (this.STDATE_YEAR + "/" + this.STDATE_MON + "/01") : null; } }
        public DateTime? STDATEBINDDT
        {
            get
            {
                string strDate = (this.STDATE_YEAR != null && this.STDATE_MON != null) ? (this.STDATE_YEAR + "/" + this.STDATE_MON + "/01") : null;

                DateTime? date = DateTime.ParseExact(strDate,
                                  "yyyy/MM/dd",
                                   CultureInfo.InvariantCulture);

                return date;
            }
        }

        /// <summary>
        /// 訓練迄日（組合的參數內容）
        /// </summary>
        public string FTDATEBIND { get { return (this.FTDATE_YEAR != null && this.FTDATE_MON != null) ? (this.FTDATE_YEAR + "/" + this.FTDATE_MON + "/01") : null; } }
        public DateTime? FTDATEBINDDT
        {
            get
            {
                string strDate = (this.FTDATE_YEAR != null && this.FTDATE_MON != null) ? (this.FTDATE_YEAR + "/" + this.FTDATE_MON + "/01") : null;

                DateTime? date = DateTime.ParseExact(strDate,
                                  "yyyy/MM/dd",
                                   CultureInfo.InvariantCulture);

                return date;
            }
        }

        /// <summary>
        /// 排序依據欄位名稱
        /// </summary>
        public string SORTFIELD { get; set; }

        /// <summary>
        /// 排序方式（遞減）
        /// </summary>
        public string SORTDESC { get; set; }
    }
    #endregion

    #region GovClassSearchGridModel
    public class GovClassSearchGridModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string DSRC { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        public Int64? CPID { get; set; }

        /// <summary>
        /// 縣市代碼
        /// </summary>
        public Int64? CTID { get; set; }

        /// <summary>
        /// 報名起日
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名迄日
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 報名起日 (民國年yyy/MM/dd)
        /// </summary>
        public string SENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日 (民國年yyy/MM/dd)
        /// </summary>
        public string FENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FENTERDATE); }
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
        /// 開訓日期 (民國年yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日期 (民國年yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        /// <summary>
        /// 班級名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 訓練單位名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 上課地點(縣市名)
        /// </summary>
        public string CLASSPOS { get; set; }

        /// <summary>
        /// 訓練時段
        /// </summary>
        public string HOURRANNAME { get; set; }
    }

    #endregion

    #region GovClassSearchDetailModel
    public class GovClassSearchDetailModel : TblCLASS_PLAN_INFO
    {
        /// <summary>
        /// 訓練時段
        /// </summary>
        public string HOURRANNAME { get; set; }

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
        /// 報名起日 (民國年 yyy/MM/dd)
        /// </summary>
        public string SENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日 (民國年 yyy/MM/dd)
        /// </summary>
        public string FENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FENTERDATE); }
        }

        /// <summary>
        /// 訓練性質 (0:不拘 1:職前 2:在職)
        /// </summary>
        public string TPROPERTY { get; set; }

        /// <summary>
        /// 甄試日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string EXAMDATE_TW { get; set; }

        /// <summary>
        /// 備註說明
        /// </summary>
        public string MEMO { get; set; }

        /// <summary>
        /// 訓練職類名稱
        /// </summary>
        public string TRAINNAME { get; set; }

        /// <summary>
        /// 性別中文描述
        /// </summary>
        public string CAPSEX_TEXT { get; set; }

        /// <summary>
        /// 年齡區間
        /// </summary>
        public string CAPAGE { get; set; }

        /// <summary>
        /// 學歷中文描述
        /// </summary>
        public string CAPDEGREE_TEXT { get; set; }

        /// <summary>
        /// 兵役中文描述
        /// </summary>
        public string CAPMILITARY_TEXT { get; set; }


    }
    #endregion 
}