using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Services
{
    public class ClaShareService
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 將字串 1,2,3,4,5,6  轉成 '1','2','3','4','5','6'
        /// </summary>
        /// <returns></returns>
        public string GetSqlInValue(string Val)
        {
            if (string.IsNullOrEmpty(Val)) return "";

            string[] aReturn = Val.Split(',');

            string sReturn = "";
            for (int i = 0; i < aReturn.Length; i++)
            {
                sReturn += $"{(sReturn != "" ? "," : "")}'{aReturn[i]}'";
            }
            return sReturn;
        }
    }
}