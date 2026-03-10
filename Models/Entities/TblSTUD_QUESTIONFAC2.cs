using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    [Serializable]
    public class TblSTUD_QUESTIONFAC2 : IDBRow
    {
        ///<summary>
        ///
        ///</summary>
        public Decimal? SOCID { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S11 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S12 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S13 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S14 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S15 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S16 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string S16_NOTE { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? S2 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? S3 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_1 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_2 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_3 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_4 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_5 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_6 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_7 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_8 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_9 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_10 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A1_10_NOTE { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? A2 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? A3 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? A4 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? A5 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? A6 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? A7 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B11 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B12 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B13 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B14 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B15 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B21 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B22 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B23 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B31 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B32 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B41 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B42 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B43 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B44 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B51 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B61 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B62 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B63 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B71 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B72 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B73 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? B74 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public Decimal? C11 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string C21_NOTE { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string MODIFYACCT { get; set; }

        ///<summary>
        ///
        ///</summary>
        public DateTime? MODIFYDATE { get; set; }

        ///<summary>
        /// 資料來源 (1: 報名網(學員外網填寫。)  , 2: TIMS系統)
        ///</summary>
        public Decimal? DASOURCE { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A2_7_NOTE { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string A3_5_NOTE { get; set; }

        /// <summary>
        /// 預設建構子
        /// </summary>
        public TblSTUD_QUESTIONFAC2()
        { }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STUD_QUESTIONFAC2;
        }
    }
}