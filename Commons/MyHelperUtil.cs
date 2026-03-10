using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using Turbo.DataLayer;
using System.Collections;
using System.Text;
using Turbo.Commons;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 放置 WDAIIP.WEB 專用的 HelperUtil 擴充,
    /// 只有當 Turbo.Commons.HelperUtil 不足時才寫新的
    /// </summary>
    public class MyHelperUtil : Turbo.Commons.HelperUtil
    {
        /// <summary>
        /// 安全的 string.Trim() 包裝, 自動判斷 string 是否為 null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Trim(string str)
        {
            return string.IsNullOrEmpty(str) ? null : str.Trim();
        }

        /// <summary>
        /// 取得 DropDownList 的縣市代碼來源清單
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetCityCodeList(string selected)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCityCodeList();
            return MyCommonUtil.ConvertSelectList(list, selected);
        }

        /// <summary>
        /// 根據指定的縣市代碼(CITYCODE), 取得 DropDownList 的(鄉鎮市區)郵遞區號(ZIP)清單
        /// </summary>
        /// <param name="sity"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList GetCountyZipList(string sity, string selected)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCountyZipList(sity);
            return MyCommonUtil.ConvertSelectList(list, selected);
        }

        /// <summary>is 判斷-檢核字串是否為空</summary>
        /// <param name="object"> 傳入物件 </param>
        /// <returns> true or false </returns>
        public static bool IsEmpty(object @object)
        {
            if (@object == null)
            {
                return true;
            }
            if (!"".Equals(@object.ToString().Trim()))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 檢核字串是否為空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsEmpty(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return true;
            }
            if (!"".Equals(str.Trim()))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 檢核List是否為空
        /// </summary>
        /// <param name="list"> 傳入物件 </param>
        /// <returns> true or false </returns>
        public static bool IsEmpty(IList list)
        {
            if (list.Count == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 檢核是否為統一編號
        /// </summary>
        /// <param name="strCardno"></param>
        /// <returns></returns>
        public static bool IsUniformNo(object arg)
        {
            var strCardno = SafeTrim(arg);

            if (strCardno.Trim().Length < 8)
            {
                return false;
            }
            else
            {
                int[] intTmpVal = new int[6];
                int intTmpSum = 0;
                for (int i = 0; i < 6; i++)
                {
                    //位置在奇數位置的*2，偶數位置*1，位置計算從0開始
                    if (i % 2 == 1)
                        intTmpVal[i] = overTen(int.Parse(strCardno[i].ToString()) * 2);
                    else
                        intTmpVal[i] = overTen(int.Parse(strCardno[i].ToString()));

                    intTmpSum += intTmpVal[i];
                }
                intTmpSum += overTen(int.Parse(strCardno[6].ToString()) * 4); //第6碼*4
                intTmpSum += overTen(int.Parse(strCardno[7].ToString())); //第7碼*1

                if (intTmpSum % 10 != 0) //除以10後餘0表示正確，反之則錯誤
                    return false;
            }
            return true;
        }

        /// <summary>
        /// (overTen)
        /// </summary>
        /// <param name="intVal"></param>
        /// <returns></returns>
        private static int overTen(int intVal) //超過10則十位數與個位數相加，直到相加後小於10
        {
            if (intVal >= 10)
                intVal = overTen((intVal / 10) + (intVal % 10));
            return intVal;
        }

        /// <summary>
        /// 檢核身份證號格式
        /// </summary>
        /// <param name="arg_Identify"></param>
        /// <returns></returns>
        public static bool IsIDNO(object arg)
        {
            var arg_Identify = SafeTrim(arg);
            var d = false;
            if (arg_Identify.Length == 10)
            {
                arg_Identify = arg_Identify.ToUpper();
                if (arg_Identify[0] >= 0x41 && arg_Identify[0] <= 0x5A)
                {
                    var a = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
                    var b = new int[11];
                    b[1] = a[(arg_Identify[0]) - 65] % 10;
                    var c = b[0] = a[(arg_Identify[0]) - 65] / 10;
                    for (var i = 1; i <= 9; i++)
                    {
                        b[i + 1] = arg_Identify[i] - 48;
                        c += b[i] * (10 - i);
                    }
                    if (((c % 10) + b[10]) % 10 == 0)
                    {
                        d = true;
                    }
                }
            }
            return d;
        }

        /// <summary>
        /// 檢核勞保證號格式
        /// </summary>
        /// <param name="actno"></param>
        /// <returns></returns>
        public static bool IsACTNO(string actno)
        {
            bool rtn = true;

            if (!string.IsNullOrWhiteSpace(actno))
            {
                string chkVal = string.Empty;
                for (int i = 0; i < actno.Length; i++)
                {
                    chkVal = actno.Substring(i, 1);
                    //只能是英文或數字
                    if (!(MyCommonUtil.IsEng(chkVal) || MyCommonUtil.isUnsignedInt(chkVal)))
                    {
                        rtn = false;
                        break;
                    }
                }
            }

            return rtn;
        }

        // Trim
        /// <summary>trim 字串 (避免傳入值為null)</summary>
        /// <param name="s">
        /// @return
        /// </param>
        public static string SafeTrim(object s)
        {
            return SafeTrim(s, "");
        }

        /// <summary>
        /// trim 字串 (避免傳入值為null)
        /// </summary>
        /// <param name="s"> 傳入字 </param>
        /// <param name="defaultStr">
        /// 預設字串
        /// @return
        /// </param>
        public static string SafeTrim(object s, string defaultStr)
        {
            if (s == null || IsEmpty(s))
            {
                return defaultStr;
            }
            return s.ToString().Trim();
        }

        /// <summary>
        /// 半型數字轉國字大寫數字, 
        /// 12345 => 一二三四五 OR  壹貳參肆伍 OR １２３４５ OR 12345
        /// <para>轉換種類(type)如下:</para>
        /// <para>ctype = "S" 轉簡單國字 Ex:一二三四五</para>
        /// <para>ctype = "C" 轉複雜國字 Ex:壹貳參肆伍</para>
        /// <para>ctype = "F" 轉全形數字 Ex:１２３４５</para>
        /// <para>ctype = 其他 不轉換, 為原來的半形數字(適用於橫式的報表列印)</para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ctype"></param>
        /// <returns></returns>
        public static string Utl_ChineseValue(string value, string ctype)
        {
            string[] TypeS = { "○", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] TypeC = { "零", "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖" };
            string[] TypeF = { "０", "１", "２", "３", "４", "５", "６", "７", "８", "９" };

            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            foreach (byte ch in Encoding.ASCII.GetBytes(value))
            {
                // ch 是 value 字串每一個字元的 ASCII 碼
                // ch - 48(Ascii of '0') = array index
                int idx = ch - 48;

                if ("S".Equals(ctype))
                {
                    sb.Append(TypeS[idx]);
                }
                else if ("C".Equals(ctype))
                {
                    sb.Append(TypeC[idx]);
                }
                else if ("F".Equals(ctype))
                {
                    sb.Append(TypeF[idx]);
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 將民國年月日(YYYMMDD)字串, 以 __年__月__日 的格式返回
        /// </summary>
        /// <param name="strTwDate"></param>
        /// <returns></returns>
        public static string FormatZHDate(string strTwDate)
        {
            if (string.IsNullOrEmpty(strTwDate) || strTwDate.Length != 7)
            {
                return "";
            }

            return string.Format("{0}年{1}月{2}日",
                strTwDate.Substring(0, 3),
                strTwDate.Substring(3, 2),
                strTwDate.Substring(5, 2)
                );
        }

        /// <summary>
        /// 民國年月日輸出文字
        /// </summary>
        /// <param name="adDate"></param>
        /// <returns></returns>
        public static string GetZHDate(DateTime? adDate)
        {
            if (!adDate.HasValue) { return ""; }
            string s_ZHdate = "{0}/{1}/{2}";
            return string.Format(s_ZHdate, (adDate.Value.Year - 1911), adDate.Value.ToString("MM"), adDate.Value.ToString("dd"));
        }


        /// <summary>
        /// 以指定的 maskLen 長度, 將傳入的 ID 字串右側 maskLen 字元取代為 'X' 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="maskLen">預設為 5</param>
        /// <returns></returns>
        public static string IDMasking(string id, int maskLen = 5)
        {
            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            if (maskLen <= 0)
            {
                maskLen = 5;
            }
            int nonMaskLen = id.Length - maskLen;
            if (nonMaskLen < 0)
            {
                nonMaskLen = 0;
            }

            return id.Substring(0, nonMaskLen) + new string('X', maskLen);
        }

        /// <summary>
        /// 字元轉換(全型-->半型)
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public static string ChangeIDNO(string idno)
        {
            if (!string.IsNullOrEmpty(idno))
            {
                idno = idno.Replace(" ", "");
                idno = idno.Replace("　", "");
                idno = idno.Replace("Ａ", "A");
                idno = idno.Replace("Ｂ", "B");
                idno = idno.Replace("Ｃ", "C");
                idno = idno.Replace("Ｄ", "D");
                idno = idno.Replace("Ｅ", "E");
                idno = idno.Replace("Ｆ", "F");
                idno = idno.Replace("Ｇ", "G");
                idno = idno.Replace("Ｈ", "H");
                idno = idno.Replace("Ｉ", "I");
                idno = idno.Replace("Ｊ", "J");
                idno = idno.Replace("Ｋ", "K");
                idno = idno.Replace("Ｌ", "L");
                idno = idno.Replace("Ｍ", "M");
                idno = idno.Replace("Ｎ", "N");
                idno = idno.Replace("Ｏ", "O");
                idno = idno.Replace("Ｐ", "P");
                idno = idno.Replace("Ｑ", "Q");
                idno = idno.Replace("Ｒ", "R");
                idno = idno.Replace("Ｓ", "S");
                idno = idno.Replace("Ｔ", "T");
                idno = idno.Replace("Ｕ", "U");
                idno = idno.Replace("Ｖ", "V");
                idno = idno.Replace("Ｗ", "W");
                idno = idno.Replace("Ｘ", "X");
                idno = idno.Replace("Ｙ", "Y");
                idno = idno.Replace("Ｚ", "Z");
                idno = idno.Replace("ａ", "A");
                idno = idno.Replace("ｂ", "B");
                idno = idno.Replace("ｃ", "C");
                idno = idno.Replace("ｄ", "D");
                idno = idno.Replace("ｅ", "E");
                idno = idno.Replace("ｆ", "F");
                idno = idno.Replace("ｇ", "G");
                idno = idno.Replace("ｈ", "H");
                idno = idno.Replace("ｉ", "I");
                idno = idno.Replace("ｊ", "J");
                idno = idno.Replace("ｋ", "K");
                idno = idno.Replace("ｌ", "L");
                idno = idno.Replace("ｍ", "M");
                idno = idno.Replace("ｎ", "N");
                idno = idno.Replace("ｏ", "O");
                idno = idno.Replace("ｐ", "P");
                idno = idno.Replace("ｑ", "Q");
                idno = idno.Replace("ｒ", "R");
                idno = idno.Replace("ｓ", "S");
                idno = idno.Replace("ｔ", "T");
                idno = idno.Replace("ｕ", "U");
                idno = idno.Replace("ｖ", "V");
                idno = idno.Replace("ｗ", "W");
                idno = idno.Replace("ｘ", "X");
                idno = idno.Replace("ｙ", "Y");
                idno = idno.Replace("ｚ", "Z");
                idno = idno.Replace("a", "A");
                idno = idno.Replace("b", "B");
                idno = idno.Replace("c", "C");
                idno = idno.Replace("d", "D");
                idno = idno.Replace("e", "E");
                idno = idno.Replace("f", "F");
                idno = idno.Replace("g", "G");
                idno = idno.Replace("h", "H");
                idno = idno.Replace("i", "I");
                idno = idno.Replace("j", "J");
                idno = idno.Replace("k", "K");
                idno = idno.Replace("l", "L");
                idno = idno.Replace("m", "M");
                idno = idno.Replace("n", "N");
                idno = idno.Replace("o", "O");
                idno = idno.Replace("p", "P");
                idno = idno.Replace("q", "Q");
                idno = idno.Replace("r", "R");
                idno = idno.Replace("s", "S");
                idno = idno.Replace("t", "T");
                idno = idno.Replace("u", "U");
                idno = idno.Replace("v", "V");
                idno = idno.Replace("w", "W");
                idno = idno.Replace("x", "X");
                idno = idno.Replace("y", "Y");
                idno = idno.Replace("z", "Z");
                idno = idno.Replace("０", "0");
                idno = idno.Replace("１", "1");
                idno = idno.Replace("２", "2");
                idno = idno.Replace("３", "3");
                idno = idno.Replace("４", "4");
                idno = idno.Replace("５", "5");
                idno = idno.Replace("６", "6");
                idno = idno.Replace("７", "7");
                idno = idno.Replace("８", "8");
                idno = idno.Replace("９", "9");
                idno = idno.Replace("＠", "@");
            }

            return idno;
        }

        /// <summary>
        /// 取得使用者輸入性別，並與身份証判斷是否正確(true 正確 ; false 錯誤)
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static bool CheckMemberSex(string idno, string sex)
        {
            bool rtn = false;

            if (idno.Length > 1)
            {
                switch (idno.Substring(1, 1))
                {
                    case "1":
                    case "A":
                    case "C":
                        //男:M
                        if ("M".Equals(sex)) rtn = true;
                        break;
                    case "2":
                    case "B":
                    case "D":
                        //女:F
                        if ("F".Equals(sex)) rtn = true;
                        break;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 轉換換行符號
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ChangeBreakLine(string s_content)
        {
            return ChgBreakLine(s_content).Replace("\n", "<br>").Replace(" ", "&nbsp;&nbsp;");
        }

        #region [依照副檔名，找到對應的附件圖示]

        public static string GetFileIcon(string myFileType)
        {
            string r_Img = string.Empty;

            if (!string.IsNullOrEmpty(myFileType))
            {
                myFileType = myFileType.Trim().ToUpper();

                string[] fileType01 = { ".DOC", ".DOCX" };                        //MS的文書檔
                string[] fileType02 = { ".XLS", ".XLSX" };                        //MS的試算表
                string[] fileType03 = { ".PPT", ".PPTX", ".PPS", ".PPSX" };       //MS的簡報檔
                string[] fileType04 = { ".ODT" };                                 //OpenFile的文書檔
                string[] fileType05 = { ".ODS" };                                 //OpenFile的試算表
                string[] fileType06 = { ".ODP" };                                 //OpenFile的簡報檔
                string[] fileType07 = { ".PDF" };                                 //可攜式文件
                string[] fileType08 = { ".ZIP", ".RAR", ".7Z", ".ZIPX", ".Z" };   //壓縮檔
                string[] fileType09 = { ".BMP", ".TIFF", ".GIF", ".JPG", ".JPEG", ".PNG", ".TIF", ".SVG", ".ICO" };  //圖片檔
                string[] fileType10 = { ".MKV", ".MP4", ".AVI", ".RMVB", ".FLV", ".WMV", ".DAT", ".MOV" };           //影片檔

                //GetFileIcon
                int myIndex = -1;

                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType01, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "doc.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType02, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "xls.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType03, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "ppt.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType04, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "odt.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType05, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "ods.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType06, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "odp.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType07, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "pdf.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType08, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "zip.png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType09, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "(image).png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    myIndex = Array.IndexOf(fileType10, myFileType);

                    if (myIndex != -1)
                    {
                        r_Img = "(movie).png";
                    }
                }
                if (string.IsNullOrEmpty(r_Img))
                {
                    r_Img = "(document).png";
                }

                //fileImg
                if (!string.IsNullOrEmpty(r_Img))
                {
                    r_Img = "fileImg/" + r_Img;
                }
            }

            return r_Img;
        }

        #endregion

        #region [將一般Decimal數值，轉換成貨幣格式]

        public static string DecimalNumToMoneyFormat(Decimal? myNum)
        {
            string myResult = "";

            if (myNum.HasValue)
            {
                Decimal tValue = myNum.Value;

                if (tValue > 0)
                {
                    myResult = String.Format("{0:0,0}", tValue);
                }
                else
                {
                    myResult = "0";
                }
            }

            return myResult;
        }

        #endregion

        /// <summary>Filters a string, returning only English letters and numbers.</summary>
        /// <param name="inputString">The string to filter.</param>
        /// <returns>A new string containing only alphanumeric characters.</returns>
        public static string GetEnglishLettersAndNumbers(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) { return string.Empty; }
            // Using LINQ to filter characters
            return new string(inputString.Where(c => char.IsLetterOrDigit(c) && (c < 128)).ToArray());
        }

    }
}