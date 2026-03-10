using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;

namespace WDAIIP.WEB.Models
{
    #region ClassMatchViewModel
    public class ClassMatchViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public ClassMatchViewModel()
        {
            this.Form = new ClassMatchFormModel();
        }

        /// <summary>
        /// 查詢條件 
        /// </summary>
        public ClassMatchFormModel Form { get; set; }

        /// <summary>
        /// 產投課程查詢結果列表
        /// </summary>
        public IList<ClassSearchGrid1Model> Grid1 { get; set; }

        /// <summary>
        /// 自辦在職課程查詢結果列表
        /// </summary>
        public IList<ClassSearchGrid2Model> Grid2 { get; set; }

        /// <summary>
        /// 自辦在職通俗職類選項清單
        /// </summary>
        public IList<CJobNoGridModel> CJobNoGrid { get; set; }

        /// <summary>
        /// 產投職業類別選項清單（2018年改的版(實際是改成共三層)）
        /// </summary>
        public IList<TMIDGridModel> TMIDGrid { get; set; }

        /// <summary>
        /// 查詢計畫類型 清單來源
        /// </summary>
        public IList<SelectListItem> PlanType_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "1","產業人才投資方案" }, { "2","分署自辦在職訓練"}, { "5","區域產業據點"}
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 策略性產業別 清單來源(複選)
        /// </summary>
        public IList<CheckBoxListItem> TMID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetTmidList();
                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }

        /// <summary>
        /// 課程地點所屬縣市別(縣市) 清單來源
        /// </summary>
        public IList<CheckBoxListItem> CTID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();

                list.Insert(0, new KeyMapModel
                {
                    TEXT = "全部",
                    CODE = "000"
                });

                return MyCommonUtil.ConvertCheckBoxItems(list);
            }
        }

        /// <summary>
        /// 課程地點所屬縣市別(鄉鎮市區) 清單來源
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

        /// <summary>
        /// 分署別 清單來源
        /// </summary>
        public IList<CheckBoxListItem> DISTID_list
        {
            get
            {
                //etraining直接寫死
                var dictionary = new Dictionary<string, string>
                {
                    {"000","全部"},
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
        /// todo 通俗職類別 清單來源
        /// <summary>
        /// 

    }
    #endregion

    #region ClassMatchFormModel
    public class ClassMatchFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public ClassMatchFormModel()
        { }

        /// <summary>
        /// 查詢類別("1":產業人才投資方案、"2":在職進修訓練、"5":區域產業據點)
        /// </summary>
        [Display(Name = "類別")]
        public string PlanType { get; set; }

        /// <summary>
        /// 上課地點（分署）
        /// </summary>
        [Display(Name = "分署別")]
        public string DISTID { get; set; }

        /// <summary>
        /// 上課地點(checkboxlist顯示用)
        /// </summary>
        public string[] DISTID_SHOW
        {
            get
            {
                if (string.IsNullOrEmpty(this.DISTID))
                {
                    return new string[0];
                }
                else
                {
                    return this.DISTID.Replace("'", "").Split(',');
                }
            }

            set
            {
                this.DISTID = MyCommonUtil.ConvertToWhereInNumberValues(value.ToList());
            }
        }

        /// <summary>
        /// 依發展署策略性產業篩選
        /// </summary>
        [Display(Name = "策略性產業別")]
        public string TMID { get; set; }

        /// <summary>
        /// 依發展署策略性產業篩選(checkboxlist顯示用)
        /// </summary>
        public string[] TMID_SHOW
        {
            get
            {
                if (string.IsNullOrEmpty(this.TMID))
                {
                    return new string[0];
                }
                else
                {
                    return this.TMID.Replace("'", "").Split(',');
                }
            }
            set
            {
                this.TMID = MyCommonUtil.ConvertToWhereInNumberValues(value.ToList());
            }
        }

        /// <summary>
        /// 縣市代碼
        /// </summary>
        [Display(Name = "課程地點所屬縣市別")]
        public string CTID { get; set; }

        public string[] CTID_SHOW
        {
            get
            {
                if (string.IsNullOrEmpty(this.CTID))
                {
                    return new string[0];
                }
                else
                {
                    return this.CTID.Replace("'", "").Split(',');
                }
            }
            set
            {
                this.CTID = MyCommonUtil.ConvertToWhereInNumberValues(value.ToList());
            }
        }

        /// <summary>
        /// 鄉鎮市區代碼
        /// </summary>
        [Display(Name = "鄉鎮市區代碼")]
        public string ZIPCODE { get; set; }

        /// <summary>
        /// 是否定期收到在職進修訓練速配課程結資訊（Y：是 N：否）
        /// </summary>
        public string SENDMAIL06 { get; set; }

        /// <summary>
        /// 是否定期收到產投速配課程結果資訊（Y：是 N：否）
        /// </summary>
        public string SENDMAIL28 { get; set; }

        /// <summary>
        /// 是否定期收到速配課程結果資訊（Y：是 N：否）-區域產業據點
        /// </summary>
        public string SENDMAIL70 { get; set; }

        /// <summary>
        /// 訓練職類第二層選取結果
        /// </summary>
        public string TMIDRESULT { get; set; }

        /// <summary>
        /// 通俗職類第三層選取結果
        /// </summary>
        public string CJOBUNKEYRESULT { get; set; }
    }
    #endregion

    #region CJobNOGridModel
    /// <summary>
    /// 通俗職類選頁清單代碼
    /// </summary>
    public class CJobNoGridModel
    {
        /// <summary>
        /// 職類代碼
        /// </summary>
        public string CJOBTYPE { get; set; }

        /// <summary>
        /// 職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 通俗職類第二層
        /// </summary>
        public IList<CJobNOLv2GridModel> CJOBNOLv2ListItem { get; set; }
    }

    /// <summary>
    /// 通俗職類第二層
    /// </summary>
    public class CJobNOLv2GridModel
    {
        /// <summary>
        /// 職類代碼
        /// </summary>
        public string CJOBNO { get; set; }

        /// <summary>
        /// 職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        public string[] CJOBNOLv3SelValue { get; set; }

        /*public string CJOBNOLv3Sel { get; set; }

        public string[] CJOBNOLv3SelValue
        {
            get
            {
                if (string.IsNullOrEmpty(this.CJOBNOLv3Sel))
                {
                    return new string[0];
                }
                else
                {
                    return this.CJOBNOLv3Sel.Replace("'", "").Split(',');
                }
            }
            set
            {
                this.CJOBNOLv3Sel = MyCommonUtil.ConvertToWhereInNumberValues(value.ToList());
            }
        }
        */


        public IList<KeyMapModel> keyMap_list { get; set; }

        /// <summary>
        /// 通俗職類第三層選項
        /// </summary>
        public IEnumerable<CheckBoxListItem> CJOBNO_list { get; set; }
    }
    #endregion

    #region TMIDGridModel
    /// <summary>
    /// 產投職業類別結構（因為第一層只有一個項目(600 職類課程分類)，所以先略過不顯示）
    /// </summary>
    public class TMIDGridModel
    {
        /// <summary>
        /// 第二層職類代碼
        /// </summary>
        public string JOBTMID { get; set; }

        /// <summary>
        /// 第二層職類名稱
        /// </summary>
        public string JOBNAME { get; set; }

        /// <summary>
        /// 第三層職類選取結果
        /// </summary>
        public string[] TMIDLv2SelValue { get; set; }

        /// <summary>
        /// 第三層代碼選項
        /// </summary>
        public IList<KeyMapModel> keyMap_list { get; set; }

        /// <summary>
        /// 第三層選項checkbox
        /// </summary>
        public IEnumerable<CheckBoxListItem> TMID_list { get; set; }
    }
    #endregion
}