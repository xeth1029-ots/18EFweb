using log4net;
using Omu.ValueInjecter;
using RSALibrary;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Web;
using System.Xml;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using Turbo.DataLayer;
using System.Net;
using System.Net.Security;
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;

namespace WDAIIP.WEB.DataLayers
{
    public class WDAIIPWEBDAO : BaseDAO
    {
        //protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 以預設的 'SqlMapTIMS.config' 起始 DAO 以存取 TIMS 資料庫
        /// </summary>
        public WDAIIPWEBDAO()
            : base("SqlMap.config")
        {
            //base.m_default_pagesize = 3;
        }

        /// <summary>
        /// 以指定的 SqlMap config 連接資料庫
        /// </summary>
        /// <param name="sqlMapConfig"></param>
        public WDAIIPWEBDAO(string sqlMapConfig)
            : base(sqlMapConfig)
        {

        }

        #region common
        /// <summary>
        /// 取得新產生的流水號 (模擬 Oracle SEQUENCE.NetVal ) SYS_AUTONUM
        /// <para>確保獨立的 transaction, 另外 new 新的 dao</para>
        /// </summary>
        /// <param name="tableSeq_Name_Pk">以逗號分隔的: TABLE_SEQ,TABLE_NAME,TABLE_PK</param>
        /// <returns></returns>
        public Int64? GetNewId(string tableSeq_Name_Pk)
        {
            if (string.IsNullOrEmpty(tableSeq_Name_Pk)) { throw new ArgumentException("TableSeq_Name_Pk 參數格式不合法: TABLE_SEQ,TABLE_NAME"); }

            string[] parmsAry = tableSeq_Name_Pk.Split(',');

            if (parmsAry.Length < 2) { throw new ArgumentException("TableSeq_Name_Pk 參數格式不合法: TABLE_SEQ,TABLE_NAME"); }

            return this.GetAutoNum(parmsAry).Value;
        }

