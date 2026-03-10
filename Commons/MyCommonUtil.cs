using JWT;
using JWT.Exceptions;
using JWT.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.DataLayers;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 放置 WDAIIP.WEB 專用的 CommonUtil 擴充,
    /// 只有當 Turbo.Commons.CommonUtil 不足時才寫新的
    /// </summary>
    public class MyCommonUtil : Turbo.Commons.CommonUtil
    {
        //public static string cst_StopSendMail = "StopSendMail";//寄錯動作停止
        //public static string cst_ErrorSendMail = "ErrorSendMail";//寄錯誤信給我
        public static string cst_MaxCanMailCount = "MaxCanMailCount";//最大寄信
        public static int cst_iMaxCanMailCount = 44;//DEFALUT: (每次)最大寄信量
        public static int GlobalMailCount = 1;//目前寄信總數量 (起始1)
        public static DateTime GlobalMailDate = DateTime.Today;//目前寄信日期

        /// <summary> 組成在 HTML &lt;select&gt; 標籤內使用的 &lt;option&gt; 標籤 HTML 字串 </summary>
        /// <param name="list">「代碼-名稱」項目集合</param>
        /// <param name="selectValue">預設選取的項目值。輸入 null 表示沒有預設選取項目</param>
        /// <param name="blankOptionCode">空白項目值。輸入 null 表示不需要加入空白項目。預設空白字元。</param>
        /// <param name="blankOptionText">空白項目顯示名稱。</param>
        /// <returns></returns>
        public static string BuildOptionHtml(IList<KeyMapModel> list, string selectValue = "",
                                             string blankOptionCode = " ", string blankOptionText = "")
        {
            var sb = new StringBuilder();
            //空白選項
            if (blankOptionCode != null)
            {
                sb.Append("<option value=\"");
                sb.Append(HttpUtility.HtmlDecode(blankOptionCode));
                sb.Append("\"");
                if (selectValue != null && selectValue == blankOptionCode) sb.Append(" selected");
                sb.Append(">");
                sb.Append(HttpUtility.HtmlDecode(blankOptionText));
                sb.Append("</option>");
            }
            //項目集合選項
            if (list != null && list.Count > 0)
            {
                foreach (var n in list)
                {
                    sb.Append("<option value=\"");
                    sb.Append(HttpUtility.HtmlDecode(n.CODE));
                    sb.Append("\"");
                    if (selectValue != null && n.CODE == selectValue) sb.Append(" selected");
                    sb.Append(">");
                    sb.Append(HttpUtility.HtmlDecode(n.TEXT));
                    sb.Append("</option>");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 傳回在 HTML &lt;select&gt; 標籤內使用的 &lt;option&gt; 標籤 HTML 的 AJAX 非同步作業結果。
        /// 注意！本方法只能與網頁端 Javascript 的 ajaxLoadMore() 方法搭配使用。
        /// </summary>
        /// <param name="list">「代碼-名稱」項目集合</param>
        /// <param name="selectValue">預設選取的項目值。輸入 null 表示沒有預設選取項目</param>
        /// <param name="blankOptionCode">空白項目值。輸入 null 表示不需要加入空白項目。預設空白字元。</param>
        /// <param name="blankOptionText">空白項目顯示名稱。</param>
        /// <returns></returns>
        public static ContentResult BuildOptionHtmlAjaxResult(IList<KeyMapModel> list, string selectValue = "", string blankOptionCode = " ", string blankOptionText = "")
        {
            var bag = new Turbo.Commons.AjaxResultStruct();
            bag.data = MyCommonUtil.BuildOptionHtml(list, selectValue, blankOptionCode, blankOptionText);
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxLoadMore() 方法搭配使用。
        /// </summary>
        /// <param name="data">要回傳的資料物件。</param>
        /// <param name="isSuccess">作業執行結果是否為成功，（true: 成功，false: 失敗）。預設為 true。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(object data, bool isSuccess = true)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(isSuccess);
            bag.data = data;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>將傳入的代碼清單轉換成 IList&lt;SelectListItem&gt 集合</summary>
        /// <param name="keyMapList">代碼-名稱項目集合。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <param name="selectedCode">預設選取的項目代碼（null 表示不選取任何項目）。</param>
        /// <param name="addBlankItem">指示是否要自動加入一個空項目。(true: 加入，false: 不加入)。預設 false。</param>
        /// <param name="blankItemCode">當 addBlankItem 參數等於 true 時，自訂的空項目代碼。預設 ""。</param>
        /// <returns></returns>
        public static IList<SelectListItem> ConvertSelItems(IList<KeyMapModel> keyMapList, bool TextWithCode = false,
                                                            string selectedCode = "", bool addBlankItem = false, string blankItemCode = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (keyMapList != null)
            {
                if (addBlankItem)
                {
                    var item = new SelectListItem() { Text = "", Value = blankItemCode };
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    if (TextWithCode) item.Text = string.Concat(item.Value, " ", item.Text);
                    list.Add(item);
                }

                foreach (KeyMapModel hash in keyMapList)
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = (TextWithCode ? hash.CODE + "." : "") + hash.TEXT;
                    item.Value = hash.CODE;
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>將傳入的代碼清單轉換成 IList&lt;SelectListItem&gt 集合</summary>
        /// <param name="keyMapList">代碼-名稱項目集合。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <param name="selectedCode">預設選取的項目代碼（null 表示不選取任何項目）。</param>
        /// <param name="addBlankItem">指示是否要自動加入一個空項目。(true: 加入，false: 不加入)。預設 false。</param>
        /// <param name="blankItemCode">當 addBlankItem 參數等於 true 時，自訂的空項目代碼。預設 ""。</param>
        public static IList<SelectListItem> ConvertSelItems(IDictionary<string, string> dictionary, bool TextWithCode = false,
                                                            string selectedCode = "", bool addBlankItem = false, string blankItemCode = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (dictionary != null)
            {
                if (addBlankItem)
                {
                    var item = new SelectListItem() { Text = "", Value = blankItemCode };
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    if (TextWithCode) item.Text = string.Concat(item.Value, " ", item.Text);
                    list.Add(item);
                }

                foreach (var pair in dictionary)
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = (TextWithCode ? pair.Key + "." : "") + pair.Value;
                    item.Value = pair.Key;
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary> 移除在代碼-名稱項目集合內的代碼項目。</summary>
        /// <param name="list">代碼-名稱項目集合。</param>
        /// <param name="itemCode">要移除的項目代碼。</param>
        public static void RemoveKeyMapItem(IList<KeyMapModel> list, string itemCode)
        {
            if (list != null)
            {
                int found = -1;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].CODE == itemCode) found = i;
                }
                if (found >= 0) list.RemoveAt(found);
            }
        }

        public static string GetMyValue1(string s_SESS, string Val1)
        {
            string rst = "";
            if (string.IsNullOrEmpty(s_SESS) || string.IsNullOrEmpty(Val1)) { return rst; }

            foreach (string sVal in s_SESS.Split('&'))
            {
                string ffsW = string.Concat(Val1, "=");
                if (sVal.StartsWith(ffsW))
                {
                    rst = sVal.Replace(ffsW, "");
                    return rst;
                }
            }
            return rst;
        }

        /// <summary>
        /// 在「代碼-名稱項目集合」內移除指定的代碼項目。
        /// </summary>
        /// <param name="list">代碼-名稱項目集合。</param>
        /// <param name="itemCodes">要移除的項目代碼字串陣列。</param>
        public static void RemoveKeyMapItem(IList<KeyMapModel> list, params string[] itemCodes)
        {
            if (list != null && itemCodes != null)
            {
                var ub = list.Count - 1;
                var found = new List<KeyMapModel>();
                foreach (var code in itemCodes)
                {
                    foreach (var item in list)
                    {
                        if (item.CODE == code)
                        {
                            found.Add(item);
                            break;
                        }
                    }
                }

                foreach (var item in found)
                {
                    list.Remove(item);
                }
            }
        }

        /// <summary>
        /// 在「代碼-名稱項目集合」內保留指定的代碼項目，不是保留的代碼項目一律移除。
        /// </summary>
        /// <param name="list">代碼-名稱項目集合。</param>
        /// <param name="itemCodes">要保留的項目代碼字串陣列。</param>
        public static void ReserveKeyMapItem(IList<KeyMapModel> list, params string[] itemCodes)
        {
            if (list != null && itemCodes != null)
            {
                var ub = list.Count - 1;
                var found = new List<KeyMapModel>();
                foreach (var code in itemCodes)
                {
                    foreach (var item in list)
                    {
                        if (item.CODE != code)
                        {
                            found.Add(item);
                        }
                    }
                }

                foreach (var item in found)
                {
                    list.Remove(item);
                }
            }
        }

        /// <summary>
        /// 是否為數字型態資料
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsInt(string content)
        {
            if (content == null) { return false; }
            string pattern = "^[0-9]*[1-9][0-9]*$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(content);
        }

        /// <summary> 是否為正整數 </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool isUnsignedInt(string content)
        {
            if (content == null) { return false; }
            Int64 chkVal = 0;
            return (Int64.TryParse(content, out chkVal));
        }

        /// <summary> 是否為正整數 </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool isDecimal(string content)
        {
            if (content == null) { return false; }
            Decimal chkVal = 0;
            return Decimal.TryParse(content, out chkVal);
        }

        /// <summary> 檢核日期 </summary>
        /// <param name="s_date"></param>
        /// <returns></returns>
        public static bool IsDate(String s_date)
        {
            bool rst = false;
            try
            {
                DateTime dt = DateTime.Parse(s_date);
                rst = true;
            }
            catch { rst = false; }
            return rst;
        }

        /// <summary>
        /// 轉換民國日期格式(ROCy/MM/dd)為西元(yyyy/MM/dd)
        /// </summary>
        /// <param name="strDate1"></param>
        /// <returns></returns>
        public static string DateRoc2Ad(string strDate1)
        {
            string rst = null;
            if (string.IsNullOrEmpty(strDate1) || strDate1.Length < 6 || strDate1.IndexOf("/") == -1) { return rst; }
            try
            {
                return string.Concat(int.Parse(strDate1.Split('/')[0]) + 1911, strDate1.Substring(strDate1.IndexOf("/"), strDate1.Length - strDate1.IndexOf("/")));
            }
            catch (Exception) { }
            return rst;
        }

        /// <summary>
        /// 是否為英文型態資料
        /// </summary>
        /// <param name="strVal"></param>
        /// <returns></returns>
        public static bool IsEng(string content)
        {
            string pattern = "^[A-Za-z]*[A-Za-z][A-Za-z]*$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(content);
        }

        /// <summary> 檢核傳入值與陣列資料比對，若存在為true </summary>
        /// <param name="val1"></param>
        /// <param name="All_Val"></param>
        /// <returns></returns>
        public static bool ChkStringValue(string val1, string All_Val)
        {
            bool rst = false;
            if (string.IsNullOrEmpty(val1)) { return rst; }
            if (string.IsNullOrEmpty(All_Val)) { return rst; }
            if (!All_Val.Contains(','))
            {
                if (All_Val.Replace("'", "").Equals(val1.Replace("'", "")))
                {
                    rst = true;
                    return rst;
                }
                return rst;
            }

            foreach (var a_V1 in All_Val.Split(','))
            {
                if (a_V1.Replace("'", "").Equals(val1.Replace("'", "")))
                {
                    rst = true;
                    return rst;
                }
            }
            return rst;
        }

        /// <summary> 檢核密碼格式 </summary>
        /// <param name="strPwd"></param>
        /// <returns></returns>
        public static bool ChkPwdFmt(string strPwd)
        {
            string strVal = "";
            bool blInt = false;
            bool blEng = false;

            //檢核資料是否含有數字
            for (int i = 0; i < strPwd.Length; i++)
            {
                strVal = strPwd.Substring(i, 1);

                if (IsInt(strVal))
                {
                    blInt = true;
                    break;
                }
            }

            //檢核資料是否含有英文
            for (int i = 0; i < strPwd.Length; i++)
            {
                strVal = strPwd.Substring(i, 1);

                if (IsEng(strVal))
                {
                    blEng = true;
                    break;
                }
            }

            return (blInt && blEng);
        }

        /// <summary>
        /// 計算中文長度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int LenB(string str)
        {
            int intLen = 0;

            intLen = str.Length;

            for (int i = 1; i <= str.Length; i++)
            {
                //判斷含中文字的半型長度 2006 XP
                if (Convert.ToString(Convert.ToChar(str.Substring(i - 1, 1)), 16).Length == 8)
                {
                    intLen += 1;
                }
            }

            return intLen;
        }

        #region 附件相關
        /// <summary>
        /// 取得上傳附件新檔名資訊
        /// </summary>
        /// <param name="preFileName"></param>
        /// <returns></returns>
        public static string GetMaxFileNo(string preFileName)
        {
            return preFileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        #endregion

        /// <summary>
        /// 複製model內容到對應的hashtable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Hashtable GetCopyModel(object model)
        {
            Hashtable rtn = new Hashtable();
            System.Reflection.PropertyInfo[] propertys = model.GetType().GetProperties();

            try
            {
                foreach (PropertyInfo pi in propertys)
                {
                    object val = null;
                    if (pi.PropertyType == typeof(DateTime))
                    {
                        DateTime newDate = (DateTime)pi.GetValue(model);
                        val = newDate;
                    }
                    else
                    {
                        val = pi.GetValue(model);
                    }
                    rtn.Add(pi.Name, val);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rtn;
        }

        /// <summary>
        /// 傳入一個 IList&lt;string&gt; 多選值字串參數，轉換為可以用在 SQL IN 子句中的 Value 數字字串(如: 11,24,21)
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public static string ConvertToWhereInNumberValues(IList<string> parms)
        {
            if (parms == null || parms.Count == 0)
            {
                return string.Empty;
            }
            // 因為 IList<string> 參數, 可能存在不連續的 index 值,
            // 不能直接用 for(i) 的方式抓每一個值, 要用 foreach 找出所有 elements
            IList<string> newArr = new List<string>();
            foreach (string p in parms)
            {
                newArr.Add(p);
            }
            string[] pArr = newArr.ToArray<string>();
            for (int i = 0; i < pArr.Length; i++)
            {
                pArr[i] = "" + pArr[i] + "";
            }
            return string.Join(",", pArr);
        }

        public static string ToSha256(string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] Result = Sha256.ComputeHash(SHA256Data);
            return Convert.ToBase64String(Result);
        }

        /// <summary> ToSha256 securityCode yyyyMMdd</summary>
        /// <returns></returns>
        public static string GetSecurityCodeByDay()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string securityCode = MyCommonUtil.ToSha256(date + "Turbo");
            return securityCode;
        }

        /// <summary> yyyyMMdd SecurityCode 檢核安全值 </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool CompareSecurityCode(string code)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string code1 = MyCommonUtil.ToSha256(date + "Turbo");
            return (code1 == code);
        }

        /// <summary> 取得主機所在ip位址 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetLocalAddr(string type)
        {
            string rtn = string.Empty;
            //System.Web.HttpContext.Current.Request
            if (HttpContext.Current.Request == null) { return rtn; }
            string localAddr = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];
            if (localAddr == null) { return rtn; }
            if (string.IsNullOrEmpty(localAddr)) { return rtn; }
            if (string.IsNullOrWhiteSpace(localAddr)) { return rtn; }

            switch (type)
            {
                case "1":
                    return localAddr; //直接反應資訊
                //break;
                case "2": //取最後三碼
                    //rtn = localAddr; IPV6
                    if (localAddr.IndexOf(":") > -1)
                    {
                        rtn = localAddr.Substring(localAddr.LastIndexOf(":") + 1);
                        if (localAddr.Split(':').Length == 6) rtn += "-v6";
                        return rtn;
                    }
                    //IPV4 取最後一組資訊(隱藏前面資訊)
                    if (localAddr.IndexOf(".") > -1)
                    {
                        rtn = localAddr.Substring(localAddr.LastIndexOf(".") + 1);
                        return rtn;
                    }
                    break;
            }
            return rtn;
        }

        /// <summary>
        /// 轉置中文編碼&#XXXXX;
        /// </summary>
        /// <param name="model"></param>
        public static void HtmlDecode(object model)
        {
            if (model != null)
            {
                foreach (var p in model.GetType().GetProperties())
                {
                    if (p.PropertyType == typeof(string))
                    {
                        MethodInfo setMethod = p.GetSetMethod();
                        if (setMethod != null)
                        {
                            object val = p.GetValue(model);
                            if (val != null)
                            {
                                string newVal = WebUtility.HtmlDecode((string)val);
                                p.SetValue(model, newVal);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 轉置中文編碼&#XXXXX;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void HtmlDecodeForList<T>(IList<T> list)
        {
            if (list != null)
            {
                foreach (T model in list)
                {
                    HtmlDecode(model);
                }
            }
        }

        /// <summary> 異動日比對超過1天:true 太短回傳:false </summary>
        /// <param name="modifydate"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static bool MODIFYDATE2OLD(DateTime? modifydate, DateTime now)
        {
            //無值回傳true
            if (!modifydate.HasValue) { return true; }
            double iDay = new TimeSpan(now.Ticks - modifydate.Value.Ticks).TotalDays;
            return (iDay > 1) ? true : false;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// object(model) 序列化 to byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) { return null; };
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///  byte array 反序列化 to object(model)
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }

        /// <summary>
        /// datatable to model
        /// ref:https://www.c-sharpcorner.com/blogs/converting-datatable-to-model-list2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static IList<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception) { }
                    }
                }
                return objT;
            }).ToList();
        }

        /// <summary> 取得正確 ip </summary>
        /// <returns></returns>
        public static string GetIpAddress(HttpContext context)
        {
            string str_UserHostIp = string.Empty;
            string cst_HTTP_X_FORWARDED_FOR = "HTTP_X_FORWARDED_FOR"; //代理伺服器
            string cst_REMOTE_ADDR = "REMOTE_ADDR"; //沒有掛代理伺服器
            //Look for a proxy address first
            str_UserHostIp = context.Request.ServerVariables[cst_HTTP_X_FORWARDED_FOR]; //撈代理伺服器
            //If there is no proxy, get the standard remote address
            if (string.IsNullOrEmpty(str_UserHostIp) || str_UserHostIp.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                str_UserHostIp = context.Request.ServerVariables[cst_REMOTE_ADDR]; //沒有掛代理伺服器
            }
            if (string.IsNullOrEmpty(str_UserHostIp) || str_UserHostIp.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                str_UserHostIp = context.Request.UserHostAddress; //上述2種資訊都沒有
            }
            return str_UserHostIp;
        }

        /// <summary>
        /// 取得 CLIENT/SERVER-ServerVariables 資訊
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetHTTP_HOST(HttpContext context)
        {
            /*
            string sTmp1 = string.Empty;
            sTmp1 = "ALL_HTTP,ALL_RAW";
            sTmp1 += ",APPL_MD_PATH";
            sTmp1 += ",APPL_PHYSICAL_PATH";
            sTmp1 += ",INSTANCE_ID";
            sTmp1 += ",INSTANCE_META_PATH";
            sTmp1 += ",LOCAL_ADDR";
            sTmp1 += ",LOGON_USER";
            sTmp1 += ",PATH_INFO";
            sTmp1 += ",PATH_TRANSLATED";
            sTmp1 += ",QUERY_STRING";
            sTmp1 += ",REMOTE_ADDR";
            sTmp1 += ",REMOTE_HOST";
            sTmp1 += ",REMOTE_USER";
            sTmp1 += ",REQUEST_METHOD";
            sTmp1 += ",SCRIPT_NAME";
            sTmp1 += ",SERVER_NAME";
            sTmp1 += ",SERVER_PORT";
            sTmp1 += ",SERVER_PROTOCOL";
            sTmp1 += ",SERVER_SOFTWARE";
            sTmp1 += ",URL";
            sTmp1 += ",HTTP_CONNECTION";
            sTmp1 += ",HTTP_ACCEPT";
            sTmp1 += ",HTTP_ACCEPT_ENCODING";
            sTmp1 += ",HTTP_ACCEPT_LANGUAGE";
            sTmp1 += ",HTTP_HOST";
            sTmp1 += ",HTTP_REFERER";
            sTmp1 += ",HTTP_USER_AGENT";
            sTmp1 += ",HTTP_UPGRADE_INSECURE_REQUESTS";
            string[] sTmp2A = sTmp1.Split(',');
            foreach (string sTmp2 in sTmp2A)
            {
                string rsVar = context.Request.ServerVariables[sTmp2];
                rst += !string.IsNullOrEmpty(rsVar) ? string.Format("{0}: {1}\n", sTmp2, rsVar) : string.Format("*******{0}: (obj IS NOTHING)*******\n\n", sTmp2);
            }
             */
            string rst = string.Empty;
            foreach (string skey in context.Request.ServerVariables.AllKeys)
            {
                rst += string.Format("{0}: {1}\n", skey, context.Request.ServerVariables[skey]);
            }
            return rst;
        }

        /// <summary>
        /// 在跳轉之前做判斷,防止重複
        /// </summary>
        /// <param name="thisContext"></param>
        /// <param name="url"></param>
        public void RedirectUrl(HttpContext thisContext, string url)
        {
            thisContext.Response.Clear();//這裡是關鍵,清除在返回前已經設定好的標頭資訊,這樣後面的跳轉才不會報錯
            thisContext.Response.BufferOutput = true;//設定輸出緩衝
            //在跳轉之前做判斷,防止重複
            if (!thisContext.Response.IsRequestBeingRedirected) { thisContext.Response.Redirect(url, true); }
            //搞定,世間清靜了很多。
        }

        public static Decimal? get_Decimal_null(string context)
        {
            Decimal? idlid = null;
            Decimal _idlid = 0;
            if (context == null) { return null; }
            if (!isDecimal(context)) { return null; }
            Decimal.TryParse(context, out _idlid); idlid = _idlid;
            return idlid;
        }

        public static Int64? get_Int64_null(string context)
        {
            Int64? ifileSeq = null;
            Int64 _ifileSeq = 0;
            if (context == null) { return null; }
            if (!isDecimal(context)) { return null; }
            Int64.TryParse(context, out _ifileSeq); ifileSeq = _ifileSeq;
            return ifileSeq;
        }

        /// <summary> Html (3+3郵遞區號查詢)(3+2郵遞區號查詢)  </summary>
        /// <returns></returns>
        public static string Get_PostCodeQry()
        {
            //<a href="@s_PostCodeQry" target="_blank" title="前往查詢3+2郵遞區號(另開新視窗)">(3+2郵遞區號查詢)</a>
            string rst = null;
            //rst = @"http://www.post.gov.tw/post/internet/f_searchzone/index.jsp?ID=190102";
            string s_href1 = "<a href=\"{0}\" target=\"_blank\" title=\"前往查詢{1}郵遞區號(另開新視窗)\">({1}郵遞區號查詢)</a>";
            string s_post3a = @"https://www.post.gov.tw/post/internet/Postal/index.jsp?ID=208&list=5";
            string s_post3b = "3+3";
            string s_post2a = @"https://www.post.gov.tw/post/internet/Postal/index.jsp?ID=208&list=1";
            string s_post2b = "3+2";
            rst = string.Concat(string.Format(s_href1, s_post3a, s_post3b), string.Format(s_href1, s_post2a, s_post2b));
            return rst;
        }

        /// <summary> 取得組合後的5碼或6碼 </summary>
        /// <param name="zipcode"></param>
        /// <param name="zipcode2W"></param>
        /// <returns></returns>
        public static string GET_ZIPCODE6W(string zipcode, string zipcode2W)
        {
            string rst = null;
            if (string.IsNullOrEmpty(zipcode)) { return rst; }
            if (string.IsNullOrEmpty(zipcode2W)) { return rst; }
            int i_zipcode;
            if (!int.TryParse(zipcode, out i_zipcode)) { return rst; }
            int i_zipcode2W;
            if (!int.TryParse(zipcode2W, out i_zipcode2W)) { return i_zipcode.ToString(); }
            rst = string.Concat(zipcode, zipcode2W);
            if (rst.Length != 5 && rst.Length != 6) rst = null; //只能等於5或6
            return rst;
        }

        /// <summary> 取得後的2碼或3碼 </summary>
        /// <param name="zipcode6W"></param>
        /// <param name="zipcode2W"></param>
        /// <returns></returns>
        public static string GET_ZIPCODE2W(string zipcode6W, string zipcode2W)
        {
            string rst = null;
            if (string.IsNullOrEmpty(zipcode6W) && string.IsNullOrEmpty(zipcode2W)) { return rst; }
            if (!string.IsNullOrEmpty(zipcode6W))
            {
                //只能等於5或6
                if (zipcode6W.Length == 5) { rst = zipcode6W.Substring(3, 2); return rst; }
                if (zipcode6W.Length == 6) { rst = zipcode6W.Substring(3, 3); return rst; }
            }
            if (!string.IsNullOrEmpty(zipcode2W))
            {
                if (zipcode2W.Length == 2 || zipcode2W.Length == 3) { rst = zipcode2W; return rst; }
            }
            return rst;
        }

        /// <summary> 檢核 郵遞區號 (含後2碼或3碼) </summary>
        /// <param name="ZIPCODE"></param>
        /// <param name="ZIPCODE_2W"></param>
        /// <param name="ADDRNM"></param>
        /// <param name="must"></param>
        /// <returns>若有值為錯誤訊息</returns>
        public static string CHK_ZIPCODE(string ZIPCODE, string ZIPCODE_2W, string ADDRNM, bool must)
        {
            string rst = null;
            string s_bb1 = "郵遞區號前3碼";
            int i_zipcode = 0;
            if (string.IsNullOrEmpty(ZIPCODE))
            {
                if (must) { rst = string.Format("請選擇或輸入{0}{1}", ADDRNM, s_bb1); }
                if (string.IsNullOrEmpty(ZIPCODE_2W)) return rst;
            }
            else if (ZIPCODE.Length != 3)
            {
                //modelState.AddModelError("", "通訊地址前3碼郵遞區號必須為3碼");
                rst = string.Format("{0}{1},長度必須為3碼", ADDRNM, s_bb1);
                return rst;
            }
            else if (!int.TryParse(ZIPCODE, out i_zipcode))
            {
                //modelState.AddModelError("", "通訊地址前3碼郵遞區號必須為3碼");
                rst = string.Format("{0}{1},必須為數字", ADDRNM, s_bb1);
                return rst;
            }

            string s_cc1 = "郵遞區號後2碼或3碼";
            if (string.IsNullOrEmpty(ZIPCODE_2W))
            {
                //modelState.AddModelError("", "請輸入通訊地址郵遞區號後2碼");
                if (must) { rst = string.Concat("請輸入", ADDRNM, s_cc1); }
                return rst;
            }
            if (ZIPCODE_2W.Length != 2 && ZIPCODE_2W.Length != 3)
            {
                //長度只可為2碼或3碼
                rst = string.Concat(ADDRNM, s_cc1, "，長度只可為2碼或3碼的數字");
                return rst;
            }

            bool flag_iZIPCODE_ok = !string.IsNullOrEmpty(ZIPCODE_2W) ? true : false;
            if (flag_iZIPCODE_ok)
            {
                int iZIPCODE_2W = -1;
                if (!int.TryParse(ZIPCODE_2W, out iZIPCODE_2W))
                {
                    rst = string.Concat(ADDRNM, s_cc1, "，只可為2碼或3碼的數字");
                    return rst;
                }
                else if (ZIPCODE_2W.Length == 2)
                {
                    if (iZIPCODE_2W < 0 || iZIPCODE_2W > 99)
                    {
                        rst = string.Concat(ADDRNM, s_cc1, "，只可為01~99的數字");
                        return rst;
                    }
                }
                else if (ZIPCODE_2W.Length == 3)
                {
                    if (iZIPCODE_2W < 0 || iZIPCODE_2W > 999)
                    {
                        rst = string.Concat(ADDRNM, s_cc1, "，只可為001~999的數字");
                        return rst;
                    }
                }
            }
            return rst;
        }

        /// <summary> 黑名單判斷, 錯誤訊息提供 </summary>
        /// <param name="listblock"></param>
        /// <returns></returns>
        public static string Show_BlockMsg1(IList<Hashtable> listblock)
        {
            #region 黑名單判斷
            //IList<Hashtable> listblock = dao.GetBlackList(Convert.ToString(model.Form.IDNO));
            if (listblock == null) { return string.Empty; }
            if (listblock.Count == 0) { return string.Empty; }
            //if (listblock != null && listblock.Count > 0) { }

            string strMsg = string.Empty;
            string errMsg = "查台端於" + Convert.ToString(listblock[0]["SBODATE"]) + "因" + Convert.ToString(listblock[0]["SBCOMMENT"]);
            errMsg += "至" + Convert.ToString(listblock[0]["SBEDATE"]) + "前將不得再申領本署補助,如需報名請洽訓練單位.";

            Int32 num = Convert.ToInt32(Math.Truncate(new decimal(errMsg.Length / 30)));
            Int32 last = Convert.ToInt32(errMsg.Length % 30);
            Int32 j = 1;
            string ErrMsgTmp = "";

            for (int i = 1; i <= num + 1; i++)
            {
                if (i == num + 1)
                {
                    ErrMsgTmp = ErrMsgTmp + errMsg.Substring(j - 1, last);
                }
                else
                {
                    ErrMsgTmp = ErrMsgTmp + errMsg.Substring(j - 1, 30) + "<br />";
                    j += 30;
                }

                if (!string.IsNullOrEmpty(ErrMsgTmp)) strMsg = ErrMsgTmp;
            }
            #endregion
            return strMsg;
        }

        /// <summary> 取得系統參數 </summary>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string Utl_GetConfigSet(string sKey)
        {
            string rst = "";
            try
            {
                //rst = ConfigurationSettings.AppSettings[sKey];
                rst = ConfigurationManager.AppSettings[sKey];
            }
            catch (Exception) { }
            rst = rst ?? "";
            return rst;
        }

        /// <summary> 圖資MAP_APIKey</summary>
        public string Get_TGOS_API
        {
            get
            {
                string cf_MAP_AppID = MyCommonUtil.Utl_GetConfigSet("MAP_AppID");
                string cf_MAP_APIKey = MyCommonUtil.Utl_GetConfigSet("MAP_APIKey");
                string MAP_AppID_test = "lTPdox5XfzyjNW8l+oaklUZjW9jljaV3Lx3DOnYJKF37byXdJlfOmg==";
                string MAP_APIKey_test = "cGEErDNy5yN/1fQ0vyTOZrghjE+jIU6uuyLUpFfm0OEY+iBNJnf9WvoCwTopOOUPMfdDpg/pihiPALA5s/gl4J/KJZyWVApuKKNoJiAu3QlYzSJuGpzz9e6C1+N6l+HGG05E0LkMpa1VOeiBrb/BJbShahGt6WGkVFErM/NJ+eg2tIbOdFZFz8rNPtZJ5mGUo2vgXchvQ1mVNurt/Qx9/dtq0F1x66xmG6KpaFP/XNA5WXLSSy6dsjNneF4notc6ttmVaJiyiRJXWejNYPv/7UJ3QwcVfgMTWQ+Me6AtKZlgTlL7gVrd0YiSY6IVhvwrNCQ5L9RJ2xRyd8BPcMcW1oaZRl8x/ejt/AJJ0LI/L9AfXWci2CP7SSpzDfQPotpreF997vUiqCb1VrSYv4HrFDCMTupMZphHoXCdp9mmJIpuTioW70IGWg==";
                string s_MAP_AppID = !string.IsNullOrEmpty(cf_MAP_AppID) ? cf_MAP_AppID : MAP_AppID_test;
                string s_MAP_APIKey = !string.IsNullOrEmpty(cf_MAP_AppID) ? cf_MAP_APIKey : MAP_APIKey_test;
                string s_gosApi1 = string.Format("https://api.tgos.tw/TGOS_API/tgos?ver=2&AppID={0}&APIKey={1}", s_MAP_AppID, s_MAP_APIKey);
                return s_gosApi1;
            }
        }

        /// <summary>  "EAT", "ATE", "TAE"  TEST </summary>
        /// <param name="str1"></param>
        /// <returns></returns>
        public static string Test1EAT(string str1)
        {
            string ret = "";
            IList<string> Englisg = new List<string> { "EAT", "ATE", "TAE" };
            //str1[0] = foreach (string x in Englisg) { if (str1[0]) }
            for (int i = 0; i < 3; i++)
            {
                foreach (string sx in Englisg)
                {
                    if (str1[i] == sx[0]) { ret += sx + "<BR>"; }
                }
            }
            return ret;
        }

        /// <summary> 寄件數量+1回傳 </summary>
        /// <returns></returns>
        public static int SendMailCount()
        {
            int iMaxCanMailCount = cst_iMaxCanMailCount;//DEFALUT: (每天)最大寄信量
            string ugMaxCanMailCount = Utl_GetConfigSet(cst_MaxCanMailCount);//MaxCanMailCount
            if (!string.IsNullOrEmpty(ugMaxCanMailCount))
            {
                bool fg_ok1 = int.TryParse(ugMaxCanMailCount, out iMaxCanMailCount);
                if (!fg_ok1) { return iMaxCanMailCount; }
            }
            try
            {
                int iRes1 = DateTime.Compare(GlobalMailDate, DateTime.Today);
                if (iRes1 == 0)
                {
                    //當日只加1
                    MyCommonUtil.GlobalMailCount += 1;
                }
                else
                {
                    //隔日重設重1開始
                    MyCommonUtil.GlobalMailCount = 1;
                    MyCommonUtil.GlobalMailDate = DateTime.Today;
                }
            }
            catch (Exception)
            {
                return iMaxCanMailCount; //throw;
            }
            return MyCommonUtil.GlobalMailCount;
        }

        #region 健保卡憑證加密
        /// <summary>健保卡憑證加密 有卡</summary>
        /// <param name="idno">身分證字號</param>
        /// <param name="srv_id">申辦項目代碼</param>
        /// <param name="modip">異動者IP</param>
        /// <returns></returns>
        public string NHIEncrypt(string idno, string modip, string appDataFolderPath, string srv_id = null, string srv_id1 = null, string apy_city_s1_hd = null, string srv_id2 = null, string apy_city_s2_hd = null, string action = null)
        {
            FrontDAO dao = new FrontDAO();
            string url = string.Empty;

            //NHIEncrypt:驗證方式 Verification Method：健保卡 National Health Insurance Card+註冊密碼 Registration for Password
            //避免guid重複
            Guid newGuid = Guid.NewGuid();
            //bool newGuidyn = false;
            //do
            //{
            //    TblNHI_LOGIN_GUID data = new TblNHI_LOGIN_GUID();
            //    data.guid = newGuid.ToString();
            //    var datalist = dao.GetRowList(data);
            //    if (datalist.Count == 0) { newGuidyn = false; }
            //    else { newGuid = Guid.NewGuid(); }
            //}
            //while (newGuidyn);

            string input = idno + ";" + newGuid.ToString();
            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);

            // Load the public key from .cer file
            string certificateFilePath = Path.Combine(appDataFolderPath, "NDC.cer");
            System.Security.Cryptography.X509Certificates.X509Certificate2 certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificateFilePath);

            // Get the RSA public key from the certificate
            RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            // Encrypt using RSA
            byte[] encryptedData = RSAEncrypt(asciiBytes, rsaPublicKey.ExportParameters(false), false);

            // Convert to Base64
            string encryptedBase64 = Convert.ToBase64String(encryptedData);

            // URL encode the Base64 string
            string encryptedUrlEncoded = HttpUtility.UrlEncode(encryptedBase64);

            //驗證成功之回傳網址
            //var urlTest = "https://euservice.mohw.gov.tw";
            var urlTest = "https://ojt.wda.gov.tw";
            // URL encode the Base64 string
            string encryptedUrlEncodedUrl = HttpUtility.UrlEncode(urlTest);

            //https://esvc.fnp.gov.tw/onlineBidding/NHIICC/TBLProcRechk
            url = "https://www.cp.gov.tw/portal/NHICardVerify.aspx?successUrl=" + encryptedUrlEncodedUrl + "&toVerify=" + encryptedUrlEncoded;

            //儲存驗證紀錄
            //dao.SaveNHILog(null, srv_id, srv_id1, apy_city_s1_hd, srv_id2, apy_city_s2_hd, action, idno, newGuid.ToString(), modip);

            return url;
        }

        /// <summary>健保卡憑證加密 無卡</summary>
        /// <param name="idno">身分證字號</param>
        /// <param name="srv_id">申辦項目代碼</param>
        /// <param name="modip">異動者IP</param>
        /// <returns></returns>
        public string PIIMEncrypt(string idno, string modip, string appDataFolderPath, string srv_id = null, string srv_id1 = null, string apy_city_s1_hd = null, string srv_id2 = null, string apy_city_s2_hd = null, string action = null)
        {
            //FrontDAO dao = new FrontDAO();
            string url = string.Empty;

            //避免guid重複
            Guid newGuid = Guid.NewGuid();
            //bool newGuidyn = false;
            //do
            //{
            //    TblNHI_LOGIN_GUID data = new TblNHI_LOGIN_GUID();
            //    data.guid = newGuid.ToString();
            //    var datalist = dao.GetRowList(data);
            //    if (datalist.Count == 0) { newGuidyn = false; }
            //    else { newGuid = Guid.NewGuid(); }
            //}
            //while (newGuidyn);

            string input = idno + ";" + newGuid.ToString();
            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);

            // Load the public key from .cer file
            string certificateFilePath = Path.Combine(appDataFolderPath, "NDC.cer");
            System.Security.Cryptography.X509Certificates.X509Certificate2 certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificateFilePath);

            // Get the RSA public key from the certificate
            RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            // Encrypt using RSA
            byte[] encryptedData = RSAEncrypt(asciiBytes, rsaPublicKey.ExportParameters(false), false);

            // Convert to Base64
            string encryptedBase64 = Convert.ToBase64String(encryptedData);

            // URL encode the Base64 string
            string encryptedUrlEncoded = HttpUtility.UrlEncode(encryptedBase64);

            //驗證成功之回傳網址
            //var urlTest = "https://euservice.mohw.gov.tw";
            var urlTest = "https://ojt.wda.gov.tw";
            // URL encode the Base64 string
            string encryptedUrlEncodedUrl = HttpUtility.UrlEncode(urlTest);

            string checkFields = "8"; //健保卡卡號(8)
            url = "https://www.cp.gov.tw/portal/PIIMVerify.aspx?" + "checkFields=" + checkFields + "&successUrl=" + encryptedUrlEncodedUrl + "&toVerify=" + encryptedUrlEncoded;

            //儲存驗證紀錄
            //dao.SaveNHILog(null, srv_id, srv_id1, apy_city_s1_hd, srv_id2, apy_city_s2_hd, action, idno, newGuid.ToString(), modip);

            return url;
        }

        #region RES 加密

        //The key size to use maybe 1024/2048
        private const int _EncryptionKeySize = 2048;

        // The buffer size to decrypt per set
        private const int _DecryptionBufferSize = (_EncryptionKeySize / 8);

        //The buffer size to encrypt per set
        private const int _EncryptionBufferSize = _DecryptionBufferSize - 11;
        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                //byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    ////Encrypt the passed byte array and specify OAEP padding.  
                    ////OAEP padding is only available on Microsoft Windows XP or
                    ////later.  
                    //encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                    //2012/10/19 rm 改用block
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[_EncryptionBufferSize];
                        int pos = 0;
                        int copyLength = buffer.Length;
                        while (true)
                        {
                            //Check if the bytes left to read is smaller than the buffer size, 
                            //then limit the buffer size to the number of bytes left

                            if (pos + copyLength > DataToEncrypt.Length)

                                copyLength = DataToEncrypt.Length - pos;

                            //Create a new buffer that has the correct size

                            buffer = new byte[copyLength];

                            //Copy as many bytes as the algorithm can handle at a time, 
                            //iterate until the whole input array is encoded

                            Array.Copy(DataToEncrypt, pos, buffer, 0, copyLength);

                            //Start from here in next iteration

                            pos += copyLength;

                            //Encrypt the data using the public key and add it to the memory buffer

                            //_DecryptionBufferSize is the size of the encrypted data

                            ms.Write(RSA.Encrypt(buffer, false), 0, _DecryptionBufferSize);

                            //Clear the content of the buffer, 
                            //otherwise we could end up copying the same data during the last iteration

                            Array.Clear(buffer, 0, copyLength);

                            //Check if we have reached the end, then exit

                            if (pos >= DataToEncrypt.Length)

                                break;
                        }
                        return ms.ToArray();
                    }
                }
                //return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }
        #endregion

        #region JWT 解密
        /// <summary>encryptStr 進行解密</summary>
        /// <param name="encryptData"></param>
        /// <returns></returns>
        public string JWTSDecrypt(string encryptData)
        {
            string rtn = string.Empty;
            try
            {
                var token = encryptData;
                //OWASP,D1FA98605B7E80040926EB2ADB99DC8AC7FD760DBFB6B82628DFC210D8D5EB3D5FB09A5872285E97AE8077C9C0A119FBFE59C6A810698349932EB2BF
                //79D4A115508A62A14237A6A46CE3B00BCDD94EED35218ACC968EE9696013A369B85EE344EA4E4696B31492296BC3428EBDCD9837362EBC8542DF4019
                //FFDFEE273F13760EE90B3A5A37A40D348F3F9831C422170FC1670C79457374FE6F81CCBB291F198FFAC823920AC087857B3FD54472C93D74F6AF7F2E
                //CAC67E12C787D628E290FEB6ABAF4938A52E3B2585D44003E450E13465633E06FBAA8376056E9FA1C1E0D15D0E3652B21CA822ED91710B469AFE38FA
                //982B0290EC79E299417AA86885378C04AE36CFC20C4BC9C42F64FAC1F5BE11C671289AF4BA644057DF91E90DA22B28D3C0DFBC5BF1D0DE7712E35E3C
                //D2C1459B2F0083B7630AAB693E3B15432DBDEBE4014F3772FDADDC8E74F2DF776CE761C70777493F32CA105EB2225F2FD415ADAEA4289485D4644079
                //54DE905C7C4DE768F0B66B82FAD663E0DA1384C15C306A459C4F20B00FA323471F5ACF278192D5DC619E13BDA11547145B0F229AEFC7F872469626AE
                //EEA015A4F6FDAF86401E49205AA3954A351095E13F7B3F37C6253C5C974E9C548E77B57FA5D5490069712A4771B066D2DFA90BED7DD60551EC134607
                //52CF9AE838FF038D68913FD654308EDC04FC0CA859765035DE53274C14A7AAC3567C856A440744744AC3A804446396D511A4FA4BADB78CD27E103C20
                //1EE9C7461EDC877040EA7F8EEB2801A4FC1CEFCF6E187DDDF69DCC49E184397FEA6EF55B <param name="key,iv">加密金鑰,加密向量</param>
                IJsonSerializer serializer = new JsonNetSerializer();
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);

                var json = decoder.Decode(token, verify: false); // {"iss":"www.gsp.gov.tw","uuid":"d535e749-31ad-4ce3-af88-9aafa72b35f8","result":"A0000:檢核正確!","iat":1548053994}

                rtn = json;

            }
            catch (TokenExpiredException)
            {
                Console.WriteLine("Token has expired");
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature");
            }

            return rtn;
        }
        #endregion

        #endregion

        public static bool IsGuidValid(string inputGuid)
        {
            string pattern = @"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$";
            return Regex.IsMatch(inputGuid, pattern);
        }

        /// <summary>字號做成加密字串</summary>
        /// <param name="toEncode"></param>
        /// <returns></returns>
        public static string EncodeString(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        /// <summary>字串解密</summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        public static string DecodeString(string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt)) { return null; };
            byte[] encodedDataAsBytes = Convert.FromBase64String(toDecrypt.Replace(" ", "+"));
            return System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
        }

        /// <summary>加密字串=</summary>
        /// <param name="toEncode"></param>
        /// <returns></returns>
        public static string TkEncrypt(string toEncode)
        {
            Turbo.Crypto.AesTk aesTk = new Turbo.Crypto.AesTk();
            return aesTk.Encrypt(toEncode);
        }

        /// <summary>加密字串2</summary>
        /// <param name="toDecode"></param>
        /// <returns></returns>
        public static string TkDecrypt(string toDecode)
        {
            Turbo.Crypto.AesTk aesTk = new Turbo.Crypto.AesTk();
            return aesTk.Decrypt(toDecode);
        }

        public static string GenerateOneStars(int n, string Star = "＊")
        {
            string result = "";
            // 如果 n 小於或等於 0，返回空字串
            if (n <= 0) { return result; }

            for (int i = 0; i < n; i++)
            {
                result += Star;
            }
            return result;
        }

        /// <summary> 取得正確的Client端IP (包括 HTTP_X_FORWARDED_FOR 判斷) </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];//撈代理伺服器
            if (!string.IsNullOrEmpty(ip))//有掛代理伺服器
            {
                string[] ipRange = ip.Split(',');
                int le = ipRange.Length - 1;
                ip = ipRange[le];
                return ip;
            }
            // 沒有掛代理伺服器
            ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            return ip;
        }

        /// <summary>聯絡電話僅可輸入數字或「-」符號</summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            //正規表示式 (Regular Expression) 來檢核字串是否符合您指定的聯絡電話規
            //using System.Text.RegularExpressions;
            if (string.IsNullOrEmpty(phoneNumber)) return false; // 空字串視為無效 
            // 最彈性的正規表示式模式，允許數字和最多一個連字號在數字之間：
            // ^：字串的開始 // \d+：一個或多個數字 // (-?\d+)*：零個或多個 (一個連字號後面跟著一個或多個數字) // $：字串的結束
            string pattern = @"^\d+(-?\d+)*$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        /// <summary>行動電話僅可輸入數字10碼-A</summary>
        /// <param name="mobileNumber"></param>
        /// <returns></returns>
        public static bool IsValidMobileNumber10(string mobileNumber)
        {
            //using System.Text.RegularExpressions;
            if (string.IsNullOrEmpty(mobileNumber)) return false; // 空字串視為無效
            // 正規表示式模式：^ 代表字串開始，$ 代表字串結束，\d{10} 代表恰好 10 個數字
            string pattern = @"^\d{10}$";
            return Regex.IsMatch(mobileNumber, pattern);
        }

        /// <summary>行動電話僅可輸入數字10碼-B</summary>
        /// <param name="mobileNumber"></param>
        /// <returns></returns>
        public static bool IsValidMobileNumber10B(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber)) return false; // 空字串視為無效
            if (mobileNumber.Length != 10) return false; // 長度必須等於 10
            foreach (char c in mobileNumber)
            {
                if (!char.IsDigit(c)) return false; // 必須是數字
            }
            return true;
        }

        /// <summary>踢出不合法的檔名字元</summary>
        /// <param name="emlFilename"></param>
        /// <returns></returns>
        public static string GetValidFileName(string emlFilename)
        {
            // Path.GetInvalidFileNameChars() 取得非法字元
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                // 移除非法字元 // Note: C# 的 string.Replace(char, char) 更好，但由於這裡是要移除，所以替換成空字串。直接替換字元會更有效率，但需要確保目標字元不存在。
                // 這裡使用 c.ToString() 是因為 Replace(string, string) 是最通用的。
                emlFilename = emlFilename.Replace(c.ToString(), "");
            }
            return emlFilename;
        }

        /// <summary>無論是 "第1期" 還是 "第01期"，都能被正確地移除</summary>
        /// <param name="inputclasscnm"></param>
        /// <returns></returns>
        public static string RegReplaceCLSNM(string inputclasscnm)
        {
            if (string.IsNullOrEmpty(inputclasscnm) || inputclasscnm.Length < 3) return inputclasscnm;
            //string[] inputs = {," 班第01期",," 班第02期",," 班第1期",," 班第2期",," 班" // 測試沒有期的情況,
            // 定義正規表達式模式
            // Regex pattern: "第" 後面跟著一個或多個數字，然後是 "期"
            // \d+ 匹配一個或多個數字
            // ? 表示匹配前面的模式（這裡是指"第\d+期"）0次或1次，確保如果沒有「第N期」也不會出錯
            // RegexOptions.IgnoreCase 可以讓模式不區分大小寫，但這裡「第N期」通常不會有大小寫問題
            //foreach (string input in inputs)
            // 使用 Regex.Replace 方法進行替換 // 第一個參數是輸入字串 // 第二個參數是正規表達式模式 // 第三個參數是替換成的字串（這裡用空字串表示刪除匹配到的部分）
            //string result = Regex.Replace(input, pattern, ""); Console.WriteLine($"原始字串: {input}"); Console.WriteLine($"處理後字串: {result}\n");
            string pattern = "第\\d+期"; // 注意：在C#字串中，反斜線 \ 需要用 \\ 來跳脫
            return Regex.Replace(inputclasscnm, pattern, "");
        }
    }
}
