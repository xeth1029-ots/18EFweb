using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using log4net;
using Turbo.DataLayer;
using WDAIIP.WEB.Models.Entities;
using Turbo.Commons;

namespace WDAIIP.WEB.DataLayers
{
    /// <summary>
    /// 放置 WDAIIP.WEB 擴充的共用代碼 KeyMapDAO 功能 
    /// </summary>
    public class MyKeyMapDAO : KeyMapDAO
    {
        private new static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //----------------------確定有在使用的function(其餘尚未清查)----------------------
        #region GetCityCodeList 取全台縣市的項目
        /// <summary>取全台縣市的項目</summary>
        public IList<KeyMapModel> GetCityCodeList()
        {
            string funcName = "KeyMap.getCityCodeList";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        /// <summary>上課時間(可複選)</summary>
        /// <returns></returns>
        public IList<KeyMapModel> GetClassTimeCodeList()
        {
            string funcName = "KeyMap.getClassTimeCodeList";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }

        #region GetCityName 取縣市名稱
        /// <summary>
        /// 取縣市名稱
        /// </summary>
        /// <param name="CTID"></param>
        /// <returns></returns>
        public string GetCityName(string CTID)
        {
            if (string.IsNullOrEmpty(CTID))
            {
                //TODO: 雛型階段允許 null, 開發階段後不允許 null //throw new ArgumentNullException("CTID");
            }

            Hashtable param = new Hashtable { ["CTID"] = CTID };
            string funcName = "KeyMap.getCityName";
            return (string)base.QueryForObject(funcName, param);
        }
        #endregion

        #region GetCountyZipList 根據指定的縣市代碼(CITYCODE), 查詢(鄉鎮市區)的郵遞區號(ZIP)清單
        /// <summary>
        /// 根據指定的縣市代碼(CTID), 查詢(鄉鎮市區)的郵遞區號(ZIPCODE)清單
        /// </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <returns></returns>
        public IList<KeyMapModel> GetCountyZipList(string CTID)
        {
            if (string.IsNullOrEmpty(CTID))
            {
                //TODO: 雛型階段允許 null, 開發階段後不允許 null //throw new ArgumentNullException("CTID");
            }

            Hashtable param = new Hashtable { ["CTID"] = CTID };
            string funcName = "KeyMap.getCountyZipList";
            return base.QueryForListAll<KeyMapModel>(funcName, param);
        }
        #endregion

        #region GetZipName 取鄉鎮市區名稱
        /// <summary> 取鄉鎮市區名稱 </summary>
        /// <param name="ZIPCODE"></param>
        /// <returns></returns>
        public string GetZipName(string ZIPCODE)
        {
            //TODO: 雛型階段允許 null, 開發階段後不允許 null
            //throw new ArgumentNullException("ZIPCODE");
            if (string.IsNullOrEmpty(ZIPCODE)) { return ""; }

            Hashtable param = new Hashtable();
            param["ZIPCODE"] = ZIPCODE;
            string funcName = "KeyMap.getZipName";
            return (string)base.QueryForObject(funcName, param);
        }
        #endregion

        #region GetZipName2 根據指定的郵遞區號(ZIPCODE), 取得地址
        /// <summary> 取得地址-郵遞區號 </summary>
        /// <param name="ZIPCODE">郵遞區號</param>
        /// <returns></returns>
        public string GetZipName2(string ZIPCODE)
        {
            string rtn = string.Empty;
            if (string.IsNullOrEmpty(ZIPCODE)) { return rtn; }

            Int32 i_zipcode = 0;
            if (!Int32.TryParse(ZIPCODE, out i_zipcode)) { return rtn; }

            Hashtable param = new Hashtable();
            param["ZIPCODE"] = i_zipcode;

            string funcName = "KeyMap.getZipName2";
            IList<KeyMapModel> list = base.QueryForListAll<KeyMapModel>(funcName, param);

            if (list == null) { return rtn; }
            if (list.Count == 0) { return rtn; }
            //return list[0].TEXT; //return list[0].CODE;
            rtn = list[0].CODE;
            return rtn;
        }
        #endregion

        #region GetClassCateList 取六大職能別
        /// <summary>訓練職能別,取六大職能別項目</summary>
        public IList<KeyMapModel> GetClassCateList()
        {
            string funcName = "KeyMap.getClassCate";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetTmidList 取策略性產業篩選
        /// <summary>取策略性產業篩選的項目</summary>
        public IList<KeyMapModel> GetTmidList()
        {
            string funcName = "KeyMap.getTmid";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        /// <summary>訓練職類 項目清單 TMID,BUSNAME</summary>
        public IList<KeyMapModel> GetTMIDBUSList()
        {
            string funcName = "KeyMap.getTMIDBUS";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }

        /// <summary>訓練職類 項目清單 TMID,JOBNAME 依 PARENT1</summary>
        public IList<KeyMapModel> GetTMIDJOBList(Int64 PARENT1)
        {
            string funcName = "KeyMap.queryTMIDJOB";
            Hashtable parms = new Hashtable();
            parms["PARENT1"] = PARENT1;
            return base.QueryForListAll<KeyMapModel>(funcName, parms);
        }

        /// <summary>
        /// 通俗職業代碼-2019-項目清單
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> GetCJOBList()
        {
            string funcName = "KeyMap.getCJOB";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }

        #region GetTMIDItemLv1List 取得訓練業別第一層選項
        /// <summary>
        /// 查詢訓練業別第一層選項(產投)
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> GetTMIDItemLv1List()
        {
            string funcName = "KeyMap.queryTMIDItemLv1";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetTMIDItemLv2List 根據指定的職類課程(JOBTMID), 查詢業別(TMID)清單
        /// <summary>
        /// 查詢訓練業別第二層選項(產投) 
        /// </summary>
        /// <param name="JOBTMID"></param>
        /// <returns></returns>
        public IList<KeyMapModel> GetTMIDItemLv2List(Int64 jobTMID)
        {
            string funcName = "KeyMap.queryTMIDItemLv2";
            Hashtable parms = new Hashtable();
            parms["JOBTMID"] = jobTMID;

            return base.QueryForListAll<KeyMapModel>(funcName, parms);
        }
        #endregion

        #region GetJobName 取得職類課程名稱（產投訓練業別）
        /// <summary>
        /// 取得職類課程名稱（產投訓練業別第二層）
        /// </summary>
        /// <param name="jobtmid"></param>
        /// <returns></returns>
        public string GetJobName(Int64 jobtmid)
        {
            string funcName = "KeyMap.getJobName";

            Hashtable parms = new Hashtable();

            parms["TMID"] = jobtmid;

            return (string)base.QueryForObject(funcName, parms);
        }
        #endregion

        /// <summary>
        /// GetBusName (訓練職類大項)
        /// </summary>
        /// <param name="TMID"></param>
        /// <returns></returns>
        public string GetBusName(Int64 TMID)
        {
            string funcName = "KeyMap.getBusName";

            Hashtable parms = new Hashtable();

            parms["TMID"] = TMID;

            return (string)base.QueryForObject(funcName, parms);
        }

        /// <summary>
        /// GetJobName2 (訓練職類中項)
        /// </summary>
        /// <param name="TMID"></param>
        /// <returns></returns>
        public string GetJobName2(Int64 TMID)
        {
            string funcName = "KeyMap.getJobName2";

            Hashtable parms = new Hashtable();

            parms["TMID"] = TMID;

            return (string)base.QueryForObject(funcName, parms);
        }

        #region GetTrainName 取得業別名稱（產投訓練業別）
        /// <summary>
        /// 取得業別名稱（產投訓練業別第三層）
        /// </summary>
        /// <param name="tmid"></param>
        /// <returns></returns>
        public string GetTrainName(Int64 tmid)
        {
            string funcName = "KeyMap.getTrainName";

            Hashtable parms = new Hashtable();

            parms["TMID"] = tmid;

            return (string)base.QueryForObject(funcName, parms);
        }
        #endregion

        /// <summary> 遠距教學 </summary>
        /// <param name="DISTANCE"></param>
        /// <returns></returns>
        public string GetDISTANCEN(string s_DA)
        {
            string s_DISTANCE_N = "";
            if (s_DA == null || string.IsNullOrEmpty(s_DA)) { return s_DISTANCE_N; }
            // 1."申請整班為遠距教學", 2."申請部分課程為遠距教學,3.申請整班為實體教學/無遠距教學
            //s_DISTANCE_N = (DISTANCE).Equals("1") ? "(此班為遠距教學)" : (DISTANCE ?? "").Equals("2") ? "(部分課程為遠距教學)" : "";
            //s_DISTANCE_N = DISTANCE.Equals("1") ? "(整班為遠距教學)" : DISTANCE.Equals("2") ? "(部分課程為遠距/實體教學)" : DISTANCE.Equals("3") ? "(整班為實體教學)" : "";
            s_DISTANCE_N = s_DA.Equals("1") ? "(遠距課程)" : s_DA.Equals("2") ? "(混成課程)" : s_DA.Equals("3") ? "(實體課程)" : "";
            return s_DISTANCE_N;
        }

        /// <summary>
        /// 個資法遮罩-姓名顯示
        /// </summary>
        /// <param name="sCNAME"></param>
        /// <returns></returns>
        public string GET_CNAME_MK(string sCNAME)
        {
            string funcName = "KeyMap.getCNAMEMK";
            var parms = new Hashtable { ["CNAME"] = sCNAME };
            return (string)base.QueryForObject(funcName, parms);
        }

        /// <summary>
        /// 個資法遮罩-身份證號
        /// </summary>
        /// <param name="sIDNO"></param>
        /// <returns></returns>
        public string GET_IDNO_MK(string sIDNO)
        {
            string funcName = "KeyMap.getIDNOMK";
            var parms = new Hashtable { ["IDNO"] = sIDNO };
            return (string)base.QueryForObject(funcName, parms);
        }

        #region GetSchoolList 取得單位清單(依據縣市代碼、單位名稱關鍵字)
        /// <summary>
        /// 取得單位清單
        /// </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <param name="SCHOOLNAME">單位名稱關鍵字</param>
        /// <returns></returns>
        public IList<KeyMapModel> GetSchoolList(Int64? CTID, Int64? ZIPCODE, string SCHOOLNAME)
        {
            Hashtable param = new Hashtable();
            param["CTID"] = CTID;
            param["ZIPCODE"] = ZIPCODE;
            param["SCHOOLNAME"] = SCHOOLNAME;

            //string s_msg1 = "";
            //s_msg1 = string.Format("##GetSchoolList-KeyMapModel CTID={0},ZIPCODE={1},SCHOOLNAME={2};", CTID, ZIPCODE, SCHOOLNAME);
            //LOG.Debug(s_msg1);
            string funcName = "KeyMap.getSchoolList";
            return base.QueryForListAll<KeyMapModel>(funcName, param);
        }
        #endregion

        #region GetEnterCount 取得報名人數 (扣除e網審核失敗)
        /// <summary>取得報名人數 (扣除e網審核失敗)</summary>
        /// <param name="OCID"></param>
        /// <returns></returns>
        public int GetEnterCount(Int64? OCID)
        {
            if (OCID == null || !OCID.HasValue) return 0;
            Hashtable param = new Hashtable();
            param["OCID"] = OCID;
            string funcName = "KeyMap.getEnterCount";
            return (int)base.QueryForObject(funcName, param);
        }
        #endregion

        #region GetOrgName 取單位(機構)名稱
        /// <summary>
        /// 取單位(機構)名稱
        /// </summary>
        /// <param name="COMIDNO">機構統一編號</param>
        /// <returns></returns>
        public string GetOrgName(string COMIDNO)
        {
            if (string.IsNullOrEmpty(COMIDNO))
            {
                //TODO: 雛型階段允許 null, 開發階段後不允許 null
                //throw new ArgumentNullException("COMIDNO");
            }

            Hashtable param = new Hashtable();
            param["COMIDNO"] = COMIDNO;

            string funcName = "KeyMap.getOrgName";
            return (string)base.QueryForObject(funcName, param);
        }
        #endregion

        #region 取得報名按鈕控制設定結果
        /// <summary>
        /// 查詢取得報名按鈕控制設定結果
        /// </summary>
        /// <param name="funID"></param>
        /// <returns></returns>
        public TblTB_CONTENT GetCtrlItemSet(string funID)
        {
            TblTB_CONTENT result = null;
            string funcName = "KeyMap.getCtrlItem";

            TblTB_CONTENT whereConds = new TblTB_CONTENT
            {
                FUNID = funID
            };

            result = base.QueryForObject<TblTB_CONTENT>(funcName, whereConds);
            return result;
        }
        #endregion

        #region GetTableMaxSeqNo 取得資料表中欄位的最大值 + 1
        /// <summary>
        /// 取得資料表中欄位的最大值 + 1 (作為表格自動序號之用)
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="fieldName">取值欄位名稱</param>
        /// <param name="fkField">關聯欄位1的欄名(optional)</param>
        /// <param name="fkValue">關聯欄位1的欄位值(optional)</param>
        /// <param name="fk2Field">關聯欄位2的欄名(optional)</param>
        /// <param name="fk2Value">關聯欄位2的欄位值(optional)</param>
        /// <param name="fk3Field">關聯欄位3的欄名(optional)</param>
        /// <param name="fk3Value">關聯欄位3的欄位值(optional)</param>
        /// <returns></returns>
        public int GetTableMaxSeqNo(DBRowTableName tableName, string fieldName, string fkField = null, object fkValue = null,
            string fk2Field = null, object fk2Value = null, string fk3Field = null, object fk3Value = null)
        {
            Hashtable param = new Hashtable();
            param["TABLE_NAME"] = tableName.ToString();
            param["FIELD_NAME"] = fieldName;
            param["FK_FIELD"] = fkField;
            param["FK_VALUE"] = fkValue;
            param["FK2_FIELD"] = fk2Field;
            param["FK2_VALUE"] = fk2Value;
            param["FK3_FIELD"] = fk3Field;
            param["FK3_VALUE"] = fk3Value;
            return (int)base.QueryForObject("KeyMap.getMaxSeqNo", param) + 1;
        }

        #endregion

        #region GetSystemMsg 取得系統提示訊息
        /// <summary>取得系統提示訊息</summary>
        /// <param name="ItemName">參數名</param>
        /// <param name="TPlanID">總計劃ID</param>
        /// <returns></returns>
        public string GetSystemMsg(string ItemName, string TPlanID)
        {
            Hashtable param = new Hashtable();
            param["ItemName"] = ItemName;
            param["TPlanID"] = TPlanID;
            return (string)base.QueryForObject("KeyMap.getSystemMsg", param);
        }

        /// <summary>取得系統參數</summary>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public string GetSystemConfig(string ItemName)
        {
            Hashtable param = new Hashtable();
            param["ItemName"] = ItemName;
            var rst = base.QueryForObject("KeyMap.getSystemConfig", param);
            return (string)rst;
        }
        #endregion

        #region GetDegreeIDList 取學歷清單
        /// <summary>
        /// 取學歷清單
        /// </summary>
        public IList<KeyMapModel> GetDegreeIDList()
        {
            string funcName = "KeyMap.getDegreeID";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetGraduateStatusList 取畢業狀況清單
        /// <summary>
        /// 取畢業狀況清單
        /// </summary>
        public IList<KeyMapModel> GetGraduateStatusList()
        {
            string funcName = "KeyMap.getGraduateStatus";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetMIdentityIDList 取主要參訓身份別
        /// <summary>取主要參訓身份別 getMIdentityID</summary>
        public IList<KeyMapModel> GetMIdentityIDList()
        {
            string funcName = "KeyMap.getMIdentityID";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetServdeptList 取服務部門
        /// <summary>取服務部門</summary>
        public IList<KeyMapModel> GetServdeptList()
        {
            string funcName = "KeyMap.getServdept";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetJobTitleList 取職稱
        /// <summary>取職稱</summary>
        public IList<KeyMapModel> GetJobTitleList()
        {
            string funcName = "KeyMap.getJobTitle";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetTradeList 取行業別
        /// <summary>取行業別</summary>
        public IList<KeyMapModel> GetTradeList()
        {
            string funcName = "KeyMap.getTrade";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetSysDateNow 取得(DB)系統時間

        /// <summary> 取得(DB)系統時間 </summary>
        /// <returns></returns>
        public DateTime GetSysDateNow()
        {
            return (DateTime)base.QueryForObject("KeyMap.getSysDateNow", null);
        }

        #endregion

        #region GetCJobNoItemLv1List 取得通俗職類第一層選項
        /// <summary>
        /// 查詢通俗職類第一層選項
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> GetCJobNoItemLv1List()
        {
            string funcName = "KeyMap.queryCJobNoItemLv1";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetCJobNoItemLv2List 取得通俗職類第二層選項
        /// <summary>
        /// 查詢通俗職類第二層選項
        /// </summary>
        /// <param name="cjobType"></param>
        /// <returns></returns>
        public IList<KeyMapModel> GetCJobNoItemLv2List()
        {
            string funcName = "KeyMap.queryCJobNoItemLv2";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }
        #endregion

        #region GetCJobNoItemLv3List 取得通俗職類第三層選項
        /// <summary>
        /// 查詢通俗職類第三層選項
        /// </summary>
        /// <param name="cjobNo"></param>
        /// <returns></returns>
        public IList<KeyMapModel> GetCJobNoItemLv3List()
        {
            string funcName = "KeyMap.queryCJobNoItemLv3";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
        }

        #endregion
        //------------------------------------------------------------------

        #region GetFuncContent 依據功能選單代碼取得對應的功能說明資訊
        /// <summary>
        /// 取得網站特定功能項目說明
        /// </summary>
        /// <param name="funID"></param>
        /// <param name="funcItem"></param>
        /// <returns></returns>
        public string GetFuncContent(string funID, string funcItem = "")
        {
            string result = string.Empty;
            string funcName = "KeyMap.getTbContent";

            TblTB_CONTENT whereConds = new TblTB_CONTENT
            {
                FUNID = funID,
                C_FUNCITEM = funcItem
            };

            TblTB_CONTENT data = (TblTB_CONTENT)base.QueryForObject(funcName, whereConds);

            if (data == null) { return result; }

            result = data.C_CONTENT1;
            return result;
        }
        #endregion

        #region GetCode1Name 傳回Code1中文名稱（從 CODE1 資料表）
        /// <summary>
        /// 傳回CODE1中文名稱
        /// </summary>
        /// <param name="ITEM">類別</param>
        /// <param name="CODE">代碼</param>
        /// <returns></returns>
        public string GetCode1Name(string item, string code)
        {
            RowBaseDAO dao = new RowBaseDAO();
            if (string.IsNullOrEmpty(item)) { }
            if (string.IsNullOrEmpty(code)) { }

            TblCODE1 model = new TblCODE1();
            string result = string.Empty;

            model = dao.GetRow<TblCODE1>(new TblCODE1 { ITEM = item, CODE = code });

            if (model != null) { result = model.DESCR; }
            return result;
        }
        #endregion

        /// <summary>回傳電話與行動</summary>
        /// <param name="s_PHONE"></param>
        /// <param name="s_MOBILE"></param>
        /// <returns></returns>
        public string GetCPHONEMOBILE(string s_PHONE, string s_MOBILE)
        {
            s_PHONE = s_PHONE ?? "";
            s_MOBILE = s_MOBILE ?? "";
            string rst = "";
            if (s_PHONE.Length > 1 && s_MOBILE.Length > 1)
            {
                rst = string.Concat(s_PHONE, "、", s_MOBILE);
            }
            else if (s_PHONE.Length > 1)
            {
                rst = s_PHONE;
            }
            else if (s_MOBILE.Length > 1)
            {
                rst = s_MOBILE;
            }
            return rst;
        }

        public IList<KeyMapModel> GetPURPOSExList()
        {
            string funcName = "KeyMap.getKEY_PURPOSE";
            return base.QueryForListAll<KeyMapModel>(funcName, null);
            //throw new NotImplementedException();
        }

        public IList<KeyMapModel> GetUSAGEUNITxList()
        {
            string funcName = "KeyMap.getKEY_USAGEUNIT";
            return base.QueryForListAll<KeyMapModel>(funcName, null);

        }

        public string GET_DCASENO_20()
        {
            //Hashtable param = new Hashtable();
            //param["CTID"] = CTID;
            //string funcName = "KeyMap.getCityName";
            //return (string)base.QueryForObject(funcName, param);
            //return base.QueryForObject<KeyMapModel>(funcName, null);
            string funcName = "KeyMap.getDCASENO_20";
            return (string)base.QueryForObject(funcName, null);
        }


    }
}