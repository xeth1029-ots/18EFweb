using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Services
{
    public static class CommonsServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string SubstringTo(this string str, int start)
        {
            // 字串空的回傳空值
            if (string.IsNullOrEmpty(str)) return "";
            // 字串長度未達起始位置
            if (str.Length < start) return str;

            return str.Substring(start);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string SubstringTo(this string str, int start,int end)
        {
            // 字串空的回傳空值
            if (string.IsNullOrEmpty(str)) return "";
            // 字串長度未達起始位置
            if (str.Length < start) return str;
            // 字串起始位置到結束位置長度超過字串長度
            if (str.Substring(start).Length < end) return str;

            return str.Substring(start, end);
        }

        /// <summary>
        /// 將民國時間轉換為民國證照模式(民國 yyy 年 MM 月 dd 日)
        /// </summary>
        public static string TransDateTimeTw(this DateTime TwDate)
        {
            if (TwDate == null)
            {
                return "";
            }
            return TwDate.AddYears(-1911).ToString("民國 yyy 年 MM 月 dd 日",new System.Globalization.CultureInfo("zh-TW"));
        }

        /// <summary>
        /// 將民國時間轉換為西元證照模式(MM dd,yyyy)
        /// </summary>
        public static string TransDateTime (this DateTime TwDate)
        {
            if (TwDate == null)
            {
                return "";
            }
            return TwDate.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US"));
        }

        /// <summary>
        /// Split進階版(可以將文字分割為IList)
        /// 若傳入NULL則傳回0的IList
        /// 若該字串沒有分割符號，則回傳該字串單筆IList
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static IList<string> ToSplit(this string str,char sp)
        {
            IList<string> splitlist = new List<string>();

            if (!string.IsNullOrEmpty(str))
            {
                if (str.IndexOf(sp) > 0)
                {
                    string[] strsplit = str.Split(sp);
                    for (int i = 0; i < strsplit.Count(); i++)
                    {
                        splitlist.Add(strsplit[i]);
                    }
                }
                else
                {
                    splitlist.Add(str);
                }
            }
            
            return splitlist;
        }

        /// <summary>
        /// 數字-除法分母用(分母不可為0)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double ToDivInt(this double num)
        {
            if (num != 0)
            {
                return num;
            }
            return 1;
        }

        /// <summary>
        /// 字串-傳回千分位字串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TOTranThousandString(this string str)
        {
            str = (str == "" || str == null) ? "0" : str;
            return Convert.ToInt64(str).ToString("#,0");
        }
    }
}