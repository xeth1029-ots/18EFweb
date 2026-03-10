using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.DataLayers;
using System.Web.Mvc;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models
{
    #region PlanClassSearchViewModel 
    public class PlanClassSearchViewModel
    {
        public PlanClassSearchViewModel()
        {
            this.Form = new PlanClassSearchFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public PlanClassSearchFormModel Form { get; set; }

        /// <summary> 查詢結果清單(for 產投) Grid1Model </summary>
        public IList<PlanClassSearchGrid1Model> Grid1 { get; set; }

        /// <summary> 查詢結果清單(for 在職進修) Grid2Model </summary>
        public IList<PlanClassSearchGrid2Model> Grid2 { get; set; }

        /// <summary> 查詢結果清單(for 接受企業委託訓練) Grid2Model</summary>
        public IList<PlanClassSearchGrid2Model> Grid3 { get; set; }

        /// <summary>
        /// 查詢結果清單(for 充電起飛) Grid1Model
        /// </summary>
        public IList<PlanClassSearchGrid1Model> Grid4 { get; set; }


        /// <summary>
        /// 查詢結果清單(for 區域產業據點) Grid5Model
        /// </summary>
        public IList<PlanClassSearchGrid5Model> Grid5 { get; set; }
    }
    #endregion

    #region PlanClassSearchFormModel
    public class PlanClassSearchFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 是否為第一次進入此功能 (Y 是, N 否)
        /// </summary>
        public string IsFirst { get; set; }

        /// <summary>
        /// 查詢類別 "1":產業人才投資方案、"2":在職進修訓練、"3":充電起飛、"4":接受企業委託訓練、"5":區域產業據點職業訓練計畫(在職)
        /// </summary>       
        public string PlanType { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        //public Int64? OCID { get; set; }

        /// <summary>
        /// 計畫代碼
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary> 職訓專長能力 </summary>
        public string ABILITYS { get; set; }

        /// <summary> 職訓專長能力 </summary>
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

        public bool CheckArgument()
        {
            bool result = (this.PlanType == null && this.TPLANID == null) ? false : true;
            return result;
        }

        /// <summary>檢核必要輸入若其中為空則為false </summary>
        /// <returns></returns>
        public bool CheckArgument2()
        {
            bool result = (string.IsNullOrEmpty(this.PlanType) || string.IsNullOrEmpty(this.ABILITYS)) ? false : true;
            return result;
        }

    }

    #endregion

    #region PlanClassSearchGrid1Model
    public class PlanClassSearchGrid1Model
    {
        /// <summary>
        /// 選取Checkbox
        /// </summary>
        //public bool SELECTIS { get; set; }

        /// <summary>
        /// 開班流水號
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary> 班別中文名稱 </summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 班別中文名稱+期別 </summary>
        public string CLASSCNAME2 { get; set; }

        /// <summary>
        /// 訓練人數
        /// </summary>
        public Int64? TNUM { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public Int64? THOURS { get; set; }

        /// <summary>
        /// 上課地址學科場地名稱
        /// </summary>
        public string CTNAME1 { get; set; }

        /// <summary>
        /// 上課地址術科場地名稱
        /// </summary>
        public string CTNAME2 { get; set; }

        /// <summary>
        /// 是否結訓
        /// </summary>
        public string ISCLOSED { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 報名起日期時間
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名訖日期時間
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 經費來源-政府負擔
        /// </summary>
        public string DEFGOVCOST { get; set; }

        /// <summary>
        /// 經費來源-學員負擔
        /// </summary>
        public string DEFSTDCOST { get; set; }

        /// <summary>
        /// 報名繳費方式中文
        /// </summary>
        public string ENTERSUPPLYSTYLE { get; set; }

        /// <summary>
        /// 招生狀態
        /// </summary>
        public Int64? ADMISSIONS { get; set; }

        /// <summary>
        /// 廠商(機構)名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 原班別中文名稱
        /// </summary>
        public string OLDCLASSCNAME { get; set; }

        // <summary> 上課距離 </summary> public Decimal? Distance { get; set; }
    }
    #endregion

    #region PlanClassSearchGrid2Model
    public class PlanClassSearchGrid2Model
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
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 課程名稱+期別
        /// </summary>
        public string TRAINCLASS { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
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
        /// 通俗職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 上課時間(在職課程)
        /// </summary>
        public string NOTE3 { get; set; }

        // <summary> 上課距離 </summary> public Decimal? Distance { get; set; }
    }
    #endregion

    public class PlanClassSearchGrid5Model
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
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 課程名稱+期別
        /// </summary>
        public string TRAINCLASS { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
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
        /// 通俗職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 上課時間(在職課程)
        /// </summary>
        public string NOTE3 { get; set; }

        // <summary> 上課距離 </summary> public Decimal? Distance { get; set; }
    }

}