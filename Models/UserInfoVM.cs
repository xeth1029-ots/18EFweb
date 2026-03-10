using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    public class UserInfoVM
    {
        public UserInfoVM() { }

        /// <summary> 加密ACID</summary>
        [DisplayName("加密ACID")]
        public string Epi { get; set; }

        /// <summary> 加密Birthday:yyyy/MM/dd</summary>
        [DisplayName("加密Birthday")]
        public string Epb { get; set; }

        /// <summary> 加密Time:yyyyMMddHHmmssfff</summary>
        [DisplayName("加密Time")]
        public string Ept { get; set; }
        // <summary> 金融機構代碼(BankID) </summary>
        //[DisplayName("金融機構代碼")]
        //public string BankID { get; set; }
        // <summary> 銀行代碼+分行代碼(BranchID) </summary>
        //[DisplayName("銀行代碼+分行代碼")]
        //public string BranchID { get; set; }
        // <summary> 金融機構名稱(BankName) </summary>
        //[DisplayName("金融機構名稱")]
        //public string BankName { get; set; }
        // <summary> 分局名稱/分行名稱(BranchName) </summary>
        //[DisplayName("分局名稱/分行名稱")]
        //public string BranchName { get; set; }
        // <summary> 匯入帳號(BankAccount) </summary>
        //[DisplayName("匯入帳號")]
        //public string BankAccount { get; set; }

        /// <summary>取得時間戳</summary>
        public static DateTime? GetDateTimeFmt1(string timetampStr1)
        {
            if (!string.IsNullOrEmpty(timetampStr1))
            {
                //string timestampString = this.Epi; //string format = "yyyyMMddHHmmssfff"; stamptime1
                return DateTime.ParseExact(timetampStr1, "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
            }
            return null;
        }

    }
}