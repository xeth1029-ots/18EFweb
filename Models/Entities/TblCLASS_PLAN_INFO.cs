using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// CLASS_PLAN_INFO  計畫課程資料檔  資料表實體
    /// </summary>
    public class TblCLASS_PLAN_INFO : IDBRow
    {
        /// <summary>
        /// 序號
        /// </summary>
        public Int64? CPID { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 機構名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 統一編號
        /// </summary>
        public string COMIDNO { get; set; }

        /// <summary>
        /// 班級名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 課程內容
        /// </summary>
        public string CONTENT { get; set; }

        /// <summary>
        /// 課程目標
        /// </summary>
        public string PURPOSE { get; set; }

        /// <summary>
        /// 訓練性質 (1: 民間機構 2: 其他政府單位)
        /// </summary>
        public Int64? TPROPERTYID { get; set; }

        /// <summary>
        /// 訓練職類代碼 (ref: Key_TrainType)
        /// </summary>
        public Int64? TMID { get; set; }

        /// <summary>
        /// 甄試日期
        /// </summary>
        public DateTime? EXAMDATE { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 報名起日
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名迄日
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 報到日期
        /// </summary>
        public DateTime? CHECKINDATE { get; set; }

        /// <summary>
        /// 訓練地點Zip
        /// </summary>
        public Int64? TADDRESSZIP { get; set; }

        /// <summary>
        /// 訓練地點
        /// </summary>
        public string TADDRESS { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public Int64? THOURS { get; set; }

        /// <summary>
        /// 訓練人數
        /// </summary>
        public Int64? TNUM { get; set; }

        /// <summary>
        /// 訓練期限代碼
        /// </summary>
        public string TDEADLINE { get; set; }

        /// <summary>
        /// 訓練時段代碼
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// 訓練目標
        /// </summary>
        public string PLANCAUSE { get; set; }

        /// <summary>
        /// 受訓資格-學歷
        /// </summary>
        public string CAPDEGREE { get; set; }

        /// <summary>
        /// 受訓資格-年齡起
        /// </summary>
        public Int64? CAPAGE1 { get; set; }

        /// <summary>
        /// 受訓資格-年齡迄
        /// </summary>
        public Int64? CAPAGE2 { get; set; }

        /// <summary>
        /// 受訓資格-性別(0:不限, M:男, F:女)
        /// </summary>
        public string CAPSEX { get; set; }

        /// <summary>
        /// 受訓資格-兵役
        /// </summary>
        public string CAPMILITARY { get; set; }

        /// <summary>
        /// 學科
        /// </summary>
        public string TMSCIENCE { get; set; }

        /// <summary>
        /// 術科
        /// </summary>
        public string TMTECH { get; set; }

        /// <summary>
        /// 課程編配-學科-一般學科時數
        /// </summary>
        public Int64? GENSCIHOURS { get; set; }

        /// <summary>
        /// 課程編配-學科-專業學科時數
        /// </summary>
        public Int64? PROSCIHOURS { get; set; }

        /// <summary>
        /// 課程編配-術科時數
        /// </summary>
        public Int64? PROTECHHOURS { get; set; }

        /// <summary>
        /// 課程編配-其它時數
        /// </summary>
        public Int64? OTHERHOURS { get; set; }

        /// <summary>
        /// 課程編配-總時數 
        /// </summary>
        public Int64? TOTALHOURS { get; set; }

        /// <summary>
        /// 經費來源-局負擔$
        /// </summary>
        public Int64? DEFMAINCOST { get; set; }

        /// <summary>
        /// 經費來源-單位負擔$
        /// </summary>
        public Int64? DEFUNITCOST { get; set; }

        /// <summary>
        /// 經費來源-學員負擔$
        /// </summary>
        public Int64? DEFSTDCOST { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public DateTime? APPLIEDDATE { get; set; }

        /// <summary>
        /// 受訓資格-其他一
        /// </summary>
        public string CAPOTHER1 { get; set; }

        /// <summary>
        /// 受訓資格-其他二
        /// </summary>
        public string CAPOTHER2 { get; set; }

        /// <summary>
        /// 受訓資格-其他三
        /// </summary>
        public string CAPOTHER3 { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public string MODIFYACCT { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>e網不顯示</summary>
        public string EVTA_NOSHOW { get; set; }

        /// <summary>計畫'1:自辦(內訓) 2:委外</summary>
        public Int64? PLANKIND { get; set; }

        /// <summary>
        /// 課程揭露管道
        /// </summary>
        public string MSGCHANNEL { get; set; }

        /// <summary>
        /// 聯絡人
        /// </summary>
        public string CONTACTNAME { get; set; }

        /// <summary>聯絡人電話</summary>
        public string CONTACTPHONE { get; set; }
        /// <summary>聯絡人行動</summary>
        public string CONTACTMOBILE { get; set; }

        /// <summary>訓練性質 1:職前 2:進修</summary>
        public Int64? TRAINICE { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string HOURSMEMO { get; set; }

        /// <summary>
        /// 備註2
        /// </summary>
        public string HOURSMEMO2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string YOUNGFLAG { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PORGNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PCOMIDNO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SUBORGNAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CLASSNOTITLE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string YOUTHJOBS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CAPAGESEL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VERYJRESULT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? VERYJDATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? VERYJRELTIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VERYJREASON { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VERYJACCOUNT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VERYJTPSTEP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int64? TPROPERTYID2 { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.CLASS_PLAN_INFO;
        }
    }
}