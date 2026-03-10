using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Models
{
    public class MemberDataModel
    {
        /// <summary>
        /// 身分證
        /// </summary>
        public string ACID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// 身分別
        /// </summary>
        public string ISFOREIGN { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string SEX { get; set; }

        /// <summary>
        /// 出生年月日-年
        /// </summary>
        public string BIRTHY { get; set; }

        /// <summary>
        /// 出生年月日-月
        /// </summary>
        public string BIRTHM { get; set; }

        /// <summary>
        /// 出生年月日-日
        /// </summary>
        public string BIRTHD { get; set; }

        /// <summary>
        /// 出生年月日 yyyy/MM/dd
        /// </summary>
        public string BIRTHDAY
        {
            get
            {
                return (this.BIRTHY + "/" + this.BIRTHM.PadLeft(2, '0') + "/" + this.BIRTHD.PadLeft(2, '0'));
            }
        }

        /// <summary>
        /// 學歷
        /// </summary>
        public string EDU { get; set; }

        /// <summary>
        /// 畢業狀況
        /// </summary>
        public string GRADUATESTATUS { get; set; }

        /// <summary>
        /// 學校名稱
        /// </summary>
        public string SCHOOLNAME { get; set; }

        /// <summary>
        /// 科系名稱
        /// </summary>
        public string INSTITUENAME { get; set; }

        /// <summary>
        /// 聯絡電話
        /// </summary>
        public string TEL { get; set; }

        /// <summary>
        /// 聯絡地址
        /// </summary>
        public string ADDR { get; set; }

        ///-----------------------------------
        ///<summary>
        /// 序號
        ///</summary>
        public decimal MEMSN { get; set; }

        ///<summary>
        /// 會員編號
        ///</summary>
        public string MEMIDNO { get; set; }

        ///<summary>
        /// 密碼
        ///</summary>
        public string MEMPWD { get; set; }

        ///<summary>
        /// 會員姓名
        ///</summary>
        public string MEMNAME { get; set; }

        ///<summary>
        /// 身份別
        ///</summary>
        public string MEMFOREIGN { get; set; }

        ///<summary>
        /// 學歷
        ///</summary>
        public string MEMEDU { get; set; }

        ///<summary>
        /// 出生日期
        ///</summary>
        public DateTime? MEMBIRTH { get; set; }

        ///<summary>
        /// 性別
        ///</summary>
        public string MEMSEX { get; set; }

        ///<summary>
        /// 兵役狀況
        ///</summary>
        public string MEMMILITARY { get; set; }

        ///<summary>
        /// 婚姻狀況
        ///</summary>
        public string MEMMARRY { get; set; }

        ///<summary> 畢業狀況 </summary>
        public string MEMGRADUATE { get; set; }

        ///<summary> 學校名稱 </summary>
        public string MEMSCHOOL { get; set; }

        ///<summary> 科系名稱 </summary>
        public string MEMDEPART { get; set; }

        ///<summary> 郵遞區號(前3碼) summary>
        public string MEMZIP { get; set; }

        ///<summary> 郵遞區號(45碼) summary> public decimal MEMZIP2W { get; set; }
        ///<summary> 郵遞區號(6碼) summary>
        public string MEMZIP6W { get; set; }

        ///<summary> 地址 </summary>
        public string MEMADDR { get; set; }

        ///<summary>
        /// 聯絡電話(日)
        ///</summary>
        public string MEMTEL { get; set; }

        ///<summary>
        /// 聯絡電話(夜)
        ///</summary>
        public string MEMTELN { get; set; }

        ///<summary>
        /// 行動電話
        ///</summary>
        public string MEMMOBILE { get; set; }

        ///<summary>
        /// 電子郵件
        ///</summary>
        public string MEMEMAIL { get; set; }

        ///<summary>
        /// 個資使用是否同意
        ///</summary>
        public string MEMOPENSEC { get; set; }

        ///<summary>
        /// 備註
        ///</summary>
        public string MEMMEMO { get; set; }

        ///<summary>
        /// TIMS學員
        ///</summary>
        public string MEMTIMS { get; set; }

        ///<summary>
        /// 訂閱電子報
        ///</summary>
        public string EPAPER { get; set; }

        ///<summary>
        /// 最近登入時間
        ///</summary>
        public DateTime? ELOGIN { get; set; }

        ///<summary>
        /// 登錄人
        ///</summary>
        public string MEMUSRID { get; set; }

        ///<summary>
        /// 登錄時間
        ///</summary>
        public DateTime? MEMREGTIME { get; set; }

        ///<summary>
        /// 異動者
        ///</summary>
        public string MEMOPUSER { get; set; }

        ///<summary>
        /// 異動時間
        ///</summary>
        public DateTime? MEMUDATE { get; set; }

        ///<summary>
        /// 障礙類別
        ///</summary>
        public string HANDTYPEID { get; set; }

        ///<summary>
        /// 障礙等級
        ///</summary>
        public string HANDLEVELID { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string STOPMEM { get; set; }

        ///<summary>
        ///
        ///</summary>
        public decimal MEMLOGINCNT { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string HANDTYPEID2 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string HANDLEVELID2 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string MFLAG { get; set; }
    }

    /// <summary> 就業通單一簽入 </summary>
    [Serializable]
    public class TWJobsMemberDataModel
    {

        [AllowHtml]
        public string minfo { get; set; }

        public string TOKEN { get; set; }
        public System.Xml.XmlDocument xmlDoc { get; set; }

        public decimal RID { get; set; }
        public string SID { get; set; }
        public decimal MEMBER_USER_ID { get; set; }
        public decimal ISFOREIGN { get; set; }
        public string ACID { get; set; }
        public string NAME { get; set; }
        public string BIRTHDAY { get; set; }
        public string SEX { get; set; }
        public string EMAIL { get; set; }
        public decimal ZIPCODE { get; set; }
        public string ADDR_CITY_1 { get; set; }
        public string ADDR_CITY_2 { get; set; }
        public string ADDR { get; set; }
        public string TEL1 { get; set; }
        public string TEL2 { get; set; }
        public string MOBILE { get; set; }
        public string FAX { get; set; }
        public string EDU { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public DateTime? MODIFYDATE { get; set; }
        public string MARRI { get; set; }
        public string GRADU { get; set; }

        /// <summary> 學校名稱 </summary>
        public string SCHOOLNAME { get; set; }

        /// <summary> 學校科系 </summary>
        public string DEPTNAME { get; set; }

        /// <summary> 最後執行頁資訊(登入系統導頁用) </summary>
        public string PAGEURL { get; set; }

        /// <summary> 查詢課程代碼 </summary>
        public decimal? OCID { get; set; }

        /// <summary> 計畫類別 (1產投 2在職進修) </summary>
        public string PLANTYPE { get; set; }

        /// <summary> 是否提供公里數計算（for產投課程） Y/N </summary>
        public string PROVIDELOCATION { get; set; }
    }
}