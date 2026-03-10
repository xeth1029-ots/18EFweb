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
    #region StudQuestionViewModel
    /// <summary>
    /// StudQuestion 課後意見調查 ViewModel
    /// </summary>
    [Serializable]
    public class StudQuestionViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public StudQuestionViewModel()
        {
            this.Form = new StudQuestionFormModel();
        }

        /// <summary>
        /// 功能說明內容
        /// </summary>
        public string FuncContent
        {
            get
            {
                return new MyKeyMapDAO().GetFuncContent("002", "5");
            }
        }

        /// <summary>
        /// 查詢條件
        /// </summary>
        public StudQuestionFormModel Form { get; set; }

        /// <summary>查詢結果清單</summary>
        public IList<StudQuestionGridModel> Grid { get; set; }

        /// <summary>參訓學員意見調查表-明細</summary>
        public FacDetailModel FacDetail { get; set; }

        /// <summary>參訓學員訓後動態調查表-明細</summary>
        public FinDetailModel FinDetail { get; set; }

        /// <summary>Train-受訓期間意見調查表</summary>
        public TrainDetailModel TrainDetail { get; set; }

        /// <summary>Tion-期末學員滿意度調查表</summary>
        public TionDetailModel TionDetail { get; set; }
    }
    #endregion

    #region StudQuestionFormModel
    [Serializable]
    public class StudQuestionFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 調查表類型 1意見調查表 2.訓後調查表 aFac-1.參訓學員意見調查表/aFin-2.參訓學員訓後動態調查表/aTrain-3.受訓期間意見調查表/aTion-4.期末學員滿意度調查表
        /// </summary>
        public string QueType { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 生日 YYYY/MM/DD
        /// </summary>
        public string BIRTHDAY { get; set; }

        public static bool CheckArgument(HttpContextBase context)
        {
            NameValueCollection parms = context.Request.Params;
            bool result = true;
            try
            {
                var props = new string[] { "QueType", "IDNO", "BIRTHDAY" };

                var match = (from a in props
                             from b in parms.AllKeys
                             where b.Contains(a)
                             select b).ToList();

                if (match == null || match.Count == 0)
                {
                    throw new ArgumentException("StudQuestionViewModel");
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

    #region StudQuestionGridModel
    /// <summary>
    /// 調查表查詢結果清單
    /// </summary>
    [Serializable]
    public class StudQuestionGridModel
    {
        /// <summary>
        /// 是否可填寫 (訓後動態意見調查表)
        /// Y 是, N 否
        /// </summary>
        public string CANWRITE { get; set; }

        /// <summary>
        /// 是否可填寫(意見調查表)
        /// Y 是, N 否
        /// </summary>
        public string FACCANWRITE { get; set; }

        /// <summary>是否已填寫 0 查無此資料 ,1 已填寫過</summary>
        public string FILLSTATUS { get; set; }

        /// <summary>
        /// 是否已填寫代碼中文描述
        /// </summary>
        public string FILLSTATUS_TEXT
        {
            get { return ("1".Equals(this.FILLSTATUS) ? "已填寫" : "尚未填寫"); }
        }

        /// <summary>
        /// 調查表版本
        /// </summary>
        public string QUEVER { get; set; }

        /// <summary>
        /// 填寫時間檢核訊息(不符填寫時段需顯示)
        /// </summary>
        public string CANWRITEMSG { get; set; }

        /// <summary>
        /// 對應開班基本資料OCID
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 班級學員序號
        /// </summary>
        public Int64? SOCID { get; set; }

        /// <summary>
        /// 學員代碼-問卷填寫記錄
        /// </summary>
        public Int64? QUESOCID { get; set; }

        /// <summary>
        /// 班級學員序號,是否有填過調查表
        /// </summary>
        public string SOCIDCHK { get; set; }

        /// <summary>
        /// 系統日
        /// </summary>
        public DateTime? ATODAY { get; set; }

        /// <summary>
        /// 系統日 yyyy/MM/dd
        /// </summary>
        public string ATODAY_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.ATODAY); }
        }

        /// <summary>
        /// 班別中文名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// 班別名稱
        /// </summary>
        public string CLASSNAME { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public string YEARS { get; set; }

        /// <summary>
        /// 計畫年度 yyyy
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 計畫年度 (民國年 yyy)
        /// </summary>
        public string PLANYEAR_TW
        {
            get
            {
                string rtn = string.Empty;
                int yr = 0;

                if (!string.IsNullOrEmpty(PLANYEAR) && int.TryParse(this.PLANYEAR, out yr))
                {
                    rtn = (yr > 1911 ? yr - 1911 : yr).ToString();
                }
                return rtn;
            }
        }

        /// <summary>
        /// 開訓日期 
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 開訓日期 (西元年 yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 結訓日期 (西元年 yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>
        /// 結訓日期往後推3個月加1天
        /// </summary>
        public DateTime? WRDATE1 { get; set; }

        /// <summary>
        /// 結訓日期往後推3個月加1天(西元年 yyyy/MM/dd)
        /// </summary>
        public string WRDATE1_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.WRDATE1); }
        }

        /// <summary>
        /// 結訓日期往後推4個月
        /// </summary>
        public DateTime? WRDATE2 { get; set; }

        /// <summary>
        /// 結訓日期往後推4個月(西元年 yyyy/MM/dd)
        /// </summary>
        public string WRDATE2_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.WRDATE2); }
        }

        /// <summary>
        /// 意見填查表填寫結止日期(起日為開訓日，迄日為結訓後21日內)
        /// </summary>
        public DateTime? FACDATE { get; set; }

        /// <summary>
        /// 意見填查表填寫結止日期（西元年 yyyy/MM/dd）
        /// </summary>
        public string FACDATE_AD { get; set; }
    }
    #endregion

    #region FACDetailModel

    [Serializable]
    public class FacDetailModel : TblSTUD_QUESTIONFAC2
    {
        public FacDetailModel()
        {
            this.QueType = "1";
            this.IsNew = true;
            this.DASOURCE = 1; //產投外網網
        }

        /// <summary>
        /// 調查表類型
        /// 1.意見調查表  2.訓後動態調查表
        /// </summary>
        [NotDBField]
        public string QueType { get; set; }

        /// <summary>
        /// 是否為新增模式
        /// </summary>
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// 用來設定資料庫儲存時的模式: CREATE, UPDATE, DELETE
        /// </summary>
        [NotDBField]
        public string DB_ACTION { get; set; }

        /// <summary>
        /// 班級編號
        /// </summary>
        [NotDBField]
        public Int64? OCID { get; set; }

        /// <summary>
        /// 計畫年度（西元年）
        /// </summary>
        [NotDBField]
        public string YEARS { get; set; }

        /// <summary>第三部份：(二)其他建議 長度超過系統範圍 (長度限制) </summary>
        [NotDBField]
        public int C21_NOTE_MaxLength
        {
            get { return 700; }
        }

        /// <summary>計畫名稱</summary>
        [NotDBField]
        public string ORGPLANNAME { get; set; }

        #region 學員基本資料
        /// <summary>
        /// 
        /// </summary>
        public new string S11
        {
            get { return (this.S11_CHECKED ? "Y" : ""); }
            set
            {
                this.S11_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.S11_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool S11_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string S12
        {
            get { return (this.S12_CHECKED ? "Y" : ""); }
            set
            {
                this.S12_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.S12_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool S12_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string S13
        {
            get { return (this.S13_CHECKED ? "Y" : ""); }
            set
            {
                this.S13_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.S13_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool S13_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string S14
        {
            get { return (this.S14_CHECKED ? "Y" : ""); }
            set
            {
                this.S14_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.S14_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool S14_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string S15
        {
            get { return (this.S15_CHECKED ? "Y" : ""); }
            set
            {
                this.S15_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.S15_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool S15_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string S16
        {
            get { return (this.S16_CHECKED ? "Y" : ""); }
            set
            {
                this.S16_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.S16_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool S16_CHECKED { get; set; }
        #endregion

        #region 第一部份：參加產投方案考量因素
        /// <summary>
        /// 
        /// </summary>
        public new string A1_1
        {
            get { return (this.A1_1_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_1_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_1_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_1_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_2
        {
            get { return (this.A1_2_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_2_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_2_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_2_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_3
        {
            get { return (this.A1_3_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_3_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_3_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_3_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_4
        {
            get { return (this.A1_4_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_4_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_4_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_4_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_5
        {
            get { return (this.A1_5_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_5_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_5_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_5_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_6
        {
            get { return (this.A1_6_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_6_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_6_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_6_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_7
        {
            get { return (this.A1_7_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_7_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_7_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_7_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_8
        {
            get { return (this.A1_8_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_8_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_8_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_8_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_9
        {
            get { return (this.A1_9_CHECKED ? "Y" : ""); }
            set
            {
                this.A1_9_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_9_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_9_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string A1_10
        {
            get { return (this.A1_10_CHECKED ? "Y" : ""); }
            set
            {
                //this.A1_10_CHECKED = false;
                if ("Y".Equals(value))
                {
                    this.A1_10_CHECKED = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool A1_10_CHECKED { get; set; }

        #endregion

        //public void MapTo()
        //{
        //    ClearFieldMap cfm = model.GetClearFieldMap();
        //    cfm.Add(this);
        //    cfm.Remove((StudQuestionFac2 x) => x.SOCID);
        //    /*ClearFieldMap cfm = this.GetClearFieldMap();
        //    cfm.Remove((StudQuestionFac2 x) => x.SOCID);*/
        //}
    }
    #endregion

    #region FinDetailModel
    [Serializable]
    public class FinDetailModel : TblSTUD_QUESTIONFIN
    {
        public FinDetailModel()
        {
            this.QueType = "2";
            this.IsNew = true;
            this.DASOURCE = 1;
        }

        /// <summary>
        /// 調查表類型
        /// 1.意見調查表  2.訓後動態調查表
        /// </summary>
        [NotDBField]
        public string QueType { get; set; }

        /// <summary>
        /// 是否為新增模式
        /// </summary>
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// 用來設定資料庫儲存時的模式: CREATE, UPDATE, DELETE
        /// </summary>
        [NotDBField]
        public string DB_ACTION { get; set; }

        /// <summary>
        /// 學員編號
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        [NotDBField]
        public string ORGPLANNAME { get; set; }

        /// <summary>
        /// 計畫年度（西元年）
        /// </summary>
        [NotDBField]
        public string YEARS { get; set; }

        /// <summary>
        /// 學號
        /// </summary>
        [NotDBField]
        public string STUDENTID { get; set; }

        /// <summary>
        /// 學員姓名 
        /// </summary>
        [NotDBField]
        public string NAME { get; set; }

        /// <summary>
        /// 學員狀態 
        /// 1.在訓2.離訓 3.退訓4.續訓 5.結訓
        /// </summary>
        [NotDBField]
        public Int64? STUDSTATUS { get; set; }

        /// <summary>
        /// 離訓日期 
        /// </summary>
        [NotDBField]
        public DateTime? REJECTTDATE1 { get; set; }

        /// <summary>
        /// 退訓日期
        /// </summary>
        [NotDBField]
        public DateTime? REJECTTDATE2 { get; set; }

        /// <summary>
        /// 機構別
        /// </summary>
        [NotDBField]
        public string ORGKIND { get; set; }

        #region 一、學員部分
        /// <summary>
        ///
        /// </summary>
        public string Q1_4Sub { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string Q7MR1
        {
            get { return (this.Q7MR1_CHECKED ? "Y" : ""); }
            set { this.Q7MR1_CHECKED = "Y".Equals(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool Q7MR1_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string Q7MR2
        {
            get { return (this.Q7MR2_CHECKED ? "Y" : ""); }
            set { this.Q7MR2_CHECKED = "Y".Equals(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool Q7MR2_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string Q7MR3
        {
            get { return (this.Q7MR3_CHECKED ? "Y" : ""); }
            set { this.Q7MR3_CHECKED = "Y".Equals(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool Q7MR3_CHECKED { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new string Q7MR4
        {
            get { return (this.Q7MR4_CHECKED ? "Y" : ""); }
            set { this.Q7MR4_CHECKED = "Y".Equals(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NotDBField]
        public bool Q7MR4_CHECKED { get; set; }
        #endregion

        //public void MapTo()
        //{
        //    ClearFieldMap cfm = this.GetClearFieldMap();
        //    cfm.Remove((StudQuestionFin x) => x.SOCID);
        //}
    }
    #endregion

    #region TrainDetailModel
    [Serializable]
    public class TrainDetailModel : TblSTUD_QUESTRAINING
    {
        public TrainDetailModel()
        {
            this.QueType = "3";
            this.IsNew = true;
            this.DASOURCE = 1;
        }

        /// <summary>
        /// 調查表類型 1.意見調查表  2.訓後動態調查表 3.受訓期間意見調查表 TblSTUD_QUESTRAINING 4.期末學員滿意度調查表 TblSTUD_QUESTIONARY 
        /// </summary>
        [NotDBField]
        public string QueType { get; set; }

        /// <summary>
        /// 是否為新增模式
        /// </summary>
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// 用來設定資料庫儲存時的模式: CREATE, UPDATE, DELETE
        /// </summary>
        [NotDBField]
        public string DB_ACTION { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        [NotDBField]
        public string PLANNAME { get; set; }

        /// <summary>
        /// 計畫年度（西元年）
        /// </summary>
        [NotDBField]
        public string YEARS { get; set; }

        /// <summary>
        /// 學號
        /// </summary>
        [NotDBField]
        public string STUDENTID { get; set; }

        /// <summary>
        /// 學員姓名 
        /// </summary>
        [NotDBField]
        public string NAME { get; set; }

        /// <summary>
        /// 學員狀態 
        /// 1.在訓2.離訓 3.退訓4.續訓 5.結訓
        /// </summary>
        [NotDBField]
        public Int64? STUDSTATUS { get; set; }

        /// <summary>
        /// 離訓日期 
        /// </summary>
        [NotDBField]
        public DateTime? REJECTTDATE1 { get; set; }

        /// <summary>
        /// 退訓日期
        /// </summary>
        [NotDBField]
        public DateTime? REJECTTDATE2 { get; set; }

        /// <summary>
        /// 機構別
        /// </summary>
        [NotDBField]
        public string ORGKIND { get; set; }


    }
    #endregion

    #region TionDetailModel
    [Serializable]
    public class TionDetailModel : TblSTUD_QUESTIONARY
    {
        public TionDetailModel()
        {
            this.QueType = "4";
            this.IsNew = true;
            //this.DASOURCE = 1;
        }

        /// <summary>
        /// 調查表類型 1.意見調查表  2.訓後動態調查表 3.受訓期間意見調查表 TblSTUD_QUESTRAINING 4.期末學員滿意度調查表 TblSTUD_QUESTIONARY 
        /// </summary>
        [NotDBField]
        public string QueType { get; set; }

        /// <summary>
        /// 是否為新增模式
        /// </summary>
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// 用來設定資料庫儲存時的模式: CREATE, UPDATE, DELETE
        /// </summary>
        [NotDBField]
        public string DB_ACTION { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        [NotDBField]
        public string PLANNAME { get; set; }

        /// <summary>問卷代號</summary>
        [NotDBField]
        public string QNAME { get; set; }

        /// <summary>問卷名稱</summary>
        [NotDBField]
        public string QNOTE { get; set; }

        /// <summary>
        /// 計畫年度（西元年）
        /// </summary>
        [NotDBField]
        public string YEARS { get; set; }

        /// <summary>
        /// 學員SOCID
        /// </summary>
        [NotDBField]
        public Int64? SOCID { get; set; }

        /// <summary>
        /// 學號
        /// </summary>
        [NotDBField]
        public string STUDENTID { get; set; }

        /// <summary>
        /// 學員姓名 
        /// </summary>
        [NotDBField]
        public string NAME { get; set; }

        /// <summary>
        /// 學員狀態 
        /// 1.在訓2.離訓 3.退訓4.續訓 5.結訓
        /// </summary>
        [NotDBField]
        public Int64? STUDSTATUS { get; set; }

        /// <summary>
        /// 離訓日期 
        /// </summary>
        [NotDBField]
        public DateTime? REJECTTDATE1 { get; set; }

        /// <summary>
        /// 退訓日期
        /// </summary>
        [NotDBField]
        public DateTime? REJECTTDATE2 { get; set; }

        /// <summary>
        /// 機構別
        /// </summary>
        [NotDBField]
        public string ORGKIND { get; set; }
    }
    #endregion

}