        /// <summary> GET SYS_AUTONUM SEQ</summary>
        /// <param name="parmsAry"></param>
        /// <returns></returns>
        public Int64? GetAutoNum(string[] parmsAry)
        {
            Int64? curval = null;
            //if (parmsAry == null || parmsAry.Length < 2) { return curval; }
            //Int64? maxVal = null;
            string tableSeq = $"{parmsAry[0]}".Trim();
            //string tableName = Convert.ToString(parmsAry[1]).Trim();
            //string tablePK = Convert.ToString(parmsAry[2]);

            //tableSeq = tableSeq.Trim();
            //LOG.Debug("GetAutoNum: TableSeq=" + tableSeq);
            //SQLMapper transaction 似乎沒有 lock single entry 的功能
            object mutex = new object();
            lock (mutex)
            {
                bool isLocalSession = (!base.transactionOn); //未有啟動Transaction
                if (isLocalSession)
                {
                    base.BeginTransaction(System.Data.IsolationLevel.ReadCommitted); //啟動Transaction
                }
                try
                {
                    //20181212, 因為 效能考量 以及 確保唯一性, 不再自動抓目標 table 已存在的最大值 //一律以 SYS_AUTONUM 記錄的值為主, 系統上線前要自行確保起始值有效性
                    //先進行異動時間更新, 以觸發 Transaction Lock, 避免其他 thread 取到相同值
                    TblSYS_AUTONUM obj = new TblSYS_AUTONUM { MTIME = DateTime.Now };
                    TblSYS_AUTONUM where = new TblSYS_AUTONUM { TABLENAME = tableSeq };
                    int rows = base.Update<TblSYS_AUTONUM>(obj, where);

                    if (rows == 0)
                    {
                        //不存在, 新增 sys_autonum
                        curval = 1;
                        this.InsertSysAutoNum(tableSeq, curval.Value);
                    }
                    else
                    {
                        //已存在, 取現值
                        curval = this.GetTablePKCurVal(tableSeq);

                        //修改 SYS_AUTONUM (值加1, 更新並回傳)
                        if (curval.HasValue) curval += 1;

                        this.UpdateSysAutoNum(tableSeq, curval.Value);
                    }

                    if (isLocalSession) { base.CommitTransaction(); }
                    string s_CurValtxt = string.Format(", CurVal={0}", (curval.HasValue ? curval.Value : -1));
                    LOG.Debug(string.Format("#GetAutoNum: TableSeq={0}{1}", tableSeq, s_CurValtxt));
                }
                catch (Exception ex)
                {
                    if (isLocalSession) { base.RollBackTransaction(); }
                    if (String.Compare(tableSeq, "TB_VIEWRECORD_SEQNO_SEQ:", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        string s_CurValtxt = string.Format(", CurVal={0}", (curval.HasValue ? curval.Value : -1));
                        LOG.Error(string.Format("#GetAutoNum: TableSeq={0}{1}, Exception,Fail: {2}", tableSeq, s_CurValtxt, ex.Message));
                    }
                    throw new Exception(string.Concat("#GetAutoNum Exception,Faild: ", ex.Message), ex);
                }
            }
            return curval;
        }

        /// <summary> 課程查詢 for 產業人才投資2-適用留用外國中階技術工作人力 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<KeepUseClassSearchGrid1Model> QueryKeepUseClassSearch(KeepUseClassSearchFormModel form)
        {
            IList<KeepUseClassSearchGrid1Model> rtn = null;
            try
            {
                rtn = base.QueryForList<KeepUseClassSearchGrid1Model>("WDAIIPWEB.queryKeepUseClassSearch", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryKeepUseClassSearch() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;

        }

        /// <summary>
        /// 取得特定資料表PK欄位現行編到的號碼
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public Int64? GetTablePKCurVal(string tableName)
        {
            Int64? rtn = null;
            TblSYS_AUTONUM whereCond = new TblSYS_AUTONUM { TABLENAME = tableName };
            TblSYS_AUTONUM data = base.GetRow<TblSYS_AUTONUM>(whereCond);
            if (data == null) { return rtn; }
            //if (data != null) { rtn = data.CURVAL; }
            return data.CURVAL; //return rtn;
        }

        internal void UpdateElFormSignature(TblSTUD_ELFORM tmp)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 取得Table PK欄位最大值
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="tablePK">資料表PK欄位欄名</param>
        /// <returns></returns>
        public Int64? GetTableMaxVal(string tableName, string tablePK)
        {
            Int64? rtn = null;
            Int64 i_num = 0;

            Hashtable data = null;
            Hashtable parms = new Hashtable { { "TABLENAME", tableName }, { "TABLEPK", tablePK } };
            try
            {
                data = (Hashtable)base.QueryForObject("WDAIIPWEB.getTableMaxPK", parms);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("GetTableMaxVal():", ex.Message), ex);
                throw ex;
            }
            if (data == null) { return rtn; }
            if (!Int64.TryParse(Convert.ToString(data["MAXPK"]), out i_num)) { return rtn; }
            return i_num; //return rtn;
        }

        /// <summary> 新增 sys_autonum </summary>
        /// <param name="tableName"></param>
        /// <param name="curVal"></param>
        private void InsertSysAutoNum(string tableName, long curVal)
        {
            TblSYS_AUTONUM data = new TblSYS_AUTONUM
            {
                TABLENAME = tableName,
                CURVAL = curVal,
                MTIME = DateTime.Now
            };

            base.Insert(data);
        }

        /// <summary>
        /// 異動 sys_autonum
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="curVal"></param>
        private void UpdateSysAutoNum(string tableName, long curVal)
        {
            TblSYS_AUTONUM whereCond = new TblSYS_AUTONUM { TABLENAME = tableName };
            TblSYS_AUTONUM newAutoNum = new TblSYS_AUTONUM { CURVAL = curVal, MTIME = DateTime.Now };

            base.Update(newAutoNum, whereCond);
        }
        #endregion

        #region 首頁(Home/Index)

        /// <summary>
        /// 查詢系統公告資料
        /// </summary>
        /// <returns></returns>
        public NoticeDetailModel GetNotice()
        {
            NoticeDetailModel result = null;
            Hashtable param = new Hashtable();
            try
            {
                result = (NoticeDetailModel)base.QueryForObject("WDAIIPWEB.getNotice", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetNotice():" + ex.Message, ex);
                //throw ex;
            }
            return result;
        }

        /// <summary>
        /// Banner-首頁大banner
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public BannerGridModel GetBanner(TblTB_BANNER where)
        {
            BannerGridModel result = null;
            try
            {
                result = (BannerGridModel)base.QueryForObject("WDAIIPWEB.queryBanner", where);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetBanner():" + ex.Message, ex);
                //throw ex;
            }
            return result;
        }

        /// <summary>
        /// Banner-清單查詢
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IList<BannerGridModel> QueryBanner(TblTB_BANNER where)
        {
            IList<BannerGridModel> result = null;
            try
            {
                result = base.QueryForListAll<BannerGridModel>("WDAIIPWEB.queryBanner", where);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryBanner():" + ex.Message, ex);
                //throw ex;
            }
            return result;
        }

        /// <summary>
        /// 上稿資料查詢(宣導影片)
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public TopContentGridModel GetTopContent(TblTB_CONTENT where)
        {
            IList<TopContentGridModel> list = null;
            TopContentGridModel rtn = null;
            try
            {
                list = this.QueryTopContent(where);
                if (list != null && list.Count > 0) { rtn = list[0]; }
                return rtn;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO GetTopContent():", ex.Message), ex);
                //throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 焦點新聞/計畫公告-查詢
        /// </summary>
        /// <returns></returns>
        public IList<TopContentGridModel> QueryTopContent(TblTB_CONTENT where)
        {
            IList<TopContentGridModel> result = null;
            try
            {
                result = base.QueryForListAll<TopContentGridModel>("WDAIIPWEB.queryTopContent", where);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryTopContent():" + ex.Message, ex);
                //throw ex;
            }
            return result;
        }

        /// <summary>
        /// 最多分享課程-查詢
        /// </summary>
        /// <returns></returns>
        public IList<TopShareClassGridModel> QueryTopShareClass()
        {
            IList<TopShareClassGridModel> result = null;
            Hashtable param = new Hashtable();
            try
            {
                result = base.QueryForListAll<TopShareClassGridModel>("WDAIIPWEB.queryTopShareClass", param);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryTopShareClass():", ex.Message), ex);
            }
            return result;
        }

        /// <summary>
        /// 政策性課程專區-Policy course area-TOP 10 
        /// </summary>
        /// <returns></returns>
        public IList<PolicyClassGridModel> QueryPolicyClass()
        {
            IList<PolicyClassGridModel> result = null;
            string strYearN1 = DateTime.Now.Year.ToString(); //.ToString("yyyy"); //當年度
            //篩選開課日期：為 當年度 //param.Clear();//當年度
            Hashtable param = new Hashtable { { "STDATE1", strYearN1 + "/01/01" }, { "STDATE2", strYearN1 + "/12/31" } };
            try
            {
                result = base.QueryForListAll<PolicyClassGridModel>("WDAIIPWEB.queryPolicyClass", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO queryPolicyClass():" + ex.Message, ex);
                //throw ex;
            }
            return result;
        }

        /// <summary>歷史政策性課程-Historical policy course-TOP 10</summary>
        /// <returns></returns>
        public IList<PolicyClassGridModel> QueryPolicyClassHis()
        {
            IList<PolicyClassGridModel> result = null;
            Hashtable param = new Hashtable();
            try
            {
                result = base.QueryForListAll<PolicyClassGridModel>("WDAIIPWEB.queryPolicyClassHis", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryPolicyClassHis():" + ex.Message, ex);
                //throw ex;
            }
            return result;
        }

        #endregion

        #region "會員登入/登出作業"
        /// <summary>
        /// 就業通單一簽入，解析minfo xml
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public TWJobsMemberDataModel HandleXmlData(TWJobsMemberDataModel model, RSAEncryption RSA)
        {
            string strResult = string.Empty;
            Hashtable execParams = new Hashtable();
            string content = "";

            System.Xml.XmlDocument xmlDoc = xmlDoc = new System.Xml.XmlDocument();
            try
            {
                //logger.Debug("minfo=["+model.minfo+"]");
                content = HttpUtility.UrlDecode(model.minfo);
                LOG.Debug(string.Concat("minfo2=[", content, "]"));
                xmlDoc.LoadXml(content);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("HandleXmlData: parse content fail, ", ex.Message));
                throw new Exception(string.Concat("解析 Web Service 回傳XML失敗: ", ex.Message), ex);
            }

            if (Convert.ToString(xmlDoc) != "")
            {
                TWJobsMemberDataModel result = null;

                //XmlNode rootNode = xmlDoc.DocumentElement.SelectSingleNode("MEMBER_INFO");
                XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("/MEMBER_INFO");

                //取出xml結果值
                decimal dcNum = 0;
                if (nodes != null && nodes.Count > 0)
                {
                    result = new TWJobsMemberDataModel();
                    result.xmlDoc = xmlDoc;

                    foreach (XmlNode node in nodes)
                    {
                        result.RID = model.RID;
                        result.SID = getChildNodeText(RSA, node, "SID");

                        try
                        {
                            if (decimal.TryParse(getChildNodeText(RSA, node, "MEMBER_USER_ID"), out dcNum))
                            {
                                result.MEMBER_USER_ID = dcNum;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Warn(string.Concat("HandleXmlData: fail, ", ex.Message), ex);
                            result.MEMBER_USER_ID = 0;
                        }

                        result.ISFOREIGN = Convert.ToDecimal(getChildNodeText(RSA, node, "ISFOREIGN"));
                        result.ACID = getChildNodeText(RSA, node, "ACID");
                        result.NAME = getChildNodeText(RSA, node, "NAME");
                        result.BIRTHDAY = getChildNodeText(RSA, node, "BIRTHDAY");
                        result.SEX = getChildNodeText(RSA, node, "SEX");
                        result.EMAIL = getChildNodeText(RSA, node, "EMAIL");

                        try
                        {
                            if (decimal.TryParse(getChildNodeText(RSA, node, "ZIPCODE"), out dcNum))
                            {
                                result.ZIPCODE = dcNum;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Warn(string.Concat("HandleXmlData: fail, ", ex.Message), ex);
                            result.ZIPCODE = 0;
                        }

                        result.ADDR_CITY_1 = getChildNodeText(RSA, node, "ADDR_CITY_1");
                        result.ADDR_CITY_2 = getChildNodeText(RSA, node, "ADDR_CITY_2");
                        result.ADDR = getChildNodeText(RSA, node, "ADDR");
                        result.TEL1 = getChildNodeText(RSA, node, "TEL1");
                        result.TEL2 = getChildNodeText(RSA, node, "TEL2");
                        result.MOBILE = getChildNodeText(RSA, node, "MOBILE");
                        result.FAX = getChildNodeText(RSA, node, "FAX");
                        result.EDU = getChildNodeText(RSA, node, "EDU");
                        result.MARRI = getChildNodeText(RSA, node, "MARRI");
                        result.GRADU = getChildNodeText(RSA, node, "GRADU");
                        result.SCHOOLNAME = getChildNodeText(RSA, node, "SCHOOLNAME");
                        result.DEPTNAME = getChildNodeText(RSA, node, "DEPTNAME");

                        execParams["RID"] = result.RID;
                        execParams["SID"] = result.SID;
                        //execParams["ACID"] = result.ACID;

                        //查詢是否已有記錄過member
                        TWJobsMemberDataModel oMem = null;
                        try
                        {
                            oMem = (TWJobsMemberDataModel)base.QueryForObject("WDAIIPWEB.queryMember", execParams);
                        }
                        catch (Exception ex)
                        {
                            LOG.Warn(string.Concat("HandleXmlData: fail, ", ex.Message), ex);
                        }
                        if (oMem != null)
                        {
                            result.OCID = oMem.OCID;
                            result.PLANTYPE = oMem.PLANTYPE;
                        }
                        break;
                    }
                }

                return result;
            }
            else
            {
                Exception ex = new Exception("解析回傳XML失敗: Content為空");
                LOG.Error(string.Concat("HandleXmlData: content is Empty", ex.Message), ex);
                throw ex;
            }
        }


        /// <summary>取得並解密xml各節點內容</summary>
        /// <param name="pNode">根節點</param>
        /// <param name="xPath">子節點名稱</param>
        /// <returns></returns>
        public string getChildNodeText(RSAEncryption RSA, XmlNode pNode, string xPath)
        {
            string strRet = "";
            string strNodeVal = "";

            XmlNode node = pNode.SelectSingleNode(xPath);
            if (node != null)
            {
                //還原編碼後遺失的特殊符號
                strNodeVal = node.InnerText.Replace(" ", "+");

                strRet = RSA.DecryptData(strNodeVal);
            }

            return strRet;
        }

        /// <summary> 查詢就業通 SSO member 登入記錄</summary>
        /// <param name="rid">SSO登入ID</param>
        /// <param name="acid">身分證號</param>
        /// <param name="memberUserId">就業通會員ID</param>
        /// <returns></returns>
        public TWJobsMemberDataModel QueryMember(string rid, string sid, string acid = "", string memberUserId = "")
        {
            Hashtable parms = new Hashtable
            {
                ["RID"] = rid,
                ["SID"] = sid,
                ["ACID"] = acid,
                ["MID"] = memberUserId
            };
            return (TWJobsMemberDataModel)base.QueryForObject("WDAIIPWEB.queryMember", parms);
        }

        /// <summary>
        /// 處理記錄就業通單一簽入資料(新增/異動 member)
        /// </summary>
        /// <param name="model"></param>
        public void processMInfo(TWJobsMemberDataModel memInfo)
        {
            string s_FuncName = "#WDAIIPWEBDAO.processMInfo";
            //Hashtable execParams = new Hashtable();//Hashtable insParams = null;
            if (memInfo == null) { return; }

            //檢核是否已有記錄過member
            TWJobsMemberDataModel oMem = this.QueryMember(Convert.ToString(memInfo.RID), memInfo.SID);

            //member.gradu 是存1碼,但 e_member.mem_graduate 是存兩碼(.0)
            int i_GRADU = 0;
            if (!string.IsNullOrEmpty(memInfo.GRADU) && int.TryParse(memInfo.GRADU, out i_GRADU))
            {
                memInfo.GRADU = Convert.ToString(i_GRADU);
            }

            Hashtable insParams = MyCommonUtil.GetCopyModel(memInfo);
            try
            {
                //object mutex = new object(); //lock (mutex) {}
                if (oMem == null)
                {
                    object mutex = new object(); //lock (mutex) {}
                    lock (mutex)
                    {
                        //新增
                        LOG.Info("Insert Member");
                        base.Insert("WDAIIPWEB.insertMember", insParams);
                    }
                }
                else
                {
                    //保存舊值
                    if (!string.IsNullOrEmpty(oMem.ACID) && oMem.ACID.Length > 1)
                    {
                        object mutex = new object(); //lock (mutex) {}
                        lock (mutex)
                        {
                            //檢核是否已有記錄過member
                            Hashtable insParams2 = MyCommonUtil.GetCopyModel(oMem);
                            //新增-保留舊值
                            LOG.Info("Insert MemberHis");
                            try
                            {
                                base.Insert("WDAIIPWEB.insertMemberHis", insParams2);
                            }
                            catch (Exception ex)
                            {
                                LOG.Error(string.Concat(s_FuncName, " fail, ", ex.Message), ex);
                                bool flag = MyCommonUtil.MODIFYDATE2OLD(oMem.MODIFYDATE, DateTime.Now);
                                if (!flag) { throw ex; }

                                oMem.MODIFYDATE = DateTime.Now;
                                insParams2 = MyCommonUtil.GetCopyModel(oMem);
                                base.Insert("WDAIIPWEB.insertMemberHis", insParams2);
                            }
                        }
                    }

                    //修改
                    LOG.Info("Upate Member");
                    base.Update("WDAIIPWEB.updateMember", insParams);
                    SessionModel sm = SessionModel.Get();
                }

            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat(s_FuncName, " fail, ", ex.Message), ex);
                throw ex;
            }

        }

        /// <summary>
        /// 儲存記錄會員資料
        /// 將member相關欄位資訊回寫到e_member
        /// </summary>
        /// <param name="twMem"></param>
        public void saveToEMember(TWJobsMemberDataModel twMem)
        {
            LOG.Info("WDAIIPWEBDAO.saveToEMember.twMem...");

            Hashtable execParams = new Hashtable
            {
                ["MEM_IDNO"] = twMem.ACID,
                ["MEM_BIRTH"] = $"{twMem.BIRTHDAY}".Replace("-", "/")
            };

            SessionModel sm = SessionModel.Get();

            MemberDataModel mem = new MemberDataModel();

            base.BeginTransaction();
            try
            {
                //查詢e_member會員資料是否已存在 (依身份證號與生日)
                mem = (MemberDataModel)base.QueryForObject("WDAIIPWEB.getEMemberByKey", execParams);

                decimal newSN = 0;
                DateTime dDate;

                if (mem == null)
                {
                    //記錄會員資料 //取得最新的會員代碼
                    //newSN = (decimal)base.QueryForObject("WDAIIPWEB.getEMemberNewSN", null);
                    //newSN = new MyKeyMapDAO().GetTableMaxSeqNo(StaticCodeMap.TableName.E_MEMBER, "MEM_SN");
                    newSN = this.GetNewId("E_MEMBER_MEMSN_SEQ,E_MEMBER,MEM_SN").Value;
                    mem = new MemberDataModel();

                    mem.MEMSN = newSN;
                    mem.MEMIDNO = twMem.ACID;
                    mem.MEMPWD = twMem.ACID;  // 20181211: 白箱掃瞄，改為預設帶入身分證號
                    mem.MEMNAME = twMem.NAME;
                    mem.MEMFOREIGN = Convert.ToString(twMem.ISFOREIGN);
                    mem.MEMEDU = twMem.EDU;

                    if (DateTime.TryParseExact($"{twMem.BIRTHDAY}".Replace("-", "").Replace("/", ""),
                               "yyyyMMdd",
                               System.Globalization.CultureInfo.InvariantCulture,
                               System.Globalization.DateTimeStyles.None,
                               out dDate))
                    {
                        mem.MEMBIRTH = dDate;
                    }

                    mem.MEMSEX = twMem.SEX;
                    mem.MEMMILITARY = " ";
                    mem.MEMMARRY = (string.IsNullOrEmpty(twMem.MARRI) ? "2" : twMem.MARRI);//無值塞 2未婚
                    mem.MEMGRADUATE = (!string.IsNullOrEmpty(twMem.GRADU)) ? twMem.GRADU.PadLeft(2, '0') : "";
                    mem.MEMSCHOOL = (Convert.ToString(twMem.SCHOOLNAME) == "" ? " " : twMem.SCHOOLNAME); //學校名稱
                    mem.MEMDEPART = (Convert.ToString(twMem.DEPTNAME) == "" ? " " : twMem.DEPTNAME); //科系名稱
                    mem.MEMZIP = Convert.ToString(twMem.ZIPCODE);
                    mem.MEMADDR = twMem.ADDR;
                    mem.MEMTEL = (string.IsNullOrEmpty(twMem.TEL1) ? " " : twMem.TEL1);
                    mem.MEMTELN = twMem.TEL2;
                    mem.MEMMOBILE = twMem.MOBILE;
                    if (mem.MEMTEL.Length > 25) { mem.MEMTEL = mem.MEMTEL.Substring(0, 25); }
                    if (mem.MEMTELN.Length > 25) { mem.MEMTELN = mem.MEMTELN.Substring(0, 25); }
                    if (mem.MEMMOBILE.Length > 25) { mem.MEMMOBILE = mem.MEMMOBILE.Substring(0, 25); }
                    mem.MEMEMAIL = twMem.EMAIL;
                    mem.MEMUSRID = twMem.ACID;
                    mem.MEMOPUSER = twMem.ACID;
                    mem.MEMLOGINCNT = 1;

                    LOG.Info("insert e_member...");
                    //新增e網會員帳號資料(等有確定會員登入流程再行測試)
                    base.Insert("WDAIIPWEB.insertEMember", mem);
                }
                else
                {
                    //異動帳號資料
                    mem.MEMNAME = twMem.NAME;
                    mem.MEMFOREIGN = Convert.ToString(twMem.ISFOREIGN);
                    mem.MEMEDU = twMem.EDU;

                    if (DateTime.TryParseExact($"{twMem.BIRTHDAY}",
                               "yyyy-MM-dd",
                               System.Globalization.CultureInfo.InvariantCulture,
                               System.Globalization.DateTimeStyles.None,
                               out dDate))
                    {
                        mem.MEMBIRTH = dDate;
                    }

                    mem.MEMSEX = twMem.SEX;
                    mem.MEMMILITARY = " ";
                    //mem.MEMMARRY = twMem.MARRI;
                    mem.MEMMARRY = ("".Equals(Convert.ToString(twMem.MARRI).Trim()) ? "2" : twMem.MARRI);//無值塞 2未婚
                    mem.MEMGRADUATE = (!string.IsNullOrEmpty(twMem.GRADU)) ? twMem.GRADU.PadLeft(2, '0') : "";
                    mem.MEMSCHOOL = (Convert.ToString(twMem.SCHOOLNAME) == "" ? mem.MEMSCHOOL : twMem.SCHOOLNAME); //學校名稱
                    mem.MEMDEPART = (Convert.ToString(twMem.DEPTNAME) == "" ? mem.MEMDEPART : twMem.DEPTNAME); //科系名稱
                    mem.MEMZIP = Convert.ToString(twMem.ZIPCODE);
                    mem.MEMADDR = twMem.ADDR;
                    mem.MEMTEL = (string.IsNullOrEmpty(twMem.TEL1) ? " " : twMem.TEL1);
                    mem.MEMTELN = twMem.TEL2;
                    mem.MEMMOBILE = twMem.MOBILE;
                    if (mem.MEMTEL.Length > 25) { mem.MEMTEL = mem.MEMTEL.Substring(0, 25); }
                    if (mem.MEMTELN.Length > 25) { mem.MEMTELN = mem.MEMTELN.Substring(0, 25); }
                    if (mem.MEMMOBILE.Length > 25) { mem.MEMMOBILE = mem.MEMMOBILE.Substring(0, 25); }
                    mem.MEMEMAIL = twMem.EMAIL;
                    mem.MEMUSRID = twMem.ACID;
                    mem.MEMOPUSER = twMem.ACID;

                    LOG.Info("update e_member...");
                    //異動e網會員帳號資料
                    base.Update("WDAIIPWEB.updateEMember", mem);
                }

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("WDAIIPWEBDAO saveToEMember: " + ex.Message, ex);
                throw ex;
            }

            //記錄登入帳號資資訊
            sm.ACID = Convert.ToString(twMem.ACID);
            sm.UserID = Convert.ToString(twMem.ACID);
            sm.UserName = Convert.ToString(twMem.NAME);
            sm.Sex = Convert.ToString(twMem.SEX);
            if (twMem.MEMBER_USER_ID != 0) { sm.MEMBER_USER_ID = Convert.ToString(twMem.MEMBER_USER_ID); }
            sm.MemSN = mem.MEMSN; //序號

            if (!string.IsNullOrEmpty(Convert.ToString(mem.MEMBIRTH)))
            {
                sm.Birthday = Convert.ToDateTime(mem.MEMBIRTH).ToString("yyyy/MM/dd");
            }

            //2018-01-18 處理中文編碼轉置問題(&#xxxxx;)
            MyCommonUtil.HtmlDecode(sm);
        }

        /// <summary>
        /// 儲存會員資料 將相關欄位資訊回寫到 E_MEMBER
        /// </summary>
        /// <param name="eMem"></param>
        public void saveToEMember(TblE_MEMBER eMem)
        {
            LOG.Info("WDAIIPWEBDAO.saveToEMember.eMem...");

            MemberDataModel mem = new MemberDataModel
            {
                MEMSN = eMem.MEM_SN.Value,
                MEMIDNO = eMem.MEM_IDNO,
                MEMBIRTH = eMem.MEM_BIRTH.Value,
                MEMNAME = eMem.MEM_NAME,
                MEMFOREIGN = eMem.MEM_FOREIGN,
                MEMEDU = eMem.MEM_EDU,
                MEMSEX = eMem.MEM_SEX,
                MEMMILITARY = eMem.MEM_MILITARY,
                MEMMARRY = eMem.MEM_MARRY, /* 無值塞 2未婚 */
                MEMGRADUATE = eMem.MEM_GRADUATE,
                MEMSCHOOL = eMem.MEM_SCHOOL, /* 學校名稱 */
                MEMDEPART = eMem.MEM_DEPART, /* 科系名稱 */
                MEMZIP = eMem.MEM_ZIP,
                MEMZIP6W = eMem.MEM_ZIP6W,
                MEMADDR = eMem.MEM_ADDR,
                MEMTEL = eMem.MEM_TEL,
                MEMTELN = eMem.MEM_TELN,
                MEMMOBILE = eMem.MEM_MOBILE,
                MEMEMAIL = eMem.MEM_EMAIL,
                MEMOPUSER = eMem.MEM_IDNO
            };

            LOG.Info("update e_member...");
            //異動e網會員帳號資料
            base.Update("WDAIIPWEB.updateEMember", mem);
            //base.Update("WDAIIPWEB.updateEMember", eMem);
        }

        /// <summary>
        /// 隨機由 IDs 中取出 ID 以模擬會員登入
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int GetRandomNumber(int min, int max)
        {
            using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
            {
                byte[] rno = new byte[7];
                int randomvalue = -1;
                while (randomvalue < min || randomvalue > max)
                {
                    rg.GetBytes(rno);
                    randomvalue = BitConverter.ToInt32(rno, 0);
                }
                return randomvalue;
            }
        }

        /// <summary>
        /// 壓力測試模式下, 隨機由 E_MEMBER 中取得/產生 SessionModel,
        /// 以模擬使用者登入
        /// <para>會直接設定SessionModel</para>
        /// </summary>
        public bool StressTestRandomUser()
        {
            string[] IDs = null;
            string mID = null;

            string idFIle = ConfigModel.StressTestIDFile;
            if (!string.IsNullOrWhiteSpace(idFIle))
            {
                // 啟用 StressTest
                object obj = HttpContext.Current.Session["StressTestIDFile"];
                if (obj != null && obj is string[])
                {
                    IDs = (string[])obj;
                }

                if (IDs == null)
                {
                    // 由 StressTestIDFile 載入 ID 清單
                    string filePath = HttpContext.Current.Server.MapPath("~/") + "\\App_Data\\" + idFIle;
                    if (File.Exists(filePath))
                    {
                        IDs = File.ReadAllLines(filePath);
                        HttpContext.Current.Session["StressTestIDFile"] = IDs;
                    }
                    else
                    {
                        LOG.Warn("StressTestRandomUser: StressTestIDFile: " + filePath + " 檔案不存在");
                    }
                }

                // 隨機由 IDs 中取出 ID 以模擬會員登入
                if (IDs != null && IDs.Length > 0)
                {
                    int idx = GetRandomNumber(0, IDs.Length - 1);
                    mID = IDs[idx];
                }
            }
            if (string.IsNullOrEmpty(mID))
            {
                return false;
            }

            return SimulateUserSession(mID);
        }

        /// <summary>
        /// 模擬指定使用者登入狀態, 由 E_MEMBER 中取得資料並產生 SessionModel
        /// </summary>
        /// <param name="mID"></param>
        /// <returns></returns>
        public bool SimulateUserSession(string mID)
        {
            SessionModel sm = SessionModel.Get();
            sm.SID = ConfigModel.SSOSystemID;
            sm.RID = "";
            sm.ACID = "";
            sm.UserID = "";
            sm.UserName = "";
            sm.Birthday = "";
            sm.MemSN = 0;

            Hashtable parm = new Hashtable { { "IDNO", mID } };
            TWJobsMemberDataModel member = base.QueryForObject<TWJobsMemberDataModel>("WDAIIPWEB.getRandomMember", parm);

            if (member == null)
            {
                LOG.Warn("StressTestRandomUser: 找不到指定的 E_MEMBER 資料, ID=" + mID);
                return false;
            }
            else
            {
                // 模擬使用者登入: 記錄登入帳號資資訊
                sm.RID = Convert.ToString(member.RID);
                sm.ACID = Convert.ToString(member.ACID);
                sm.UserID = Convert.ToString(member.ACID);
                sm.UserName = Convert.ToString(member.NAME) + "(SIM)";
                sm.Sex = Convert.ToString(member.SEX);
                sm.MemSN = member.MEMBER_USER_ID;
                sm.Birthday = Convert.ToDateTime(member.BIRTHDAY).ToString("yyyy/MM/dd");

                return true;
            }
        }

        /// <summary>
        /// 查詢登入者(學員)是否存在自訓後第1天到訓後21天內未填寫學員意見調查表的資料
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<TblCLASS_CLASSINFO> QueryQuestionFacNoWrite(string idno)
        {
            IList<TblCLASS_CLASSINFO> result = null;
            string funcName = "WDAIIPWEB.queryQuestionFacNoWrite";
            TblSTUD_STUDENTINFO where = new TblSTUD_STUDENTINFO { IDNO = idno };
            try
            {
                result = base.QueryForListAll<TblCLASS_CLASSINFO>(funcName, where);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryQuestionFacNoWrite():", ex.Message), ex);
                throw ex;
            }
            return result;
        }
        #endregion

        #region 最新消息
        /// <summary>
        /// 查詢最新消息特定類型資料清單
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IList<ContentGridModel> QueryContent(TblTB_CONTENT where)
        {
            IList<ContentGridModel> result = null;
            try
            {
                result = base.QueryForList<ContentGridModel>("WDAIIPWEB.queryContent", where);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryContent():", ex.Message), ex);
                throw ex;
            }
            return result;
        }

        public static string Replace_url_1(string sec_content, string s_tag1, string s_linku)
        {
            if (s_linku != null && !string.IsNullOrEmpty(s_linku) && s_linku.Length > 1 && !string.IsNullOrEmpty(sec_content) && sec_content.Length > 1)
            {
                string s_url_usetxt = @"<a href=""{0}"" target=""_blank"" title=""另開新視窗"">{1}</a><br>";
                string s_url_linktxt = "連結網址「另開新視窗」";
                string s_linkurl2 = string.Format(s_url_usetxt, s_linku, s_url_linktxt);
                sec_content = sec_content.Replace(s_tag1, s_linkurl2);
            }
            return sec_content;
        }

        /// <summary>
        /// 查詢明細資料
        /// </summary>
        /// <param name="seqno"></param>
        /// <returns></returns>
        public ContentDetailModel GetContentDetail(TblTB_CONTENT where)
        {
            ContentDetailModel result = null;

            try
            {
                TblTB_CONTENT data = base.GetRow<TblTB_CONTENT>(where);

                if (data != null)
                {
                    result = new ContentDetailModel();
                    result.InjectFrom(data);

                    string s_url1 = "#[url1]";
                    string s_url2 = "#[url2]";
                    string s_url3 = "#[url3]";
                    string s_url4 = "#[url4]";
                    string s_url5 = "#[url5]";
                    //string s_url_usetxt = @"<a href=""{0}"" target=""_blank"" title=""另開新視窗"">{1}</a><br>";
                    //string s_url_linktxt = "連結網址「另開新視窗」";

                    // 查詢分段內容資料
                    result.SECTIONGrid = this.QueryContentSection(where);

                    foreach (TblTB_CONTENT_SECTION item in result.SECTIONGrid)
                    {
                        if (item.SEC_CONTENT != null) { item.SEC_CONTENT = MyHelperUtil.ChangeBreakLine(item.SEC_CONTENT); }

                        item.SEC_CONTENT = Replace_url_1(item.SEC_CONTENT, s_url1, data.C_LINKURL1);
                        item.SEC_CONTENT = Replace_url_1(item.SEC_CONTENT, s_url2, data.C_LINKURL2);
                        item.SEC_CONTENT = Replace_url_1(item.SEC_CONTENT, s_url3, data.C_LINKURL3);
                        item.SEC_CONTENT = Replace_url_1(item.SEC_CONTENT, s_url4, data.C_LINKURL4);
                        item.SEC_CONTENT = Replace_url_1(item.SEC_CONTENT, s_url5, data.C_LINKURL5);
                    }

                    // 查詢附件下載清單資料
                    TblTB_FILE fileParam = new TblTB_FILE
                    {
                        F_GROUPID = data.F_GROUPID,
                        F_FUNID = data.SUB_FUNID
                    };
                    result.FILEGrid = this.QueryFile(fileParam);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetContentDetail() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 查詢最新消息分段內容
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IList<TblTB_CONTENT_SECTION> QueryContentSection(TblTB_CONTENT where)
        {
            IList<TblTB_CONTENT_SECTION> result = null;
            try
            {
                result = base.QueryForListAll<TblTB_CONTENT_SECTION>("WDAIIPWEB.queryContentSection", where);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryContentSection():", ex.Message), ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢附件下載清單
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IList<FileGridModel> QueryFile(TblTB_FILE where)
        {
            IList<FileGridModel> result = null;
            try
            {
                result = base.QueryForListAll<FileGridModel>("WDAIIPWEB.queryFile", where);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryFile():", ex.Message), ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢特定單筆附件資料
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public TblTB_FILE GetFile(TblTB_FILE where)
        {
            return base.GetRow(where);
        }

        public void UpdateFileDLCount(TblTB_FILE uFile)
        {

            try
            {
                TblTB_FILE where = new TblTB_FILE { FILEID = uFile.FILEID };

                // 取得原附件資料
                TblTB_FILE oFile = base.GetRow(new TblTB_FILE { FILEID = where.FILEID });

                base.Update(uFile, oFile, where);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateFileDLCount failed: " + ex.Message);
                throw new Exception("UpdateFileDLCount failed: " + ex.Message, ex);
            }
        }
        #endregion

        #region News 最新消息/焦點新聞
        /// <summary>
        /// 焦點新聞-查詢所有上架中的最新消息資料
        /// </summary>
        /// <returns></returns>
        public IList<NewsGridModel> QueryNews()
        {
            IList<NewsGridModel> result = null;
            Hashtable param = new Hashtable();
            try
            {
                //base.PageSize = 999;
                result = base.QueryForList<NewsGridModel>("WDAIIPWEB.queryNews", param);
            }
            catch (Exception ex)
            {
                LOG.Error("NewsDAO QueryNews() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 焦點新聞-明細頁
        /// </summary>
        /// <param name="seqno"></param>
        /// <returns></returns>
        public NewsDetailModel GetNewsDetail(Int64 seqno)
        {
            NewsDetailModel result = null;

            try
            {
                TblTB_CONTENT news = base.GetRow<TblTB_CONTENT>(new TblTB_CONTENT { SEQNO = seqno });

                if (news != null)
                {
                    result = new NewsDetailModel();
                    result.InjectFrom(news);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetNewsDetail() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }
        #endregion

        #region 課後意見調查
        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<StudQuestionGridModel> QueryStudQuestion(StudQuestionFormModel form)
        {
            IList<StudQuestionGridModel> result = null;

            SessionModel sm = SessionModel.Get();
            if (form.IDNO != sm.ACID) { return result; }
            form.IDNO = sm.ACID ?? "";
            form.BIRTHDAY = sm.Birthday ?? "";

            string[] s_Include = { "1", "2", "3", "4" };
            if (string.IsNullOrEmpty(form.QueType)) { form.QueType = "1"; }
            bool flag_OK = false;
            foreach (var s_V1 in s_Include)
            {
                if (s_V1.Equals(form.QueType)) { flag_OK = true; break; }
            }
            if (!flag_OK) { form.QueType = "1"; }
            Hashtable param = new Hashtable
            {
                ["IDNO"] = form.IDNO,
                ["BIRTHDAY"] = form.BIRTHDAY,
                ["QueType"] = form.QueType
            };

            try
            {
                //base.PageSize = 2; //test
                result = base.QueryForList<StudQuestionGridModel>("WDAIIPWEB.queryStdQuestionByQueType", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetStudQuestion() error:" + ex.Message, ex);
                //throw ex;
            }

            return result;
        }

        /// <summary>檢核是否確實有此學生資料</summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        public bool chkStudent(Int64 ocid, Int64 socid)
        {
            bool result = false;

            TblCLASS_STUDENTSOFCLASS chk = base.GetRow<TblCLASS_STUDENTSOFCLASS>(new TblCLASS_STUDENTSOFCLASS { OCID = ocid, SOCID = socid });
            result = (chk != null) ? true : false;

            return result;
        }

        /// <summary> 檢核是否確實有此學生資料 </summary>
        /// <param name="ocid"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public bool chkStudent1(Int64 ocid, string sid)
        {
            bool result = false;

            TblCLASS_STUDENTSOFCLASS chk = base.GetRow<TblCLASS_STUDENTSOFCLASS>(new TblCLASS_STUDENTSOFCLASS { OCID = ocid, SID = sid });
            result = (chk != null) ? true : false;

            return result;
        }

        public TblSTUD_STUDENTINFO QueryStudentinfo(TblSTUD_STUDENTINFO where)
        {
            TblSTUD_STUDENTINFO info = base.GetRow<TblSTUD_STUDENTINFO>(where);
            return info;
        }

        #region 處理參訓學員意見調查表(FAC)
        /// <summary>
        /// 查詢參訓學員意見調查表經費資訊(2017年以前版本)
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        public FacDetailModel getFacCost(Int64 ocid, Int64 socid)
        {
            FacDetailModel result = new FacDetailModel();
            Hashtable param = new Hashtable
            {
                ["OCID"] = ocid,
                ["SOCID"] = socid
            };

            try
            {
                result = (FacDetailModel)base.QueryForObject("WDAIIPWEB.queryFacCost", param);
            }
            catch (Exception ex)
            {
                LOG.Error("StudQuestionDAO getFacCost() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢取得參訓學員意見調查表資訊（2017年前版本）
        /// </summary>
        /// <param name="socid"></param>
        /// <returns></returns>
        public FacDetailModel getFac(decimal? socid)
        {
            FacDetailModel result = null;

            try
            {
                TblSTUD_QUESTIONFAC2 fac = base.GetRow<TblSTUD_QUESTIONFAC2>(new TblSTUD_QUESTIONFAC2 { SOCID = socid });

                if (fac != null)
                {
                    result = new FacDetailModel();
                    result.InjectFrom(fac);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getFac() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 儲存調查表填寫結果
        /// 有異常時直接丟出 Exception
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void SaveFacData(string strXmlName, FacDetailModel model)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            try
            {
                this.FacStoreModel(strXmlName, model);
            }
            catch (Exception ex)
            {
                LOG.Error("#SaveFacData: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 儲存意見調查表
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void FacStoreModel(string strXmlName, FacDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            TblSTUD_QUESTIONFAC2 newFac = new TblSTUD_QUESTIONFAC2();
            TblSTUD_QUESTIONFAC2 whereConds = null;

            try
            {
                //有勾其它才記錄其它說明
                model.S16_NOTE = (model.S16_CHECKED ? model.S16_NOTE : null);
                model.A1_10_NOTE = (model.A1_10_CHECKED ? model.A1_10_NOTE : null);
                model.A2_7_NOTE = (model.A2.HasValue && model.A2 == 7 ? model.A2_7_NOTE : null);
                model.A3_5_NOTE = (model.A3.HasValue && model.A3 == 5 ? model.A3_5_NOTE : null);

                switch (model.DB_ACTION)
                {
                    case "CREATE": //新增
                        newFac.InjectFrom(model);

                        newFac.DASOURCE = 1; //資料來源
                        newFac.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newFac.MODIFYDATE = DateTime.Now;
                        base.Insert<TblSTUD_QUESTIONFAC2>(newFac);

                        break;
                    case "UPDATE": //修改
                        whereConds = new TblSTUD_QUESTIONFAC2 { SOCID = model.SOCID };
                        newFac.InjectFrom(model);
                        newFac.DASOURCE = 1;
                        newFac.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newFac.MODIFYDATE = DateTime.Now;

                        ClearFieldMap clearFieldMap = new ClearFieldMap();
                        clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.C21_NOTE);
                        if (!model.S16_CHECKED)
                            clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.S16_NOTE);
                        if (!model.A1_10_CHECKED)
                            clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.A1_10_NOTE);
                        if (model.A2 != 7)
                            clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.A2_7_NOTE);
                        if (model.A3 != 5)
                            clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.A3_5_NOTE);

                        base.Update<TblSTUD_QUESTIONFAC2>(newFac, whereConds, clearFieldMap);

                        break;
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO.FacStoreModel() action:" + model.DB_ACTION + " exception !", ex);
                throw new Exception("FacStoreModel: " + ex.Message, ex);
            }
        }
        #endregion

        #region 處理參訓學員訓後動態調查表(FIN)
        /// <summary>
        /// 查詢訓後動態調查表計畫資訊 queryFinPlan
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        public FinDetailModel getFinPlan(Int64 ocid, Int64 socid)
        {
            FinDetailModel result = null;
            Hashtable param = new Hashtable
            {
                ["OCID"] = ocid,
                ["SOCID"] = socid
            };

            try
            {
                result = (FinDetailModel)base.QueryForObject("WDAIIPWEB.queryFinPlan", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getFinPlan() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢訓後動態調查表填寫資訊
        /// </summary>
        /// <param name="socid"></param>
        /// <returns></returns>
        public FinDetailModel getFin(decimal socid)
        {
            FinDetailModel result = null;

            try
            {
                TblSTUD_QUESTIONFIN fin = base.GetRow<TblSTUD_QUESTIONFIN>(new TblSTUD_QUESTIONFIN { SOCID = socid });

                if (fin != null)
                {
                    result = new FinDetailModel();
                    result.InjectFrom(fin);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getFin() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 儲存訓後動態填寫結果
        /// 有異常時直接丟出 Exception
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void SaveFinData(string strXmlName, FinDetailModel model)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            try
            {
                this.FinStoreModel(strXmlName, model);
            }
            catch (Exception ex)
            {
                LOG.Error("SaveFinData: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>儲存訓後動態調查表</summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void FinStoreModel(string strXmlName, FinDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            TblSTUD_QUESTIONFIN newFin = new TblSTUD_QUESTIONFIN();
            TblSTUD_QUESTIONFIN whereConds = null;

            try
            {
                if (model.Q1 != 4)
                {
                    model.Q1A = null;
                    model.Q1B = null;
                    model.Q1C = null;
                }

                switch (model.DB_ACTION)
                {
                    case "CREATE": //新增
                        newFin.InjectFrom(model);

                        newFin.DASOURCE = 1;
                        newFin.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newFin.MODIFYDATE = DateTime.Now;
                        base.Insert<TblSTUD_QUESTIONFIN>(newFin);
                        break;

                    case "UPDATE": //修改
                        whereConds = new TblSTUD_QUESTIONFIN { SOCID = model.SOCID };
                        newFin.InjectFrom(model);

                        newFin.DASOURCE = 1;
                        newFin.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newFin.MODIFYDATE = DateTime.Now;

                        ClearFieldMap clearFieldMap = new ClearFieldMap();
                        clearFieldMap.Add((TblSTUD_QUESTIONFIN m) => m.Q3_NOTE);
                        if (newFin.Q1 != 4)
                        {
                            clearFieldMap.Add((TblSTUD_QUESTIONFIN m) => m.Q1A);
                            clearFieldMap.Add((TblSTUD_QUESTIONFIN m) => m.Q1B);
                            clearFieldMap.Add((TblSTUD_QUESTIONFIN m) => m.Q1C);
                        }

                        base.Update<TblSTUD_QUESTIONFIN>(newFin, whereConds, clearFieldMap);
                        break;
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO.FinStoreModel() action:" + model.DB_ACTION + " exception !", ex);
                throw new Exception("FinStoreModel: " + ex.Message, ex);
            }
        }
        #endregion

        #region 受訓期間意見調查表(Train)

        /// <summary>
        /// 受訓期間意見調查表 queryTrainPlan
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        public TrainDetailModel getTrainPlan(long ocid, long socid)
        {
            TrainDetailModel result = null;
            Hashtable param = new Hashtable
            {
                ["OCID"] = ocid,
                ["SOCID"] = socid
            };

            try
            {
                result = (TrainDetailModel)base.QueryForObject("WDAIIPWEB.queryTrainPlan", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getTrainPlan() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢 受訓期間意見調查表 填寫資訊
        /// </summary>
        /// <param name="socid"></param>
        /// <returns></returns>
        public TrainDetailModel getTrain(long socid)
        {
            TrainDetailModel result = null;

            try
            {
                TblSTUD_QUESTRAINING train = base.GetRow<TblSTUD_QUESTRAINING>(new TblSTUD_QUESTRAINING { SOCID = socid });

                if (train == null) { return result; }

                result = new TrainDetailModel();
                result.InjectFrom(train);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getTrain() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 儲存 受訓期間意見調查表
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void TrainStoreModel(string strXmlName, TrainDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            TblSTUD_QUESTRAINING newData = new TblSTUD_QUESTRAINING();
            TblSTUD_QUESTRAINING whereConds = null;

            try
            {
                switch (model.DB_ACTION)
                {
                    case "CREATE": //新增
                        newData.InjectFrom(model);
                        newData.DASOURCE = 1;
                        newData.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newData.MODIFYDATE = DateTime.Now;
                        base.Insert<TblSTUD_QUESTRAINING>(newData);
                        break;
                    case "UPDATE": //修改
                        whereConds = new TblSTUD_QUESTRAINING { SOCID = model.SOCID };
                        newData.InjectFrom(model);
                        newData.DASOURCE = 1;
                        newData.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newData.MODIFYDATE = DateTime.Now;
                        //ClearFieldMap clearFieldMap = new ClearFieldMap();
                        //clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.C21_NOTE);
                        base.Update<TblSTUD_QUESTRAINING>(newData, whereConds);
                        break;
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("WDAIIPWEBDAO.TrainStoreModel() action:{0} exception !", model.DB_ACTION), ex);
                throw new Exception("TrainStoreModel: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 儲存 受訓期間意見調查表 有異常時直接丟出 Exception
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void SaveTrainData(string strXmlName, TrainDetailModel model)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            if (model.SUGGESTION.Length > 700)
            {
                ArgumentException ex = new ArgumentException("SaveTrainData: 其他意見-超過700字");
                LOG.Warn("SaveTrainData: " + ex.Message, ex);
                throw ex;
            }
            try
            {
                this.TrainStoreModel(strXmlName, model);
            }
            catch (Exception ex)
            {
                LOG.Error("SaveTrainData: " + ex.Message, ex);
                throw ex;
            }
        }

        #endregion

        #region 期末學員滿意度調查表(Tion)

        /// <summary>
        /// 查詢 期末學員滿意度調查表 計畫資訊 queryTionPlan
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="socid"></param>
        /// <returns></returns>
        public TionDetailModel getTionPlan(long ocid, long socid)
        {
            TionDetailModel result = null;
            Hashtable param = new Hashtable { ["OCID"] = ocid, ["SOCID"] = socid };

            try
            {
                result = (TionDetailModel)base.QueryForObject("WDAIIPWEB.queryTionPlan", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getTionPlan() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢 期末學員滿意度調查表 填寫資訊
        /// </summary>
        /// <param name="socid"></param>
        /// <returns></returns>
        public TionDetailModel getTion(long ocid, string studid)
        {
            TionDetailModel result = null;

            try
            {
                TblSTUD_QUESTIONARY tion = base.GetRow<TblSTUD_QUESTIONARY>(new TblSTUD_QUESTIONARY { OCID = ocid, STUDID = studid });

                if (tion == null) { return result; }

                result = new TionDetailModel();

                result.InjectFrom(tion);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO getTion() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary> 
        /// 儲存 期末學員滿意度調查表
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void TionStoreModel(string strXmlName, TionDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            TblSTUD_QUESTIONARY newData = new TblSTUD_QUESTIONARY();
            TblSTUD_QUESTIONARY whereConds = null;

            try
            {
                switch (model.DB_ACTION)
                {
                    case "CREATE": //新增
                        newData.InjectFrom(model);
                        //newData.DASOURCE = 1;
                        newData.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newData.MODIFYDATE = DateTime.Now;
                        base.Insert<TblSTUD_QUESTIONARY>(newData);
                        break;
                    case "UPDATE": //修改
                        whereConds = new TblSTUD_QUESTIONARY { OCID = model.OCID, STUDID = model.STUDID };
                        newData.InjectFrom(model);
                        //newData.DASOURCE = 1;
                        newData.MODIFYACCT = sm.ACID; //SessionModel.ACID (登入者帳號)
                        newData.MODIFYDATE = DateTime.Now;
                        //ClearFieldMap clearFieldMap = new ClearFieldMap();
                        //clearFieldMap.Add((TblSTUD_QUESTIONFAC2 m) => m.C21_NOTE);
                        base.Update<TblSTUD_QUESTIONARY>(newData, whereConds);
                        break;
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("WDAIIPWEBDAO.TionStoreModel() action:{0} exception !", model.DB_ACTION), ex);
                throw new Exception("TionStoreModel: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 儲存 期末學員滿意度調查表 有異常時直接丟出 Exception
        /// </summary>
        /// <param name="strXmlName"></param>
        /// <param name="model"></param>
        public void SaveTionData(string strXmlName, TionDetailModel model)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            try
            {
                this.TionStoreModel(strXmlName, model);
            }
            catch (Exception ex)
            {
                LOG.Error("SaveTionData: " + ex.Message, ex);
                throw ex;
            }
        }

        #endregion

        #endregion

        #region 課程查詢報名/線上報名
        /// <summary>
        /// 依輸入課程代碼查詢課程相關資訊
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public ClassClassInfoExtModel GetOCIDDateByOCID(Int64 ocid)
        {
            string funcName = "WDAIIPWEB.getOCIDDateByOCID";
            TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = ocid };
            return (ClassClassInfoExtModel)base.QueryForObject(funcName, where);
        }

        /// <summary>
        /// 依輸入的課程代碼傳回該班的報名人數 (扣除e網審核失敗)
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public int GetEnterCount(Int64 ocid)
        {
            string funcName = "WDAIIPWEB.getEnterCountByOCID";
            TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = ocid };

            return (int)base.QueryForObject(funcName, where);
        }
        #endregion

        #region 課程查詢報名/課程查詢
        /// <summary> 課程查詢 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<T> QueryClassSearch<T>(ClassSearchFormModel form)
        {
            IList<T> results = new List<T>();

            //取得目前(DB)系統時間
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            DateTime nowTime = keyDao.GetSysDateNow();
            form.STDATE = string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_STDATE_ok = MyCommonUtil.IsDate(form.STDATE);
            form.FTDATE = string.Concat(form.FTDATE_YEAR, "/", (form.FTDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_FTDATE_ok = MyCommonUtil.IsDate(form.FTDATE);
            form.STDATE = flag_STDATE_ok ? string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01 00:00:00") : null;
            int iDayInM = flag_FTDATE_ok ? DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)) : -1;
            form.FTDATE = flag_FTDATE_ok ? string.Concat(form.FTDATE_YEAR, "/", (form.FTDATE_MON ?? "").PadLeft(2, '0'), "/", iDayInM.ToString().PadLeft(2, '0'), " 23:59:59") : null;
            if (!string.IsNullOrEmpty(form.CLASSCNAME)) form.CLASSCNAME = MyCommonUtil.RegReplaceCLSNM(form.CLASSCNAME);
            if (!string.IsNullOrEmpty(form.KEYWORDS)) form.KEYWORDS = MyCommonUtil.RegReplaceCLSNM(form.KEYWORDS);

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            switch (form.PlanType)
            {
                case "1": //產投
                    switch (form.CASETYPE)
                    {
                        case "0":
                            if (!flag_STDATE_ok) { form.STDATE = null; }
                            if (!flag_FTDATE_ok) { form.FTDATE = null; }
                            if (string.IsNullOrEmpty(form.STDATE) || string.IsNullOrEmpty(form.FTDATE))
                            {
                                form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                                form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            break;
                        case "1":
                            form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                            form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                            break;
                        case "2": //2019-02-14 ADD 今日開放報名
                            form.STDATE = null;
                            form.FTDATE = null;
                            break;
                        default:
                            //產業人才投資方案，快速搜尋課程區間預設為最近3個月之開訓課程，查詢後欲看更多課程，請點選「修改查詢條件」按鍵。
                            form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                            form.FTDATE = nowTime.AddMonths(3).ToString("yyyy/MM/dd HH:mm:ss");
                            break;
                    }

                    //轉換成大寫
                    if (!string.IsNullOrEmpty(form.CLASSCNAME))
                        form.CLASSCNAME = form.CLASSCNAME.ToUpper();
                    if (!string.IsNullOrEmpty(form.SCHOOLNAME))
                        form.SCHOOLNAME = form.SCHOOLNAME.ToUpper();

                    //組合-上課時段
                    form.TPERIOD28S_Sql = "";
                    for (int i = 0; i < form.TPERIOD28S_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.TPERIOD28S_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.TPERIOD28S_Sql += $"{(string.IsNullOrEmpty(form.TPERIOD28S_Sql) ? " AND (" : "")}";
                            form.TPERIOD28S_Sql += $"{(i > 0 ? " OR " : "")}TP.TPERIOD28_{form.TPERIOD28S_SHOW[i]}='Y'";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.TPERIOD28S_Sql)) { form.TPERIOD28S_Sql += ") "; }

                    //組合星期條件
                    form.WEEKS_Sql = "";
                    for (int i = 0; i < form.WEEKS_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.WEEKS_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.WEEKS_Sql += $"{(string.IsNullOrEmpty(form.WEEKS_Sql) ? " AND (" : "")}";
                            form.WEEKS_Sql += $"{(i > 0 ? " OR " : "")}OC.WEEKS{form.WEEKS_SHOW[i]}=1";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.WEEKS_Sql)) { form.WEEKS_Sql += ") "; }

                    if (form.CTID_SHOW != null && (form.CTID_SHOW.Length > 0 || !string.IsNullOrEmpty(form.CTID_KEY)))
                    {
                        form.CTID = null;//若有複選資料，單選資料則為空
                        form.ZIPCODE = null;//若有複選資料，單選資料則為空
                    }

                    SessionModel sess = SessionModel.Get();
                    if (sess.IsLogin)
                    {
                        form.MemSN = sess.MemSN;
                        form.MEM_ACID = sess.ACID;
                    }
                    //包含已截止報名課程(N:不包含)
                    form.IsContainsOverEnter = "N";
                    results = QueryForList<T>("WDAIIPWEB.queryClassSearch_1", form);

                    form.TPERIOD28S_Sql = "";//清空記錄
                    form.WEEKS_Sql = ""; //要清空記錄 where sql 參數值，此變數會影響分頁功能無效（出現ajax run error 錯誤問題）
                    break;

                case "2": //自辦在職
                    if (!flag_STDATE_ok) { form.STDATE = null; }
                    if (!flag_FTDATE_ok) { form.FTDATE = null; }
                    if (string.IsNullOrEmpty(form.STDATE) || string.IsNullOrEmpty(form.FTDATE))
                    {
                        form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                        form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                    }

                    //組合星期條件 //依數字轉為星期幾 0-6 日-六
                    form.WEEKS_Sql = "";
                    for (int i = 0; i < form.WEEKS_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.WEEKS_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.WEEKS_Sql += $"{(string.IsNullOrEmpty(form.WEEKS_Sql) ? " AND (" : "")}";
                            form.WEEKS_Sql += $"{(i > 0 ? " OR " : "")}OC.WEEKS{form.WEEKS_SHOW[i]}=1";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.WEEKS_Sql)) { form.WEEKS_Sql += ") "; }

                    //包含已截止報名課程(N:不包含)
                    form.IsContainsOverEnter = "N";
                    results = base.QueryForList<T>("WDAIIPWEB.queryClassSearch_2", form);
                    break;

                case "5": //區域據點(tplanid=70)
                    //string s_IsTestT701 = MyCommonUtil.Utl_GetConfigSet("IsTestT701");
                    //if (!string.IsNullOrEmpty(s_IsTestT701)) form.IsTestT701 = s_IsTestT701;
                    if (!flag_STDATE_ok) { form.STDATE = null; }
                    if (!flag_FTDATE_ok) { form.FTDATE = null; }
                    if (string.IsNullOrEmpty(form.STDATE) || string.IsNullOrEmpty(form.FTDATE))
                    {
                        form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                        form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    //組合星期條件 //依數字轉為星期幾 0-6 日-六
                    form.WEEKS_Sql = "";
                    for (int i = 0; i < form.WEEKS_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.WEEKS_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.WEEKS_Sql += $"{(string.IsNullOrEmpty(form.WEEKS_Sql) ? " AND (" : "")}";
                            form.WEEKS_Sql += $"{(i > 0 ? " OR " : "")}OC.WEEKS{form.WEEKS_SHOW[i]}=1";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.WEEKS_Sql)) { form.WEEKS_Sql += ") "; }

                    //包含已截止報名課程(N:不包含)
                    form.IsContainsOverEnter = "N";
                    results = base.QueryForList<T>("WDAIIPWEB.queryClassSearch_5", form);
                    break;
            }

            return results;
        }

        /// <summary>
        /// 取得課程明細
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plantype"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T GetClassSearchDetail<T>(string plantype, TblCLASS_CLASSINFO param)
        {
            object result;
            string funName = "";
            switch (plantype)
            {
                case "1"://產投
                    funName = "WDAIIPWEB.getClassSearchDetail_1";
                    break;
                case "2"://自辦在職
                    funName = "WDAIIPWEB.getClassSearchDetail_2";
                    break;
                case "3"://充飛
                    funName = "WDAIIPWEB.getClassSearchDetail_3";
                    break;
                case "4"://接受企業委託
                    funName = "WDAIIPWEB.getClassSearchDetail_4";
                    break;
                case "5"://區域據點
                    funName = "WDAIIPWEB.getClassSearchDetail_5";
                    break;
            }

            Hashtable hash = new Hashtable { ["OCID"] = param.OCID };

            SessionModel sess = SessionModel.Get();

            //SessionModel.Get().MemSN;
            if (sess != null && sess.IsLogin) { hash["MemSN"] = sess.MemSN; }

            result = base.QueryForObject<T>(funName, hash);

            return (T)result;
        }

        /// <summary>
        /// 取得課程訓練內容
        /// </summary>
        /// <param name="plantype">計畫類別</param>
        /// <param name="planid">計畫代碼</param>
        /// <param name="comidno"></param>
        /// <param name="seqno"></param>
        /// <returns></returns>
        public IList<TblPLAN_TRAINDESC> GetClassSearchTrain(string plantype, Int64? planid, string comidno, Int64? seqno)
        {
            IList<TblPLAN_TRAINDESC> result = new List<TblPLAN_TRAINDESC>();
            TblPLAN_TRAINDESC param = new TblPLAN_TRAINDESC
            {
                PLANID = planid,
                COMIDNO = comidno,
                SEQNO = seqno
            };

            switch (plantype)
            {
                case "1"://產投
                    result = base.QueryForListAll<TblPLAN_TRAINDESC>("WDAIIPWEB.getClassSearchTrain_1", param);
                    break;
                case "2"://自辦在職
                    result = base.QueryForListAll<TblPLAN_TRAINDESC>("WDAIIPWEB.getClassSearchTrain_2", param);
                    break;
                case "5"://區域據點
                    result = base.QueryForListAll<TblPLAN_TRAINDESC>("WDAIIPWEB.getClassSearchTrain_5", param);
                    break;
            }

            return result;
        }

        /// <summary>
        /// 取得課程訓練內容
        /// </summary>
        /// <param name="plantype">計畫類別</param>
        /// <param name="planid"></param>
        /// <param name="comidno"></param>
        /// <param name="seqno"></param>
        /// <returns></returns>
        public IList<ClassSearchTranDetail2Model> GetClassSearchTrain_v2(string plantype, Int64? planid, string comidno, Int64? seqno)
        {
            IList<ClassSearchTranDetail2Model> result = new List<ClassSearchTranDetail2Model>();
            TblPLAN_TRAINDESC param = new TblPLAN_TRAINDESC
            {
                PLANID = planid,
                COMIDNO = comidno,
                SEQNO = seqno
            };

            //switch (plantype) {} 1:產投 2:自辦在職 5:區域據點
            result = base.QueryForListAll<ClassSearchTranDetail2Model>("WDAIIPWEB.getClassSearchTrain_1_v2", param);

            return result;
        }

        /// <summary>
        /// 取得師資
        /// </summary>
        /// <param name="OCID"></param>
        /// <returns></returns>
        public IList<TblTEACH_TEACHERINFO> GetClassSearchTeacher(Int64? OCID)
        {
            IList<TblTEACH_TEACHERINFO> result = null;
            Hashtable param = new Hashtable { ["OCID"] = OCID };
            try
            {
                result = base.QueryForListAll<TblTEACH_TEACHERINFO>("WDAIIPWEB.getClassSearchTeacher", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetClassSearchTeacher() error:" + ex.Message, ex);
                throw ex;

            }
            return result;
        }

        /// <summary>
        /// 取得是否暫停報名訊息
        /// </summary>
        /// <returns></returns>
        public string StopEnterTempMsg()
        {
            string result = "";
            MyKeyMapDAO KMdao = new MyKeyMapDAO();
            string sAltMsg = KMdao.GetSystemMsg("AltMsg", "SE");
            string AltMsgSDate = KMdao.GetSystemMsg("AltMsgSDate", "SE");
            string AltMsgEDate = KMdao.GetSystemMsg("AltMsgEDate", "SE");
            if (GetAltMsg(AltMsgSDate, AltMsgSDate)) { result = sAltMsg; }
            //如果不為空返回
            if (!string.IsNullOrEmpty(result)) { return result; }

            IList<Hashtable> homenews3 = new List<Hashtable>();
            homenews3 = base.QueryForListAll<Hashtable>("WDAIIPWEB.getHomeNews3", null);
            if (homenews3 == null) { return result; }

            foreach (var row in homenews3)
            {
                sAltMsg = Convert.ToString(row["SUBJECT"]);
                AltMsgSDate = Convert.ToString(row["STOPSDATE"]);
                AltMsgEDate = Convert.ToString(row["STOPEDATE"]);
                if (GetAltMsg(AltMsgSDate, AltMsgEDate)) { result = sAltMsg; }
                //如果不為空返回
                if (!string.IsNullOrEmpty(result)) { return result; }
            }
            return result;
        }

        public string StopEnterTempMsg4()
        {
            string result = "";
            IList<Hashtable> homenews4 = new List<Hashtable>();
            homenews4 = base.QueryForListAll<Hashtable>("WDAIIPWEB.getHomeNews4", null);
            if (homenews4 == null) { return result; }

            foreach (var row in homenews4)
            {
                string sAltMsg = Convert.ToString(row["SUBJECT"]);
                string AltMsgSDate = Convert.ToString(row["STOPSDATE"]);
                string AltMsgEDate = Convert.ToString(row["STOPEDATE"]);
                if (GetAltMsg(AltMsgSDate, AltMsgEDate)) { result = sAltMsg; }
                //如果不為空返回
                if (!string.IsNullOrEmpty(result)) { return result; }
            }
            return result;
        }

        /// <summary>
        /// 判斷現時刻是否顯示提示訊息
        /// </summary>
        /// <param name="altmsg">訊息</param>
        /// <param name="sdate">起始時間</param>
        /// <param name="edate">迄止時間</param>
        /// <returns></returns>
        public bool GetAltMsg(string sdate, string edate)
        {
            bool isShow = false;

            DateTime SDate;
            DateTime EDate;
            //取得目前(DB)系統時間
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            DateTime nowTime = keyDao.GetSysDateNow();
            //form.STDATE = string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_sdate_ok = MyCommonUtil.IsDate(sdate);
            bool flag_edate_ok = MyCommonUtil.IsDate(edate);
            if (!flag_sdate_ok) { sdate = null; }
            if (!flag_edate_ok) { edate = null; }
            if (!string.IsNullOrEmpty(sdate) && !string.IsNullOrEmpty(edate))
            {
                SDate = Convert.ToDateTime(sdate);
                EDate = Convert.ToDateTime(edate);
                if (DateTime.Compare(nowTime, SDate) >= 0 && DateTime.Compare(nowTime, EDate) <= 0)
                    isShow = true;
            }

            return isShow;
        }

        /// <summary> 2019-01-31 問題9:add 檢核是否已加入課程收藏清單過 </summary>
        /// <param name="plantype"></param>
        /// <param name="OCID"></param>
        /// <param name="sm"></param>
        /// <returns></returns>
        public bool IsClassTraceExists(string plantype, Int64 OCID, SessionModel sm)
        {
            bool rtn = false;
            TblE_CLSTRACE where = new TblE_CLSTRACE
            {
                TRC_MSN = sm.MemSN,
                TRC_OCID = OCID
            };

            //會員類別 1:會員(自辦在職) 2:企業 3.產投會員
            switch (plantype)
            {
                case "1"://產投
                    where.TRC_MTYPE = "3";
                    break;
                case "2"://自辦在職
                    where.TRC_MTYPE = "1";
                    break;
                case "5"://區域產業據點
                    where.TRC_MTYPE = "1";
                    break;
            }

            TblE_CLSTRACE data = base.GetRow<TblE_CLSTRACE>(where);

            if (data != null && "N".Equals(data.ISDELETE)) { rtn = true; }

            return rtn;
        }

        /// <summary>
        /// 課程刪除
        /// </summary>
        /// <param name="classlist">課程代碼清單</param>
        /// <param name="sm">SessionModel</param>
        public void DelClassTrace(IList<Int64?> classlist, SessionModel sm)
        {
            //Int64 sn = 0;
            //base.BeginTransaction();
            try
            {
                if (classlist != null)
                {
                    //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
                    foreach (var item in classlist)
                    {
                        //查詢舊資料是否存在
                        if (item != null)
                        {
                            TblE_CLSTRACE where = new TblE_CLSTRACE { TRC_MSN = sm.MemSN, TRC_OCID = item };
                            var tmp = base.GetRow<TblE_CLSTRACE>(where);
                            if (tmp != null)
                            {
                                TblE_CLSTRACE param = new TblE_CLSTRACE { TRC_SN = tmp.TRC_SN };
                                this.UpdateClassTrace("D", param);
                                //sm.LastResultMessage = "課程收藏移除成功！";
                                //TblE_CLSTRACE param = new TblE_CLSTRACE { ISDELETE = "N" };
                                //base.Update<TblE_CLSTRACE>(param, tmp);
                            }
                        }
                    }
                }
                //base.CommitTransaction();
                //base.RollBackTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                LOG.Error("WDAIIPWEBDAO DelClassTrace MultiDelClassTrace failed: " + ex.Message, ex);
                throw new Exception("WDAIIPWEBDAO DelClassTrace  MultiDelClassTrace failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 課程收藏
        /// </summary>
        /// <param name="classlist">課程代碼清單</param>
        /// <param name="sm">SessionModel</param>
        /// <returns></returns>
        public void AddClassTrace(string plantype, IList<Int64?> classlist, SessionModel sm)
        {
            Int64 sn = 0;

            base.BeginTransaction();
            try
            {
                if (classlist != null)
                {
                    foreach (var item in classlist)
                    {
                        //查詢舊資料是否存在
                        if (item != null)
                        {
                            TblE_CLSTRACE where = new TblE_CLSTRACE
                            {
                                TRC_MSN = sm.MemSN,
                                TRC_OCID = item
                            };

                            //會員類別 1:會員(自辦在職) 2:企業 3.產投會員
                            switch (plantype)
                            {
                                case "1"://產業人才投資方案
                                    where.TRC_MTYPE = "3";
                                    break;
                                case "2"://在職
                                case "5"://區域產業據點
                                    where.TRC_MTYPE = "1";
                                    break;
                            }

                            var tmp = base.GetRow<TblE_CLSTRACE>(where);
                            if (tmp == null)
                            {
                                where.TRC_ISENTER = "0";//0:未報名  1:已報名
                                where.TRC_CTIME = DateTime.Now;
                                where.ISSHARE = "N";
                                where.ISDELETE = "N";
                                //取得E_CLSTRACE的PK欄位值
                                //int sn = new MyKeyMapDAO().GetTableMaxSeqNo(StaticCodeMap.TableName.E_CLSTRACE, "TRC_SN");
                                //sn = (new WDAIIPWEBDAO()).GetNewId("E_CLSTRACE_TRCSN_SEQ,E_CLSTRACE,TRC_SN").Value;
                                sn = this.GetNewId("E_CLSTRACE_TRCSN_SEQ,E_CLSTRACE,TRC_SN").Value;
                                where.TRC_SN = sn;
                                base.Insert<TblE_CLSTRACE>(where);
                            }
                            else
                            {
                                TblE_CLSTRACE param = new TblE_CLSTRACE { ISDELETE = "N" };
                                base.Update<TblE_CLSTRACE>(param, tmp);
                            }
                        }
                    }

                }
                base.CommitTransaction();
                //base.RollBackTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("WDAIIPWEBDAO MultiAddClassTrace failed: " + ex.Message, ex);
                throw new Exception("WDAIIPWEBDAO MultiAddClassTrace failed:" + ex.Message, ex);
            }
        }

        /// <summary>加入課程瀏覽記錄</summary>
        /// <param name="ocid"></param>
        public void AddClassViewRecord(string tplanid, Int64 ocid)
        {
            //在期間內回傳true HH:mm-HH:mm不記錄瀏覽人次
            if ((new Services.WDAIIPWEBService()).StopNORECORDVISITS()) return;

            //HttpContext context = HttpContext.Current;
            string str_UserHostIp = MyCommonUtil.GetIpAddress(HttpContext.Current);

            try
            {
                object mutex = new object(); //lock (mutex) {}
                lock (mutex)
                {
                    //Int64 seqno = new MyKeyMapDAO().GetTableMaxSeqNo(StaticCodeMap.TableName.TB_VIEWRECORD, "SEQNO");
                    //long i_seqno = this.GetNewId("TB_VIEWRECORD_SEQNO_SEQ,TB_VIEWRECORD,SEQNO").Value;
                    //SELECT NEXT VALUE FOR TB_VIEWRECORD_SEQNO_SEQ' getVIEWRECORD_SEQNO
                    Hashtable parms = new Hashtable();
                    long i_seqno = (long)this.QueryForObject("WDAIIPWEB.getVIEWRECORD_SEQNO", parms);
                    //base.BeginTransaction();
                    TblTB_VIEWRECORD data = new TblTB_VIEWRECORD
                    {
                        SEQNO = i_seqno,
                        TPLANID = tplanid,
                        OCID = ocid,
                        IP = str_UserHostIp,
                        VIEWTIME = DateTime.Now
                    };
                    base.Insert(data);
                    //base.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                //LOG.Error("WDAIIPWEBDAO AddClassViewRecord failed: " + ex.Message, ex);
                //throw new Exception("WDAIIPWEBDAO AddClassViewRecord failed:" + ex.Message, ex);
                LOG.Warn(string.Concat("WDAIIPWEBDAO AddClassViewRecord TB_VIEWRECORD failed: ", ex.Message));
                return;
            }
        }

        /// <summary>加入 TB_VIEWRECORD2 瀏覽點擊紀錄資料表</summary>
        /// <param name="s_UserHostIp"></param>
        public void AddTBViewRecord2(string s_UserHostIp, int? i_CLICKTYPE)
        {
            //在期間內回傳true HH:mm-HH:mm不記錄瀏覽人次
            //if ((new Services.WDAIIPWEBService()).StopNORECORDVISITS()) return;
            try
            {
                object mutex = new object(); //lock (mutex) {}
                lock (mutex)
                {
                    long i_seqno = this.GetNewId("TB_VIEWRECORD2_SEQNO_SEQ,TB_VIEWRECORD2,SEQNO").Value;
                    //base.BeginTransaction();
                    TblTB_VIEWRECORD2 data = new TblTB_VIEWRECORD2
                    {
                        SEQNO = i_seqno,
                        IP = s_UserHostIp,
                        CLICKTIME = DateTime.Now,
                        CLICKTYPE = i_CLICKTYPE
                    };
                    base.Insert(data);
                    //base.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                //LOG.Error("WDAIIPWEBDAO AddTBViewRecord2 failed: " + ex.Message, ex);
                LOG.Warn(string.Concat("WDAIIPWEBDAO AddTBViewRecord2 TB_VIEWRECORD2 failed: ", ex.Message));
                //throw new Exception("WDAIIPWEBDAO AddTBViewRecord2 failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 查詢課程瀏覽記錄(排除自己看過的)
        /// </summary>
        /// <param name="tplanid"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassViewRecordGridModel> QueryClassViewRecord(string tplanid, Int64 ocid)
        {
            TblTB_VIEWRECORD where = new TblTB_VIEWRECORD
            {
                TPLANID = tplanid,
                OCID = ocid,
                IP = HttpContext.Current.Request.UserHostAddress
            };

            return base.QueryForListAll<ClassViewRecordGridModel>("WDAIIPWEB.queryClassViewRecord", where);
        }

        /// <summary>
        /// 查詢產投招訓簡章資訊
        /// </summary>
        /// <param name="planid"></param>
        /// <param name="ocid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public WGReportModel GetWGReport(Int64? planid, Int64? ocid, string rid)
        {
            WGReportModel rtn = null;
            string funName = "WDAIIPWEB.getWGReport";

            WGReportModel parms = new WGReportModel { PLANID = planid, OCID = ocid, RID = rid };

            rtn = (WGReportModel)base.QueryForObject(funName, parms);

            return rtn;
        }

        /// <summary>
        /// 記錄課程資料瀏覽次數
        /// </summary>
        /// <param name="ocid"></param>
        public Int64 AddBrowseCnt(Int64 ocid)
        {
            Int64? curval = 0;
            object mutex = new object();
            lock (mutex)
            {
                bool isLocalSession = false;
                if (!base.transactionOn)
                {
                    isLocalSession = true;
                    base.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                }

                try
                {
                    // 先進行異動更新, 以觸發 Transaction Lock, 避免其他 thread 取到相同值
                    TblCLASS_CLASSINFO obj = new TblCLASS_CLASSINFO { OCID = ocid };
                    TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = ocid };
                    base.Update<TblCLASS_CLASSINFO>(obj, where);

                    //取得課程目前瀏覽人次
                    curval = this.GetCurrBrowseCnt(ocid);

                    //修改課程瀏覽人 (值加1)
                    curval = (curval.HasValue ? curval : 0) + 1;
                    this.UpdateBrowseCnt(ocid, curval);

                    if (isLocalSession)
                    {
                        base.CommitTransaction();
                    }

                    LOG.Debug("AddBrowseCnt: ocid=" + ocid + ", browsecnt curval=" + curval.Value);
                }
                catch (Exception ex)
                {
                    if (isLocalSession) { base.RollBackTransaction(); }
                    LOG.Error("WDAIIPWEBDAO AddBrowseCnt failed: " + ex.Message, ex);
                    throw new Exception("WDAIIPWEBDAO AddBrowseCnt failed:" + ex.Message, ex);
                }
            }

            return curval.Value;
        }

        /// <summary>取得課程最新瀏覽人次資料</summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public Int64? GetCurrBrowseCnt(Int64 ocid)
        {
            Int64? rtn = null;

            TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = ocid };
            TblCLASS_CLASSINFO data = base.GetRow<TblCLASS_CLASSINFO>(where);
            if (data != null) { rtn = data.BROWSECNT; }

            return rtn;
        }

        /// <summary>異動課程瀏覽人次資料</summary>
        /// <param name="ocid"></param>
        /// <param name="curVal"></param>
        public void UpdateBrowseCnt(Int64 ocid, Int64? curVal)
        {
            TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = ocid };
            TblCLASS_CLASSINFO newBrowseCnt = new TblCLASS_CLASSINFO { BROWSECNT = curVal };
            base.Update(newBrowseCnt, where);
        }
        #endregion

        #region 課程查詢報名/計畫課程列表
        /// <summary> 課程查詢 for 產業人才投資、充電起飛 </summary>
        /// <returns></returns>
        public IList<PlanClassSearchGrid1Model> QueryPlanClassSearch(PlanClassSearchFormModel form)
        {
            IList<PlanClassSearchGrid1Model> rtn = null;
            try
            {
                rtn = base.QueryForList<PlanClassSearchGrid1Model>("WDAIIPWEB.queryPlanClassSearch", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryPlanClassSearch() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 課程查詢 for 自辦在職、接受企業委託訓練
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IList<PlanClassSearchGrid2Model> QueryPlanClassSearch_2(PlanClassSearchFormModel form)
        {
            IList<PlanClassSearchGrid2Model> rtn = null;
            try
            {

                rtn = base.QueryForList<PlanClassSearchGrid2Model>("WDAIIPWEB.queryPlanClassSearch_2", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryPlanClassSearch_2() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }


        /// <summary>
        /// 課程查詢 for 區域產業據點
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<PlanClassSearchGrid5Model> QueryPlanClassSearch_5(PlanClassSearchFormModel form)
        {
            IList<PlanClassSearchGrid5Model> rtn = null;
            try
            {
                rtn = base.QueryForList<PlanClassSearchGrid5Model>("WDAIIPWEB.queryPlanClassSearch_5", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryPlanClassSearch_5() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }
        #endregion

        #region 課程查詢/分署課程列表
        /// <summary>
        /// 依分署查詢課程資料（找未開訓的班）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<T> QueryDistClassSearch<T>(DistClassSearchFormModel form)
        {
            IList<T> results = new List<T>();
            string funcName = "";

            switch (form.PLANTYPE)
            {
                case "1": //產業人才投資方案
                    funcName = "WDAIIPWEB.queryDistClassSearch_1";
                    results = base.QueryForList<T>(funcName, form);
                    break;
                case "2": //在職
                    funcName = "WDAIIPWEB.queryDistClassSearch_2";
                    results = base.QueryForList<T>(funcName, form);
                    break;
                case "5": //區域產業據點
                    funcName = "WDAIIPWEB.queryDistClassSearch_5";
                    results = base.QueryForList<T>(funcName, form);
                    break;
            }

            return results;
        }
        #endregion

        #region 課程查詢/其他政府單位職訓課程
        /// <summary>
        /// 查詢其他政府單位職訓課程清單(依輸入條件)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<GovClassSearchGridModel> QueryGovClassSearch(GovClassSearchFormModel form)
        {
            string funcName = "ETrainTrans.queryGovClassSearch";
            //if (!string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
            //    form.STDATE = form.STDATE_YEAR + "/" + form.STDATE_MON.PadLeft(2, '0') + "/01 00:00:00";
            //if (!string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
            //{
            //    DateTime tmp = new DateTime(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON), DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)));
            //    form.FTDATE = tmp.ToString("yyyy/MM/dd") + " 23:59:59";
            //}
            return base.QueryForList<GovClassSearchGridModel>(funcName, form);
        }

        /// <summary>
        /// 查詢其他政府單位職訓課程明細資料
        /// </summary>
        /// <param name="cpid"></param>
        /// <returns></returns>
        public GovClassSearchDetailModel GetGovClassSearch(Int64 cpid)
        {
            string funcName = "ETrainTrans.getGovernClassSearchDetail";
            TblCLASS_PLAN_INFO whereConds = new TblCLASS_PLAN_INFO { CPID = cpid };
            return (GovClassSearchDetailModel)base.QueryForObject(funcName, whereConds);
        }
        #endregion

        #region 課程查詢/歷史課程列表
        public IList<T> QueryHistoryClassSearch<T>(ClassSearchFormModel form)
        {
            IList<T> results = new List<T>();

            //取得目前(DB)系統時間
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            DateTime nowTime = keyDao.GetSysDateNow();
            form.STDATE = string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_STDATE_ok = MyCommonUtil.IsDate(form.STDATE);
            form.FTDATE = string.Concat(form.FTDATE_YEAR, "/", (form.FTDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_FTDATE_ok = MyCommonUtil.IsDate(form.FTDATE);
            form.STDATE = flag_STDATE_ok ? string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01 00:00:00") : null;
            int iDayInM = flag_FTDATE_ok ? DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)) : -1;
            form.FTDATE = flag_FTDATE_ok ? string.Concat(form.FTDATE_YEAR, "/", (form.FTDATE_MON ?? "").PadLeft(2, '0'), "/", iDayInM.ToString().PadLeft(2, '0'), " 23:59:59") : null;

            switch (form.PlanType)
            {
                case "1"://產投
                    if (!flag_STDATE_ok) { form.STDATE = null; }
                    if (!flag_FTDATE_ok) { form.FTDATE = null; }
                    if (string.IsNullOrEmpty(form.STDATE) || string.IsNullOrEmpty(form.FTDATE))
                    {
                        form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                        form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                    }

                    //轉換成大寫
                    if (!string.IsNullOrEmpty(form.CLASSCNAME))
                        form.CLASSCNAME = form.CLASSCNAME.ToUpper();
                    if (!string.IsNullOrEmpty(form.SCHOOLNAME))
                        form.SCHOOLNAME = form.SCHOOLNAME.ToUpper();

                    //組合-上課時段
                    form.TPERIOD28S_Sql = "";
                    for (int i = 0; i < form.TPERIOD28S_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.TPERIOD28S_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.TPERIOD28S_Sql += $"{(string.IsNullOrEmpty(form.TPERIOD28S_Sql) ? " AND (" : "")}";
                            form.TPERIOD28S_Sql += $"{(i > 0 ? " OR " : "")}TP.TPERIOD28_{form.TPERIOD28S_SHOW[i]}='Y'";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.TPERIOD28S_Sql)) { form.TPERIOD28S_Sql += ") "; }

                    //組合星期條件
                    form.WEEKS_Sql = "";
                    for (int i = 0; i < form.WEEKS_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.WEEKS_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.WEEKS_Sql += $"{(string.IsNullOrEmpty(form.WEEKS_Sql) ? " AND (" : "")}";
                            form.WEEKS_Sql += $"{(i > 0 ? " OR " : "")}OC.WEEKS{form.WEEKS_SHOW[i]}=1";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.WEEKS_Sql)) { form.WEEKS_Sql += ") "; }

                    results = base.QueryForList<T>("WDAIIPWEB.queryHistoryClassSearch_1", form);
                    break;

                case "2": //在職 //訓練期間
                    if (!flag_STDATE_ok) { form.STDATE = null; }
                    if (!flag_FTDATE_ok) { form.FTDATE = null; }
                    if (string.IsNullOrEmpty(form.STDATE) || string.IsNullOrEmpty(form.FTDATE))
                    {
                        form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                        form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                    }

                    if (!string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
                        form.STDATE = form.STDATE_YEAR + "/" + form.STDATE_MON.PadLeft(2, '0') + "/01 00:00:00";

                    if (!string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
                    {
                        DateTime tmp = new DateTime(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON), DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)));
                        form.FTDATE = tmp.ToString("yyyy/MM/dd") + " 23:59:59";
                    }
                    results = base.QueryForList<T>("WDAIIPWEB.queryClassSearch_2", form);
                    break;

                case "5": //區域據點 //訓練期間
                    if (!string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
                        form.STDATE = form.STDATE_YEAR + "/" + form.STDATE_MON.PadLeft(2, '0') + "/01 00:00:00";

                    if (!string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
                    {
                        DateTime tmp = new DateTime(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON), DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)));
                        form.FTDATE = tmp.ToString("yyyy/MM/dd") + " 23:59:59";
                    }

                    results = base.QueryForList<T>("WDAIIPWEB.queryClassSearch_5", form);
                    break;
            }

            return results;
        }
        #endregion

        #region 課程查詢/歷史政策性課程列表

        /// <summary>
        /// 課程查詢/歷史政策性課程列表
        /// QueryHistoryPolicySch - 政策性課程專區-Policy course area-查詢結果清單頁
        /// QueryHistoryPolicySch_1 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<T> QueryHistoryPolicySch<T>(ClassSearchFormModel form)
        {
            IList<T> results = new List<T>();

            //取得目前(DB)系統時間
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            DateTime nowTime = keyDao.GetSysDateNow();
            form.STDATE = string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_STDATE_ok = MyCommonUtil.IsDate(form.STDATE);
            form.FTDATE = string.Concat(form.FTDATE_YEAR, "/", (form.FTDATE_MON ?? "").PadLeft(2, '0'), "/01");
            bool flag_FTDATE_ok = MyCommonUtil.IsDate(form.FTDATE);
            form.STDATE = flag_STDATE_ok ? string.Concat(form.STDATE_YEAR, "/", (form.STDATE_MON ?? "").PadLeft(2, '0'), "/01 00:00:00") : null;
            int iDayInM = flag_FTDATE_ok ? DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)) : -1;
            form.FTDATE = flag_FTDATE_ok ? string.Concat(form.FTDATE_YEAR, "/", (form.FTDATE_MON ?? "").PadLeft(2, '0'), "/", iDayInM.ToString().PadLeft(2, '0'), " 23:59:59") : null;

            switch (form.PlanType)
            {
                case "1": //產投
                    if (!flag_STDATE_ok) { form.STDATE = null; }
                    if (!flag_FTDATE_ok) { form.FTDATE = null; }
                    if (string.IsNullOrEmpty(form.STDATE) || string.IsNullOrEmpty(form.FTDATE))
                    {
                        form.STDATE = nowTime.ToString("yyyy/MM/dd HH:mm:ss");
                        form.FTDATE = nowTime.AddMonths(1).ToString("yyyy/MM/dd HH:mm:ss");
                    }

                    //if (!string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
                    //    form.STDATE = form.STDATE_YEAR + "/" + form.STDATE_MON.PadLeft(2, '0') + "/01 00:00:00";
                    //if (!string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
                    //{
                    //    DateTime tmp = new DateTime(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON), DateTime.DaysInMonth(Convert.ToInt32(form.FTDATE_YEAR), Convert.ToInt32(form.FTDATE_MON)));
                    //    form.FTDATE = tmp.ToString("yyyy/MM/dd") + " 23:59:59";
                    //}
                    //轉換成大寫
                    if (!string.IsNullOrEmpty(form.CLASSCNAME))
                        form.CLASSCNAME = form.CLASSCNAME.ToUpper();
                    if (!string.IsNullOrEmpty(form.SCHOOLNAME))
                        form.SCHOOLNAME = form.SCHOOLNAME.ToUpper();

                    //組合星期條件
                    form.WEEKS_Sql = "";
                    for (int i = 0; i < form.WEEKS_SHOW.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(form.WEEKS_SHOW[i]))
                        { //修正查詢條件「上課時間」未勾選時，在執行完收藏會傳一個空的物件造成死掉的問題
                            form.WEEKS_Sql += $"{(string.IsNullOrEmpty(form.WEEKS_Sql) ? " AND (" : "")}";
                            form.WEEKS_Sql += $"{(i > 0 ? " OR " : "")}OC.WEEKS{form.WEEKS_SHOW[i]}=1";
                        }
                    }
                    if (!string.IsNullOrEmpty(form.WEEKS_Sql)) { form.WEEKS_Sql += ") "; }

                    results = base.QueryForList<T>("WDAIIPWEB.queryHistoryPolicySch_1", form);
                    break;

            }

            return results;
        }
        #endregion

        #region 會員專區/會員報名資料維護 & 課程報名資料維護 (共用)
        public TblSTUD_ENTERTYPE GetEnterTypeBySETID(decimal setid, DateTime enterdate, decimal sernum)
        {
            TblSTUD_ENTERTYPE whereConds = new TblSTUD_ENTERTYPE { SETID = setid, ENTERDATE = enterdate, SERNUM = sernum };
            return base.GetRow<TblSTUD_ENTERTYPE>(whereConds);
        }

        /// <summary>
        /// 查詢會員資料
        /// </summary>
        /// <param name="idno">身分證</param>
        /// <param name="birth">出生日期</param>
        /// <returns></returns>
        public EMemberExtModel GetEMember(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.getEMemberInfo";
            EMemberExtModel rst = new EMemberExtModel();
            TblE_MEMBER whereConds = new TblE_MEMBER { MEM_IDNO = idno, MEM_BIRTH = birth };
            rst = (EMemberExtModel)base.QueryForObject(funcName, whereConds);
            return rst;
        }

        /// <summary>
        /// 查詢班級資料
        /// 06:在職 /28:產投 /70:區域產業據點 課程報名作業
        /// </summary>
        /// <param name="ocid">班級代碼</param>
        /// <returns></returns>
        public ClassClassInfoExtModel GetClassInfoByOCID(decimal ocid)
        {
            string funcName = "WDAIIPWEB.getClassInfoByOCID";
            TblCLASS_CLASSINFO whereConds = new TblCLASS_CLASSINFO { OCID = ocid };
            return (ClassClassInfoExtModel)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 查詢班級訓練單位資料
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassClassInfoExtModel> QueryClassOrgInfoByOCID(decimal ocid)
        {
            string funcName = "WDAIIPWEB.queryClassOrgInfoByOCID";

            Hashtable parms = new Hashtable { { "OCID", ocid } };

            return base.QueryForListAll<ClassClassInfoExtModel>(funcName, parms);
        }

        /// <summary>
        /// 查詢收件完成(審核中)，報名成功的報名資料
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<TblSTUD_ENTERTYPE2> QueryStudEnterType2ByIDNO(string idno)
        {
            string funcName = "WDAIIPWEB.queryStudEnterType2ByIDNO";

            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2 { IDNO = idno };

            return base.QueryForListAll<TblSTUD_ENTERTYPE2>(funcName, whereConds);
        }

        /// <summary>
        /// 查詢目前報名的班是否已在報名成功階段
        /// </summary>
        /// <param name="ocids"></param>
        /// <param name="orgid"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassClassInfoExtModel> QueryClassOrgInfoByOCIDs(string ocids, decimal orgid, decimal ocid)
        {
            string funcName = "WDAIIPWEB.queryClassOrgInfoByOCIDs";
            Hashtable parms = new Hashtable
            {
                { "OCIDS", ocids },
                { "ORGID", orgid },
                { "OCID", ocid }
            };
            return base.QueryForListAll<ClassClassInfoExtModel>(funcName, parms);
        }

        /// <summary>
        /// 查詢內網報名資料
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<TblSTUD_ENTERTYPE> QueryStudEnterTypeByIDNO(string idno)
        {
            string funcName = "WDAIIPWEB.queryStudEnterTypeByIDNO";

            TblSTUD_ENTERTEMP whereConds = new TblSTUD_ENTERTEMP { IDNO = idno };

            return base.QueryForListAll<TblSTUD_ENTERTYPE>(funcName, whereConds);
        }

        /// <summary>
        /// 查詢目前報名的班是否已在報名成功階段(排除判斷是否開班)
        /// </summary>
        /// <param name="ocids"></param>
        /// <param name="orgid"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassClassInfoExtModel> QueryClassOrgInfoByOCIDs2(string ocids, decimal orgid, decimal ocid)
        {
            string funcName = "WDAIIPWEB.queryClassOrgInfoByOCIDs2";

            Hashtable parms = new Hashtable
            {
                { "OCIDS", ocids },
                { "ORGID", orgid },
                { "OCID", ocid }
            };

            return base.QueryForListAll<ClassClassInfoExtModel>(funcName, parms);
        }

        /// <summary>
        /// 查詢目前報名的班(報名年齡資格)
        /// </summary>
        /// <param name="ocids"></param>
        /// <param name="orgid"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassClassInfoExtModel> QueryClassOrgInfoByOCIDs3(decimal ocid)
        {
            string funcName = "WDAIIPWEB.queryClassOrgInfoByOCIDs3";

            Hashtable parms = new Hashtable { { "OCIDS", ocid } };

            return base.QueryForListAll<ClassClassInfoExtModel>(funcName, parms);
        }

        /// <summary>
        /// 報名儲存作業-全範圍搜尋IDNO '有資料(修改模式) 'ALL '沒有資料(新增模式)
        /// </summary>
        /// <param name="idno">身分證</param>
        /// <param name="birth">出生日期</param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP2 GetStudEnterTemp2ByIDNO(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.GetStudEnterTemp2ByIDNO";
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2
            {
                IDNO = idno,
                BIRTHDAY = birth
            };

            return (TblSTUD_ENTERTEMP2)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 只依 idno搜尋
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP2 GetStudEnterTemp2ByIDNO(string idno)
        {
            string funcName = "WDAIIPWEB.GetStudEnterTemp2ByNoBirth";
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2
            {
                IDNO = idno
            };
            return (TblSTUD_ENTERTEMP2)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 報名儲存作業-全範圍搜尋IDNO(len(esetid less then 8)) '有資料(修改模式) 'ALL '沒有資料(新增模式)
        /// </summary>
        /// <param name="idno">身分證</param>
        /// <param name="birth">出生日期</param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP2 GetStudEnterTemp2ByESETIDLen(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.GetStudEnterTemp2ByESETIDLen";
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2
            {
                IDNO = idno,
                BIRTHDAY = birth
            };

            return (TblSTUD_ENTERTEMP2)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 報名儲存作業-找出 （stud_entertemp）可能的 SETID '沒有為空
        /// </summary>
        /// <param name="idno">身分證</param>
        /// <param name="birth">出生日期</param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP GetEnterTempMaxSETID(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.GetStudEnterTempMaxSETID";
            TblSTUD_ENTERTEMP whereConds = new TblSTUD_ENTERTEMP
            {
                IDNO = idno,
                BIRTHDAY = birth
            };

            return (TblSTUD_ENTERTEMP)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 報名儲存作業-找出 （stud_entertemp）可能的 SETID '沒有為空
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP GetEnterTempMaxSETID(string idno)
        {
            string funcName = "WDAIIPWEB.GetStudEnterTempMaxNoBirth";
            TblSTUD_ENTERTEMP whereConds = new TblSTUD_ENTERTEMP
            {
                IDNO = idno
            };
            return (TblSTUD_ENTERTEMP)base.QueryForObject(funcName, whereConds);
        }


        /// <summary> 查詢會基本資料 </summary>
        /// <param name="idno">身分證</param>
        /// <param name="birth">出生日期</param>
        /// <returns></returns>
        public TblE_MEMBER GetEMemberByIDNO(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.GetEMemberByIDNO";

            TblE_MEMBER whereConds = new TblE_MEMBER
            {
                MEM_IDNO = idno,
                MEM_BIRTH = birth
            };

            return (TblE_MEMBER)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 新增e網報名資料
        /// </summary>
        /// <param name="memInfo"></param>
        /// <param name="setID"></param>
        /// <param name="eSETID"></param>
        public void InsertStudEnterTemp2(TblE_MEMBER memInfo, Int64? setID, Int64 eSETID)
        {
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();
            try
            {
                TblSTUD_ENTERTEMP2 newTemp2 = new TblSTUD_ENTERTEMP2
                {
                    SETID = setID,
                    ESETID = eSETID,
                    IDNO = memInfo.MEM_IDNO,
                    BIRTHDAY = memInfo.MEM_BIRTH,
                    NAME = memInfo.MEM_NAME,
                    SEX = memInfo.MEM_SEX,
                    DEGREEID = memInfo.MEM_EDU,
                    GRADID = memInfo.MEM_GRADUATE, /* GRADID - 畢業狀況 */
                    SCHOOL = memInfo.MEM_SCHOOL,
                    DEPARTMENT = memInfo.MEM_DEPART,
                    MILITARYID = memInfo.MEM_MILITARY,
                    ZIPCODE = memInfo.MEM_ZIP,
                    ZIPCODE6W = memInfo.MEM_ZIP6W,
                    ADDRESS = memInfo.MEM_ADDR,
                    PHONE1 = memInfo.MEM_TEL,
                    PHONE2 = memInfo.MEM_TELN,
                    CELLPHONE = memInfo.MEM_MOBILE,
                    EMAIL = memInfo.MEM_EMAIL,
                    MODIFYACCT = memInfo.MEM_IDNO,
                    MODIFYDATE = aNow
                };

                newTemp2.SCHOOL = !string.IsNullOrEmpty(memInfo.MEM_SCHOOL) ? memInfo.MEM_SCHOOL : newTemp2.SCHOOL;
                newTemp2.DEPARTMENT = !string.IsNullOrEmpty(memInfo.MEM_DEPART) ? memInfo.MEM_DEPART : newTemp2.DEPARTMENT;

                //PassPortNO(代碼置換) 除了"1"為外國:2 "0"或其它為本國:1 //本國 //外國
                decimal? dec_PASSPORTNO = memInfo.MEM_FOREIGN == "1" ? 2 : 1;
                newTemp2.PASSPORTNO = dec_PASSPORTNO.Value;

                //MARITALSTATUS(代碼轉型)
                int i_marry = -1;
                newTemp2.MARITALSTATUS = int.TryParse(memInfo.MEM_MARRY, out i_marry) ? i_marry : (decimal?)null;

                //ZIPCODE(代碼轉型)  int i_zip = 0;
                //newTemp2.ZIPCODE = memInfo.MEM_ZIP;// int.TryParse(memInfo.MEM_ZIP, out i_zip) ? i_zip : (decimal?)null;

                //個資使用是否同意
                string str_ISAGREE = "Y";
                if (!string.IsNullOrEmpty(memInfo.MEM_OPENSEC) && "0".Equals(memInfo.MEM_OPENSEC)) { str_ISAGREE = "N"; }
                newTemp2.ISAGREE = str_ISAGREE;

                //是否重新執行勞保勾稽（報名沒用到，塞預設值 0)
                newTemp2.LAINFLAG = 0;

                base.Insert(newTemp2);
            }
            catch (Exception ex)
            {
                LOG.Error("InsertStudEnterTemp2 ex:" + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 修改e網報名資料
        /// </summary>
        /// <param name="memInfo"></param>
        /// <param name="eSETID"></param>
        public void UpdateStudEnterTemp2(TblE_MEMBER memInfo, Int64 eSETID)
        {
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            try
            {
                TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2 { ESETID = eSETID };
                TblSTUD_ENTERTEMP2 oldTemp2 = base.GetRow(whereConds);
                TblSTUD_ENTERTEMP2 newTemp2 = new TblSTUD_ENTERTEMP2();

                newTemp2.InjectFrom(oldTemp2);

                newTemp2.IDNO = memInfo.MEM_IDNO;
                newTemp2.BIRTHDAY = memInfo.MEM_BIRTH;
                newTemp2.NAME = memInfo.MEM_NAME;
                newTemp2.SEX = memInfo.MEM_SEX;
                newTemp2.DEGREEID = memInfo.MEM_EDU;
                newTemp2.GRADID = memInfo.MEM_GRADUATE;
                newTemp2.SCHOOL = !string.IsNullOrEmpty(memInfo.MEM_SCHOOL) ? memInfo.MEM_SCHOOL : newTemp2.SCHOOL;
                newTemp2.DEPARTMENT = !string.IsNullOrEmpty(memInfo.MEM_DEPART) ? memInfo.MEM_DEPART : newTemp2.DEPARTMENT;
                newTemp2.MILITARYID = memInfo.MEM_MILITARY;
                newTemp2.ZIPCODE = memInfo.MEM_ZIP;
                newTemp2.ZIPCODE6W = memInfo.MEM_ZIP6W;
                newTemp2.ADDRESS = memInfo.MEM_ADDR;
                newTemp2.PHONE1 = memInfo.MEM_TEL;
                newTemp2.PHONE2 = memInfo.MEM_TELN;
                newTemp2.CELLPHONE = memInfo.MEM_MOBILE;
                newTemp2.EMAIL = memInfo.MEM_EMAIL;
                newTemp2.MODIFYACCT = memInfo.MEM_IDNO;
                newTemp2.MODIFYDATE = aNow;

                //PassPortNO(代碼置換) 除了"1"為外國:2 "0"或其它為本國:1 //本國 //外國
                decimal? dec_PASSPORTNO = memInfo.MEM_FOREIGN == "1" ? 2 : 1;
                newTemp2.PASSPORTNO = dec_PASSPORTNO.Value;

                //MARITALSTATUS(代碼轉型)
                int i_marry = -1;
                newTemp2.MARITALSTATUS = int.TryParse(memInfo.MEM_MARRY, out i_marry) ? i_marry : (decimal?)null;

                //ZIPCODE(代碼轉型) int i_zip = 0;
                //newTemp2.ZIPCODE = int.TryParse(memInfo.MEM_ZIP, out i_zip) ? i_zip : (decimal?)null;

                //個資使用是否同意
                string str_ISAGREE = "Y";
                if (!string.IsNullOrEmpty(memInfo.MEM_OPENSEC) && "0".Equals(memInfo.MEM_OPENSEC)) { str_ISAGREE = "N"; }
                newTemp2.ISAGREE = str_ISAGREE;

                base.Update(newTemp2, oldTemp2, whereConds);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateStudEnterTemp2 ex :" + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 取得最新esernum (len(esernum) ＜ 8)
        /// </summary>
        /// <returns></returns>
        public Int64 GetNewESERNUMBByLen()
        {
            /*
            long newESERNUM = (new WDAIIPWEBDAO()).GetNewId("STUD_ENTERTYPE2_ESERNUM_SEQ2,STUD_ENTERTYPE2,ESERNUM").Value;
            return newESERNUM;
            SELECT ISNULL(MAX(ESERNUM),0)+1 NEWESERNUM FROM STUD_ENTERTYPE2 WHERE ESERNUM<=9999999--831063 
            --select top 11 * from SYS_AUTONUM where TABLENAME like '%STUD_ENTERTYPE2_ESERNUM_SEQ%'
            insert into SYS_AUTONUM (TABLENAME ,CURVAL,MTIME) values('STUD_ENTERTYPE2_ESERNUM_SEQ2', 831063, getdate());
            update SYS_AUTONUM  set CURVAL=(SELECT MAX(ESERNUM) NEWESERNUM FROM STUD_ENTERTYPE2 WHERE ESERNUM < 9999999)+1,mtime=getdate() where TABLENAME='STUD_ENTERTYPE2_ESERNUM_SEQ2';
            */
            //string funcName = "WDAIIPWEB.GetNewESERNUMByLen";
            string funcName = "WDAIIPWEB.GetSTUD_ENTERTYPE2_ESERNUM_SEQ2";
            Hashtable data = (Hashtable)base.QueryForObject(funcName, null);
            Int64 rtn = -1;
            if (data == null) { return -1; }
            //return Int64.TryParse($"{data["NEWESERNUM"]}", out rtn) ? rtn : -1;
            return Int64.TryParse($"{data["CURVAL"]}", out rtn) ? rtn : -1;
        }

        /// <summary>
        /// 取得最新sernum 
        /// </summary>
        /// <returns></returns>
        public Int64 GetNewSERNUM(Int64 eSETID, DateTime aNow)
        {
            string funcName = "WDAIIPWEB.GetNewSERNUM";
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2
            {
                ESETID = eSETID,
                ENTERDATE = aNow
            };

            Hashtable data = (Hashtable)base.QueryForObject(funcName, whereConds);
            Int64 rtn = 1;
            if (data == null) { return 1; }
            return Int64.TryParse($"{data["NEWSERNUM"]}", out rtn) ? rtn : 1;
        }

        /// <summary>
        /// 查詢課程報名結果資料（by 報名當天）
        /// </summary>
        /// <param name="eSETID"></param>
        /// <param name="ocid">班級代碼</param>
        /// <param name="enterDate">報名日期</param>
        /// <returns></returns>
        public TblSTUD_ENTERTYPE2 GetStudEnterType2ByEnterDate(Int64 eSETID, decimal ocid, DateTime enterDate)
        {
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2
            {
                ESETID = eSETID,
                OCID1 = Convert.ToInt64(ocid),
                ENTERDATE = enterDate
            };

            return base.GetRow(whereConds);
        }

        /// <summary>
        /// 2019-02-11 add 查詢報名班級
        /// </summary>
        /// <param name="eSETID"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<TblSTUD_ENTERTYPE2> QueryStudEnterType2ByOCID(Int64 eSETID, decimal ocid)
        {
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2 { ESETID = eSETID, OCID1 = Convert.ToInt64(ocid) };

            return base.GetRowList<TblSTUD_ENTERTYPE2>(whereConds);
        }

        /// <summary>
        /// 查詢是否已有報名資料(同一門課非透過E網報名)
        /// </summary>
        /// <param name="eSETID"></param>
        /// <param name="ocid"></param>
        /// <param name="enterDate"></param>
        /// <returns></returns>
        public IList<TblSTUD_ENTERTYPE2> QueryStudEnterType2NotE(Int64 eSETID, decimal ocid, DateTime enterDate)
        {
            string funcName = "WDAIIPWEB.QueryStudEnterType2NotE";
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2
            {
                ESETID = eSETID,
                OCID1 = Convert.ToInt64(ocid),
                ENTERDATE = enterDate
            };

            return base.QueryForListAll<TblSTUD_ENTERTYPE2>(funcName, whereConds);
        }

        /// <summary>
        /// 2019-02-11 add 查詢是否有現場報名資料（同一門課）
        /// </summary>
        /// <param name="idn"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<TblSTUD_ENTERTYPE> QueryStudEnterTypeNotE(string acid, decimal ocid)
        {
            string funcName = "WDAIIPWEB.QueryStudEnterTypeNotE";
            Hashtable parms = new Hashtable { ["IDN"] = acid, ["OCID1"] = ocid };

            return base.QueryForListAll<TblSTUD_ENTERTYPE>(funcName, parms);
        }
        #endregion

        #region 課程查詢-課程報名-報名作業 SignUp/SignUp
        /// <summary>
        /// 報名時檢核重疊時段的機制 
        /// 1:查詢民眾已報名課程(尚未開訓)
        /// 2:含已開訓(14天內)
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassClassInfoExtModel> QueryEnterClassByIDNO(string idno, decimal ocid)
        {
            string funcName = "WDAIIPWEB.queryEnterClassByIDNO";

            Hashtable parms = new Hashtable { { "OCID", ocid }, { "IDNO", idno } };

            return base.QueryForListAll<ClassClassInfoExtModel>(funcName, parms);
        }

        /// <summary> 報名時檢核重疊時段的機制  2:查詢民眾已參訓課程(尚未結訓) </summary>
        /// <param name="idno"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<ClassClassInfoExtModel> QueryTrainClassByIDNO(string idno, decimal ocid)
        {
            string funcName = "WDAIIPWEB.queryTrainClassByIDNO";

            Hashtable parms = new Hashtable
            {
                { "OCID", ocid },
                { "IDNO", idno }
            };

            return base.QueryForListAll<ClassClassInfoExtModel>(funcName, parms);
        }

        /// <summary>
        /// 取得只能是產投的班級資料
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public ClassClassInfoExtModel GetOCIDDate28(decimal ocid)
        {
            ClassClassInfoExtModel rtn = null;
            string funcName = "WDAIIPWEB.GetOCIDDate28";

            ClassClassInfoExtModel whereConds = new ClassClassInfoExtModel { OCID = ocid };
            IList<ClassClassInfoExtModel> list = base.QueryForListAll<ClassClassInfoExtModel>(funcName, whereConds);

            if (list != null && list.Count > 0)
            {
                rtn = list[0];
            }

            return rtn;
        }

        /// <summary> 報名時檢核重疊時段的機制 查詢課程時段重疊之情形資料 </summary>
        /// <param name="ocid1"></param>
        /// <param name="ocid2"></param>
        /// <returns></returns>
        public IList<Hashtable> QueryClassDescDouble(decimal ocid1, decimal ocid2)
        {
            string funcName = "WDAIIPWEB.queryClassDescDouble";

            Hashtable parms = new Hashtable
            {
                { "OCID1", ocid1 },
                { "OCID2", ocid2 }
            };

            return base.QueryForListAll<Hashtable>(funcName, parms);
        }

        /// <summary>
        /// 以身分證號查詢報名暫存檔(STUD_ENTERTEMP), 若資料存在(任一筆)則回傳, 不存在則回傳 null
        /// <para>此月決議以身份證號為搜尋條件 by AMU 20090417</para>
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP GeteSETIDByStudEnterTemp(string idno)
        {
            TblSTUD_ENTERTEMP whereCond = new TblSTUD_ENTERTEMP { IDNO = idno };
            return base.GetRow<TblSTUD_ENTERTEMP>(whereCond);
        }

        /// <summary>
        /// 查詢班級初始資料(Class_ClassInfo)
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public ClassClassInfoExtModel GetClassClassInfo(Int64 ocid)
        {
            TblCLASS_CLASSINFO whereCond = new TblCLASS_CLASSINFO { OCID = ocid };

            return (ClassClassInfoExtModel)base.QueryForObject("WDAIIPWEB.getClassClassInfo", whereCond);
        }

        /// <summary> 依idno查詢其所有e網報名暫存資料(stud_entertemp2) </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP2 GeteSETID(string idno)
        {
            TblSTUD_ENTERTEMP2 whereCond = new TblSTUD_ENTERTEMP2 { IDNO = idno };
            return base.GetRow<TblSTUD_ENTERTEMP2>(whereCond);
        }

        /// <summary>
        /// 查詢班級報名&訓練起迄日資訊
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="tplanid"></param>
        /// <returns></returns>
        public ClassClassInfoExtModel GetOCIDDateByPlan(Int64 ocid, string tplanid)
        {
            Hashtable parms = new Hashtable
            {
                { "OCID", ocid },
                { "TPLANID", tplanid }
            };
            return (ClassClassInfoExtModel)base.QueryForObject("WDAIIPWEB.getOCIDDate", parms);
        }

        /// <summary>
        /// 查詢會員資料
        /// </summary>
        /// <param name="RID"></param>
        /// <param name="IDNO"></param>
        /// <returns></returns>
        public TblMEMBER GetMemberByRIDIDNO(Int64 RID, string IDNO)
        {
            TblMEMBER whereCond = new TblMEMBER()
            {
                RID = RID,
                ACID = IDNO
            };

            return base.GetRow(whereCond);
        }

        /// <summary>
        /// 以 eSETID 查詢e網報名資料(stud_entertemp2)
        /// </summary>
        /// <param name="eSETID"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTEMP2 GetEnterTemp2ByeSETID(Int64 eSETID)
        {
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2 { ESETID = eSETID };
            return base.GetRow<TblSTUD_ENTERTEMP2>(whereConds);
        }

        /// <summary>新增e網報名資料(insert stud_entertemp2)</summary>
        /// <param name="model"></param>
        public void InsertEnterTemp2(SignUpViewModel model)
        {
            TblSTUD_ENTERTEMP2 newTemp2 = new TblSTUD_ENTERTEMP2();
            string noDataMsg = "不詳";

            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            newTemp2.ESETID = model.Detail.ESETID;
            newTemp2.IDNO = model.Detail.IDNO;
            newTemp2.NAME = model.Detail.NAME;
            newTemp2.SEX = ("M".Equals(model.Detail.SEX) ? "M" : "F");
            newTemp2.BIRTHDAY = model.Detail.BIRTHDAY;
            newTemp2.PASSPORTNO = (model.Detail.PASSPORTNO == 1 ? 1 : 2);

            //婚姻狀況 '1.已;2.未 3.暫不提供(預設)
            switch (model.Detail.MARITALSTATUS)
            {
                case 1:
                case 2:
                    newTemp2.MARITALSTATUS = model.Detail.MARITALSTATUS;
                    break;
                default:
                    newTemp2.MARITALSTATUS = null;
                    break;
            }

            newTemp2.DEGREEID = model.Detail.DEGREEID;
            newTemp2.GRADID = (string.IsNullOrEmpty(model.Detail.GRADID) ? "01" : model.Detail.GRADID);
            newTemp2.SCHOOL = (string.IsNullOrWhiteSpace(model.Detail.SCHOOLNAME) ? noDataMsg : model.Detail.SCHOOLNAME);
            newTemp2.DEPARTMENT = (string.IsNullOrWhiteSpace(model.Detail.DEPARTMENT) ? noDataMsg : model.Detail.DEPARTMENT);
            newTemp2.MILITARYID = ("M".Equals(model.Detail.SEX) ? "01" : "03");
            newTemp2.ZIPCODE = model.Detail.ZIPCODE;
            newTemp2.ZIPCODE6W = MyCommonUtil.GET_ZIPCODE6W(model.Detail.ZIPCODE, model.Detail.ZIPCODE_2W);//model.Detail.ZIPCODE_6W;
            newTemp2.ADDRESS = model.Detail.ADDRESS;

            newTemp2.PHONE1 = model.Detail.PHONED;
            newTemp2.PHONE2 = model.Detail.PHONEN;
            newTemp2.CELLPHONE = model.Detail.CELLPHONE;
            newTemp2.EMAIL = (string.IsNullOrWhiteSpace(model.Detail.EMAIL) ? "無" : model.Detail.EMAIL.Trim());
            newTemp2.MODIFYACCT = model.Detail.IDNO;
            newTemp2.MODIFYDATE = aNow;
            newTemp2.LAINFLAG = 0; //default 0

            base.Insert<TblSTUD_ENTERTEMP2>(newTemp2);
        }

        /// <summary> 備份e網報名資料 (insert into STUD_ENTERTEMP2DELDATA select from STUD_ENTERTEMP2...) </summary>
        /// <param name="oldTemp2"></param>
        public void BackUpEnterTemp2(TblSTUD_ENTERTEMP2 oldTemp2)
        {
            TblSTUD_ENTERTEMP2DELDATA backupItem = new TblSTUD_ENTERTEMP2DELDATA();
            backupItem.InjectFrom(oldTemp2);
            decimal i_zipcode = 0;
            //decimal i_zipcode2w = 0;
            backupItem.LAINFLAG = (backupItem.LAINFLAG.HasValue ? backupItem.LAINFLAG : 0);
            if (decimal.TryParse(oldTemp2.ZIPCODE, out i_zipcode)) backupItem.ZIPCODE = i_zipcode;
            //if (decimal.TryParse(oldTemp2.ZIPCODE2W, out i_zipcode2w)) backupItem.ZIPCODE2W = i_zipcode2w;
            base.Insert(backupItem);
        }

        /// <summary>
        /// 異動e網報名資料(update stud_entertemp2)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="oldTemp2"></param>
        public void UpdateEnterTemp2(SignUpViewModel model, TblSTUD_ENTERTEMP2 oldTemp2)
        {
            TblSTUD_ENTERTEMP2 newTemp2 = new TblSTUD_ENTERTEMP2();
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2 { IDNO = oldTemp2.IDNO };
            Int64? eSETID = oldTemp2.ESETID;
            string noDataMsg = "不詳";

            newTemp2.InjectFrom(oldTemp2);

            /*更動下列欄位*/
            newTemp2.NAME = model.Detail.NAME;
            newTemp2.SEX = ("M".Equals(model.Detail.SEX) ? "M" : "F");
            newTemp2.BIRTHDAY = model.Detail.BIRTHDAY;
            newTemp2.PASSPORTNO = (model.Detail.PASSPORTNO == 1 ? 1 : 2);

            //婚姻狀況 '1.已;2.未 3.暫不提供(預設)
            switch (model.Detail.MARITALSTATUS)
            {
                case 1:
                case 2:
                    newTemp2.MARITALSTATUS = model.Detail.MARITALSTATUS;
                    break;
                default:
                    newTemp2.MARITALSTATUS = null;
                    break;
            }

            newTemp2.DEGREEID = model.Detail.DEGREEID;
            newTemp2.GRADID = (string.IsNullOrEmpty(model.Detail.GRADID) ? "01" : model.Detail.GRADID);
            newTemp2.SCHOOL = (string.IsNullOrWhiteSpace(model.Detail.SCHOOLNAME) ? noDataMsg : model.Detail.SCHOOLNAME);
            newTemp2.DEPARTMENT = (string.IsNullOrWhiteSpace(model.Detail.DEPARTMENT) ? noDataMsg : model.Detail.DEPARTMENT);
            newTemp2.MILITARYID = ("M".Equals(model.Detail.SEX) ? "01" : "03");
            newTemp2.ZIPCODE = model.Detail.ZIPCODE;
            newTemp2.ZIPCODE6W = MyCommonUtil.GET_ZIPCODE6W(model.Detail.ZIPCODE, model.Detail.ZIPCODE_2W); //model.Detail.ZIPCODE_6W;
            newTemp2.ADDRESS = model.Detail.ADDRESS;

            newTemp2.PHONE1 = model.Detail.PHONED;
            newTemp2.PHONE2 = model.Detail.PHONEN;
            newTemp2.CELLPHONE = model.Detail.CELLPHONE;
            newTemp2.EMAIL = (string.IsNullOrWhiteSpace(model.Detail.EMAIL) ? "無" : model.Detail.EMAIL.Trim());
            newTemp2.MODIFYACCT = model.Detail.IDNO;
            newTemp2.IDNO = model.Detail.IDNO;

            ClearFieldMap clearFieldMap = new ClearFieldMap();
            clearFieldMap.Add("MARITALSTATUS");
            //clearFieldMap.Add("ZIPCODE");
            clearFieldMap.Add("ZIPCODE6W");
            //clearFieldMap.Add("ZIPCODE"); clearFieldMap.Add("ZIPCODE2W");
            //clearFieldMap.Add("ADDRESS");
            clearFieldMap.Add("PHONE1");
            clearFieldMap.Add("CELLPHONE");
            clearFieldMap.Add("EMAIL");

            base.Update(newTemp2, oldTemp2, whereConds, clearFieldMap);
        }

        /// <summary>
        /// 回傳可用序號(eSETID流水ID與報名日期相同時加一)
        /// Stud_EnterType2序號(產投) 
        /// </summary>
        /// <param name="eSETID"></param>
        /// <returns></returns>
        public Int64 GetIntSerNum(decimal eSETID)
        {
            Int64 rtn = 1; //回傳可用序號
            Int64 rowCnt = 0;
            Int64 maxSerNum = 0;
            string funcName = string.Empty;

            Hashtable parms = new Hashtable
            {
                { "ESETID", eSETID }
            };

            // 查詢 STUD_ENTERTYPE2 筆數
            funcName = "WDAIIPWEB.getStudEnterType2Count";
            rowCnt += this.GetAboutType2Count(funcName, parms);

            // 查詢 STUD_ENTERTYPE2DELDATA 筆數
            funcName = "WDAIIPWEB.getStudEnterType2DelDataCount";
            rowCnt += this.GetAboutType2Count(funcName, parms);

            // 查詢 STUD_DELENTERTYPE2 筆數
            funcName = "WDAIIPWEB.getStudDelEnterType2Count";
            rowCnt += this.GetAboutType2Count(funcName, parms);

            // 若當天有報名資料
            if (rowCnt > 0)
            {
                // 取得目前最大序號+1
                maxSerNum = this.GetType2MaxSerNum(parms);

                // maxSerNum序號大於筆數
                if (maxSerNum > rowCnt)
                {
                    // 使用最大序號(應該不會有此種狀況
                    rtn = maxSerNum;
                }
                else
                {
                    // 否則回傳筆數+1
                    rtn = rowCnt + 1;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 查詢筆數 (by eSETID)
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        private Int64 GetAboutType2Count(string funcName, Hashtable parms)
        {
            Int64 rtn = 0;

            IList<Hashtable> list = base.QueryForListAll<Hashtable>(funcName, parms);
            if (list != null && list.Count > 0)
            {
                rtn = Convert.ToInt64(list[0]["TOTAL"]);
            }

            return rtn;
        }

        /// <summary>取得目前 stud_entertype2 最大序號 SerNum + 1 (by eSETID)</summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        private Int64 GetType2MaxSerNum(Hashtable parms)
        {
            Int64 rtn = 0;
            string funcName = "WDAIIPWEB.getMaxSerNum";

            IList<Hashtable> list = base.QueryForListAll<Hashtable>(funcName, parms);
            if (list != null && list.Count > 0)
            {
                rtn = Convert.ToInt64(list[0]["MAXSERNUM"]);
            }

            return rtn;
        }

        /// <summary>查詢e網報名資料 stud_entertype2 (by esetid, ocid1)
        /// <para>會抓取這些欄位: ESERNUM,ESETID,SETID,ENTERDATE,SERNUM,RELENTERDATE, OCID1,TMID1,RID,PLANID,IDENTITYID,ENTERPATH,SIGNNO</para>
        /// </summary>
        /// <param name="eSETID"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTYPE2 GetStudEnterType2ByESETID(Int64 eSETID, Int64 ocid)
        {
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2
            {
                ESETID = eSETID,
                OCID1 = ocid
            };

            return (TblSTUD_ENTERTYPE2)base.QueryForObject("WDAIIPWEB.getStudEnterType2ByESETID", whereConds);
        }

        /// <summary>新增e網報名職類檔資料 - stud_entertype2</summary>
        /// <param name="newType2"></param>
        public void InsertStudEnterType2(TblSTUD_ENTERTYPE2 newType2)
        {
            base.Insert(newType2);
        }

        /// <summary>更新e網報名職類檔資料 - stud_entertype2</summary>
        /// <param name="newType2"></param>
        public void UpdateStudEnterType2(TblSTUD_ENTERTYPE2 newType2)
        {
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2 { ESERNUM = newType2.ESERNUM };

            base.Update(newType2, whereConds);
        }

        /// <summary>取得（產投課程）報名序號 SignNo
        /// <para>會對 Class_EnterSignNo 進行 Insert 或 Update 但不會 Commit, 要由呼叫者自行控管Transaction</para>
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public long GetSignNoxEnterType3(decimal ocid)
        {
            // eric,
            // 不再呼叫預存程序 Do_InsertT2SignNo
            // 改由程式直接處理 Class_EnterSignNo 的累加
            // 也不再考慮指定 IDNO 的報名資料(Stud_EnterType2)是否存在的邏輯

            if (ocid <= 0)
            {
                throw new ArgumentException("班級代碼 OCID 參數要大於0");
            }

            decimal signNo;

            TblCLASS_ENTERSIGNNO where = new TblCLASS_ENTERSIGNNO()
            {
                OCID = ocid
            };

            TblCLASS_ENTERSIGNNO entity = base.GetRow<TblCLASS_ENTERSIGNNO>(where);
            if (entity == null)
            {
                // 不存在, 起始 SIGNNO=1 並新增
                signNo = 1;
                entity = new TblCLASS_ENTERSIGNNO()
                {
                    OCID = ocid,
                    SIGNNO = signNo
                };
                base.Insert<TblCLASS_ENTERSIGNNO>(entity);
            }
            else if (entity.SIGNNO.HasValue)
            {
                // 資料存在, SIGNNO 累加並寫回
                entity.SIGNNO = entity.SIGNNO.Value + 1;
                signNo = entity.SIGNNO.Value;
                base.Update<TblCLASS_ENTERSIGNNO>(entity, where);
            }
            else
            {
                signNo = -1;
                throw new ArgumentException("無法取得報名序號!");
            }

            return Convert.ToInt64(signNo);
        }

        /// <summary>
        /// 查詢線上報名資料(stud_entertrain2)
        /// </summary>
        /// <param name="eSERNUM"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTRAIN2 GetStudEnterTrain2ByESERNUM(Int64 eSERNUM)
        {
            TblSTUD_ENTERTRAIN2 whereConds = new TblSTUD_ENTERTRAIN2 { ESERNUM = eSERNUM };
            return base.GetRow(whereConds);
        }

        /// <summary>
        /// 新增線上報名資料 - stud_entertrain2
        /// </summary>
        /// <param name="model"></param>
        public void InsertStudEnterTrain2(SignUpViewModel model)
        {
            var data = model.Detail;
            data.ACTTEL = (data.ACTTEL ?? "").Trim();
            model.Detail.ACTTEL = data.ACTTEL;
            data.ACTADDRESS = (data.ACTADDRESS ?? "").Trim();
            model.Detail.ACTADDRESS = data.ACTADDRESS;

            /*
            // 序號異常不寫入
            if (!data.ESERNUM.HasValue || data.ESERNUM <= 0) return;

            // 查詢 stud_entertrain2
            TblSTUD_ENTERTRAIN2 oldTrain2 = this.GetStudEnterTrain2ByESERNUM(data.ESERNUM.Value);

            // 已有記錄資料時就不用再寫入
            if (oldTrain2 != null) return;
            */

            // 取得一組新序號pk值
            // 確保獨立的 transaction, 另外 new 新的 dao
            //data.SEID = (new WDAIIPWEBDAO()).GetNewId("STUD_ENTERTRAIN2_SEID_SEQ,STUD_ENTERTRAIN2,SEID").Value;
            data.SEID = this.GetNewId("STUD_ENTERTRAIN2_SEID_SEQ,STUD_ENTERTRAIN2,SEID").Value;

            TblSTUD_ENTERTRAIN2 newTrain2 = new TblSTUD_ENTERTRAIN2();
            newTrain2.InjectFrom(data);

            newTrain2.ESERNUM = data.ESERNUM;

            newTrain2.ZIPCODE2 = data.ZIPCODE2;
            newTrain2.ZIPCODE2_6W = MyCommonUtil.GET_ZIPCODE6W(data.ZIPCODE2, data.ZIPCODE2_2W);
            newTrain2.HOUSEHOLDADDRESS = data.HOUSEHOLDADDRESS;
            newTrain2.PRIORWORKPAY = string.IsNullOrWhiteSpace(data.PRIORWORKPAY) ? (long?)null : Convert.ToInt64(data.PRIORWORKPAY);

            //銀行/郵局帳號
            //long acctmode = data.ACCTMODE.HasValue ? data.ACCTMODE.Value : -1;
            long acctmode = data.ACCTMODE ?? -1;
            switch (acctmode)
            {
                case 0: //郵局帳號
                    newTrain2.POSTNO = data.POSTNO;
                    newTrain2.ACCTNO = data.POST_ACCTNO;
                    break;
                case 1: //銀行帳號
                    newTrain2.ACCTHEADNO = data.ACCTHEADNO;
                    newTrain2.BANKNAME = data.BANKNAME;
                    newTrain2.ACCTEXNO = data.ACCTEXNO;
                    newTrain2.EXBANKNAME = data.EXBANKNAME;
                    newTrain2.ACCTNO = data.BANK_ACCTNO;
                    break;
                case 2: //訓練單位代轉現金
                    break;
            }

            //服務部門(新版因已改成下拉，要再回塞舊欄位)
            newTrain2.SERVDEPT = (string.IsNullOrEmpty(data.SERVDEPTID) ? newTrain2.SERVDEPT : this.getServDeptName(data.SERVDEPTID));

            newTrain2.ACTNO = (string.IsNullOrWhiteSpace(data.ACTNO) ? null : data.ACTNO);
            newTrain2.ACTNAME = (string.IsNullOrWhiteSpace(data.ACTNAME) ? null : data.ACTNAME);

            newTrain2.ACTTEL = string.IsNullOrWhiteSpace(data.ACTTEL) ? null : data.ACTTEL;
            //newTrain2.ZIPCODE3_2W = (!data.ZIPCODE3_2W.HasValue ? null : Convert.ToString(data.ZIPCODE3_2W).Trim());
            newTrain2.ZIPCODE3 = data.ZIPCODE3;// (!data.ZIPCODE3.HasValue ? null : Convert.ToString(data.ZIPCODE3).Trim());
            newTrain2.ZIPCODE3_6W = MyCommonUtil.GET_ZIPCODE6W(data.ZIPCODE3, data.ZIPCODE3_2W);
            newTrain2.ACTADDRESS = string.IsNullOrWhiteSpace(data.ACTADDRESS) ? null : data.ACTADDRESS;

            //配合現行改成下拉方式填值，要再將所選結果中文名稱回塞給舊欄位
            newTrain2.JOBTITLE = (string.IsNullOrWhiteSpace(data.JOBTITLEID) ? null : this.getJobTitleName(data.JOBTITLEID));

            // 任職公司其他資料地址
            newTrain2.ZIP = -1; //97產業人才投資方案取消輸入'不可為null 給-1
            newTrain2.ADDR = " "; //97產業人才投資方案取消輸入 '不可為null 給空格
            newTrain2.TEL = " "; //97產業人才投資方案取消輸入 '不可為null 給空格
            newTrain2.SHOWDETAIL = " "; //97產業人才投資方案取消輸入 '不可為null 給空格
            if (!newTrain2.Q1.HasValue) { newTrain2.Q1 = 0; } //必填

            if (!string.IsNullOrEmpty(data.Q2_1))
            {
                newTrain2.Q2_1 = Convert.ToInt64(data.Q2_1);
            }

            if (!string.IsNullOrEmpty(data.Q2_2))
            {
                newTrain2.Q2_2 = Convert.ToInt64(data.Q2_2);
            }

            if (!string.IsNullOrEmpty(data.Q2_3))
            {
                newTrain2.Q2_3 = Convert.ToInt64(data.Q2_3);
            }

            if (!string.IsNullOrEmpty(data.Q2_4))
            {
                newTrain2.Q2_4 = Convert.ToInt64(data.Q2_4);
            }

            if (!string.IsNullOrEmpty(data.Q61))
            {
                newTrain2.Q61 = Convert.ToDouble(data.Q61);
            }

            if (!string.IsNullOrEmpty(data.Q62))
            {
                newTrain2.Q62 = Convert.ToDouble(data.Q62);
            }

            if (!string.IsNullOrEmpty(data.Q63))
            {
                newTrain2.Q63 = Convert.ToDouble(data.Q63);
            }

            if (!string.IsNullOrEmpty(data.Q64))
            {
                newTrain2.Q64 = Convert.ToDouble(data.Q64);
            }

            newTrain2.ISEMAIL = ("Y".Equals(data.ISEMAIL) ? "Y" : "N");

            switch (data.ISCHECK)
            {
                case "Y":
                case "N":
                    newTrain2.INSURED = data.ISCHECK;
                    break;
                default:
                    newTrain2.INSURED = null;
                    break;
            }
            newTrain2.MIDENTITYID = (model.Detail.MIDENTITYID ?? "01");
            newTrain2.MODIFYACCT = model.Detail.IDNO;
            newTrain2.MODIFYDATE = model.SignUpTime;
            base.Insert(newTrain2);
        }

        /// <summary>
        /// 異動線上報名資料(產學訓) - stud_entertrain2
        /// </summary>
        public void UpdateStudEnterTrain2(SignUpViewModel model)
        {
            return;
        }

        /// <summary>
        /// 取得部門單位名稱
        /// </summary>
        /// <param name="servDeptID"></param>
        /// <returns></returns>
        public string getServDeptName(string servDeptID)
        {
            string rtn = string.Empty;

            if (!string.IsNullOrEmpty(servDeptID))
            {
                TblKEY_SERVDEPT data = base.GetRow(new TblKEY_SERVDEPT { SERVDEPTID = servDeptID });
                if (data != null)
                {
                    rtn = data.SDNAME;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 查詢職稱中文名稱
        /// </summary>
        /// <param name="jobTitleID"></param>
        /// <returns></returns>
        public string getJobTitleName(string jobTitleID)
        {
            string rtn = string.Empty;

            if (!string.IsNullOrEmpty(jobTitleID))
            {
                TblKEY_JOBTITLE data = base.GetRow(new TblKEY_JOBTITLE { JOBTITLEID = jobTitleID });
                if (data != null)
                {
                    rtn = data.JTNAME;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 備份stud_entertemp3 to stud_entertemp3deldata
        /// </summary>
        public void BackUpEnterTemp3(string idno, Int64 eSETID3)
        {
            // 取得系統時間
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            // stud_entertemp3 註記異動日期資訊
            TblSTUD_ENTERTEMP3 whereConds = new TblSTUD_ENTERTEMP3 { ESETID3 = eSETID3 };
            TblSTUD_ENTERTEMP3 oldTemp3 = base.GetRow(whereConds);
            TblSTUD_ENTERTEMP3 newTemp3 = new TblSTUD_ENTERTEMP3();

            newTemp3.InjectFrom(oldTemp3);
            newTemp3.MODIFYACCT = idno;
            newTemp3.MODIFYDATE = aNow;
            base.Update(newTemp3, oldTemp3, whereConds);

            // 複製備份資料(stud_entertemp3 --> stud_entertemp3deldata)    
            TblSTUD_ENTERTEMP3DELDATA newTemp3Del = new TblSTUD_ENTERTEMP3DELDATA();
            newTemp3Del.InjectFrom(newTemp3);
            base.Insert(newTemp3Del);
        }

        /// <summary>
        /// 課程報名結果-取得產投計畫班級資料
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public ClassClassInfoExtModel GetClassByOCID(decimal ocid)
        {
            ClassClassInfoExtModel whereConds = new ClassClassInfoExtModel { OCID = ocid };

            return base.QueryForObject<ClassClassInfoExtModel>("WDAIIPWEB.queryOCIDClass", whereConds);
        }

        /// <summary>
        /// 測試線上報名狀況
        /// TIMS.Check_EnterType2
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <returns></returns>
        public bool CheckEnterType2(decimal ocid, string idno)
        {
            bool rtn = false;
            string funcName = "WDAIIPWEB.GetCheckEnterType2";

            Hashtable parms = new Hashtable
            {
                { "OCID1", ocid },
                { "IDNO", idno }
            };

            IList<TblSTUD_ENTERTYPE2> list = base.QueryForListAll<TblSTUD_ENTERTYPE2>(funcName, parms);

            if (list != null && list.Count > 0)
            {
                rtn = true; //收件成功
            }

            return rtn;
        }

        /// <summary>
        /// 報名成功 取得 線上報名序號 SignNo
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTYPE2 GetEnterType2SignNo(decimal ocid, string idno)
        {
            string funcName = "WDAIIPWEB.GetEnterType2SigNo";

            Hashtable parms = new Hashtable
            {
                { "OCID1", ocid },
                { "IDNO", idno }
            };

            return (TblSTUD_ENTERTYPE2)base.QueryForObject(funcName, parms);
        }

        /// <summary>
        /// 查詢該學員 報名且未開訓之班級已達3班者
        /// </summary>
        /// <param name="IDNO"></param>
        /// <returns></returns>
        public IList<Hashtable> GetEnterType2Cnt(string IDNO)
        {
            IList<Hashtable> result = null;
            if (string.IsNullOrEmpty(IDNO)) { return result; }
            Hashtable parms = new Hashtable
            {
                { "IDNO", IDNO }
            };

            try
            {
                result = base.QueryForListAll<Hashtable>("WDAIIPWEB.GetEnterType2Cnt", parms);
            }
            catch (Exception ex)
            {
                LOG.Error("GetEnterType2Cnt: " + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 查詢有問題的 entertype2 資料(正常只會有一筆)
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<Hashtable> QueryEnterTypeErr1List(decimal ocid, string idno)
        {
            if (string.IsNullOrEmpty(idno)) { return null; }

            string funcName = "WDAIIPWEB.GetEnterType2Err1List";
            Hashtable parms = new Hashtable
            {
                { "OCID1", ocid },
                { "IDNO", idno }
            };

            return base.QueryForListAll<Hashtable>(funcName, parms);
        }

        /// <summary>
        /// 查詢 entertype2.signno為空值的異常資料
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<Hashtable> QueryEnterTypeErr2List(decimal ocid, decimal eSETID)
        {
            string funcName = "WDAIIPWEB.GetEnterType2Err2List";
            Hashtable parms = new Hashtable { { "OCID1", Convert.ToInt64(ocid) }, { "ESETID", eSETID } };
            return base.QueryForListAll<Hashtable>(funcName, parms);
        }

        /// <summary>
        /// 刪除 stud_entertype2 異常資料（signno is null）
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="eSETID"></param>
        public void DeleteEnterType2Err(decimal ocid, Int64 eSETID)
        {
            try
            {
                string funcName = "WDAIIPWEB.deleteEnterType2Err";
                var whereConds = new TblSTUD_ENTERTYPE2 { OCID1 = Convert.ToInt64(ocid), ESETID = eSETID };
                base.Delete(funcName, whereConds);
            }
            catch (Exception ex)
            {
                LOG.Error("DeleteEnterTypeErr ex:" + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 找出 6小時內 第1筆 STUD_ENTERTYPE2 報名序號
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <param name="signno"></param>
        public TblSTUD_ENTERTYPE2 GetEnterType2MinESERNUM(decimal ocid, string idno, decimal signno)
        {
            if (string.IsNullOrEmpty(idno)) { return null; }

            string funcName = "WDAIIPWEB.GetEnterType2MinESERNUM";
            Hashtable parms = new Hashtable
            {
                { "IDNO", idno },
                { "OCID1", ocid },
                { "SIGNNO", signno }
            };

            return (TblSTUD_ENTERTYPE2)base.QueryForObject(funcName, parms);
        }

        /// <summary>
        /// 找出 stud_entertrain2 最大重複資料序號
        /// </summary>
        /// <param name="eSERNUM"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTRAIN2 GetEnterTrain2MaxSEID(Int64 eSERNUM)
        {
            string funcName = "WDAIIPWEB.GetEnterTrain2MaxSEID";
            TblSTUD_ENTERTRAIN2 whereConds = new TblSTUD_ENTERTRAIN2
            {
                ESERNUM = eSERNUM
            };

            return (TblSTUD_ENTERTRAIN2)base.QueryForObject(funcName, whereConds);
        }

        /// <summary>
        /// 刪除 stud_entertrain2 資料
        /// </summary>
        /// <param name="seID"></param>
        public void DeleteEnterTrain2(string idno, Int64 seID)
        {
            if (string.IsNullOrEmpty(idno)) { return; }

            // 取得系統時間
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            // stud_entertrain2 註記異動日期資訊
            TblSTUD_ENTERTRAIN2 whereConds = new TblSTUD_ENTERTRAIN2 { SEID = seID };
            TblSTUD_ENTERTRAIN2 oldTrain2 = base.GetRow(whereConds);
            TblSTUD_ENTERTRAIN2 newTrain2 = new TblSTUD_ENTERTRAIN2();
            newTrain2.InjectFrom(oldTrain2);
            newTrain2.MODIFYACCT = idno;
            newTrain2.MODIFYDATE = aNow;
            base.Update(newTrain2, oldTrain2, whereConds);

            // 複製備份資料(stud_entertrain2 --> stud_entertrain2deldata)
            TblSTUD_ENTERTRAIN2DELDATA newTrain2Del = new TblSTUD_ENTERTRAIN2DELDATA();
            newTrain2Del.InjectFrom(newTrain2);
            base.Insert(newTrain2Del);

            // 刪除 stud_entertrain2
            base.Delete(whereConds);
        }
        #endregion

        #region 會員專區/會員報名資料維護 & 課程報名資料維護 (共用)

        /// <summary>取得S1時間</summary>
        /// <param name="strIDNO"></param>
        /// <returns></returns>
        public string GetS1Date(string strIDNO)
        {
            string strRtn = "";
            if (string.IsNullOrEmpty(strIDNO)) { return null; }
            Hashtable param = new Hashtable { ["IDNO"] = strIDNO };
            try
            {
                string funcName = "WDAIIPWEB.getS1Date";

                IList<Hashtable> list = base.QueryForListAll<Hashtable>(funcName, param);

                if (list != null && list.Count > 0) strRtn = Convert.ToString(list[0]);

            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetS1Date: " + ex.Message, ex);
                throw ex;
            }
            return strRtn;
        }

        /// <summary>取得S3時間</summary>
        /// <param name="strIDNO"></param>
        /// <returns></returns>
        public string GetS3Date(string strIDNO)
        {
            string strRtn = "";
            if (string.IsNullOrEmpty(strIDNO)) { return null; }
            Hashtable param = new Hashtable { ["IDNO"] = strIDNO };
            try
            {
                string funcName = "WDAIIPWEB.getS3Date";

                IList<Hashtable> list = base.QueryForListAll<Hashtable>(funcName, param);

                if (list != null && list.Count > 0) strRtn = Convert.ToString(list[0]);

            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetS3Date: " + ex.Message, ex);
                throw ex;
            }
            return strRtn;
        }

        /// <summary>
        /// 使用前次報名資料 STUD_STUDENTINFO / STUD_ENTERTEMP2
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public SignUpDetailModel GetStudEnterTmp12()
        {
            SignUpDetailModel detail = null;
            SessionModel sm = SessionModel.Get();
            Hashtable param = new Hashtable
            {
                ["IDNO"] = sm.ACID ?? "",
                ["BIRTHDAY"] = sm.Birthday ?? ""
            };
            try
            {
                var strPoint = "";
                if (!string.IsNullOrEmpty(GetS1Date(sm.ACID))) strPoint = "SINFO";
                if (!string.IsNullOrEmpty(GetS3Date(sm.ACID))) strPoint = "STEMP2";

                string funcName = "";
                switch (strPoint)
                {
                    case "STEMP2":
                        funcName = "WDAIIPWEB.getStemp2"; break;
                    case "SINFO":
                        funcName = "WDAIIPWEB.getSinfo"; break;
                }

                detail = base.QueryForObject<SignUpDetailModel>(funcName, param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetStudEnterTmp12: " + ex.Message, ex);
                throw ex;
            }

            return detail;
        }

        /// <summary>
        /// 查詢 Stud_EnterTemp3 報名資料維護
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public SignUpDetailModel GetStudEnterTmp3()
        {
            SignUpDetailModel detail = null;
            SessionModel sm = SessionModel.Get();
            Hashtable param = new Hashtable
            {
                ["IDNO"] = sm.ACID,
                ["BIRTHDAY"] = sm.Birthday
            };
            try
            {
                string funcName = "WDAIIPWEB.getStemp3";
                detail = base.QueryForObject<SignUpDetailModel>(funcName, param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetStudEnterTmp3: " + ex.Message, ex);
                throw ex;
            }

            return detail;
        }

        /// <summary>
        /// 登入成功 '採用 Member 資料
        /// (SelectMemberByRidIDNO)
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public SignUpDetailModel GetEMember()
        {
            SessionModel sm = SessionModel.Get();
            SignUpDetailModel detail = null;
            Hashtable param = new Hashtable { ["RID"] = sm.RID, ["ACID"] = sm.ACID };
            try
            {
                string funcName = "WDAIIPWEB.getEMember";

                detail = base.QueryForObject<SignUpDetailModel>(funcName, param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetEMember: " + ex.Message, ex);
                throw ex;
            }
            return detail;
        }

        public SignUpDetailModel GetEMemberTest()
        {
            SessionModel sm = SessionModel.Get();
            SignUpDetailModel detail = null;
            Hashtable param = new Hashtable { ["RID"] = null, ["ACID"] = null };
            try
            {
                string funcName = "WDAIIPWEB.getEMember";

                detail = base.QueryForObject<SignUpDetailModel>(funcName, param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO GetEMember: " + ex.Message, ex);
                throw ex;
            }
            return detail;
        }

        /// <summary> 取得黑名單 </summary>
        /// <param name="IDNO"></param>
        /// <returns></returns>
        public IList<Hashtable> GetBlackList(string IDNO)
        {
            IList<Hashtable> list = null;
            Hashtable param = new Hashtable();
            param["IDNO"] = IDNO;
            try
            {
                string funcName = "WDAIIPWEB.getBlackList";

                list = base.QueryForListAll<Hashtable>(funcName, param);
            }
            catch (Exception ex)
            {
                LOG.Error("GetBlackList: " + ex.Message, ex);
                throw ex;
            }
            return list;
        }

        /// <summary> 取得黑名單 </summary>
        /// <param name="IDNO"></param>
        /// <returns></returns>
        public IList<Hashtable> GetBlackList2(string IDNO, string TPLANID)
        {
            IList<Hashtable> list = null;
            Hashtable param = new Hashtable
            {
                ["IDNO"] = IDNO,
                ["TPLANID"] = TPLANID
            };
            try
            {
                string funcName = "WDAIIPWEB.getBlackList2";
                list = base.QueryForListAll<Hashtable>(funcName, param);
            }
            catch (Exception ex)
            {
                LOG.Error("GetBlackList: " + ex.Message, ex);
                throw ex;
            }
            return list;
        }

        /// <summary>
        /// 取得eSETID
        /// </summary>
        /// <param name="IDNO"></param>
        /// <returns></returns>
        public IList<TblSTUD_ENTERTEMP3> GeteSETID3(string idno)
        {
            try
            {
                TblSTUD_ENTERTEMP3 where = new TblSTUD_ENTERTEMP3 { IDNO = idno };

                return base.GetRowList<TblSTUD_ENTERTEMP3>(where);
            }
            catch (Exception ex)
            {
                LOG.Error($"WDAIIPDAO GeteSETID3: {ex.Message}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 若為新增(CREATE), 儲存成功會回傳自動產生的 MTA_ID, 
        /// 若為異動(UPDATE), 儲存成功則回傳原本的 MTA_ID.
        /// 有異常時直接丟出 Exception
        /// </summary>
        /// <param name="model"></param>
        /// <param name="action">執行此報名資料儲存的作業功能, <see cref="ActionItem"/></param>
        /// <returns></returns>
        public void SaveCaseData(String strXmlName, SignUpDetailModel model, bool transaction)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //TblSTUD_ENTERTEMP3 newTemp3 = new TblSTUD_ENTERTEMP3();
            //TblSTUD_ENTERTEMP3 Detail = new TblSTUD_ENTERTEMP3();
            //model.ZIPCODE_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE, model.ZIPCODE_2W);
            //model.ZIPCODE2_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE2, model.ZIPCODE2_2W);
            //model.ZIPCODE3_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE3, model.ZIPCODE3_2W);
            //LOG.DebugFormat("#ZIPCODE_6W :{0},ZIPCODE:{1}, ZIPCODE_2W:{2}", model.ZIPCODE_6W, model.ZIPCODE, model.ZIPCODE_2W);
            //LOG.DebugFormat("#ZIPCODE2_6W :{0},ZIPCODE2:{1}, ZIPCODE2_2W:{2}", model.ZIPCODE2_6W, model.ZIPCODE2, model.ZIPCODE2_2W);
            //LOG.DebugFormat("#ZIPCODE3_6W :{0},ZIPCODE3:{1}, ZIPCODE3_2W:{2}", model.ZIPCODE3_6W, model.ZIPCODE3, model.ZIPCODE3_2W);
            try
            {
                if (transaction) { base.BeginTransaction(); }

                DateTime today = new MyKeyMapDAO().GetSysDateNow(); //Int64 eSetID3 = -1;
                model.ISEMAIL = ("Y".Equals(model.ISEMAIL) ? "Y" : "N");
                model.ISAGREE = ("Y".Equals(model.ISAGREE) ? "Y" : "N");
                MyCommonUtil.HtmlDecode(model); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）
                //用DB_ACTION判斷Insert或Update
                switch (model.DB_ACTION)
                {
                    case "CREATE":
                        // 新增 stud_entertemp3
                        this.InsertEnterTemp3(model);
                        break;
                    case "UPDATE":
                        // 修改 stud_enter_temp3
                        this.UpdateEnterTemp3(model);
                        break;
                }

                if (transaction) { base.CommitTransaction(); }

                string[] strYN = { "Y", "N" };
                var wStd = new TblSTUD_STUDENTINFO { IDNO = model.IDNO };
                var itemStd = base.GetRow(wStd);
                if (itemStd != null && $"{itemStd.SID}".Length > 1 && !string.IsNullOrEmpty(model.ISAGREE) && strYN.Contains(model.ISAGREE))
                {
                    // 修改 stud_studinfo,ISAGREE
                    this.UpdateStudIsAgree(model);
                }
            }
            catch (Exception ex)
            {
                if (transaction) { base.RollBackTransaction(); }
                LOG.Error($"#SaveCaseData: {ex.Message}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 處理將 SignUpDetailModel 傳入 TblSTUD_ENTERTEMP3 
        /// </summary>
        /// <param name="newTemp3"></param>
        /// <param name="model"></param>
        public void Utl_Model2EnterTemp3(SignUpDetailModel model, TblSTUD_ENTERTEMP3 newTemp3)
        {
            //以下為同時要編輯產投報名資料時才會寫入的欄位(flag IsEditPlan28='Y')
            //newTemp3.MIDENTITYID = model.MIDENTITYID;
            //newTemp3.ZIPCODE2 = model.ZIPCODE2;
            //newTemp3.ZIPCODE2_2W = model.ZIPCODE2_2W;
            //newTemp3.HOUSEHOLDADDRESS = model.HOUSEHOLDADDRESS;
            newTemp3.PRIORWORKPAY = !string.IsNullOrEmpty(model.PRIORWORKPAY) ? Convert.ToInt64(model.PRIORWORKPAY) : (long?)null; //default(int);

            // 郵政/銀行帳號資訊
            newTemp3.ACCTMODE = model.ACCTMODE;
            newTemp3.POSTNO = null;
            newTemp3.ACCTNO = null;
            newTemp3.BANKNAME = null;
            newTemp3.ACCTHEADNO = null;
            newTemp3.ACCTEXNO = null;
            newTemp3.EXBANKNAME = null;

            switch (model.ACCTMODE)
            {
                case 0: //郵局
                    newTemp3.POSTNO = model.POSTNO;
                    newTemp3.ACCTNO = model.POST_ACCTNO;
                    break;
                case 1: //銀行
                    newTemp3.BANKNAME = model.BANKNAME;
                    newTemp3.ACCTHEADNO = model.ACCTHEADNO;
                    newTemp3.ACCTEXNO = model.ACCTEXNO;
                    newTemp3.EXBANKNAME = model.EXBANKNAME;
                    newTemp3.ACCTNO = model.BANK_ACCTNO;
                    break;
                case 2: //訓練單位代轉現金
                    break;
            }

            //newTemp3.UNAME = model.UNAME;
            //newTemp3.INTAXNO = model.INTAXNO;
            //newTemp3.SERVDEPT = (string.IsNullOrEmpty(model.SERVDEPTID) ? model.SERVDEPT : this.getServDeptName(model.SERVDEPTID));
            //newTemp3.JOBTITLE = (string.IsNullOrEmpty(model.JOBTITLEID) ? model.JOBTITLE : this.getJobTitleName(model.JOBTITLEID));
            //newTemp3.ACTNAME = model.ACTNAME;
            //newTemp3.ACTTYPE = model.ACTTYPE;
            //newTemp3.ACTNO = model.ACTNO;
            //newTemp3.ACTTEL = model.ACTTEL;
            ////2019-02-28 修正當沒有郵遞區號資訊則不要塞0
            //newTemp3.ZIPCODE3 = (model.ZIPCODE3 == 0 ? null : model.ZIPCODE3);
            //newTemp3.ZIPCODE3_2W = (model.ZIPCODE3_2W == 0 ? null : model.ZIPCODE3_2W);
            //newTemp3.ACTADDRESS = model.ACTADDRESS;
            //newTemp3.SERVDEPTID = model.SERVDEPTID; //服務單位已改為下拉選項輸入
            //newTemp3.JOBTITLEID = model.JOBTITLEID; //職務已改為下拉選項輸入

            //是否由公司推薦參訓 Q1
            switch (model.Q1)
            {
                case 0:
                case 1:
                    newTemp3.Q1 = model.Q1;
                    break;
                default:
                    newTemp3.Q1 = null;
                    break;
            }

            if (string.IsNullOrEmpty(Convert.ToString(model.Q2_1))) { newTemp3.Q2_1 = null; }
            else { newTemp3.Q2_1 = Convert.ToInt64(model.Q2_1); }
            if (string.IsNullOrEmpty(Convert.ToString(model.Q2_2))) { newTemp3.Q2_2 = null; }
            else { newTemp3.Q2_2 = Convert.ToInt64(model.Q2_2); }
            if (string.IsNullOrEmpty(Convert.ToString(model.Q2_3))) { newTemp3.Q2_3 = null; }
            else { newTemp3.Q2_3 = Convert.ToInt64(model.Q2_3); }
            if (string.IsNullOrEmpty(Convert.ToString(model.Q2_4))) { newTemp3.Q2_4 = null; }
            else { newTemp3.Q2_4 = Convert.ToInt64(model.Q2_4); }

            newTemp3.Q3 = Convert.ToInt64(model.Q3);
            newTemp3.Q3_OTHER = (model.Q3 == 3 ? model.Q3_OTHER : null);
            newTemp3.Q4 = model.Q4;
            newTemp3.Q5 = Convert.ToInt64(model.Q5);
            newTemp3.Q61 = Convert.ToDouble(model.Q61);
            newTemp3.Q62 = Convert.ToDouble(model.Q62);
            newTemp3.Q63 = Convert.ToDouble(model.Q63);
            newTemp3.Q64 = Convert.ToDouble(model.Q64);
            newTemp3.ISEMAIL = ("Y".Equals(model.ISEMAIL) ? "Y" : "N");
            newTemp3.ISAGREE = ("Y".Equals(model.ISAGREE) ? "Y" : "N");
        }

        /// <summary>
        /// 新增產投/在職報名基本資料 stud_entertemp3
        /// </summary>
        /// <param name="model"></param>
        public void InsertEnterTemp3(SignUpDetailModel model)
        {
            TblSTUD_ENTERTEMP3 newTemp3 = new TblSTUD_ENTERTEMP3();
            DateTime today = new MyKeyMapDAO().GetSysDateNow();

            newTemp3.InjectFrom(model);

            //Int64 eSetID3 = new MyKeyMapDAO().GetTableMaxSeqNo(StaticCodeMap.TableName.STUD_ENTERTEMP3, "ESETID3");
            long i_eSetID3 = this.GetNewId("STUD_ENTERTEMP3_ESETID3_SEQ,STUD_ENTERTEMP3,ESETID3").Value;
            model.ESETID3 = i_eSetID3;

            newTemp3.ESETID3 = i_eSetID3;
            newTemp3.IDNO = model.IDNO;
            newTemp3.NAME = model.NAME;
            newTemp3.SEX = ("M".Equals(model.SEX) ? "M" : "F");
            newTemp3.BIRTHDAY = Convert.ToDateTime(model.BIRTHDAY);
            newTemp3.PASSPORTNO = (model.PASSPORTNO == 1 ? 1 : 2);

            // 婚姻狀態
            switch (model.MARITALSTATUS)
            {
                case 1:
                case 2:
                    newTemp3.MARITALSTATUS = model.MARITALSTATUS;
                    break;
                default:
                    newTemp3.MARITALSTATUS = null;
                    break;
            }

            newTemp3.DEGREEID = model.DEGREEID;
            newTemp3.GRADID = (string.IsNullOrEmpty(model.GRADID) ? "01" : model.GRADID);
            newTemp3.SCHOOL = !string.IsNullOrEmpty(model.SCHOOLNAME) ? model.SCHOOLNAME : newTemp3.SCHOOL;
            newTemp3.DEPARTMENT = !string.IsNullOrEmpty(model.DEPARTMENT) ? model.DEPARTMENT : newTemp3.DEPARTMENT;
            //newTemp3.MILITARYID = model.MILITARYID; //兵役已不提供填寫
            //if (model.ZIPCODE.HasValue && model.ZIPCODE.Value.ToString().Length > 3) { model.ZIPCODE = Convert.ToInt64(model.ZIPCODE.Value.ToString().Substring(0, 3)); }
            //newTemp3.ZIPCODE1 = model.ZIPCODE;
            //newTemp3.ZIPCODE1_6W = model.ZIPCODE_6W;
            newTemp3.ZIPCODE1 = model.ZIPCODE;
            newTemp3.ZIPCODE1_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE, model.ZIPCODE_2W);
            newTemp3.ADDRESS = model.ADDRESS;
            newTemp3.PHONE1 = model.PHONED;
            newTemp3.PHONE2 = model.PHONEN;
            newTemp3.CELLPHONE = model.CELLPHONE;
            newTemp3.EMAIL = model.EMAIL;

            newTemp3.MIDENTITYID = model.MIDENTITYID;

            //if (model.ZIPCODE2.HasValue && model.ZIPCODE2.Value.ToString().Length > 3) { model.ZIPCODE2 = Convert.ToInt64(model.ZIPCODE2.Value.ToString().Substring(0, 3)); }
            newTemp3.ZIPCODE2 = model.ZIPCODE2;
            newTemp3.ZIPCODE2_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE2, model.ZIPCODE2_2W);
            newTemp3.HOUSEHOLDADDRESS = model.HOUSEHOLDADDRESS;

            newTemp3.UNAME = model.UNAME;
            newTemp3.INTAXNO = model.INTAXNO;
            newTemp3.SERVDEPT = (string.IsNullOrEmpty(model.SERVDEPTID) ? model.SERVDEPT : this.getServDeptName(model.SERVDEPTID));
            newTemp3.JOBTITLE = (string.IsNullOrEmpty(model.JOBTITLEID) ? model.JOBTITLE : this.getJobTitleName(model.JOBTITLEID));
            newTemp3.ACTNAME = model.ACTNAME;
            newTemp3.ACTTYPE = model.ACTTYPE;
            newTemp3.ACTNO = model.ACTNO;
            newTemp3.ACTTEL = model.ACTTEL;
            //2019-02-28 修正當沒有郵遞區號資訊則不要塞0
            //if (model.ZIPCODE3.HasValue && model.ZIPCODE3.Value.ToString().Length > 3) { model.ZIPCODE3 = Convert.ToInt64(model.ZIPCODE3.Value.ToString().Substring(0, 3)); }
            //newTemp3.ZIPCODE3 = (model.ZIPCODE3 == 0 ? null : model.ZIPCODE3);
            //newTemp3.ZIPCODE3_2W = (model.ZIPCODE3_2W == 0 ? null : model.ZIPCODE3_2W);
            newTemp3.ZIPCODE3 = model.ZIPCODE3;
            newTemp3.ZIPCODE3_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE3, model.ZIPCODE3_2W);
            newTemp3.ACTADDRESS = model.ACTADDRESS;

            newTemp3.SERVDEPTID = model.SERVDEPTID; //服務單位已改為下拉選項輸入
            newTemp3.JOBTITLEID = model.JOBTITLEID; //職務已改為下拉選項輸入

            //判斷是否為產投資訊
            bool flag_ISPLAN28 = false;
            if ("Y".Equals(model.ISPLAN28)) { flag_ISPLAN28 = true; }
            newTemp3.ISPLAN28 = "N";
            if (flag_ISPLAN28)
            {
                newTemp3.ISPLAN28 = model.ISPLAN28;
                //以下為同時要編輯產投報名資料時才會寫入的欄位(flag IsEditPlan28='Y')
                Utl_Model2EnterTemp3(model, newTemp3);
            }
            else
            {
                //在職進修的參訓身份別「預設成一般身份別(01)」
                if (string.IsNullOrEmpty(newTemp3.MIDENTITYID)) { newTemp3.MIDENTITYID = "01"; }
            }

            newTemp3.MODIFYACCT = model.IDNO;
            newTemp3.MODIFYDATE = today;

            base.Insert<TblSTUD_ENTERTEMP3>(newTemp3);
        }

        /// <summary>
        /// 異動產投/在職報名基本資料 stud_entertemp3
        /// </summary>
        /// <param name="model"></param>
        public void UpdateEnterTemp3(SignUpDetailModel model)
        {
            TblSTUD_ENTERTEMP3 newTemp3 = new TblSTUD_ENTERTEMP3();
            Int64 eSetID3 = -1;

            IList<TblSTUD_ENTERTEMP3> list = this.GeteSETID3(model.IDNO);
            if (list != null && list.Count > 0) eSetID3 = Convert.ToInt64(list[0].ESETID3);

            TblSTUD_ENTERTEMP3 whereConds = new TblSTUD_ENTERTEMP3 { ESETID3 = eSetID3 };
            TblSTUD_ENTERTEMP3 oldTemp3 = base.GetRow(whereConds);

            DateTime today = new MyKeyMapDAO().GetSysDateNow();

            newTemp3.InjectFrom(oldTemp3);
            //newTemp3.InjectFrom(model);

            //newTemp3.IDNO = model.IDNO;
            newTemp3.NAME = model.NAME;
            newTemp3.SEX = ("M".Equals(model.SEX) ? "M" : "F");
            newTemp3.BIRTHDAY = Convert.ToDateTime(model.BIRTHDAY);
            newTemp3.PASSPORTNO = (model.PASSPORTNO == 1 ? 1 : 2);

            // 婚姻狀態
            switch (model.MARITALSTATUS)
            {
                case 1:
                case 2:
                    newTemp3.MARITALSTATUS = model.MARITALSTATUS;
                    break;
                default:
                    newTemp3.MARITALSTATUS = null;
                    break;
            }

            newTemp3.DEGREEID = model.DEGREEID;
            newTemp3.GRADID = (string.IsNullOrEmpty(model.GRADID) ? "01" : model.GRADID);
            newTemp3.SCHOOL = !string.IsNullOrEmpty(model.SCHOOLNAME) ? model.SCHOOLNAME : newTemp3.SCHOOL;
            newTemp3.DEPARTMENT = !string.IsNullOrEmpty(model.DEPARTMENT) ? model.DEPARTMENT : newTemp3.DEPARTMENT;
            //newTemp3.MILITARYID = model.MILITARYID; //兵役已不提供填寫
            //if (model.ZIPCODE.HasValue && model.ZIPCODE.Value.ToString().Length > 3) { model.ZIPCODE = Convert.ToInt64(model.ZIPCODE.Value.ToString().Substring(0, 3)); }
            newTemp3.ZIPCODE1 = model.ZIPCODE;
            newTemp3.ZIPCODE1_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE, model.ZIPCODE_2W);// model.ZIPCODE_6W;
            newTemp3.ADDRESS = model.ADDRESS;

            newTemp3.PHONE1 = model.PHONED;
            newTemp3.PHONE2 = model.PHONEN;
            newTemp3.CELLPHONE = model.CELLPHONE;
            newTemp3.EMAIL = model.EMAIL;

            newTemp3.MIDENTITYID = model.MIDENTITYID;
            //if (model.ZIPCODE2.HasValue && model.ZIPCODE2.Value.ToString().Length > 3) { model.ZIPCODE2 = Convert.ToInt64(model.ZIPCODE2.Value.ToString().Substring(0, 3)); }
            newTemp3.ZIPCODE2 = model.ZIPCODE2;
            newTemp3.ZIPCODE2_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE2, model.ZIPCODE2_2W);
            newTemp3.HOUSEHOLDADDRESS = model.HOUSEHOLDADDRESS;

            newTemp3.UNAME = model.UNAME;
            newTemp3.INTAXNO = model.INTAXNO;
            newTemp3.SERVDEPT = (string.IsNullOrEmpty(model.SERVDEPTID) ? model.SERVDEPT : this.getServDeptName(model.SERVDEPTID));
            newTemp3.JOBTITLE = (string.IsNullOrEmpty(model.JOBTITLEID) ? model.JOBTITLE : this.getJobTitleName(model.JOBTITLEID));
            newTemp3.ACTNAME = model.ACTNAME;
            newTemp3.ACTTYPE = model.ACTTYPE;
            newTemp3.ACTNO = model.ACTNO;
            newTemp3.ACTTEL = model.ACTTEL;
            //2019-02-28 修正當沒有郵遞區號資訊則不要塞0
            //if (model.ZIPCODE3.HasValue && model.ZIPCODE3.Value.ToString().Length > 3) { model.ZIPCODE3 = Convert.ToInt64(model.ZIPCODE3.Value.ToString().Substring(0, 3)); }
            //newTemp3.ZIPCODE3_2W = (model.ZIPCODE3_2W == 0 ? null : model.ZIPCODE3_2W);
            //newTemp3.ZIPCODE3 = (model.ZIPCODE3 == 0 ? null : model.ZIPCODE3); newTemp3.ZIPCODE3_6W = model.ZIPCODE3_6W;
            newTemp3.ZIPCODE3 = model.ZIPCODE3;
            newTemp3.ZIPCODE3_6W = MyCommonUtil.GET_ZIPCODE6W(model.ZIPCODE3, model.ZIPCODE3_2W);
            newTemp3.ACTADDRESS = model.ACTADDRESS;

            newTemp3.SERVDEPTID = model.SERVDEPTID; //服務單位已改為下拉選項輸入
            newTemp3.JOBTITLEID = model.JOBTITLEID; //職務已改為下拉選項輸入

            //判斷是否為產投資訊
            bool flag_ISPLAN28 = false;
            if (string.IsNullOrEmpty(model.ISPLAN28) || "Y".Equals(model.ISPLAN28)) { flag_ISPLAN28 = true; }
            newTemp3.ISPLAN28 = "N";
            if (flag_ISPLAN28)
            {
                newTemp3.ISPLAN28 = "Y";
                /*以下為同時要編輯產投報名資料時才會寫入的欄位(flag IsEditPlan28='Y')*/
                Utl_Model2EnterTemp3(model, newTemp3);
            }
            else
            {
                //在職進修的參訓身份別「預設成一般身份別(01)」
                if (string.IsNullOrEmpty(newTemp3.MIDENTITYID)) { newTemp3.MIDENTITYID = "01"; }
            }

            newTemp3.MODIFYACCT = model.IDNO;
            newTemp3.MODIFYDATE = today;

            // 移除不可被清空的欄位
            ClearFieldMap cfm = newTemp3.GetClearFieldMap();
            cfm.Remove((TblSTUD_ENTERTEMP3 x) => x.ESETID3);

            base.Update<TblSTUD_ENTERTEMP3>(newTemp3, oldTemp3, whereConds, cfm);
        }

        /// <summary>
        /// 異動學員資料檔isagree
        /// </summary>
        /// <param name="model"></param>
        public void UpdateStudIsAgree(SignUpDetailModel model)
        {
            //TblSTUD_STUDENTINFO whereCond = new TblSTUD_STUDENTINFO { IDNO = model.IDNO }; //newStud.MODIFYDATE = DateTime.Now; //newStud.MODIFYACCT = model.IDNO; base.Update(newStud, whereCond);
            Hashtable parmsU1 = new Hashtable { { "IDNO", model.IDNO }, { "ISAGREE", model.ISAGREE } };
            base.Update("WDAIIPWEB.UpdateStudIsAgree", parmsU1);

        }
        #endregion

        #region 課程錄訓名單/產業人才投資方案
        /// <summary>
        /// 錄訓名單/產業人才投資方案-查詢
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IList<ClassConfirm001GridModel> QueryClassConfirm001(ClassConfirm001FormModel from)
        {
            IList<ClassConfirm001GridModel> rtn = null;

            try
            {
                rtn = base.QueryForList<ClassConfirm001GridModel>("WDAIIPWEB.queryClassConfirm001", from);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm001() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 查詢人員錄訓名單-單位課程資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ClassConfirm001DetailModel GetClassConfirm001Detail(ClassConfirm0012FormModel form)
        {
            ClassConfirm001DetailModel rtn = new ClassConfirm001DetailModel();
            IList<ClassConfirm001GridModel> userlist = null;

            try
            {
                userlist = base.QueryForListAll<ClassConfirm001GridModel>("WDAIIPWEB.queryClassConfirm001Detail", form);

                if (userlist != null && userlist.Count > 0)
                {
                    var item = userlist[0];
                    rtn.OCID = item.OCID;
                    rtn.ORGNAME = item.ORGNAME;
                    rtn.CONFIRDATE = item.CONFIRDATE;
                    rtn.CLASSCNAME = item.CLASSCNAME;
                    rtn.CONFIRDATERANGE = item.CONFIRDATERANGE; //2018-12-25 add 公告期間
                    rtn.ODNUMBER = item.ODNUMBER; //2018-12-25 add 文號
                    rtn.CFORGNAME = item.CFORGNAME; //2018-12-25 add 發布單位
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm001Detail() error:" + ex.Message, ex);
                throw ex;
            }

            return rtn;
        }

        /// <summary>
        /// 錄訓名單/產業人才投資方案-Detail查詢
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IList<ClassConfirm0012GridModel> QueryClassConfirm0012(ClassConfirm0012FormModel form)
        {
            IList<ClassConfirm0012GridModel> rtn = null;

            try
            {
                rtn = base.QueryForListAll<ClassConfirm0012GridModel>("WDAIIPWEB.queryClassConfirm0012", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm0012() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }
        #endregion

        #region 課程錄訓名單/自辦在職進修
        /// <summary>
        /// 錄訓名單/自辦在職進修-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassConfirm002GridModel> QueryTrainClassListEnroll(ClassConfirm002FormModel form)
        {
            IList<ClassConfirm002GridModel> rtn = null;
            try
            {
                rtn = base.QueryForList<ClassConfirm002GridModel>("WDAIIPWEB.queryTrainClassListEnroll", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryTrainClassListEnroll() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }


        /// <summary>
        /// 查詢人員錄訓名單-單位課程資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ClassConfirm002DetailModel GetQueryTrainClassUintEnroll(ClassConfirm002ClassFormModel form)
        {
            ClassConfirm002DetailModel rtn = new ClassConfirm002DetailModel();
            IList<ClassConfirm002GridModel> userlist = null;
            try
            {
                userlist = base.QueryForListAll<ClassConfirm002GridModel>("WDAIIPWEB.queryTrainClassUintEnroll", form);
                if (userlist != null && userlist.Count > 0)
                {
                    var item = userlist[0];

                    rtn.OCID = item.OCID;
                    rtn.TRAINORG = item.TRAINORG;
                    rtn.CONFIRDATE = item.CONFIRDATE;
                    rtn.TRAINCLASS = item.TRAINCLASS;
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryTrainClassUintEnroll() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 錄訓名單/自辦在職進修-Detail查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassConfirm002StudGridModel> QueryTrainClassListEnrollStud(ClassConfirm002ClassFormModel form)
        {
            IList<ClassConfirm002StudGridModel> rtn = null;

            try
            {
                rtn = base.QueryForListAll<ClassConfirm002StudGridModel>("WDAIIPWEB.queryTrainClassListEnrollStud", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryTrainClassListEnrollStud() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }
        #endregion

        #region 課程錄訓名單/區域產業據點
        /// <summary>
        /// 錄訓名單/查詢-區域產業據點
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassConfirm005GridModel> QueryTrainClassListEnroll(ClassConfirm005FormModel form)
        {
            IList<ClassConfirm005GridModel> rtn = null;
            try
            {
                rtn = base.QueryForList<ClassConfirm005GridModel>("WDAIIPWEB.queryTrainClassListEnroll70", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryTrainClassListEnroll70() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }


        /// <summary>
        /// 查詢人員錄訓名單/單位課程資料-區域產業據點
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ClassConfirm005DetailModel GetQueryTrainClassUintEnroll(ClassConfirm005ClassFormModel form)
        {
            ClassConfirm005DetailModel rtn = new ClassConfirm005DetailModel();

            try
            {
                IList<ClassConfirm005GridModel> userlist = null;

                userlist = base.QueryForListAll<ClassConfirm005GridModel>("WDAIIPWEB.queryTrainClassUintEnroll70", form);

                if (userlist != null && userlist.Count > 0)
                {
                    ClassConfirm005GridModel item = userlist[0];
                    rtn.OCID = item.OCID;
                    rtn.TRAINORG = item.TRAINORG;
                    rtn.CONFIRDATE = item.CONFIRDATE;
                    rtn.TRAINCLASS = item.TRAINCLASS;
                    rtn.CFORGNAME = item.CFORGNAME;
                    rtn.ODNUMBER = item.ODNUMBER;
                    rtn.ODNUMBER2 = item.ODNUMBER2;
                    rtn.CONFIRDATERANGE = item.CONFIRDATERANGE;
                    rtn.ANNMENTDATE = item.ANNMENTDATE;
                    rtn.ANNMENTDATERANGE = item.ANNMENTDATERANGE;
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryTrainClassUintEnroll70() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 錄訓名單/Detail查詢-區域產業據點
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassConfirm005StudGridModel> QueryTrainClassListEnrollStud(ClassConfirm005ClassFormModel form)
        {
            IList<ClassConfirm005StudGridModel> rtn = null;

            try
            {
                rtn = base.QueryForListAll<ClassConfirm005StudGridModel>("WDAIIPWEB.queryTrainClassListEnrollStud70", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEB.queryTrainClassListEnrollStud70() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }
        #endregion

        #region 課程錄訓名單/充電起飛
        /// <summary>
        /// 分署清單查詢
        /// </summary>
        /// <returns></returns>
        public IList<ClassConfirm003DistNameGridModel> QueryClassConfirm003DistName()
        {
            IList<ClassConfirm003DistNameGridModel> rtn = null;
            try
            {
                //不分頁所以用"QueryForListAll"
                rtn = base.QueryForListAll<ClassConfirm003DistNameGridModel>("WDAIIPWEB.queryClassConfirm003DistName", null);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm003DistName() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 課程清單查詢
        /// </summary>
        public IList<ClassConfirm003GridModel> QueryClassConfirm003Class(ClassConfirm003FormModel form)
        {
            IList<ClassConfirm003GridModel> rtn = null;
            try
            {
                //要分頁所以用"QueryForList"
                rtn = base.QueryForList<ClassConfirm003GridModel>("WDAIIPWEB.queryClassConfirm003Class", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm003Class() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 單位課程查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassConfirm003UnitGridModel> GetQueryClassConfirm003ClassUnit(ClassConfirm003StudFormModel form)
        {
            IList<ClassConfirm003UnitGridModel> rtn = null;
            try
            {
                //要分頁所以用"QueryForList"
                rtn = base.QueryForList<ClassConfirm003UnitGridModel>("WDAIIPWEB.queryClassConfirm003ClassUnit", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm003ClassUnit() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 學生清單查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassConfirm003StudGridModel> QueryClassConfirm003Stud(ClassConfirm003StudFormModel form)
        {
            IList<ClassConfirm003StudGridModel> rtn = null;
            try
            {
                //不分頁所以用"QueryForListAll"
                rtn = base.QueryForListAll<ClassConfirm003StudGridModel>("WDAIIPWEB.queryClassConfirm003Stud", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryClassConfirm003Stud() error:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }
        #endregion

        #region 相關連結

        /// <summary>
        /// 相關連結-查詢
        /// </summary>
        /// <returns></returns>
        public IList<LinkGridModel> QueryLink(LinkFormModel form)
        {
            IList<LinkGridModel> result = null;

            try
            {
                result = base.QueryForListAll<LinkGridModel>("WDAIIPWEB.queryLink", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryLink() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }


        /// <summary>
        /// 相關連結-查詢2
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<LinkGridModel> QueryLink2(LinkFormModel form)
        {
            IList<LinkGridModel> result = null;

            try
            {
                result = base.QueryForListAll<LinkGridModel>("WDAIIPWEB.queryLink2", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryLink2() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        #endregion

        #region 資料下載

        /// <summary>查詢[資料下載]計畫別清單(啟用中)</summary>
        /// <returns></returns>
        public IList<DownloadTypeGridModel> QueryDownloadPlan()
        {
            return base.QueryForListAll<DownloadTypeGridModel>("WDAIIPWEB.queryDownloadPlan", null);
        }

        /// <summary>資料下載-查詢</summary>
        /// <returns></returns>
        public IList<DownloadGridModel> QueryDownload(DownloadFormModel form)
        {
            IList<DownloadGridModel> result = null;

            try
            {
                result = base.QueryForList<DownloadGridModel>("WDAIIPWEB.queryDownload", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryDownload() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 以PK欄位(DLID)為條件,
        /// 取得 TblTB_DLFILE 資料
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public DownloadDetailModel GetDownload(TblTB_DLFILE parms)
        {
            TblTB_DLFILE file = (TblTB_DLFILE)base.QueryForObject("WDAIIPWEB.getDownload", parms);
            DownloadDetailModel detail = new DownloadDetailModel();
            detail.InjectFrom(file);

            return detail;
        }

        /// <summary>
        /// 資料下載-更新點擊次數
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public void UpdateDownloadHits(DownloadDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblTB_DLFILE data = new TblTB_DLFILE();
            TblTB_DLFILE where = null;

            try
            {
                data.InjectFrom(detail);
                data.DLCOUNT = detail.DLCOUNT;

                where = new TblTB_DLFILE { DLID = detail.DLID };

                base.Update(data, where, where);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateDownloadHits failed: " + ex.Message);
                throw new Exception("UpdateDownloadHits failed: " + ex.Message, ex);
            }
        }

        #endregion

        #region 會員專區/參訓歷程記錄

        /// <summary>
        /// 會員專區/參訓歷程記錄-查詢
        /// </summary>
        /// <returns></returns>
        public IList<TrainingHistoryGridModel> QueryTrainingHistory(TrainingHistoryFormModel form)
        {
            IList<TrainingHistoryGridModel> result = null;

            if (string.IsNullOrEmpty(form.IDNO) || string.IsNullOrEmpty(form.BIRTHDAY_STR)) { return result; }
            try
            {
                result = base.QueryForListAll<TrainingHistoryGridModel>("WDAIIPWEB.queryTrainingHistory", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryTrainingHistory() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 會員專區/參訓歷程記錄-查詢(不含產投課程)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<TrainingHistoryGridModel> QueryTrainingHistory06(TrainingHistoryFormModel form)
        {
            IList<TrainingHistoryGridModel> result = null;

            try
            {
                result = base.QueryForListAll<TrainingHistoryGridModel>("WDAIIPWEB.queryTrainingHistory06", form);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryTrainingHistory06() error:" + ex.Message, ex);
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 查詢取得學員舊版上課資料（from stdall）
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<TblSTDALL> QueryStdAll(string idno)
        {
            TblSTDALL whereCond = new TblSTDALL { SID = idno };

            return base.GetRowList<TblSTDALL>(whereCond);
        }

        /// <summary>
        /// 查詢取得學員舊版上課資料(from history_studenntinfo93)
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<HistoryStudentInfo93GridModel> QueryHistoryStudentInfo93(string idno)
        {
            HistoryStudentInfo93GridModel whereCond = new HistoryStudentInfo93GridModel { IDNO = idno };

            return base.GetRowList<HistoryStudentInfo93GridModel>(whereCond);
        }

        /// <summary>
        /// 查詢學員上課資料(from class_studentsofclass)
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<HistoryClassStudsOfClassGridModel> QueryHistoryClassStudsOfClass(string idno)
        {
            string funcName = "WDAIIPWEB.queryHisClassStudentOfClass";
            HistoryClassStudsOfClassGridModel whereCond = new HistoryClassStudsOfClassGridModel { IDNO = idno };

            return base.QueryForListAll<HistoryClassStudsOfClassGridModel>(funcName, whereCond);
        }

        /// <summary>
        /// 查詢學員上課資料(from class_studentsofclassdeldata)
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<HistoryClassStudsOfClassGridModel> QueryHistoryClassStudsOfClassDelData(string idno)
        {
            string funcName = "WDAIIPWEB.queryHisClassStudentOfClassDelData";
            HistoryClassStudsOfClassGridModel whereCond = new HistoryClassStudsOfClassGridModel { IDNO = idno };

            return base.QueryForListAll<HistoryClassStudsOfClassGridModel>(funcName, whereCond);
        }
        #endregion

        #region 速配課程設定
        /// <summary>
        /// 查詢課程速配結果
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<T> QueryClassMatch<T>(ClassMatchFormModel form)
        {
            IList<T> rtn = new List<T>();
            string funcName = string.Empty;

            switch (form.PlanType)
            {
                case "1": //產投
                    funcName = "WDAIIPWEB.queryClassMatch_1";
                    break;
                case "2": //自辦在職
                    funcName = "WDAIIPWEB.queryClassMatch_2";
                    break;
                case "5": //區域產業據點
                    funcName = "WDAIIPWEB.queryClassMatch_5";
                    break;
            }

            return base.QueryForList<T>(funcName, form);
        }
        #endregion

        #region 會員專區/補助額度使用情形

        /// <summary>
        /// 計算補助歷史
        /// </summary>
        public const string cst_TPlanID28_1a = "'28','54'";//,'58'

        /// <summary>
        /// 計算補助歷史 cst_TPlanID28_1b1b Cst_TPlanID28_1b: 
        /// 從2010/04/01開始算 且為在職者  
        /// and c.STDate>='2010/04/01'
        /// and b.WorkSuppIdent='Y'
        /// </summary>
        public const string cst_TPlanID28_1b1b = "'46','47','69','58'";
        //public const string cstTPlanID28_1B = "'46','47'";

        /// <summary>
        /// 已實際請領補助費'已報名申請補助費
        /// </summary>
        public const string cstTPlanID28_1C = "'28','46','47','54','58'";

        /// <summary>
        /// 線上報名預算的政府補助
        /// </summary>
        public const string cstTPlanID28_1D = "'28','46','47','54','58'";

        /// <summary>
        /// 依身分證與出生日期查詢線上報名資料
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <returns></returns>
        public IList<Hashtable> QueryStudEnterType2(string idno, string birth)
        {
            IList<Hashtable> result = null;
            Hashtable param = new Hashtable();
            param["IDNO"] = idno;
            param["BIRTHDAY"] = birth;
            try
            {
                result = base.QueryForListAll<Hashtable>("WDAIIPWEB.queryStudEnterType2", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryStudEnterType2() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 補助金申請查詢
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <returns></returns>
        public IList<DefStdCostGridModel> QueryDefStdCost(string idno, string birth)
        {
            IList<DefStdCostGridModel> result = null;
            Hashtable param = new Hashtable();
            param["IDNO"] = idno;
            //param["BIRTHDAY"] = birth;
            //param["TPLANID28_1A"] = cstTPlanID28_1A;
            //param["TPLANID28_1B"] = cstTPlanID28_1B;

            try
            {
                result = base.QueryForListAll<DefStdCostGridModel>("WDAIIPWEB.queryDefStdCost", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryHistoryDefStdCost() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// (近N年) 取得該學員報名資料
        /// ref TIMS.GetRelEnterDateDtY3
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<NearYearTrainGridModel> QueryNearYearTrain(string idno, string birth, string sDate = "", string eDate = "")
        {
            IList<NearYearTrainGridModel> result = null;
            Hashtable param = new Hashtable();
            param["IDNO"] = idno;
            param["BIRTHDAY"] = birth;
            param["TPLANID28_1D"] = cstTPlanID28_1D;
            string specConds = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sDate) || string.IsNullOrEmpty(eDate))
                {
                    specConds = "AND cc.STDATE >= cast(getdate()-(365*3) as date) ";
                }
                else
                {
                    specConds = "AND cc.STDate >= cast(" + sDate + " as date) ";
                    specConds += "AND cc.STDate <= cast( " + eDate + "  as date) ";
                }

                param["STDATE_COND"] = specConds;

                result = base.QueryForListAll<NearYearTrainGridModel>("WDAIIPWEB.queryNearYearTrain", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryNearYearTrain() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢取得要比對的課程資料
        /// </summary>
        /// <param name="ocids"></param>
        /// <returns></returns>
        public IList<TrainDescGridModel> QueryTrainDesc(string ocids)
        {
            IList<TrainDescGridModel> result = null;
            Hashtable param = new Hashtable();
            param["OCIDS"] = ocids;

            try
            {
                result = base.QueryForListAll<TrainDescGridModel>("WDAIIPWEB.queryTrainDesc", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO queryTrainDesc() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢取得政府負擔補助費用資訊
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public IList<GovCostGridModel> QueryGovCost(Int64 ocid)
        {
            IList<GovCostGridModel> result = null;
            Hashtable param = new Hashtable();
            param["OCID"] = ocid;

            try
            {
                result = base.QueryForListAll<GovCostGridModel>("WDAIIPWEB.queryGovCost", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryGovCost() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢審核通過的三年區間起迄日期
        /// 先試著取審核通過的三年區間
        /// 先試著取參訓過的三年區間
        /// Get_YearsPeriod
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<YearsPeriodGridModel> QueryYearsPeriod(string idno, string edate)
        {
            IList<YearsPeriodGridModel> result = null;
            Hashtable param = new Hashtable();
            param["IDNO"] = idno;
            //param["TPLANID28_1A"] = cstTPlanID28_1A;
            //param["TPLANID28_1B"] = cstTPlanID28_1B;
            //param["STDATE_V1"] = cstSTDate_V1;
            param["EDATE"] = edate;

            //const string cstSTDate_V1 = "2010/04/01";
            try
            {
                result = base.QueryForListAll<YearsPeriodGridModel>("WDAIIPWEB.queryYearsPeriod", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryGovCost() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢(本期) 已實際請領補助費(限定產業人才投資方案)
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<SubsidyCost28GridModel> QueryActSubsidyCost28(string idno, string sDate, string eDate)
        {
            IList<SubsidyCost28GridModel> result = null;
            Hashtable param = new Hashtable();
            param["TPLANID28_1C"] = cstTPlanID28_1C;
            param["IDNO"] = idno;
            param["SDATE"] = sDate;
            param["EDATE"] = eDate;

            try
            {

                result = base.QueryForListAll<SubsidyCost28GridModel>("WDAIIPWEB.QueryActSubsidyCost", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryActSubsidyCost28() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 參訓中課程預估補助費用(尚未請領補助的):只要是學生在訓，就計算，未申請也加入
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<SubsidyCost28GridModel> QuerySignSubsidyCost28(string idno, string sDate, string eDate)
        {
            IList<SubsidyCost28GridModel> result = null;
            Hashtable param = new Hashtable();
            param["TPLANID28_1C"] = cstTPlanID28_1C;
            param["IDNO"] = idno;
            param["SDATE"] = sDate;
            param["EDATE"] = eDate;

            try
            {
                result = base.QueryForListAll<SubsidyCost28GridModel>("WDAIIPWEB.QuerySignSubsidyCost", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QuerySignSubsidyCost28() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢參訓學員
        /// 排除 離退訓班級(不要計算)
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<Hashtable> QueryStudStatus23(string idno, string sDate, string eDate)
        {
            IList<Hashtable> result = null;
            Hashtable param = new Hashtable();
            param["IDNO"] = idno;
            param["SDATE"] = sDate;
            param["EDATE"] = eDate;

            try
            {
                result = base.QueryForListAll<Hashtable>("WDAIIPWEB.queryStudStatus23", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryStudStatus23() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢未成為學員者
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<Hashtable> QuerySTDateAdd15DayA(string idno, string sDate, string eDate)
        {
            IList<Hashtable> result = null;
            Hashtable param = new Hashtable();
            param["TPLANID28_1D"] = cstTPlanID28_1D;
            param["IDNO"] = idno;
            param["SDATE"] = sDate;
            param["EDATE"] = eDate;

            try
            {
                result = base.QueryForListAll<Hashtable>("WDAIIPWEB.querySTDateAdd15DayA", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QuerySTDateAdd15DayA() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢(錄取作業) 審核成功
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<Hashtable> QuerySTDateAdd15DayB(string idno, string sDate, string eDate)
        {
            IList<Hashtable> result = null;
            Hashtable param = new Hashtable();
            param["TPLANID28_1D"] = cstTPlanID28_1D;
            param["IDNO"] = idno;
            param["SDATE"] = sDate;
            param["EDATE"] = eDate;

            try
            {
                result = base.QueryForListAll<Hashtable>("WDAIIPWEB.querySTDateAdd15DayB", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QuerySTDateAdd15DayB() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// (計算範圍)排除 其他(2:報名失敗 5:未錄取) 資訊
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public IList<SubsidyCost28GridModel> QueryDefGovCost28(string idno, string sDate, string eDate)
        {
            IList<SubsidyCost28GridModel> result = null;
            Hashtable param = new Hashtable();
            param["TPLANID28_1D"] = cstTPlanID28_1D;
            param["IDNO"] = idno;
            param["SDATE"] = sDate;
            param["EDATE"] = eDate;

            try
            {
                result = base.QueryForListAll<SubsidyCost28GridModel>("WDAIIPWEB.queryDefGovCost28", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryDefGovCost28() error:" + ex.Message, ex);
                throw ex;
            }
            return result;
        }
        #endregion

        #region 會員專區/會員課程收藏清單
        /// <summary>
        /// 會員收藏課程清單-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassTraceGridModel> QueryClassTrace(ClassTraceFormModel form)
        {
            return base.QueryForList<ClassTraceGridModel>("WDAIIPWEB.queryClassTrace", form);
        }

        /// <summary>
        /// 更新課程收藏
        /// </summary>
        /// <param name="param"></param>
        /// <param name="modtype">D:刪除、S:分享/取消分享</param>
        public void UpdateClassTrace(string modtype, TblE_CLSTRACE param)
        {
            base.BeginTransaction();
            try
            {
                //維護E_CLSTRACE
                TblE_CLSTRACE eclstrace_pk = new TblE_CLSTRACE { TRC_SN = param.TRC_SN };
                TblE_CLSTRACE eclstrace_param = new TblE_CLSTRACE();

                if (modtype == "D")
                {
                    eclstrace_param.ISDELETE = "Y";//ISDELETE狀態改為Y
                }
                else
                {
                    eclstrace_param.ISSHARE = param.ISSHARE;
                }
                base.Update<TblE_CLSTRACE>(eclstrace_param, eclstrace_pk);

                if (modtype == "S")
                {
                    //維護CLASS_CLASSINFO SHARE_COUNT
                    var eclstrace_share = new TblE_CLSTRACE
                    {
                        TRC_OCID = param.TRC_OCID,
                        ISSHARE = "Y"
                    };
                    var eclstrace_count = base.GetRowList<TblE_CLSTRACE>(eclstrace_share);

                    var classinfo_pk = new TblCLASS_CLASSINFO
                    {
                        OCID = param.TRC_OCID
                    };
                    var classinfo_param = new TblCLASS_CLASSINFO
                    {
                        SHARE_COUNT = eclstrace_count.Count
                    };
                    base.Update<TblCLASS_CLASSINFO>(classinfo_param, classinfo_pk);
                }

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("WDAIIPWEBDAO UpdateClassTrace() error:" + ex.Message, ex);
                throw ex;
            }
        }
        #endregion

        #region 會員專區/速配課程設定
        /// <summary>
        /// 查詢通俗職類第二層選項
        /// </summary>
        /// <param name="cjobType"></param>
        /// <returns></returns>
        public IList<Hashtable> GetCJobNoItemLv2List()
        {
            string funcName = "WDAIIPWEB.queryCJobNoItemLv2";
            return base.QueryForListAll<Hashtable>(funcName, null);
        }

        /// <summary>
        /// 查詢通俗職類第三層選項
        /// </summary>
        /// <param name="cjobNo"></param>
        /// <returns></returns>
        public IList<Hashtable> GetCJobNoItemLv3List()
        {
            string funcName = "WDAIIPWEB.queryCJobNoItemLv3";
            return base.QueryForListAll<Hashtable>(funcName, null);
        }

        /// <summary>
        /// 查詢訓練業別第二層選項
        /// </summary>
        /// <returns></returns>
        public IList<Hashtable> GetTMIDItemLv2List()
        {
            string funcName = "WDAIIPWEB.queryTMIDItemLv2";
            return base.QueryForListAll<Hashtable>(funcName, null);
        }

        /// <summary>
        /// 查詢會員速配課程資料
        /// </summary>
        /// <param name="memsn"></param>
        /// <returns></returns>
        public TblTB_MEMSEARCH GetMemSearch(decimal? memsn)
        {
            return base.GetRow<TblTB_MEMSEARCH>(new TblTB_MEMSEARCH { MEM_SN = memsn.Value });
        }

        /// <summary>
        /// 新增會員速配課程資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sm"></param>
        public void InsertMemSearch(ClassMatchFormModel model, SessionModel sm)
        {
            TblTB_MEMSEARCH data = new TblTB_MEMSEARCH();

            try
            {
                data.MEM_SN = sm.MemSN;
                data.CREATEDATE = DateTime.Now;

                switch (model.PlanType) //計畫別
                {
                    case "1": //產投
                        data.TMID = model.TMIDRESULT;
                        data.CTID = model.CTID;
                        data.SENDMAIL28 = (string.IsNullOrEmpty(model.SENDMAIL28) ? "N" : model.SENDMAIL28);
                        break;
                    case "2": //在職
                        data.DISTID = model.DISTID;
                        data.CJOBNO = model.CJOBUNKEYRESULT;
                        data.SENDMAIL06 = (string.IsNullOrEmpty(model.SENDMAIL06) ? "N" : model.SENDMAIL06);
                        break;
                    case "5": //區域產業據點
                        data.DISTID = model.DISTID;
                        data.CJOBNO = model.CJOBUNKEYRESULT;
                        data.SENDMAIL70 = (string.IsNullOrEmpty(model.SENDMAIL70) ? "N" : model.SENDMAIL70);
                        break;
                }

                //新增會員速配課程設定值
                base.Insert<TblTB_MEMSEARCH>(data);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO InsertMemSearch failed: " + ex.Message, ex);
                throw new Exception("WDAIIPWEBDAO InsertMemSearch failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 異動速配課程
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sm"></param>
        public void UpdateMemSearch(ClassMatchFormModel model, SessionModel sm)
        {
            TblTB_MEMSEARCH oldSearch = null;
            TblTB_MEMSEARCH newSearch = new TblTB_MEMSEARCH();
            TblTB_MEMSEARCH whereConds = new TblTB_MEMSEARCH { MEM_SN = sm.MemSN };

            try
            {
                oldSearch = base.GetRow<TblTB_MEMSEARCH>(whereConds);

                if (oldSearch != null)
                {
                    newSearch.InjectFrom(oldSearch);
                    newSearch.MODIFYDATE = DateTime.Now;

                    ClearFieldMap clearFieldMap = new ClearFieldMap();
                    clearFieldMap.Add((TblTB_MEMSEARCH m) => m.MODIFYDATE);

                    switch (model.PlanType) //計畫別
                    {
                        case "1": //產投
                            newSearch.TMID = model.TMIDRESULT;
                            newSearch.CTID = model.CTID;
                            newSearch.SENDMAIL28 = (string.IsNullOrEmpty(model.SENDMAIL28) ? "N" : model.SENDMAIL28);

                            //設定允許清空值的欄位
                            clearFieldMap.Add((TblTB_MEMSEARCH m) => m.TMID);
                            clearFieldMap.Add((TblTB_MEMSEARCH m) => m.CTID);
                            break;

                        case "2": //在職進修
                            newSearch.DISTID = model.DISTID;
                            newSearch.CJOBNO = model.CJOBUNKEYRESULT;
                            newSearch.SENDMAIL06 = (string.IsNullOrEmpty(model.SENDMAIL06) ? "N" : model.SENDMAIL06);

                            //設定允許清空值的欄位
                            clearFieldMap.Add((TblTB_MEMSEARCH m) => m.DISTID);
                            clearFieldMap.Add((TblTB_MEMSEARCH m) => m.CJOBNO);
                            break;

                        case "5": //區域產業據點
                            newSearch.DISTID = model.DISTID;
                            newSearch.CJOBNO = model.CJOBUNKEYRESULT;
                            newSearch.SENDMAIL70 = (string.IsNullOrEmpty(model.SENDMAIL70) ? "N" : model.SENDMAIL70);

                            //設定允許清空值的欄位
                            clearFieldMap.Add((TblTB_MEMSEARCH m) => m.DISTID);
                            clearFieldMap.Add((TblTB_MEMSEARCH m) => m.CJOBNO);
                            break;
                    }

                    //修改速配設定結果
                    base.Update<TblTB_MEMSEARCH>(newSearch, oldSearch, whereConds, clearFieldMap);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO InsertMemSearch failed: " + ex.Message, ex);
                throw new Exception("WDAIIPWEBDAO InsertMemSearch failed:" + ex.Message, ex);
            }
        }
        #endregion

        #region 會員專區/速配課程清單
        /// <summary>
        /// 查詢產投速配課程清單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassMatchGrid1Model> QueryClassMatchList_1(ClassMatchListFormModel form)
        {
            string funcName = "WDAIIPWEB.queryClassMatchList_1";
            return base.QueryForList<ClassMatchGrid1Model>(funcName, form);
        }

        /// <summary>
        /// 查詢在職進修速配課程清單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ClassMatchGrid2Model> QueryClassMatchList_2(ClassMatchListFormModel form)
        {
            string funcName = "WDAIIPWEB.queryClassMatchList_2";
            return base.QueryForList<ClassMatchGrid2Model>(funcName, form);
        }
        #endregion

        #region 會員專區/報名記錄
        /// <summary>
        /// 查詢e網報名資料
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <returns></returns>
        public IList<TblCLASS_CLASSINFO> QueryStudEnterType2Class(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.queryStudEnterType2Class";
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2
            {
                IDNO = idno,
                BIRTHDAY = birth
            };
            return base.QueryForListAll<TblCLASS_CLASSINFO>(funcName, whereConds);
        }

        /// <summary>STUD_ENTERTYPE2DELDATA 增修需求 OJT-21012502：報名查詢及取消：顯示取消的課程，並註記時間</summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <returns></returns>
        public IList<TblCLASS_CLASSINFO> QueryStudEnterType2ClassDel(string idno, DateTime birth)
        {
            string funcName = "WDAIIPWEB.queryStudEnterType2ClassDel";
            TblSTUD_ENTERTEMP2 whereConds = new TblSTUD_ENTERTEMP2
            {
                IDNO = idno,
                BIRTHDAY = birth
            };
            return base.QueryForListAll<TblCLASS_CLASSINFO>(funcName, whereConds);
        }

        /// <summary>
        /// 撈取現場報名資料-排除網路報名
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <param name="eOCIDs"></param>
        /// <returns></returns>
        public IList<TblCLASS_CLASSINFO> QueryStudEnterTypeClass(string idno, DateTime birth, string eOCIDs)
        {
            string funcName = "WDAIIPWEB.queryStudEnterTypeClass";
            StudEnterTemp2ExtModel whereConds = new StudEnterTemp2ExtModel
            {
                IDNO = idno,
                BIRTHDAY = birth
            };

            if (!string.IsNullOrEmpty(eOCIDs))
            {
                whereConds.E_OCIDS = eOCIDs;
            }

            return base.QueryForListAll<TblCLASS_CLASSINFO>(funcName, whereConds);
        }

        /// <summary>
        /// 查詢學員所有報名資料（未開訓的班）-含網路／現場
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <param name="eOCIDs"></param>
        /// <param name="timsOCIDs"></param>
        /// <returns></returns>
        public IList<EnterClassGridModel> QueryAllEnterClass(string idno, DateTime birth, string eOCIDs, string timsOCIDs)
        {
            IList<EnterClassGridModel> result = null;
            string funcName = "WDAIIPWEB.queryAllEnterClass";
            if (string.IsNullOrEmpty(eOCIDs)) { eOCIDs = "-1"; } //eOCIDs:絕對不可以空白(SQL語法需要)

            StudEnterTemp2ExtModel whereConds = new StudEnterTemp2ExtModel
            {
                IDNO = idno,
                BIRTHDAY = birth,
                E_OCIDS = eOCIDs,
                TIMS_OCIDS = timsOCIDs,
                HasEnterTemp = (string.IsNullOrEmpty(timsOCIDs) ? "N" : "Y")
            };

            try
            {
                result = base.QueryForListAll<EnterClassGridModel>(funcName, whereConds);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryAllEnterClass():", ex.Message), ex);
                //throw;
            }
            return result;
        }

        /// <summary>
        /// 學員 已取消報名查詢-網路
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        /// <param name="eOCIDs"></param>
        /// <returns></returns>
        public IList<EnterClassDelGridModel> QueryAllEnterClassDel(string idno, DateTime birth, string eOCIDs)
        {
            IList<EnterClassDelGridModel> result = null;
            string funcName = "WDAIIPWEB.queryAllEnterClassDel";
            if (string.IsNullOrEmpty(eOCIDs)) { eOCIDs = "-1"; } //eOCIDs:絕對不可以空白(SQL語法需要)

            StudEnterTemp2ExtModel whereConds = new StudEnterTemp2ExtModel
            {
                IDNO = idno,
                BIRTHDAY = birth,
                E_OCIDS = eOCIDs,
                TIMS_OCIDS = "",
                HasEnterTemp = "N"
            };
            try
            {
                result = base.QueryForListAll<EnterClassDelGridModel>(funcName, whereConds);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryAllEnterClassDel():", ex.Message), ex);
                //throw ex;
            }
            return result;
        }

        /// <summary>
        /// 查詢報名資料（處於送件成功狀態, signstatus=0）
        /// </summary>
        /// <param name="esernum"></param>
        /// <returns></returns>
        public TblSTUD_ENTERTYPE2 GetEnterType2ByESERNUM(Int64 esetid, Int64 esernum, Int64 ocid1)
        {
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2 { ESETID = esetid, ESERNUM = esernum, OCID1 = ocid1 };
            TblSTUD_ENTERTYPE2 rtn = null;
            try
            {
                rtn = base.GetRow(whereConds);
            }
            catch (Exception ex)
            {
                LOG.Error("GetEnterType2ByESERNUM ex:" + ex.Message, ex);
                throw ex;
            }
            return rtn;
        }

        /// <summary>
        /// 取得最新 stud_delentertype2.esernum (len(esernum) less than 8)
        /// </summary>
        /// <returns></returns>
        public Int64 GetDelEnterType2NewESERNUMByLen()
        {
            string funcName = "WDAIIPWEB.GetDelEnterType2NewESERNUMByLen";
            Hashtable data = (Hashtable)base.QueryForObject(funcName, null);
            Int64 num = 0;
            Int64 rtn = 1;

            if (data != null)
            {
                if (Int64.TryParse(Convert.ToString(data["NEWESERNUM"]), out num))
                {
                    rtn = num;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 備份 stud_entertype2 to stud_entertype2deldata
        /// </summary>
        /// <param name="tplanid"></param>
        /// <param name="esernum"></param>
        public void BackUpEnterType2(string tplanid, Int64 esernum)
        {
            SessionModel sm = SessionModel.Get();
            // 取得系統時間
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            try
            {
                // stud_entertype2 註記異動日期資訊
                TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2 { ESERNUM = esernum };
                TblSTUD_ENTERTYPE2 oldType2 = base.GetRow(whereConds);
                TblSTUD_ENTERTYPE2 newType2 = new TblSTUD_ENTERTYPE2();

                //收件成功 or 審核中（報名成功時）
                newType2.InjectFrom(oldType2);
                newType2.MODIFYACCT = sm.ACID;
                newType2.MODIFYDATE = aNow;
                base.Update(newType2, oldType2, whereConds);

                switch (tplanid)
                {
                    case "28":
                        //產投 // 複製備份資料（stud_entertype2 --> stud_entertype2deldata）
                        TblSTUD_ENTERTYPE2DELDATA newType2Del = new TblSTUD_ENTERTYPE2DELDATA();
                        newType2Del.InjectFrom(newType2);
                        base.Insert(newType2Del);
                        break;
                    default:
                        // case "06": //在職
                        // 複製備份資料（stud_entertype2 --> stud_delentertype2）
                        Int64 newESERNUM = this.GetDelEnterType2NewESERNUMByLen();
                        TblSTUD_DELENTERTYPE2 newDelType2 = new TblSTUD_DELENTERTYPE2();

                        newDelType2.InjectFrom(newType2);
                        newDelType2.ESERNUM = newESERNUM;
                        base.Insert(newDelType2);
                        break;
                }

            }
            catch (Exception ex)
            {
                LOG.Error("BackUpEnterType2 ex: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 刪除報名資料（stud_entertype2）
        /// </summary>
        /// <param name="esernum"></param>
        public void DelEnterType2(Int64 esernum)
        {
            TblSTUD_ENTERTYPE2 whereConds = new TblSTUD_ENTERTYPE2 { ESERNUM = esernum };
            base.Delete(whereConds);
        }

        /// <summary>
        /// 備份 stud_entertype to  stud_delentertype / stud_entertypedeldata
        /// </summary>
        /// <param name="tplanid"></param>
        /// <param name="esernum"></param>
        public void BackUpEnterType(string tplanid, decimal setid, DateTime enterdate, decimal sernum)
        {
            SessionModel sm = SessionModel.Get();
            // 取得系統時間
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();

            try
            {
                // stud_entertype2 註記異動日期資訊
                TblSTUD_ENTERTYPE whereConds = new TblSTUD_ENTERTYPE { SETID = setid, ENTERDATE = enterdate, SERNUM = sernum };
                TblSTUD_ENTERTYPE oldType1 = base.GetRow(whereConds);
                TblSTUD_ENTERTYPE newType1 = new TblSTUD_ENTERTYPE();
                //收件成功 or 審核中（報名成功時）
                newType1.InjectFrom(oldType1);
                newType1.MODIFYACCT = sm.ACID;
                newType1.MODIFYDATE = aNow;
                base.Update(newType1, oldType1, whereConds);

                switch (tplanid)
                {
                    case "28":
                        //產投 // 複製備份資料（stud_entertype --> stud_entertypedeldata）
                        TblSTUD_ENTERTYPEDELDATA newType1Del = new TblSTUD_ENTERTYPEDELDATA();
                        newType1Del.InjectFrom(newType1);
                        base.Insert(newType1Del);
                        break;

                    default:
                        // case "06": //在職 // 複製備份資料（stud_entertype --> stud_delentertype/stud_entertypedeldata）
                        TblSTUD_DELENTERTYPE newDelType1 = new TblSTUD_DELENTERTYPE();
                        newDelType1.InjectFrom(newType1);
                        base.Insert(newDelType1);
                        break;
                }

            }
            catch (Exception ex)
            {
                LOG.Error("BackUpEnterType ex: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 刪除報名資料（stud_entertype）
        /// </summary>
        /// <param name="esernum"></param>
        public void DelEnterType(decimal setid, DateTime enterdate, decimal sernum)
        {
            TblSTUD_ENTERTYPE whereConds = new TblSTUD_ENTERTYPE { SETID = setid, ENTERDATE = enterdate, SERNUM = sernum };
            base.Delete(whereConds);
        }
        #endregion

        #region Q&A/常見問題

        /// <summary>
        /// 查詢常見問題類別清單(啟用中)
        /// </summary>
        /// <returns></returns>
        public IList<QATYPEGridModel> QueryQAType()
        {
            return base.QueryForListAll<QATYPEGridModel>("WDAIIPWEB.queryQAType", null);
        }

        /// <summary>
        /// 查詢常見問題QA清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IList<QAGridModel> QueryQA(QAFormModel model)
        {
            IList<QAGridModel> list = null;
            Hashtable param = new Hashtable();
            param["TYPEID"] = model.TYPEID;

            IList<string> keyWordAry = new List<string>();
            if (!string.IsNullOrEmpty(model.KEYWORD))
            {
                //多組關鍵字要以空白區隔
                keyWordAry = model.KEYWORD.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            param["WHERE"] = keyWordAry;

            try
            {
                list = base.QueryForList<QAGridModel>("WDAIIPWEB.queryQA", param);
            }
            catch (Exception ex)
            {
                LOG.Error("WDAIIPWEBDAO QueryQA() error:" + ex.Message, ex);
                throw ex;
            }
            return list;
        }

        /// <summary> 新增關鍵字查詢Log紀錄 </summary>
        /// <param name="list"></param>
        public void InsertKeyWordLog(IList<TblTB_KEYWORD_LOG> list)
        {
            try
            {
                //base.BeginTransaction(); //取得TB_KEYWORD_LOG的PK欄位值
                //int kwlid = new MyKeyMapDAO().GetTableMaxSeqNo(StaticCodeMap.TableName.TB_KEYWORD_LOG, "KWLID");
                //decimal kwlid = 0;

                if (list != null)
                {
                    foreach (TblTB_KEYWORD_LOG item in list)
                    {
                        item.KWLID = this.GetNewId("TB_KEYWORD_LOG_KWLID_SEQ,TB_KEYWORD_LOG,KWLID").Value;
                        //執行新增
                        base.Insert(item);
                    }
                }

                //base.CommitTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                //LOG.Error("WDAIIPWEBDAO InsertKeyWordLog() error:" + ex.Message, ex);
                LOG.Warn("WDAIIPWEBDAO Insert TB_KEYWORD_LOG failed: " + ex.Message, ex);
                //throw ex;
            }
        }
        #endregion

        #region 課程查詢報名--地圖找課程
        public IList<ClassMapSchGrid1Model> QueryClassMapSchList_G1(ClassMapSchFormModel form)
        {
            string funcName = "WDAIIPWEB.queryClassMapSchList_G1";
            return base.QueryForListAll<ClassMapSchGrid1Model>(funcName, form);
        }
        #endregion

        #region 相關連結--服務據點-地圖查詢   
        public IList<OrgMapSchGrid1Model> QueryOrgMapSchList_G1(OrgMapSchFormModel form)
        {
            string funcName = "WDAIIPWEB.queryOrgMapSchList_G1";
            return base.QueryForListAll<OrgMapSchGrid1Model>(funcName, form);
        }
        #endregion

        /// <summary>下載訓練證明</summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<TrainCertGridModel> QueryStudTrainCert(TrainCertFormModel form)
        {
            IList<TrainCertGridModel> result = null;

            SessionModel sm = SessionModel.Get();
            if (form.IDNO != sm.ACID) { return result; }
            form.IDNO = sm.ACID ?? "";
            form.BIRTHDAY = sm.Birthday ?? "";

            Hashtable param = new Hashtable(); //param.Clear();
            param["IDNO"] = sm.ACID ?? "";
            param["BIRTHDAY"] = sm.Birthday ?? "";
            try
            {
                //result = base.QueryForListAll<TrainCertGridModel>("WDAIIPWEB.queryStudTrainCert", whereConds);
                result = base.QueryForList<TrainCertGridModel>("WDAIIPWEB.queryStudTrainCert", param);
                return result;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("WDAIIPWEBDAO QueryStudTrainCert():", ex.Message), ex);
            }
            return result;
        }

        //rte, tmd, socid, ocid, clsid, studstatus
        /// <summary> 會員專區-下載訓練證明 查詢班級學號資料 確認報表 </summary>
        /// <param name="rte"></param>
        /// <param name="tmd"></param>
        /// <param name="socid"></param>
        /// <param name="ocid"></param>
        /// <param name="clsid"></param>
        /// <param name="studstatus"></param>
        /// <returns></returns>
        public TrainCertReportModel GetOjtReport(string rte, string tmd, string socid, string ocid, string clsid, string studstatus)
        {
            TrainCertReportModel rtn = null;

            if (string.IsNullOrEmpty(rte) || string.IsNullOrEmpty(tmd) || string.IsNullOrEmpty(socid)
                || string.IsNullOrEmpty(ocid) || string.IsNullOrEmpty(clsid) || string.IsNullOrEmpty(studstatus)) return rtn;
            long i_socid = 0; if (!long.TryParse(socid, out i_socid)) return rtn;
            long i_ocid = 0; if (!long.TryParse(ocid, out i_ocid)) return rtn;
            long i_clsid = 0; if (!long.TryParse(clsid, out i_clsid)) return rtn;
            long i_studstatus = 0; if (!long.TryParse(studstatus, out i_studstatus)) return rtn;

            TrainCertReportModel parms = new TrainCertReportModel { RTE = rte, TMD = tmd, SOCID = i_socid, OCID = i_ocid, CLSID = i_clsid, STUDSTATUS = i_studstatus };

            rtn = (TrainCertReportModel)base.QueryForObject("WDAIIPWEB.getOjtReport", parms);

            return rtn;
        }

        /// <summary>GetZipCodeName GetZipName 取得 縣市轄區中文 查詢郵遞區號</summary>
        /// <param name="zipcode"></param>
        /// <returns></returns>
        public string GetZipCodeName(string zipcode)
        {
            string rtn = null;
            if (string.IsNullOrEmpty(zipcode)) { return rtn; }

            int i_zipcode = 0;
            if (!int.TryParse(zipcode, out i_zipcode)) { return rtn; }

            Hashtable param = new Hashtable { ["ZIPCODE"] = i_zipcode }; //param.Clear();

            rtn = (string)base.QueryForObject("KeyMap.getZipCodeName", param);

            return rtn;
        }

        /// <summary>【就業通】職訓專長能力標籤斷字分詞處API_1130301</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public StrSegmentModel GetStrSegment(string str)
        {
            string FunctionName = "WDAIIPWEBDAO: *GetStrSegment";
            //Logging http://localhost:2866/Ajax/GetSolrApi
            //LOG.Debug($"{FunctionName} ,q={q}");
            if (string.IsNullOrEmpty(str))
            {
                LOG.Warn($"[ALERT] {FunctionName}: str 不可為空值");
                return null; //throw new ArgumentNullException("ocid");
            }
            LOG.Debug($"{FunctionName} ,str={str}");

            // WebRequest物件如何忽略憑證問題 // Ignoring validation for demonstration purposes
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ExceptionHandler.ValidateServerCertificate);

            // TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            // Set TLS protocol (if needed) // client.DefaultRequestHeaders.Add("Connection", "close"); // May be necessary for TLS 1.2 // client.DefaultRequestHeaders.Expect = null; // May be necessary for TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072

            using (var client = new HttpClient())
            {
                // Get WebApi URL from configuration https://job.taiwanjobs.gov.tw/StrSegment.ashx?str=網規企
                string default_WebApi_URL2 = "https://job.taiwanjobs.gov.tw/StrSegment.ashx";
                string webApiUrl = ConfigurationManager.AppSettings["StrSegment_URL2"];
                // Replace with your default URL
                if (string.IsNullOrEmpty(webApiUrl)) { webApiUrl = default_WebApi_URL2; }

                string urlWithParams = $"{webApiUrl}?str={str}";
                LOG.Info($"{FunctionName} ,urlWithParams: {webApiUrl}, {urlWithParams}");

                // Send GET request
                HttpResponseMessage response = null;
                try
                {
                    // Send GET request
                    response = client.GetAsync(urlWithParams).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string strResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return JsonConvert.DeserializeObject<StrSegmentModel>(strResponse);
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error($"#{FunctionName}: " + ex.Message, ex); // throw;
                }
                //var x = JsonConvert.SerializeObject(strResponse); //if (Segm==null) return Segm; //return Segm.keywords;
                if (response != null) { LOG.Error($"{FunctionName},HTTP request failed with status code: {response.StatusCode}"); }
                return null;
            }
        }

        /// <summary>【就業通】職訓專長能力標籤斷字分詞</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string[] GetSegmentkeywords(string str)
        {
            var Segm = GetStrSegment(str);
            if (Segm == null) return null;
            return Segm.keywords;
        }

        /// <summary>【就業通】職訓專長能力標籤斷字分詞</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetSegmentkeyword(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            string rst = "";
            //Replace-start
            string s_sp2 = "|";
            string s_sp3 = " ";
            if (!string.IsNullOrEmpty(str) && str.IndexOf(s_sp2) > -1) { str = str.Replace(s_sp2, s_sp3); }
            //Replace-end
            var strX = GetSegmentkeywords(str);
            if (strX == null) return null;

            foreach (string s1 in strX)
            {
                string s2 = Convert.ToString(s1).Trim();
                if (!string.IsNullOrEmpty(s2) && (string.Concat($" {rst} ").IndexOf(string.Concat($" {s2} ")) == -1))
                {
                    rst += string.Concat((string.IsNullOrEmpty(rst) ? "" : " "), s2);
                }
            }

            if (string.IsNullOrEmpty(rst)) { return null; }
            return rst;
        }

        public IList<DigiCertGridModel> QueryDigiCertClass(DigiCertPageFormModel pform)
        {
            var rst = base.QueryForList<DigiCertGridModel>("WDAIIPWEB.queryDigiCertClass", pform);
            return UpdateDigiIscheck(rst);
        }

        private IList<DigiCertGridModel> UpdateDigiIscheck(IList<DigiCertGridModel> dglist)
        {
            if (dglist == null || dglist.Count == 0) { return dglist; }
            foreach (var row1 in dglist)
            {
                row1.SELECTIS = (row1.ISCHECK == "Y" ? true : false);
            }
            return dglist;
        }

        /// <summary>批次刪除課程</summary>
        /// <param name="classlist">課程代碼清單</param>
        /// <param name="sm">SessionModel</param>
        /// <returns></returns>
        public void DelDigiCertClass(IList<Int64?> classlistdel, SessionModel sm)
        {
            if (classlistdel == null || sm == null) { return; }
            base.BeginTransaction();
            try
            {
                foreach (var iOCID in classlistdel)
                {
                    //查詢資料是否存在
                    if (iOCID != null)
                    {
                        var owc1 = new TblSTUD_DIGICERTCLASS { DCANO = long.Parse(sm.DCANO), OCID = iOCID };
                        var tmp = base.GetRow<TblSTUD_DIGICERTCLASS>(owc1);
                        if (tmp != null)
                        {
                            var param = new TblSTUD_DIGICERTCLASS { MODIFYDATE = DateTime.Now, ISCHECK = "N" };
                            base.Update<TblSTUD_DIGICERTCLASS>(param, tmp);
                            base.Delete(owc1);
                        }
                    }
                }

                base.CommitTransaction(); //base.RollBackTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("WDAIIPWEBDAO AddDigiCertClass failed: " + ex.Message, ex);
                throw new Exception("WDAIIPWEBDAO AddDigiCertClass failed:" + ex.Message, ex);
            }
        }


        /// <summary>批次加入課程</summary>
        /// <param name="classlist">課程代碼清單</param>
        /// <param name="sm">SessionModel</param>
        /// <returns></returns>
        public void AddDigiCertClass(IList<Int64?> classlist, SessionModel sm)
        {
            if (classlist == null || sm == null) { return; }
            base.BeginTransaction();
            try
            {
                foreach (var iOCID in classlist)
                {
                    //查詢資料是否存在
                    if (iOCID != null)
                    {
                        var owc1 = new TblSTUD_DIGICERTCLASS { DCANO = long.Parse(sm.DCANO), OCID = iOCID };
                        var tmp = base.GetRow<TblSTUD_DIGICERTCLASS>(owc1);
                        if (tmp == null)
                        {
                            //var aNow = new MyKeyMapDAO().GetSysDateNow();
                            var iDCLNO = this.GetNewId("STUD_DIGICERTCLASS_DCLNO_SEQ,STUD_DIGICERTCLASS,DCLNO").Value;
                            owc1.DCLNO = iDCLNO;
                            owc1.ISCHECK = "Y";
                            owc1.MODIFYACCT = sm.ACID;
                            owc1.MODIFYDATE = DateTime.Now; //aNow;
                            base.Insert<TblSTUD_DIGICERTCLASS>(owc1);
                        }
                        else
                        {
                            var param = new TblSTUD_DIGICERTCLASS { MODIFYDATE = DateTime.Now };
                            base.Update<TblSTUD_DIGICERTCLASS>(param, tmp);
                        }
                    }
                }

                base.CommitTransaction(); //base.RollBackTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("WDAIIPWEBDAO AddDigiCertClass failed: " + ex.Message, ex);
                throw new Exception("WDAIIPWEBDAO AddDigiCertClass failed:" + ex.Message, ex);
            }
        }


        //QueryEmailCodeShort1
        public IList<TblE_EMAILCODE> QueryEmailCodeShort1(Hashtable param1)
        {
            return base.QueryForListAll<TblE_EMAILCODE>("WDAIIPWEB.queryEmailCodeShort1", param1);
        }

        //QueryDigiCertToday1
        public IList<TblSTUD_DIGICERTAPPLY> QueryDigiCertToday1(Hashtable param1)
        {
            return base.QueryForList<TblSTUD_DIGICERTAPPLY>("WDAIIPWEB.queryDigiCertToday1", param1);
        }
        /// <summary>數位結訓證明下載-LIST</summary>
        /// <param name="dpform"></param>
        /// <returns></returns>
        public IList<DigiCertApplyGridModel> QueryDigiCertApply(DigiCertDLPageFormModel dpform)
        {
            return base.QueryForList<DigiCertApplyGridModel>("WDAIIPWEB.queryDigiCertApply", dpform);
        }
        public DigiCertRptModel QueryOjtSD14031C(string sDCANO, string sDCASENO, string sEMVCODE)
        {
            Hashtable param = new Hashtable
            {
                ["DCANO"] = sDCANO,
                ["DCASENO"] = sDCASENO,
                ["EMVCODE"] = sEMVCODE
            };
            return (DigiCertRptModel)base.QueryForObject("WDAIIPWEB.queryOjtSD14031C", param);
        }
        public DigiCertRptModel QueryOjtSD14031D(string sDCANO, string sDCASENO, string sEMVCODE)
        {
            Hashtable param = new Hashtable
            {
                ["DCANO"] = sDCANO,
                ["DCASENO"] = sDCASENO,
                ["EMVCODE"] = sEMVCODE
            };
            return (DigiCertRptModel)base.QueryForObject("WDAIIPWEB.queryOjtSD14031D", param);
        }
        public string CheckGetGUID1(string g1)
        {
            //iDCANO = dao.GetNewId("STUD_DIGICERTAPPLY_DCANO_SEQ,STUD_DIGICERTAPPLY,DCANO").Value; //dao.GetAutoNum()'
            var w1 = new TblSTUD_DIGICERTAPPLY { GUID1 = g1 };
            var o1 = base.GetRow(w1);
            //if (o1 == null) { return g1; }
            while (o1 != null)
            {
                g1 = Guid.NewGuid().ToString().ToUpper();
                w1 = new TblSTUD_DIGICERTAPPLY { GUID1 = g1 };
                o1 = base.GetRow(w1);
            }
            return g1;
        }

        public string CheckGetGUID1N()
        {
            string g1 = Guid.NewGuid().ToString().ToUpper();
            return CheckGetGUID1(g1);
        }

        public IList<StudOnlineGridModel> QueryStudOnlineSign1(StudOnlineFormModel form)
        {
            if (string.IsNullOrEmpty(form.IDNO) || string.IsNullOrEmpty(form.BIRTHDAY_STR)) { return null; }

            return base.QueryForListAll<StudOnlineGridModel>("WDAIIPWEB.queryStudOnlineSign1", form);
        }

        public IList<StudOnlineGrid2Model> QueryStudOnlineSign2(StudOnlineFormModel form)
        {
            if (string.IsNullOrEmpty(form.IDNO) || string.IsNullOrEmpty(form.BIRTHDAY_STR)
                || string.IsNullOrEmpty(form.SOCID) || string.IsNullOrEmpty(form.OCID) || string.IsNullOrEmpty(form.SID)) { return null; }

            return base.QueryForListAll<StudOnlineGrid2Model>("WDAIIPWEB.queryStudOnlineSign2", form);
        }

        public StudOnlineReportModel QuerySignReport1(string socid, string ocid, string sid, string elno)
        {
            if (string.IsNullOrEmpty(socid) || string.IsNullOrEmpty(ocid)
                || string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(elno)) { return null; }

            if (elno == "1")
            {
                int iOCID = int.Parse(ocid);
                int iSOCID = int.Parse(socid);
                var parms = new Hashtable { { "OCID", iOCID }, { "SOCID", iSOCID }, { "SID", sid } };
                var rtn = (StudOnlineReportModel)base.QueryForObject("WDAIIPWEB.querySignReport1", parms);
                return rtn;
            }
            else if (elno == "2")
            {
                int iOCID = int.Parse(ocid);
                int iSOCID = int.Parse(socid);
                var parms = new Hashtable { { "OCID", iOCID }, { "SOCID", iSOCID }, { "SID", sid } };
                var rtn = (StudOnlineReportModel)base.QueryForObject("WDAIIPWEB.querySignReport2", parms);
                return rtn;
            }
            else if (elno == "3")
            {
                int iOCID = int.Parse(ocid);
                int iSOCID = int.Parse(socid);
                var parms = new Hashtable { { "OCID", iOCID }, { "SOCID", iSOCID }, { "SID", sid } };
                var rtn = (StudOnlineReportModel)base.QueryForObject("WDAIIPWEB.querySignReport3", parms);
                return rtn;
            }

            return null;
        }
        //銀行代碼 V_BANKLIST
        public IList<BankVM> QueryBankList()
        {
            IList<BankVM> result = null;
            try
            {
                result = base.QueryForListAll<BankVM>("WDAIIPWEB.queryBankList", null);
            }
            catch (Exception ex)
            {
                LOG.Error("QueryBankList():" + ex.Message, ex);
            }
            return result;
        }

        //queryBranchList: SELECT BranchCode,BranchName,[Text],BankName,APK1 FROM V_BRANCHLIST ORDER BY BranchCode
        public IList<BankVM> QueryBranchList(string code)
        {
            IList<BankVM> result = null;
            Hashtable param = new Hashtable { { "APK1", code } };
            try
            {
                result = base.QueryForListAll<BankVM>("WDAIIPWEB.queryBranchList", param);
            }
            catch (Exception ex)
            {
                LOG.Error("QueryBranchList():" + ex.Message, ex);
            }
            return result;
        }

        /// <summary>使用原子 SQL：Update 與 Set 在同一條指令完成,加入 WHERE 條件確保減法不會低於 0</summary>
        /// <param name="host"></param>
        /// <param name="column"></param>
        /// <param name="isIncrement"></param>
        /// <returns></returns>
        public int UpdateCounterAtomic(string host, string column, bool isIncrement)
        {
            // 確保 column 參數安全，避免 SQL Injection
            if (column != "PCOUNT" && column != "NCOUNT") return 0;
            //if (column != "PCOUNT" && column != "NCOUNT") throw new ArgumentException("Invalid column");
            string op = isIncrement ? "+" : "-";
            try
            {
                Hashtable parms = new Hashtable { { "column", column }, { "op", op }, { "host", host } };
                return base.Update("WDAIIPWEB.UpdateCounterAtomic", parms);
            }
            catch (Exception ex)
            {
                LOG.Error($"UpdateCounterAtomic ex:{ex.Message}", ex);
                return 0;  //throw ex;
            }
        }

    }
}
