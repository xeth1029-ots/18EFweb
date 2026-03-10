using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models
{
    #region EnterClassViewModel
    public class EnterClassViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public EnterClassViewModel()
        {

        }


        /// <summary>
        /// 報名記錄列表
        /// </summary>
        public IList<EnterClassGridModel> Grid { get; set; }
    }
    #endregion

    #region EnterClassGridModel
    public class EnterClassGridModel : ClassClassInfoExtModel
    {
        /// <summary>計畫別</summary>
        public string PLANTYPE
        {
            get
            {
                string rtn = string.Empty;
                //在職
                if (this.TPLANID == "06") { return "2"; }
                //產投
                if (this.TPLANID == "28") { return "1"; }
                //區域產業據點職業訓練計畫(在職)
                if (this.TPLANID == "70") { return "5"; }
                return rtn;
            }
        }

        /// <summary> 資料別 （1 產投, 2 在職（e網報名）, 3在職（現場報名））</summary>
        public string PTYPE { get; set; }

        /// <summary>是否允許取消報名 (Y:允許,N:不允許)</summary>
        public string CANCANCEL { get; set; }

        /// <summary>報名管道 (1.網;2.現;3.通;4.推)   報名: 網路、現場(含通訊)、推介</summary>
        public Int64? ENTERCHANNEL { get; set; }

        /// <summary>stud_entertemp2 pk</summary>
        public Int64? ESERNUM { get; set; }

        /// <summary>stud_entertype2 pk</summary>
        public Int64? ESETID { get; set; }

        /// <summary>stud_entertrain2 pk</summary>
        public Int64? SEID { get; set; }

        /// <summary>計畫名稱</summary>
        public string PLANNAME { get; set; }

        /// <summary>報名序號</summary>
        public Int64? SIGNNO { get; set; }

        /// <summary>報名狀態(中文描述)</summary>
        public string SIGNNO_TEXT { get; set; }

        /// <summary>報名狀態 (0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取) </summary>
        public Int64? SIGNUPSTATUS { get; set; }

        /// <summary>報名狀態(顯示用) (1:審核中 3:正取 4:備取 5:未錄取)</summary>
        public Int64? SIGNUPSTATUS_NEW { get; set; }

        /// <summary>報名備註</summary>
        public string SIGNUPMEMO { get; set; }

        /// <summary>若超過開訓日 且超過報名結束日 則為1 否為0</summary>
        public int? XDAY1 { get; set; }

        /// <summary>開訓後14天</summary>
        public int? XDAY14 { get; set; }

        /// <summary>姓名</summary>
        public string NAME { get; set; }

        /// <summary>是否為不開班</summary>
        public string NOTOPENM { get; set; }

        /// <summary>准考證號</summary>
        public string EXAMNO { get; set; }

        /// <summary>准考證號顯示文字</summary>
        public string EXAMNO_TEXT { get; set; }

        /// <summary>報名日期</summary>
        public DateTime? RELENTERDATE { get; set; }

        /// <summary>報名日期 (民國年 yyy/MM/dd)</summary>
        public string RELENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.RELENTERDATE); }
        }

        /// <summary>系統時間</summary>
        public DateTime? ATODAY { get; set; }

        /// <summary>系統時間 (民國年 yyy/MM/dd HH:mm:ss)</summary>
        public string ATODAY_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.ATODAY); }
        }

        /// <summary>取消報名說明</summary>
        public string NOCANCELMSG { get; set; }

        /// <summary>班級狀況說明</summary>
        public string CLASSDATEMSG { get; set; }

        /// <summary>學員狀況說明</summary>
        public string STUDTRAINMSG { get; set; }

        /// <summary>學員狀況</summary>
        public Int16? STUDSTATUS { get; set; }

        /// <summary>放棄報到</summary> 
        public string ABANDON { get; set; }
        /// <summary>放棄報到(原因)</summary> 
        public string ABANDONReason { get; set; }
        /// <summary>放棄報到異動者</summary> 
        public string ABANDONACCT { get; set; }
        /// <summary>放棄報到異動時間</summary> 
        public DateTime? ABANDONDATE { get; set; }

    }
    #endregion
}