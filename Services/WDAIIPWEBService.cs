using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.DataLayers;
using Turbo.Commons;
using log4net;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Threading;
using WDAIIP.WEB.Commons;
using System.Net;
using System.Data;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace WDAIIP.WEB.Services
{
    public class WDAIIPWEBService
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // 提供Lock鎖定的物件
        private static readonly object thisLock = new object();
        private const string cstSYSNAME = "PSHTL001";


        #region 顯示參訓歷史清單資料
        public void StoreHistoryDefStdCost(TrainingHistoryViewModel model, DataTable dt)
        {
            TrainingHistoryGridModel item = null;

            if (dt != null)
            {
                model.Grid = new List<TrainingHistoryGridModel>();
                foreach (DataRow dr in dt.Rows)
                {
                    item = new TrainingHistoryGridModel();
                    item.IDNO = model.Form.IDNO;
                    if (!Convert.IsDBNull(dr["SOCID"]))
                    {
                        item.SOCID = Convert.ToInt64(dr["SOCID"]);
                    }

                    item.PLANYEAR = Convert.ToString(dr["YEARS"]);

                    if (!Convert.IsDBNull(dr["YEARS"]))
                    {
                        item.PLANYEAR_TW = Convert.ToString(Convert.ToInt32(dr["YEARS"]) - 1911);
                    }

                    item.PLANNAME = Convert.ToString(dr["PLANNAME"]);
                    item.STDATE = Convert.ToDateTime(dr["STDATE"]);
                    item.FTDATE = Convert.ToDateTime(dr["FTDATE"]);
                    item.CLASSCNAME = Convert.ToString(dr["ClassCName"]);

                    if (!Convert.IsDBNull(dr["SumOfMoney"]))
                    {
                        item.DEFSTDCOST = Convert.ToInt64(dr["SumOfMoney"]);
                    }

                    item.RedBoldStar = false;
                    if (!string.IsNullOrEmpty(Convert.ToString(dr["YEAR3"])) && "Y".Equals(Convert.ToString(dr["YEAR3"])))
                    {
                        //每三年區間的頭一筆
                        item.RedBoldStar = true;
                        item.ToolTips = "計算起始點";
                    }


                    model.Grid.Add(item);
                }
            }
        }

        /// <summary>
        /// 額外查詢自辦在職課程併入webservice撈的參訓歷程清單中(產投+職前（托育）)
        /// </summary>
        /// <param name="model"></param>
        public void MergeHistoryDefStdCost(TrainingHistoryViewModel model)
        {
            if (model.Grid06 != null)
            {
                foreach (TrainingHistoryGridModel item in model.Grid06)
                {
                    if (model.Grid == null)
                    {
                        model.Grid = new List<TrainingHistoryGridModel>();
                    }

                    model.Grid.Add(item);
                }
            }

            if (model.Grid != null)
            {
                model.Grid = model.Grid.OrderBy(x => x.STDATE).ToList();
            }
        }

        /// <summary>
        /// 顯示參訓歷史清單資料
        /// </summary>
        /// <param name="model"></param>
        public void ShowHistoryDefStdCost(TrainingHistoryViewModel model)
        {
            Decimal? sumMoney = 0;
            string sTDate = string.Empty;
            DateTime? fTDate = null;
            DateTime? sTDateYr3 = null;

            //處理參訓歷史欄位資訊
            if (model.Grid != null)
            {
                if (model.Grid.Count > 0)
                {
                    for (int i = 0; i < model.Grid.Count; i++)
                    {
                        var item = model.Grid[i];
                        //item.RedBoldStar = true;
                        item.RedBoldStar = false;

                        bool flag_continue = false;//-continue
                        //算三年期間要排除在職課程-continue
                        if (!"28".Equals(item.TPLANID)) flag_continue = true;
                        if (flag_continue) continue;

                        if (string.IsNullOrEmpty(sTDate))
                        {
                            sTDate = item.STDATE_AD;
                            sumMoney = item.DEFSTDCOST;
                            item.ToolTips = "計算起始點";

                            if (item.DEFSTDCOST.HasValue)
                            {
                                item.RedBoldStar = true;
                            }
                        }
                        else
                        {
                            sTDateYr3 = Convert.ToDateTime(sTDate).AddYears(3);
                            //if (DateTime.Compare(sTDateYr3.Value, item.STDATE.Value) >= 0)
                            //if (DateTime.Compare(sTDateYr3.Value, item.STDATE.Value) <= 0)
                            if (DateTime.Compare(item.STDATE.Value, sTDateYr3.Value) >= 0)
                            {
                                //該開訓日超過上一個3年內的補助期間
                                //開始下一個3年的補助期間
                                sTDate = item.STDATE_AD;
                                sumMoney = item.DEFSTDCOST;
                                item.ToolTips = "計算起始點";

                                if (item.DEFSTDCOST.HasValue)
                                {
                                    item.RedBoldStar = true;
                                }
                            }
                            else
                            {
                                sumMoney += item.DEFSTDCOST;
                            }
                        }
                    }

                    // 表尾統計
                    TrainingHistoryGridModel footer = new TrainingHistoryGridModel();
                    footer.PLANYEAR_TW = "期間";

                    if (!string.IsNullOrEmpty(sTDate))
                    {
                        footer.STDATE = Convert.ToDateTime(sTDate);
                        fTDate = Convert.ToDateTime(sTDate).AddYears(3).AddDays(-1);
                        footer.FTDATE = fTDate;
                    }

                    footer.CLASSCNAME = @"已使用補助費<b style=""color:red;"">(未包含在訓中及已結訓尚未撥款之課程)</b>合計";

                    const int cst_sumMoneyMax7 = 70000;
                    string s_sumMoneyMax7 = "超過 7萬";
                    const int cst_sumMoneyMax10 = 100000;
                    string s_sumMoneyMax10 = "超過 10萬";
                    bool fg_test = ConfigModel.CHK_IS_TEST_ENVC; //(fg_test || fg_use1)
                    bool fg_use1 = (DateTime.Now.Year >= 2025);
                    footer.DEFSTDCOST = sumMoney;
                    if (fg_test || fg_use1)
                    {
                        if (sumMoney > cst_sumMoneyMax10)
                        {
                            footer.DEFSTDCOST = cst_sumMoneyMax10;
                            footer.ToolTips = s_sumMoneyMax10;
                        }
                    }
                    else
                    {
                        if (sumMoney > cst_sumMoneyMax7)
                        {
                            footer.DEFSTDCOST = cst_sumMoneyMax7;
                            footer.ToolTips = s_sumMoneyMax7;
                        }
                    }

                    model.Grid.Add(footer);
                }

                IList<TrainingHistoryGridModel> defStdCostList = null;
                TrainingHistoryGridModel defStdCost = null;
                defStdCostList = model.Grid.Where(x => x.CANWRITE == 1).ToList();
                if (defStdCostList.Count > 0)
                {
                    defStdCost = defStdCostList.FirstOrDefault();

                    if (DateTime.Compare(defStdCost.WRDATE1.Value, defStdCost.ATODAY.Value) >= 0 && DateTime.Compare(defStdCost.ATODAY.Value, defStdCost.WRDATE2.Value) >= 0)
                    {
                        model.DefStdCostMsg = "您尚有結訓課程未填寫參訓學員訓後動態調查表，請儘快填寫完畢。";
                    }
                }
            }
        }

        /// <summary>
        /// 顯示參訓歷史清單資料
        /// </summary>
        /// <param name="model"></param>
        public void ShowHistoryDefStdCost2(TrainingHistoryViewModel model, ojt0219ws1.GetOjt0219ws1 ws)
        {
            //處理參訓歷史欄位資訊
            if (model.Grid != null)
            {
                /*for (int i = 0 ; i < model.Grid.Count; i++ )
                {
                    var item = model.Grid[i];
                    item.RedBoldStar = false;

                    //算三年期間要排除在職課程
                    if ("06".Equals(item.TPLANID)) continue;
                }*/

                if (model.Grid.Count > 0)
                {
                    var lastItem = model.Grid[model.Grid.Count - 1];
                    var footerItem = new TrainingHistoryGridModel();

                    ws = new WEB.ojt0219ws1.GetOjt0219ws1();

                    footerItem.DEFSTDCOST = Convert.ToInt64(ws.GetSumOfMoneyCost(lastItem.IDNO, lastItem.STDATE_AD));
                    footerItem.TDATERANGE = ws.GetLabCostDay(lastItem.IDNO, lastItem.STDATE_AD, " ");

                    // 表尾統計
                    TrainingHistoryGridModel footer = new TrainingHistoryGridModel();
                    footer.PLANYEAR_TW = "期間";
                    footer.CLASSCNAME = @"已使用補助費<b style=""color:red;"">(未包含在訓中及已結訓尚未撥款之課程)</b>合計";

                    const int cst_sumMoneyMax7 = 70000;
                    string s_sumMoneyMax7 = "超過 7萬";
                    const int cst_sumMoneyMax10 = 100000;
                    string s_sumMoneyMax10 = "超過 10萬";
                    bool fg_test = ConfigModel.CHK_IS_TEST_ENVC; //(fg_test || fg_use1)
                    bool fg_use1 = (DateTime.Now.Year >= 2025);
                    footer.DEFSTDCOST = footerItem.DEFSTDCOST;
                    if ((fg_test || fg_use1))
                    {
                        if (footerItem.DEFSTDCOST > cst_sumMoneyMax10)
                        {
                            footer.DEFSTDCOST = cst_sumMoneyMax10;
                            footer.ToolTips = s_sumMoneyMax10;
                        }
                    }
                    else
                    {
                        if (footerItem.DEFSTDCOST > cst_sumMoneyMax7)
                        {
                            footer.DEFSTDCOST = cst_sumMoneyMax7;
                            footer.ToolTips = s_sumMoneyMax7;
                        }
                    }

                    if (!string.IsNullOrEmpty(footerItem.TDATERANGE) && !"~".Equals(footerItem.TDATERANGE))
                    {
                        string[] tDateAry = footerItem.TDATERANGE.Split('~');
                        if (tDateAry.Length == 2)
                        {
                            footer.STDATE = Convert.ToDateTime(Convert.ToString(tDateAry[0]).Trim());
                            footer.FTDATE = Convert.ToDateTime(Convert.ToString(tDateAry[1]).Trim());
                        }
                    }

                    model.Grid.Add(footer);
                }
            }
        }

        /// <summary>
        /// 顯示參訓歷史清單結果
        /// ref SD_05_010_pop
        /// </summary>
        /// <param name="model"></param>
        public void ShowTrainingHistory(TrainingHistoryViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string idno = model.Form.IDNO;

            //查詢學生舊版參訓資料(stdall)
            IList<TblSTDALL> stdAllList = dao.QueryStdAll(idno);
            this.SetStdAll(model, stdAllList);

            //查詢學生93年版參訓資料(history_studentInfo93)
            IList<HistoryStudentInfo93GridModel> hisStudInfo93List = dao.QueryHistoryStudentInfo93(idno);
            this.SetHisStudInfo93(model, hisStudInfo93List);

            //查詢學生參訓紀錄(class_studentsofclass)
            IList<HistoryClassStudsOfClassGridModel> hisClassStudsOfClassList = dao.QueryHistoryClassStudsOfClass(idno);
            this.SetHisClassStudsOfClass(model, hisClassStudsOfClassList);

            //查詢學生參訓紀錄(class_studentsofclassdeldata)
            IList<HistoryClassStudsOfClassGridModel> hisClassStudsOfClassDelDataList = dao.QueryHistoryClassStudsOfClassDelData(idno);
            this.SetHisClassStudsOfClass(model, hisClassStudsOfClassDelDataList);

            //查詢職前系統參訓紀錄(webservice)
            IList<WsGetTrainingGridModel> wsTrainingList = this.GetWsTraining(idno);
            this.SetWsTraining(model, wsTrainingList);

            if (model.Grid2 != null)
            {
                //以開訓日遞減排序顯示
                //model.Grid2 = model.Grid2.OrderByDescending(x => x.STDATE).ToList();
                //以開訓日排序顯示
                model.Grid2 = model.Grid2.OrderBy(x => x.STDATE).ToList();
            }
        }

        /// <summary>
        /// 載入顯示學員舊的參訓資料查詢結果(stdall)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="list"></param>
        public void SetStdAll(TrainingHistoryViewModel model, IList<TblSTDALL> list)
        {
            TrainingHistoryGrid2Model item = null;
            if (list != null)
            {
                foreach (var classItem in list)
                {
                    if (model.Grid == null)
                    {
                        model.Grid2 = new List<TrainingHistoryGrid2Model>();
                    }

                    item = new TrainingHistoryGrid2Model();

                    item.DISTNAME = classItem.DISTNAME;
                    item.YEARS = classItem.YEARS;
                    item.PLANNAME = classItem.PLANNAME;
                    item.ORGNAME = classItem.TRINUNIT;
                    item.CLASSNAME = classItem.CLASSNAME;

                    //開結訓日都有值時才顯示
                    if (classItem.SDATE.HasValue && classItem.EDATE.HasValue)
                    {
                        item.STDATE = classItem.SDATE;
                        item.FTDATE = classItem.EDATE;
                    }

                    item.STUDSTATUS_TEXT = "結訓"; //預設結訓

                    model.Grid2.Add(item);
                }
            }
        }

        /// <summary>
        /// 載入學員舊的參訓資料查詢結果(history_studentinfo93)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="list"></param>
        public void SetHisStudInfo93(TrainingHistoryViewModel model, IList<HistoryStudentInfo93GridModel> list)
        {
            TrainingHistoryGrid2Model item = null;

            if (list != null)
            {
                foreach (var classItem in list)
                {
                    if (model.Grid == null)
                    {
                        model.Grid2 = new List<TrainingHistoryGrid2Model>();
                    }

                    item = new TrainingHistoryGrid2Model();

                    item.DISTNAME = classItem.DISTNAME;
                    item.PLANNAME = classItem.PLANNAME;
                    item.ORGNAME = classItem.TRINUNIT;
                    item.CLASSNAME = classItem.CLASSNAME;
                    item.STDATE = classItem.SDATE;
                    item.FTDATE = classItem.EDATE;
                    item.STUDSTATUS_TEXT = "結訓"; //預設結訓

                    model.Grid2.Add(item);
                }
            }
        }

        /// <summary>
        /// 轉換取得訓練狀態代碼中文描述
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetStudStatusText(int studStatus)
        {
            string rtn = string.Empty;

            switch (studStatus)
            {
                case 1://"1":
                    rtn = "在訓";
                    break;
                case 2:// "2":
                    rtn = "離訓";
                    break;
                case 3://"3":
                    rtn = "退訓";
                    break;
                case 4://"4":
                    rtn = "續訓";
                    break;
                case 5://"5":
                    rtn = "結訓";
                    break;
                case 9://"9":
                    rtn = "不符參訓資格";
                    break;
            }

            return rtn;
        }

        /// <summary>
        /// 載入顯示學員記錄於產投系統的資料（class_studentsofclass/class_studentsofclassdeldata）
        /// </summary>
        /// <param name="model"></param>
        /// <param name="list"></param>
        public void SetHisClassStudsOfClass(TrainingHistoryViewModel model, IList<HistoryClassStudsOfClassGridModel> list)
        {
            TrainingHistoryGrid2Model item = null;
            string studStatusText = string.Empty;

            if (list != null)
            {
                foreach (var classItem in list)
                {
                    if (model.Grid2 == null)
                    {
                        model.Grid2 = new List<TrainingHistoryGrid2Model>();
                    }

                    item = new TrainingHistoryGrid2Model();

                    item.DISTNAME = classItem.DISTNAME;
                    item.ORGNAME = classItem.ORGNAME;
                    item.CLASSNAME = classItem.CLASSNAME;
                    item.PLANNAME = classItem.PLANNAME;
                    item.YEARS = classItem.YEARS;
                    item.STDATE = classItem.STDATE;
                    item.TRAINHOURS = Convert.ToString(classItem.TRAINHOURS);

                    switch (classItem.STUDSTATUS) //訓練狀態，以 Class_StudentsOfClass 為優先資料顯示 Class_ClassInfo 為副
                    {
                        case 2: //離訓
                            item.FTDATE = classItem.REJECTTDATE1;//結訓日=離訓日
                            item.THOURS = string.Format("<span color='red'>{0}</span>", item.TRAINHOURS);

                            break;
                        case 3: //退訓
                            item.FTDATE = classItem.REJECTTDATE2;//結訓日=退訓日
                            item.THOURS = string.Format("<span color='red'>{0}</span>", item.TRAINHOURS);

                            break;
                        default:
                            item.FTDATE = classItem.FTDATE;
                            item.THOURS = Convert.ToString(classItem.THOURS); //參訓時數以 Class_StudentsOfClass 為優先資料顯示 Class_ClassInfo 為副

                            break;
                    }

                    item.WEEKS = classItem.WEEKS; //上課時間
                    item.STUDSTATUS = classItem.STUDSTATUS;
                    item.RTREASONID = classItem.RTREASONID;
                    item.RTREASONID_TEXT = classItem.RTREASONID_TEXT;
                    item.STUDSTATUS_TEXT = this.GetStudStatusText(item.STUDSTATUS);

                    //學員訓練狀態顯示離退訓原因
                    switch (item.STUDSTATUS)
                    {
                        case 2: //離訓
                        case 3: //退訓
                            //顯示離退原因
                            if (!string.IsNullOrWhiteSpace(classItem.RTREASONID_TEXT))
                            {
                                item.STUDSTATUS_TEXT += "：" + classItem.RTREASONID_TEXT.Trim();
                            }

                            //顯示離退原因(其他)
                            if (!string.IsNullOrWhiteSpace(classItem.RTREASOOTHER))
                            {
                                item.STUDSTATUS_TEXT += "(" + classItem.RTREASOOTHER.Trim() + ")";
                            }
                            break;
                    }

                    model.Grid2.Add(item);
                }
            }
        }

        public static bool ValidateServerCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //using System.Net.Security;
            //using System.Security.Cryptography.X509Certificates;
            return true;
        }

        /// <summary>
        /// 呼叫職前系統webservice，以取得職前系統相關計畫的參訓歷程
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public IList<WsGetTrainingGridModel> GetWsTraining(string idno)
        {
            IList<WsGetTrainingGridModel> rtn = null;
            WsGetTrainingGridModel item = null;

            //來自職前回傳的參訓歷程要再排除不顯示的計畫資料
            var planlist = new List<string>() { "06", "07", "28", "54", "70" };
            ArrayList planConds = new ArrayList(planlist);

            //WebRequest物件如何忽略憑證問題
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
            //TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072
            //查詢職前系統參訓紀錄(webservice)
            //GetTraining.GetTrainingList wsGTL = new GetTraining.GetTrainingList();
            //http://wltims.wda.gov.tw/ITSWebc/twsITSWebc.asmx
            //https://ojfile119.ejob.gov.tw/ITSWebc/twsITSWebc.asmx
            ITSWebc.twsITSWebc GTL_2 = new ITSWebc.twsITSWebc();
            //DataSet ds = new DataSet();
            //查詢職前參訓資料
            string cst_s_AUTH = "ce130f098d90e52491a8097fcd119b4d";
            ITSWebc.ResultOfSTUDENT_TRAINING_HISTORY_VIEW ITSrc1 = new ITSWebc.ResultOfSTUDENT_TRAINING_HISTORY_VIEW();
            ITSrc1 = GTL_2.GetStudHis1(cst_s_AUTH, idno, "");
            //LOG.Debug(string.Format("#ITSrc1.Count :{0}", ITSrc1.Count));
            if (ITSrc1 == null) return rtn;
            if (ITSrc1.Count == 0) return rtn;
            rtn = new List<WsGetTrainingGridModel>();

            //GetTraining.TrainingList[] wslist = wsGTL.ReturnList(cstSYSNAME, idno);
            //if (wslist != null) { }
            foreach (ITSWebc.STUDENT_TRAINING_HISTORY_VIEW mlist in ITSrc1.Records)
            {
                //排除非職前的資料--'略過-計畫為在職
                //if (planConds.Contains(mlist.TPLANID)) continue;
                //if (planConds.Contains(mlist.TPLANID)) continue;

                item = new WsGetTrainingGridModel();
                item.STDATE = mlist.CDI_BEGIN_DATE; //'訓練日期（起）STDate 
                item.FTDATE = mlist.CDI_END_DATE; // '訓練日期（迄）FTDate
                item.IDNO = mlist.IDNO; // ' 身分證字號
                item.NAME = mlist.CNAME; // '姓名 CNAME/NAME
                item.BIRTHDAY = mlist.BIRTHDAY;  //'出生日期 DateTime
                item.DISTNAME = mlist.DISTNAME; // '分署名稱
                item.COMIDNO = mlist.BNA; // '訓練單位統一編號 COMIDNO
                item.DISTID = mlist.DISTID; // '分署代號
                item.YEARS = (mlist.TWYEAR + 1911);  //'int 年度 民國
                item.TPLANID = mlist.TPLANID; // '舊系統計畫KEY
                item.PLANNAME = mlist.NTPNAME; // 'PLANNAME 計畫名稱
                item.ORGNAME = mlist.BTINAME; // 'ORGNAME 訓練單位名稱
                item.TRAINNAME = mlist.TRAINNAME; // '訓練職類
                item.CJOB_NAME = mlist.CGJT_NAME; // 'CJOB_NAME'通俗職類
                // 'CLASSCNAME 班級名稱
                item.CLASSCNAME = mlist.CLASSNAME;
                // '開結訓日期組合
                string s_TROUND = string.Format("{0}-{1}", mlist.CDI_BEGIN_DATE.ToString("yyyy/MM/dd"), mlist.CDI_END_DATE.ToString("yyyy/MM/dd"));
                item.TROUND = s_TROUND;
                //'dr("WEEKS") = mlist.WEEKS // '上課時間
                item.THOURS = mlist.THOURS; // 'int 班級訓練時數
                item.TFLAG = mlist.CSS_STATUS; // 'TFLAG 訓練狀態(中文)
                item.WORKSUPPIDENT = mlist.WORKSUPPIDENT; // 'mlist.WorkSuppIdent 'CHG_YESNO(mlist.WorkSuppIdent) 是否為在職者補助身份
                //'int 補助金額
                item.SUMOFMONEY = mlist.SUMOFMONEY;
                //'dr("PayMoney") = mlist.PayMoney 'int 個人支付
                //  '審核備註
                item.APPLIEDNOTE = mlist.APPLIEDNOTE;
                //'dr("SupplyId") = mlist.SupplyId '補助比例 代碼 (中文)
                //'dr("MEMO1") = mlist.MEMO1 '備註欄位 可顯示「在職者」或「待業者」
                item.BUDID = mlist.CBTID; // 'BudID 預算別
                // 'BudIDName 預算別(中文)
                item.BUDIDNAME = mlist.CBTNAME;
                //'dr("AllotDate") = mlist.AllotDate '撥款日期
                item.APPLIEDSTATUSM = mlist.APPLIEDSTATUSM; // '學員經費審核狀態-申請
                //'dr("AppliedStatusMName") = mlist.AppliedStatusMName '學員經費審核狀態(中文)
                item.APPLIEDSTATUS = mlist.APPLIEDSTATUS; // '學員經費撥款狀態
                //'dr("AppliedStatusName") = mlist.AppliedStatusName '學員經費撥款狀態(中文)
                item.STUDSTATUS = mlist.CSSNO; // 'StudStatus 訓練狀態代號
                item.STUDSTATUSNAME = mlist.CSS_STATUS; // 'StudStatusName '訓練狀態(中文)
                rtn.Add(item);

                //排除非職前的資料--'略過-計畫為在職
                //item.IDNO = classItem.IDNO;
                //item.NAME = classItem.NAME;
                //item.BIRTHDAY = classItem.BIRTHDAY;
                //item.DISTNAME = classItem.DISTNAME;
                //item.COMIDNO = classItem.COMIDNO;
                //item.DISTID = classItem.DISTID;
                //item.YEARS = classItem.YEARS; //int
                //item.TPLANID = classItem.TPLANID;
                //item.PLANNAME = classItem.PLANNAME;
                //item.ORGNAME = classItem.ORGNAME;
                //item.CJOB_NAME = classItem.CJOB_NAME;
                //item.CLASSCNAME = classItem.CLASSCNAME;
                //item.THOURS = classItem.THours; //int
                //item.TROUND = classItem.TRound;
                //item.WEEKS = classItem.WEEKS;
                //item.TFLAG = classItem.StudStatus;
                //item.WORKSUPPIDENT = classItem.WorkSuppIdent;
                //item.SUMOFMONEY = classItem.SumOfMoney; //int
                //item.PAYMONEY = classItem.PayMoney; //int
                //item.APPLIEDNOTE = classItem.AppliedNote;
                //item.SUPPLYID = classItem.SupplyId;
                //item.MEMO1 = classItem.MEMO1;
                //item.BUDID = classItem.BudID;
                //item.BUDIDNAME = classItem.BudIDName;
                //item.ALLOTDATE = classItem.AllotDate;
                //item.APPLIEDSTATUSM = classItem.AppliedStatusM;
                //item.APPLIEDSTATUSMNAME = classItem.AppliedStatusMName;
                //item.APPLIEDSTATUS = classItem.AppliedStatus;
                //item.APPLIEDSTATUSNAME = classItem.AppliedStatusName;
                //item.STUDSTATUS = classItem.StudStatus;
                //item.STUDSTATUSNAME = classItem.StudStatusName;
                //rtn.Add(item);
            }
            return rtn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="list"></param>
        public void SetWsTraining(TrainingHistoryViewModel model, IList<WsGetTrainingGridModel> list)
        {
            TrainingHistoryGrid2Model item = null;

            if (list != null)
            {
                foreach (var classItem in list)
                {
                    if (model.Grid2 == null)
                    {
                        model.Grid2 = new List<TrainingHistoryGrid2Model>();
                    }

                    item = new TrainingHistoryGrid2Model();

                    item.DISTNAME = classItem.DISTNAME;
                    item.YEARS = Convert.ToString(classItem.YEARS);
                    item.PLANNAME = classItem.PLANNAME;
                    item.ORGNAME = classItem.ORGNAME;
                    item.CLASSNAME = classItem.CLASSCNAME;
                    item.THOURS = Convert.ToString(classItem.THOURS);
                    item.STDATE = classItem.STDATE;
                    item.FTDATE = classItem.FTDATE;
                    item.STUDSTATUS = classItem.STUDSTATUS;
                    item.STUDSTATUS_TEXT = this.GetStudStatusText(item.STUDSTATUS);

                    model.Grid2.Add(item);
                }
            }
        }
        #endregion

        private WDAIIPWEBDAO dao;
        private const decimal cstAlertCost6 = 60000;
        private const string money6Msg = "提醒您，預估您目前補助費使用已達6萬元（包含已核撥、參訓中、已報名的課程），請您留意！";
        private const decimal cstAlertCost9 = 90000;
        private const string money9Msg = "提醒您，預估您目前補助費使用已達9萬元（包含已核撥、參訓中、已報名的課程），請您留意！";
        private const string titleS1 = "<span color=Red>前次3年補助額:</span>";
        private const string titleS2 = "<span color=Red>最新3年補助額:</span>";

        private const string cst_AppliedStatusMMsg_Y = "審核通過";
        private const string cst_AppliedStatusMMsg_N = "審核不通過";
        private const string cst_AppliedStatusMMsg_R = "退件修正";
        private const string cst_AppliedStatusMMsg_S = "審核中";
        private const string cst_AppliedStatusMMsg_NOINFO = "無資訊";
        private const string cst_AppliedStatusM_NOINFO = "NOINFO";

        private const string cst_AppliedStatusMsg_1 = "已撥款";
        private const string cst_AppliedStatusMsg_Y = "待撥款"; //撥款中";
        private const string cst_AppliedStatusMsg_N = "不撥款";
        private const string cst_AppliedStatusMsg_R = "未撥款";
        private const string cst_AppliedStatusMsg_X = "不予補助";
        private const string cst_AppliedStatusMsg_NOINFO = "無資訊";

        public WDAIIPWEBService()
        {
            this.dao = new WDAIIPWEBDAO();
        }

        public WDAIIPWEBService(WDAIIPWEBDAO dao)
        {
            if (dao == null)
            {
                throw new ArgumentNullException("dao 不能為 null");
            }

            this.dao = dao;
        }

        #region (近三年內課程報名及參訓情形) (check_2)
        /// <summary>
        /// 記錄webservice
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dt"></param>
        public void StoreSubsidyDefStdCost(TrainingSubsidyViewModel model, DataTable dt)
        {
            TrainingSubsidyGridModel item = null;

            if (dt == null) { return; }

            model.NearYearDetail.Grid = new List<TrainingSubsidyGridModel>();
            foreach (DataRow dr in dt.Rows)
            {
                item = new TrainingSubsidyGridModel();

                item.PLANYEAR = Convert.ToString(dr["YEARS"]);
                item.PLANNAME = Convert.ToString(dr["PLANNAME"]);
                item.ORGNAME = Convert.ToString(dr["OrgName"]);
                item.CLASSCNAME = Convert.ToString(dr["ClassCName"]);
                item.STDATE = Convert.ToDateTime(dr["STDATE"]);
                item.FTDATE = Convert.ToDateTime(dr["FTDATE"]);
                item.APPLIEDSTATUS = Convert.ToString(dr["AppliedStatus"]);
                item.APPLIEDSTATUSM = Convert.ToString(dr["AppliedStatusM"]);

                if (!string.IsNullOrEmpty(Convert.ToString(dr["SumOfMoney"])))
                {
                    item.SUMOFMONEY = Convert.ToInt64(dr["SumOfMoney"]);
                }

                item.SDSTATUS = Convert.ToString(dr["SDStatus"]);

                model.NearYearDetail.Grid.Add(item);
            }

            if (model.NearYearDetail.Grid != null && model.NearYearDetail.Grid.Count > 0)
            {
                model.NearYearDetail.Grid = model.NearYearDetail.Grid.OrderBy(x => x.STDATE).ToList();
            }
        }

        /// <summary>
        /// 查詢邏輯處理作業-近三年內課程報名及參訓情形
        /// </summary>
        /// <param name="model"></param>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        public void ProcessNearYearTrain(NearYearTrainDetailModel model, string idno, string birth, ref string ocids)
        {
            //string ocids = string.Empty;
            //IList<TrainDescGridModel> trainDescList = null;
            NearYearTrainGridModel nearYrClass = null;

            bool blStdFlag = true; //該學員無上課資料，無請領補助資料，只有報名資料
            string sDate = string.Empty;
            string eDate = string.Empty;

            //取出 補助金申請查詢
            model.StdCostGrid = dao.QueryDefStdCost(idno, birth);

            //(近N年) 取得該學員所有報名日期
            //TIMS.GetRelEnterDateDtY3
            model.NearYearGrid = dao.QueryNearYearTrain(idno, birth);

            if (model.StdCostGrid.Count == 0 && model.NearYearGrid != null && model.NearYearGrid.Count > 0)
            {
                //取得近3年所有報名資料（組ocid字串)
                ocids = this.GetSSOCID<NearYearTrainGridModel>(model.NearYearGrid);

                //查詢取得要比對的課程資料
                model.TrainDescList = this.GetTrainDesc(ocids, model.NearYearGrid);

                //取得近3年所有報名資料。(保留有效，刪除無效資料(報名失敗排除)
                //取得目前table的所有ocid以逗點分隔(排除一些沒有課程的資料)
                ocids = this.GetSSOCID<TrainDescGridModel>(model.TrainDescList);

                //該學員無上課資料，無請領補助資料，只有報名資料
                blStdFlag = false;

                //XDAY2 == 1 :某限期內資料筆數('6個月內的報名資料，供計算使用金額比較使用。)
                if (model.NearYearGrid.Where(x => x.XDAY2 == 1).ToList().Count > 0)
                {
                    //有條件的取第1筆資料
                    nearYrClass = model.NearYearGrid.Where(x => x.XDAY2 == 1).OrderBy(x => x.STDATE).ToList().FirstOrDefault();
                }
                else
                {
                    //無條件的取最後1筆資料
                    nearYrClass = model.NearYearGrid[model.NearYearGrid.Count - 1];
                }

                this.CheckGovCost("1", model, idno, nearYrClass.OCID.Value);
            }

            if (blStdFlag && model.NearYearGrid != null && model.NearYearGrid.Count > 0)
            {
                //取得近3年所有報名資料
                ocids = this.GetSSOCID<NearYearTrainGridModel>(model.NearYearGrid);

                //取得要比對的課程資料
                model.TrainDescList = this.GetTrainDesc(ocids, model.NearYearGrid);

                //取得近3年所有報名資料。(保留有效，刪除無效資料(報名失敗排除)
                //取得目前table的所有ocid以逗點分隔(排除一些沒有課程的資料)
                ocids = this.GetSSOCID<TrainDescGridModel>(model.TrainDescList);

                //XDAY2 == 1 :某限期內資料筆數('6個月內的報名資料，供計算使用金額比較使用。)
                if (model.NearYearGrid.Where(x => x.XDAY2 == 1).ToList().Count == 0)
                {
                    //6個月內無資料挑最後一筆資料
                    nearYrClass = model.NearYearGrid[model.NearYearGrid.Count - 1];
                    this.CheckGovCost("1", model, idno, nearYrClass.OCID.Value);
                }
                else
                {
                    /* 有資料,取第1筆資料 */
                    nearYrClass = model.NearYearGrid.Where(x => x.XDAY2 == 1).OrderBy(x => x.STDATE).ToList().FirstOrDefault();

                    //取得三年區間起迄日資料
                    this.GetSubsidyCostDay(idno, nearYrClass.STDATE_AD, ref sDate, ref eDate);

                    model.SDate = sDate;
                    model.EDate = eDate;

                    //補助金使用檢核(顯示目前使用金額)
                    this.CheckGovCost("1", model, idno, nearYrClass.OCID.Value);

                    /* 比較最後一筆 */
                    nearYrClass = model.NearYearGrid[model.NearYearGrid.Count - 1];

                    model.SDate2 = sDate;
                    model.EDate2 = eDate;

                    //若計算區間不相同，顯示該筆資料
                    if (!model.SDate.Equals(model.SDate2))
                    {
                        this.CheckGovCost("2", model, idno, nearYrClass.OCID.Value);
                    }

                    if (!string.IsNullOrEmpty(model.MoneyShow1) && !string.IsNullOrEmpty(model.MoneyShow2))
                    {
                        if (model.MoneyShow1.Equals(model.MoneyShow2))
                        {
                            //相同清除
                            model.MoneyShow2 = string.Empty;
                        }
                        else
                        {
                            //不相同加註
                            //前次
                            model.MoneyShow1 = titleS1 + model.MoneyShow1;
                            model.NearYearGrid2 = dao.QueryNearYearTrain(idno, birth, model.SDate, model.EDate); //XDAY1,XDAY2,XDAY3

                            //本次
                            model.MoneyShow2 = titleS2 + model.MoneyShow2;
                            model.NearYearGrid = dao.QueryNearYearTrain(idno, birth, model.SDate2, model.EDate2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取得所有課程ocid組合字串（以逗點分隔）
        /// ref: TIMS.Get_ssOCID
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string GetSSOCID<T>(IList<T> model)
        {
            string result = string.Empty;

            if (model != null)
            {
                foreach (T item in model)
                {
                    PropertyInfo[] props = item.GetType().GetProperties();
                    //.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (PropertyInfo p in props)
                    {
                        if ("OCID".Equals(p.Name.ToUpper()))
                        {
                            object val = p.GetValue(item);

                            //排除重覆的資料
                            if (result.IndexOf(Convert.ToString(val)) == -1)
                            {
                                result += (string.IsNullOrEmpty(result) ? "" : ",") + val;
                            }
                            else
                            {
                                result += val;
                            }

                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 取得要比對的課程資料
        /// ref: TIMS.Get_TRAINDESC
        /// </summary>
        /// <param name="ocids"></param>
        /// <param name="trainlist"></param>
        /// <returns></returns>
        public IList<TrainDescGridModel> GetTrainDesc(string ocids, IList<NearYearTrainGridModel> trainlist)
        {
            IList<TrainDescGridModel> result = null;

            result = dao.QueryTrainDesc(ocids);

            if (trainlist != null)
            {
                //保留可能錄訓的班級，排除無效班級
                foreach (var trainClass in trainlist)
                {
                    //0:收件完成 1:報名成功 2:未錄取 3:正取 4:備取 5:未錄取
                    switch (trainClass.SIGNUPSTATUS)
                    {
                        case 0:
                        case 1:
                        case 3:
                            break;
                        default:
                            if (result != null)
                            {
                                if (result.Where(x => x.OCID == trainClass.OCID).ToList().Count > 0)
                                {
                                    for (int i = result.Count - 1; i >= 0; i--)
                                    {
                                        var trainDesc = result[i];
                                        if (trainDesc.OCID == trainClass.OCID)
                                        {
                                            result.RemoveAt(i);
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 輔助金使用檢核
        /// ref check_2 Check_GovCost
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="ocid"></param>
        /// <returns></returns>
        public void CheckGovCost(string chkMoneyType, NearYearTrainDetailModel model, string idno, Int64 ocid)
        {
            //IList<GovCostGridModel> govCostList = dao.QueryGovCost(ocid);
            GovCostGridModel govCost = null;

            string lociDate = string.Empty; //本班的結訓日期 '本班的開訓日期 (最後使用經費日期)
            decimal actSubsidyCost = 0; //已實際請領補助費(限定產業人才投資方案)
            decimal signSubsidyCost = 0; //已報名申請補助費(限定產業人才投資方案)
            decimal defGovCost = 0; //(=線上報名預算的政府補助)(全部)(限定產業人才投資方案)
            string ccSTDate = string.Empty;
            string ccFTDate = string.Empty;
            string ccClsDefGovCost = string.Empty;
            string ccClsTtlCost = string.Empty;

            //查詢取得政府負擔補助費用資訊
            govCost = dao.QueryGovCost(ocid).ToList().FirstOrDefault();

            lociDate = govCost.STDATE_AD; //本班的開訓日期 (最後使用經費日期)
            string sDate = "";
            string eDate = "";
            //string clsDefGovCost = ""; //此班級的政府補助額－每人費用－本班政府補助預算
            //string clsTtlCost = ""; //課程總費用

            //取得審核區間日期3年為一期
            this.GetSubsidyCostDay(idno, lociDate, ref sDate, ref eDate);

            actSubsidyCost = this.GetActSubsidyCost28(idno, sDate, eDate);
            signSubsidyCost = this.GetSignSubsidyCost28(idno, sDate, eDate);
            defGovCost = this.GetDefGovCost28(idno, sDate, eDate);

            string msg = string.Format("實際核撥補助費用 {0} 元，參訓中課程預估補助費用 {1} 元，報名中課程預估補助費用 {2} 元"
                        , actSubsidyCost.ToString("0"), signSubsidyCost.ToString("0"), defGovCost.ToString("0"));
            switch (chkMoneyType)
            {
                case "1":
                    model.MoneyShow1 = msg;
                    break;
                case "2":
                    model.MoneyShow2 = msg;
                    break;
            }

            bool fg_test = ConfigModel.CHK_IS_TEST_ENVC; //(fg_test || fg_use1)
            bool fg_use1 = (DateTime.Now.Year >= 2025);
            //☆警示額度
            string s_AlertCostMsg = string.Empty;
            if (fg_test || fg_use1)
            {
                //使用已達6萬元
                if ((actSubsidyCost + signSubsidyCost + defGovCost) >= cstAlertCost6) { s_AlertCostMsg = money6Msg; }
            }
            else
            {
                //使用已達9萬元
                if ((actSubsidyCost + signSubsidyCost + defGovCost) >= cstAlertCost9) { s_AlertCostMsg = money9Msg; }
            }
            model.Money69Msg = s_AlertCostMsg;
        }

        /// <summary>
        /// 取得審核區間日期3年為一期 
        /// INPUT:IDNO,STDate  
        /// OUTPUT:sDate,eDate
        /// ref TIMS.Get_SubSidyCostDay
        /// </summary>
        /// <param name="idno">身分證號</param>
        /// <param name="STDate">訓練起日yyyy/MM/dd</param>
        /// <param name="sDate">訓後三年區間起日yyyy/MM/dd</param>
        /// <param name="eDate">訓後亖年區間迄日yyyy/MM/dd</param>
        public void GetSubsidyCostDay(string idno, string STDate, ref string sDate, ref string eDate)
        {
            bool blChk = false;
            bool blErrChk = false;

            if (string.IsNullOrEmpty(STDate))
            {
                STDate = DateTime.Now.ToString("yyyy/MM/dd");
            }

            while (!blChk)
            {
                try
                {
                    //先試著取審核過/參訓過的三年區間
                    this.GetYearsPeriod(idno, ref sDate, ref eDate);

                    //沒取到 就直接用目前的開訓日期的三年區間
                    if (string.IsNullOrEmpty(sDate) || string.IsNullOrEmpty(eDate))
                    {
                        sDate = STDate;
                        eDate = Convert.ToDateTime(sDate).AddYears(3).AddDays(-1).ToString("yyyy/MM/dd");
                    }

                    //判斷開訓日期是否落入取得區間，不是的話就重新取得
                    if (!string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                    {
                        if (string.Compare(STDate, sDate) >= 0 && string.Compare(STDate, eDate) <= 0)
                        {
                            //開訓日期在區間內OK
                            blChk = true;
                        }

                        if (!blChk)
                        {
                            //清除開始日期繼續下一輪
                            sDate = string.Empty;
                        }
                    }

                    //異常情況(1)
                    if (!string.IsNullOrEmpty(sDate))
                    {
                        //if (((Convert.ToDateTime(STDate) - Convert.ToDateTime(sDate)).TotalDays / 365) >= 4)
                        if (((Convert.ToDateTime(sDate) - Convert.ToDateTime(STDate)).TotalDays / 365) >= 4)
                        {
                            blErrChk = true;
                        }
                    }

                    //異常情況(2)
                    if (!string.IsNullOrEmpty(eDate))
                    {
                        //if (((Convert.ToDateTime(STDate) - Convert.ToDateTime(eDate)).TotalDays / 365) >= 4)
                        if (((Convert.ToDateTime(eDate) - Convert.ToDateTime(STDate)).TotalDays / 365) >= 4)
                        {
                            blErrChk = true;
                        }
                    }

                    if (blErrChk)
                    {
                        sDate = STDate;
                        eDate = Convert.ToDateTime(sDate).AddYears(3).AddDays(-1).ToString("yyyy/MM/dd");
                        blChk = true; //false持續迴圈 true:離開迴圈
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error("GetSubsidyCostDay ex:" + ex.Message, ex);
                    sDate = STDate;
                    eDate = Convert.ToDateTime(sDate).AddYears(3).AddDays(-1).ToString("yyyy/MM/dd");
                    blChk = true;
                }
            }
        }

        /// <summary>
        /// 取審核/參訓過的三年區間
        /// ref TIMS.Get_YearsPeriod
        /// </summary>
        public void GetYearsPeriod(string idno, ref string sDate, ref string eDate)
        {
            IList<YearsPeriodGridModel> result = null;

            if (!string.IsNullOrEmpty(idno))
            {
                idno = idno.Trim().ToUpper();
            }

            //先試著取審核通過的三年區間

            result = dao.QueryYearsPeriod(idno, eDate);

            if (result != null && result.Count > 0)
            {
                //有資料
                var item = result[0];
                sDate = item.SDATE_AD;
                eDate = item.EDATE_AD;
            }
            else
            {
                //有結束日，表示超過範圍尚未申請，設定新的 補助申請區間(起迄)日期
                if (!string.IsNullOrEmpty(eDate))
                {
                    sDate = Convert.ToDateTime(eDate).AddDays(1).ToString("yyyy/MM/dd");
                    eDate = Convert.ToDateTime(sDate).AddYears(3).AddDays(-1).ToString("yyyy/MM/dd");
                }
            }
        }

        /// <summary>
        /// 取得(本期) 已實際請領補助費(限定產業人才投資方案)
        /// ref TIMS.Get_ActSubsidyCost28
        /// 
        /// 已實際請領補助費
        /// 加入過濾條件，只限定產業人才投資方案的經費
        /// (限定產業人才投資方案) 20090325 BY AMU
        /// 加入補助費用 46 補助辦理保母職業訓練 '47 補助辦理照顧服務員職業訓練 20110223 by AMU
        /// 加入 58 補助辦理托育人員職業訓練 20130426 BY AMU
        /// 限定 '領取產投補助費用 3年5萬的計畫 201107 BY AMU
        /// 實際核撥補助費用 只要是學生結訓，請領就計算 201506 BY AMU
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public decimal GetActSubsidyCost28(string idno, string sDate, string eDate)
        {
            decimal? sumCost = 0; //小數
            IList<SubsidyCost28GridModel> list = null;

            list = dao.QueryActSubsidyCost28(idno, sDate, eDate);

            if (list != null)
            {
                foreach (var item in list)
                {
                    sumCost += item.SUMOFMONEY;
                }
            }

            return sumCost.Value;
        }

        /// <summary>
        /// 取得(本期) 已報名申請補助費(限定產業人才投資方案)
        /// ref TIMS.Get_SignSubsidyCost28
        /// 
        /// 已報名申請補助費
        /// 加入過濾條件，只限定產業人才投資方案的經費
        /// (限定產業人才投資方案) 20090325 BY AMU
        /// 加入補助費用 46 補助辦理保母職業訓練 '47 補助辦理照顧服務員職業訓練 20110223 by AMU
        /// 限定 '領取產投補助費用 3年5萬的計畫 201107 BY AMU
        /// 參訓中課程預估補助費用(尚未請領補助的):只要是學生在訓，就計算，未申請也加入 201506 BY AMU
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public decimal GetSignSubsidyCost28(string idno, string sDate, string eDate)
        {
            decimal? sumCost = 0; //小數
            IList<SubsidyCost28GridModel> list = null;

            list = dao.QuerySignSubsidyCost28(idno, sDate, eDate);

            if (list != null)
            {
                foreach (var item in list)
                {
                    sumCost += item.SUMOFMONEY;
                }
            }

            return sumCost.Value;
        }

        /// <summary>
        /// 查詢政府補助
        /// ref TIMS.Get_DefGeoCost28
        /// 
        /// 線上報名預算的政府補助 
        /// 所有線上報名的歷史，排除已補助的班級經費，所剩下的餘額(=線上報名預算的政府補助)
        /// 加入要開班與尚未達到開訓日的條件( notopen='N' AND STDate>=SYSDATE )
        /// 已為加入要開班 的條件( notopen='N'  ) 20100722 BY AMU
        /// 加入過濾條件，只限定產業人才投資方案的經費
        /// 排除報名失敗與未錄取 (a.SIGNUPSTATUS NOT IN (2,5)) by AMU 20090915
        /// (限定產業人才投資方案) 20090325 BY AMU
        /// 排除離退訓班級 20100826 BY AMU
        /// 加入補助費用 46 補助辦理保母職業訓練 '47 補助辦理照顧服務員職業訓練 20110223 by AMU
        /// 排除當學員為備取或正取時，開訓時間超過15天，未成為學員者 20110223 BY AMU
        /// 限定 '領取產投補助費用 3年5萬的計畫 201107 BY AMU
        /// 報名中課程預估補助費用:只要是報名(排除是學生的，排除審核異常的)，就計算 201506 BY AMU
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <returns></returns>
        public decimal GetDefGovCost28(string idno, string sDate, string eDate)
        {
            decimal? result = 0;
            bool blAddFlag1 = true; //計算該筆補助金(有離退訓班級資料者)
            bool blAddFlag2 = true; //計算該筆補助金(備取或正取時，開訓時間超過15天，未成為學員者)
            bool blAddFlag3 = true; //開訓時間超過15天，未成為學員者2
            decimal defGovCost = 0;

            //查詢參訓學員 (dtStudStatus23)
            var studStatus23List = dao.QueryStudStatus23(idno, sDate, eDate);

            //查詢未成為學員者 排除 報名後，開訓時間超過15天，未成為學員者 (dtSTDateAdd15)
            IList<Hashtable> sTDateAdd15DayAList = dao.QuerySTDateAdd15DayA(idno, sDate, eDate);

            //查詢(錄取作業) 審核成功, 排除 報名後，開訓時間超過15天，未成為學員者 (dtSTDateAdd15b)
            IList<Hashtable> sTDateAdd15DayBList = dao.QuerySTDateAdd15DayB(idno, sDate, eDate);

            //(計算範圍)排除 其他(2:報名失敗 5:未錄取) 資訊 (dt2)
            IList<SubsidyCost28GridModel> defGovCost28List = dao.QueryDefGovCost28(idno, sDate, eDate);

            if (defGovCost28List != null)
            {
                foreach (var govCost28 in defGovCost28List)
                {
                    blAddFlag1 = true;
                    blAddFlag2 = true;
                    blAddFlag3 = true;

                    //有離退訓班級資料者
                    if (studStatus23List != null && studStatus23List.Count > 0)
                    {
                        //排除 離退訓班級
                        if (studStatus23List.Where(x => x["OCID"].ToString() == govCost28.OCID.ToString()).ToList().Count > 0)
                        {
                            //不計算此筆報名資料
                            blAddFlag1 = false;
                        }
                    }

                    //開訓時間超過15天，未成為學員者
                    if (sTDateAdd15DayAList != null && sTDateAdd15DayAList.Count > 0)
                    {
                        //開訓時間超過15天，未成為學員者
                        if (sTDateAdd15DayAList.Where(x => x["OCID"].ToString() == govCost28.OCID.ToString()).ToList().Count > 0)
                        {
                            //不計算此筆報名資料
                            blAddFlag2 = false;
                        }
                    }

                    //開訓時間超過15天，未成為學員者2
                    if (sTDateAdd15DayBList != null && sTDateAdd15DayBList.Count > 0)
                    {
                        //開訓時間超過15天，未成為學員者
                        if (sTDateAdd15DayBList.Where(x => x["OCID"].ToString() == govCost28.OCID.ToString()).ToList().Count > 0)
                        {
                            //不計算此筆報名資料
                            blAddFlag3 = false;
                        }
                    }

                    if (blAddFlag1 && blAddFlag2 && blAddFlag3)
                    {
                        decimal.TryParse(govCost28.DEFGOVCOST, out defGovCost);
                        result += defGovCost;
                    }
                }
            }

            return result.Value;
        }

        /// <summary>
        /// 檢查是否有重複的課程時間 
        /// true:有 false:沒有
        /// ref TIMS.Chk_DoubleDESC
        /// </summary>
        /// <param name="ocid1"></param>
        /// <param name="ocids"></param>
        /// <param name="trainDescList"></param>
        /// <returns></returns>
        public bool ChkDoubleDesc(Int64 ocid1, string ocids, IList<TrainDescGridModel> trainDescList)
        {
            bool result = false;
            //STUD_ENTERDOUBLE  報名重複訊息資料檔
            string[] ocidAry = ocids.Split(',');

            IList<TrainDescGridModel> list1 = trainDescList.Where(x => x.OCID == ocid1).ToList();
            IList<TrainDescGridModel> list2 = null;

            if (list1 != null)
            {
                foreach (var item in list1)
                {
                    list2 = list1.Where(x => ocidAry.Contains(Convert.ToString(x.OCID)) && x.OCID != ocid1
                        && (
                        1 != 1
                        || (x.STRAINDATE1_DT <= item.STRAINDATE1_DT && x.STRAINDATE2_DT >= item.STRAINDATE1_DT)
                        || (x.STRAINDATE1_DT <= item.STRAINDATE2_DT && x.STRAINDATE2_DT >= item.STRAINDATE2_DT)
                        || (x.STRAINDATE1_DT <= item.STRAINDATE1_DT && x.STRAINDATE2_DT >= item.STRAINDATE2_DT)
                        || (x.STRAINDATE1_DT >= item.STRAINDATE1_DT && x.STRAINDATE2_DT <= item.STRAINDATE2_DT)
                        )).ToList();

                    if (list2.Count() > 0)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 顯示近3年參訓課程結果清單
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ocids"></param>
        public void ShowNearYearTrain(TrainingSubsidyViewModel model, string ocids)
        {
            IList<DefStdCostGridModel> stdCostList = null;
            DefStdCostGridModel stdCostItem = null;
            string applyStatusFlag = string.Empty;
            bool blDoubleDesc = false; //重複的課程時間  true.有重複

            if (model.NearYearDetail != null)
            {
                //本次(參訓課程) (DG_ClassInfo)
                if (model.NearYearDetail.NearYearGrid != null)
                {
                    foreach (var item in model.NearYearDetail.NearYearGrid)
                    {
                        item.DataType = "1";

                        //檢核是否有重覆的課程時間
                        if (this.ChkDoubleDesc(item.OCID.Value, ocids, model.NearYearDetail.TrainDescList))
                        {
                            //0:收件完成 1:報名成功 2:未錄取 3:正取 4:備取 5:未錄取
                            switch (item.SIGNUPSTATUS)
                            {
                                case 0:
                                case 2:
                                case 3:
                                    //檢查是否有重複的課程時間
                                    item.DOUBLE_DESC = "1";
                                    item.DOUBLE_DESC_TEXT = "(重)";
                                    blDoubleDesc = true;

                                    break;
                            }
                        }

                        //signUpStatus 0:收件完成 1:報名成功 2:未錄取 3:正取 4:備取 5:未錄取
                        switch (item.SIGNUPSTATUS)
                        {
                            case 0: //收件完成
                                if (item.XDAY1 == 1)
                                {
                                    item.SIGNUPSTATUS_TEXT = "未錄取";
                                }
                                break;

                            case 4: //備取
                                if (item.XDAY3 == 1)
                                {
                                    item.SIGNUPSTATUS_TEXT = "未錄取";
                                }
                                break;

                        }

                        if (item.SIGNUPSTATUS != 2)
                        {
                            item.SIGNUPMEMO = string.Empty;
                        }

                        if (model.NearYearDetail.StdCostGrid != null)
                        {
                            stdCostList = model.NearYearDetail.StdCostGrid.Where(x => x.OCID == item.OCID).ToList();

                            //預設
                            item.SUMOFMONEY = "-";
                            item.SUBSIDYCOST_TEXT = "未申請";

                            if (stdCostList.Count > 0)
                            {
                                stdCostItem = stdCostList.FirstOrDefault();

                                if (stdCostItem.SC_SOCID.HasValue && stdCostItem.SC_SOCID.Value > 0)
                                {
                                    if (string.IsNullOrEmpty(stdCostItem.APPLIEDSTATUS))
                                    {
                                        item.SUBSIDYCOST_TEXT = "審核中";
                                    }
                                    else
                                    {
                                        applyStatusFlag = "0";
                                        if (string.Compare(item.YEARS, "2010") > 0)
                                        {
                                            //2011後的資料
                                            if ("1".Equals(stdCostItem.APPLIEDSTATUS)
                                                && stdCostItem.ALLOTDATE != null
                                                && stdCostItem.ALLOTDATE.HasValue)
                                            {
                                                applyStatusFlag = "1";
                                            }
                                        }
                                        else
                                        {
                                            //2010(含)前資料
                                            if ("1".Equals(stdCostItem.APPLIEDSTATUS))
                                            {
                                                applyStatusFlag = "1";
                                            }
                                        }

                                        switch (applyStatusFlag)
                                        {
                                            case "1":
                                                item.SUBSIDYCOST_TEXT = "已撥款";
                                                item.SUMOFMONEY = (stdCostItem.SUMOFMONEY.HasValue ? Convert.ToString(stdCostItem.SUMOFMONEY) : "-");
                                                break;
                                            case "0":
                                                //學員經費審核狀態-申請 (R退件修正.Y成功.N失敗.NULL失敗)
                                                switch (stdCostItem.APPLIEDSTATUSM)
                                                {
                                                    case "Y": //審核成功
                                                        item.SUBSIDYCOST_TEXT = "審核通過";
                                                        item.SUMOFMONEY = (stdCostItem.SUMOFMONEY.HasValue ? Convert.ToString(stdCostItem.SUMOFMONEY) : "-");

                                                        break;
                                                    default:
                                                        item.SUBSIDYCOST_TEXT = "未通過";

                                                        break;
                                                }

                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //前次(參訓課程)(DG_ClassInfo2)
                if (model.NearYearDetail.NearYearGrid2 != null)
                {
                    applyStatusFlag = string.Empty;

                    foreach (var item in model.NearYearDetail.NearYearGrid2)
                    {
                        item.DataType = "2";
                        item.DOUBLE_DESC_TEXT = string.Empty;

                        //檢核是否有重覆的課程時間
                        if (this.ChkDoubleDesc(item.OCID.Value, ocids, model.NearYearDetail.TrainDescList))
                        {
                            //0:收件完成 1:報名成功 2:未錄取 3:正取 4:備取 5:未錄取
                            switch (item.SIGNUPSTATUS)
                            {
                                case 0:
                                case 2:
                                case 3:
                                    //檢查是否有重複的課程時間
                                    item.DOUBLE_DESC = "1";
                                    item.DOUBLE_DESC_TEXT = "(重)";
                                    blDoubleDesc = true;

                                    break;
                            }
                        }

                        //signUpStatus 0:收件完成 1:報名成功 2:未錄取 3:正取 4:備取 5:未錄取
                        switch (item.SIGNUPSTATUS)
                        {
                            case 0: //收件完成
                                if (item.XDAY1 == 1)
                                {
                                    item.SIGNUPSTATUS_TEXT = "未錄取";
                                }
                                break;
                        }

                        if (item.SIGNUPSTATUS != 2)
                        {
                            item.SIGNUPMEMO = string.Empty;
                        }

                        if (model.NearYearDetail.StdCostGrid != null)
                        {
                            stdCostList = model.NearYearDetail.StdCostGrid.Where(x => x.OCID == item.OCID).ToList();

                            //預設
                            item.SUMOFMONEY = "-";
                            item.SUBSIDYCOST_TEXT = "未申請";

                            if (stdCostList.Count > 0)
                            {
                                stdCostItem = stdCostList.FirstOrDefault();

                                if (stdCostItem.SC_SOCID.HasValue && stdCostItem.SC_SOCID.Value > 0)
                                {
                                    if (string.IsNullOrEmpty(stdCostItem.APPLIEDSTATUS))
                                    {
                                        item.SUBSIDYCOST_TEXT = "審核中";
                                    }
                                    else
                                    {
                                        applyStatusFlag = "0";
                                        if (string.Compare(item.YEARS, "2010") > 0)
                                        {
                                            //2011後的資料
                                            if ("1".Equals(stdCostItem.APPLIEDSTATUS)
                                                && stdCostItem.ALLOTDATE != null
                                                && stdCostItem.ALLOTDATE.HasValue)
                                            {
                                                applyStatusFlag = "1";
                                            }
                                        }
                                        else
                                        {
                                            //2010(含)前資料
                                            if ("1".Equals(stdCostItem.APPLIEDSTATUS))
                                            {
                                                applyStatusFlag = "1";
                                            }
                                        }

                                        switch (applyStatusFlag)
                                        {
                                            case "1":
                                                item.SUBSIDYCOST_TEXT = "已撥款";
                                                item.SUMOFMONEY = (stdCostItem.SUMOFMONEY.HasValue ? Convert.ToString(stdCostItem.SUMOFMONEY) : "-");
                                                break;
                                            case "0":
                                                //學員經費審核狀態-申請 (R退件修正.Y成功.N失敗.NULL失敗)
                                                switch (stdCostItem.APPLIEDSTATUSM)
                                                {
                                                    case "Y": //審核成功
                                                        item.SUBSIDYCOST_TEXT = "審核通過";
                                                        item.SUMOFMONEY = (stdCostItem.SUMOFMONEY.HasValue ? Convert.ToString(stdCostItem.SUMOFMONEY) : "-");

                                                        break;
                                                    default:
                                                        item.SUBSIDYCOST_TEXT = "未通過";

                                                        break;
                                                }

                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (blDoubleDesc)
            {
                model.NearYearDetail.DoubleDescMsg = "為充分運用訓練資源，請避免同時段重疊參訓。您有報名參訓時段重疊之課程，請擇一參訓，並取消餘課程報名資料。<br>如已無法自行取消請主動洽詢訓練單位協助，謝謝。";
            }
        }

        /// <summary>
        /// 顯示補助歷史相關欄位（代碼轉中文...等）
        /// </summary>
        /// <param name="model"></param>
        public void ShowNearYearTrain2(TrainingSubsidyViewModel model, ojt0219ws1.GetOjt0219ws1 ws)
        {
            string strAppliedStatusMText = string.Empty;
            string strAppliedStatusText = string.Empty;
            string lastSTDate = string.Empty; //最後一筆補助歷程的開訓日
            string idno = model.Form.IDNO;

            if (model.NearYearDetail.Grid != null)
            {
                foreach (var item in model.NearYearDetail.Grid)
                {
                    //補助歷程的開訓日
                    lastSTDate = MyHelperUtil.DateTimeToString(item.STDATE);
                    strAppliedStatusMText = string.Empty;
                    strAppliedStatusText = string.Empty;

                    //審核狀態
                    switch (item.APPLIEDSTATUSM)
                    {
                        case cst_AppliedStatusM_NOINFO: //無資訊
                            strAppliedStatusMText = cst_AppliedStatusMMsg_NOINFO;
                            break;
                        case "Y": //審核通過
                            strAppliedStatusMText = cst_AppliedStatusMMsg_Y;
                            break;
                        case "N": //審核不通過
                            strAppliedStatusMText = cst_AppliedStatusMMsg_N;
                            break;
                        case "R": //退件修正
                            strAppliedStatusMText = cst_AppliedStatusMMsg_R;
                            break;
                        default:
                            //審核中
                            if (item.SUMOFMONEY.HasValue && item.SUMOFMONEY > 0) { strAppliedStatusMText = cst_AppliedStatusMMsg_S; }
                            break;
                    }

                    item.APPLIEDSTATUSM_TEXT = strAppliedStatusMText;

                    //撥款狀態
                    if (!string.IsNullOrEmpty(item.APPLIEDSTATUS) && "1".Equals(item.APPLIEDSTATUS))
                    {
                        //已撥款
                        strAppliedStatusText = cst_AppliedStatusMsg_1;
                    }
                    else
                    {
                        switch (item.APPLIEDSTATUSM)
                        {
                            case cst_AppliedStatusM_NOINFO:
                                strAppliedStatusText = cst_AppliedStatusMsg_NOINFO;
                                break;
                            case "Y": //審核通過-待撥款-撥款中
                                strAppliedStatusText = cst_AppliedStatusMsg_Y;
                                break;
                            case "N": //審核不通過-不撥款
                                strAppliedStatusText = cst_AppliedStatusMsg_N;
                                break;
                            case "R": //退件修正-未撥款
                                strAppliedStatusText = cst_AppliedStatusMsg_R;
                                break;
                            default: //審核中
                                switch (item.SDSTATUS)
                                {
                                    case "2":
                                    case "3":
                                        strAppliedStatusText = cst_AppliedStatusMsg_X; //不予補助
                                        break;
                                }
                                break;
                        }
                    }

                    item.APPLIEDSTATUS_TEXT = strAppliedStatusText;
                }

                //const int cst_sumMoneyMax10 = 100000;//string s_sumMoneyMax10 = "超過 10萬";
                bool fg_test = ConfigModel.CHK_IS_TEST_ENVC; //(fg_test || fg_use1)
                bool fg_use1 = (DateTime.Now.Year >= 2025);
                if (model.NearYearDetail.Grid.Count > 0)
                {
                    //合計列
                    TrainingSubsidyGridModel footerItem = new TrainingSubsidyGridModel();

                    //取得最近三年補助日期區間資料
                    string costDay = ws.GetLabCostDay(idno, lastSTDate, " ");

                    if (!string.IsNullOrEmpty(costDay))
                    {
                        string[] tDateAry = costDay.Trim().Split('~');

                        if (!string.IsNullOrEmpty(costDay) && !"~".Equals(costDay.Trim()))
                        {
                            if (tDateAry.Length == 2)
                            {
                                footerItem.STDATE = Convert.ToDateTime(Convert.ToString(tDateAry[0]).Trim());
                                footerItem.FTDATE = Convert.ToDateTime(Convert.ToString(tDateAry[1]).Trim());
                            }
                        }
                    }

                    //取得每3年所使用的補助金 
                    footerItem.SUMOFMONEY = Convert.ToInt64(ws.GetSumOfMoneyCost(idno, lastSTDate));

                    model.NearYearDetail.Grid.Add(footerItem);

                    //----------------------------------------------
                    //取得可補助總額
                    if (fg_test || fg_use1)
                    {
                        model.NearYearDetail.MoneyTotal = ConfigModel.IntSubCostMoneyTotalMax1;//Convert.ToInt64(ws.GetTotalSubCost(idno, lastSTDate));
                    }
                    else
                    {
                        model.NearYearDetail.MoneyTotal = Convert.ToInt64(ws.GetTotalSubCost(idno, lastSTDate));
                    }

                    //取得已使用補助總額
                    model.NearYearDetail.DEFSTDCOST = Convert.ToInt64(ws.GetSumOfMoneyCost(idno, lastSTDate));

                    //取得剩餘可用補助額度
                    model.NearYearDetail.RemainSub = Convert.ToInt64(ws.GetRemSubCost(idno, lastSTDate));
                }
                else
                {
                    if (fg_test || fg_use1)
                    {
                        model.NearYearDetail.MoneyTotal = ConfigModel.IntSubCostMoneyTotalMax1;//Convert.ToInt64(ws.GetTotalSubCost(idno, MyHelperUtil.DateTimeToString(DateTime.Today)));
                    }
                    else
                    {
                        model.NearYearDetail.MoneyTotal = Convert.ToInt64(ws.GetTotalSubCost(idno, MyHelperUtil.DateTimeToString(DateTime.Today)));
                    }
                }
            }
        }
        #endregion

        #region 不記錄瀏覽人次
        /// <summary>HH:mm-HH:mm不記錄瀏覽人次</summary>
        /// <returns></returns>
        public bool StopNORECORDVISITS()
        {
            //調11:50-12:20不記錄瀏覽人次
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            //取得系統時間            
            DateTime aNow = keyDao.GetSysDateNow();
            DateTime sDate = DateTime.Now;
            DateTime eDate = DateTime.Now;

            TblSYS_VAR whereNR = new TblSYS_VAR() { SPAGE = "Config", ITEMNAME = "NORECORDVISITS" };
            TblSYS_VAR whereNRs = new TblSYS_VAR() { SPAGE = "Config", ITEMNAME = "NORECORDVISITS_STIME" };
            TblSYS_VAR whereNRe = new TblSYS_VAR() { SPAGE = "Config", ITEMNAME = "NORECORDVISITS_ETIME" };
            TblSYS_VAR dataNR = dao.GetRow<TblSYS_VAR>(whereNR);
            TblSYS_VAR dataNRs = dao.GetRow<TblSYS_VAR>(whereNRs);
            TblSYS_VAR dataNRe = dao.GetRow<TblSYS_VAR>(whereNRe);
            if (dataNR == null || dataNRs == null || dataNRe == null) { return false; }
            if (string.IsNullOrEmpty(dataNR.ITEMVALUE) || dataNR.ITEMVALUE != "Y") { return false; }
            if (string.IsNullOrEmpty(dataNRs.ITEMVALUE) || string.IsNullOrEmpty(dataNRe.ITEMVALUE)) { return false; }
            if (!DateTime.TryParse(string.Concat(aNow.ToString("yyyy/MM/dd"), " ", dataNRs.ITEMVALUE), out sDate)) return false;
            if (!DateTime.TryParse(string.Concat(aNow.ToString("yyyy/MM/dd"), " ", dataNRe.ITEMVALUE), out eDate)) return false;

            return (DateTime.Compare(aNow, sDate) >= 0 && DateTime.Compare(aNow, eDate) <= 0);
        }

        #endregion

        #region  訊息處理       
        /// <summary>檢測是否停止報名</summary>
        /// <returns></returns>
        public bool StopEnterTempMsg()
        {
            BaseDAO dao = new BaseDAO();
            const string cst_StopFlag1 = "SE";  //SE:產投停止報名

            SessionModel sm = SessionModel.Get();
            //IList<HomeNews3ExtModel> homeNews3 = null;
            bool result = false;

            //string altMsg = string.Empty;        //訊息
            //string altMsgSDate = string.Empty;   //訊息公佈日
            //string altMsgEDate = string.Empty;   //訊息結束日
            string errMsg = string.Empty;

            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            string altMsg = keyDao.GetSystemMsg("AltMsg", cst_StopFlag1);//訊息
            string altMsgSDate = keyDao.GetSystemMsg("AltMsgSDate", cst_StopFlag1);//訊息公佈日
            string altMsgEDate = keyDao.GetSystemMsg("AltMsgEDate", cst_StopFlag1);//訊息結束日

            //取得系統時間            
            DateTime aNow = keyDao.GetSysDateNow();
            errMsg = this.GetAltMsgSystemMsg(altMsg, altMsgSDate, altMsgEDate, aNow);
            if (!string.IsNullOrEmpty(errMsg))
            {
                sm.LastResultMessage = errMsg;
                result = true;
                return result;
            }

            IList<HomeNews3ExtModel> homeNews3 = dao.QueryForListAll<HomeNews3ExtModel>("Share.queryStopEnterMsg", null);
            if (homeNews3 != null)
            {
                foreach (var item in homeNews3)
                {
                    altMsg = item.SUBJECT;
                    altMsgSDate = (item.STOPSDATE.HasValue ? item.STOPSDATE.Value.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty);
                    altMsgEDate = (item.STOPEDATE.HasValue ? item.STOPEDATE.Value.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty);
                    errMsg = this.GetAltMsgSystemMsg(altMsg, altMsgSDate, altMsgEDate, aNow);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        sm.LastResultMessage = errMsg;
                        result = true;
                        return result;
                    }
                }
            }

            //sm.LastResultMessage = string.Empty;
            if (!string.IsNullOrEmpty(errMsg))
            {
                sm.LastResultMessage = errMsg;
                result = true;
                return result;
            }
            return result;
        }

        /// <summary>System_Msg 處理 ref: TIMS.Get_AltMsg_System_Msg</summary>
        /// <param name="altMsg"></param>
        /// <param name="altMsgSDate"></param>
        /// <param name="altMsgEDate"></param>
        /// <param name="aNow"></param>
        /// <returns></returns>
        public string GetAltMsgSystemMsg(string altMsg, string altMsgSDate, string altMsgEDate, DateTime? aNow)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(altMsg)) return result;
            if (string.IsNullOrEmpty(altMsgSDate)) return result;
            if (string.IsNullOrEmpty(altMsgEDate)) return result;
            DateTime sDate = DateTime.Now;
            if (!DateTime.TryParse(altMsgSDate, out sDate)) return result;
            DateTime eDate = DateTime.Now;
            if (!DateTime.TryParse(altMsgEDate, out eDate)) return result;
            //DateTime sDate = DateTime.Now;
            //DateTime eDate = DateTime.Now;
            //DateTime.TryParse(altMsgSDate, out sDate);
            if (!aNow.HasValue) aNow = DateTime.Now;
            if (DateTime.Compare(aNow.Value, sDate) >= 0 && DateTime.Compare(aNow.Value, eDate) <= 0)
            {
                result = altMsg;
            }
            return result;
        }

        #endregion

        #region 會員報名資料維護(課程查詢/會員專區)
        /// <summary> 載入資料 </summary>
        public void LoadEnterData(SignUpViewModel model)
        {
            //1.使用前次報名資料 STUD_STUDENTINFO / STUD_ENTERTEMP2
            this.LoadTemp2(model);

            //2.使用Stud_EnterTemp3 報名資料維護。
            this.LoadTemp3(model);

            //3.登入成功 '採用 Member 資料
            this.LoadMember(model);

            //4.已完成身分驗證
            this.LoadUploadIMG(model);
        }

        /// <summary>
        /// 載入在職進修報名資料(使用前次報名資料 Stud_StudentInfo / Stud_EnterTemp2)
        /// SHOW_Stud_EnterTemp12
        /// </summary>
        /// <param name="model"></param>
        private void LoadTemp2(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            SignUpDetailModel temp2Info = dao.GetStudEnterTmp12();

            LoadMemberBase(model, temp2Info, "1");
        }

        /// <summary>
        /// 載入產投報名資料(使用 Stud_EnterTemp3 報名資料維護)
        /// </summary>
        /// <param name="model"></param>
        private void LoadTemp3(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel temp3Info = dao.GetStudEnterTmp3();

            if (temp3Info == null)
            {
                model.Detail.ISPLAN28 = "N";
            }
            else
            {
                LoadMemberBase(model, temp3Info, "2");

                if (!string.IsNullOrEmpty(model.Detail.NAME))
                {
                    model.Detail.NAME3 = HttpUtility.HtmlDecode(model.Detail.NAME);
                }

                model.Detail.ESETID3 = temp3Info.ESETID3;
                /*開始載入產投報名用相關欄位*/
                model.Detail.ZIPCODE2 = temp3Info.ZIPCODE2;
                model.Detail.ZIPCODE2_6W = temp3Info.ZIPCODE2_6W;
                model.Detail.ZIPCODE2_2W = MyCommonUtil.GET_ZIPCODE2W(temp3Info.ZIPCODE2_6W, null);// temp3Info.ZIPCODE2_2W; 
                model.Detail.HOUSEHOLDADDRESS = temp3Info.HOUSEHOLDADDRESS;
                model.Detail.MIDENTITYID = temp3Info.MIDENTITYID;
                model.Detail.PRIORWORKPAY = temp3Info.PRIORWORKPAY;

                //郵政/銀行帳號資訊
                model.Detail.ACCTMODE = temp3Info.ACCTMODE;
                model.Detail.POSTNO = "";
                model.Detail.BANKNAME = "";
                model.Detail.ACCTHEADNO = "";
                model.Detail.EXBANKNAME = "";
                model.Detail.ACCTEXNO = "";
                model.Detail.ACCTNO = "";

                //0:郵局帳號 1:銀行帳號 2:訓練單位代轉現金
                switch (temp3Info.ACCTMODE)
                {
                    case 0:
                        //郵局 //局號
                        model.Detail.POSTNO = (!string.IsNullOrEmpty(temp3Info.POSTNO)) ? temp3Info.POSTNO.Replace("-", "") : temp3Info.POSTNO;
                        //帳號
                        model.Detail.POST_ACCTNO = (!string.IsNullOrEmpty(temp3Info.ACCTNO)) ? temp3Info.ACCTNO.Replace("-", "") : temp3Info.ACCTNO;
                        break;
                    case 1: //銀行
                        model.Detail.BANKNAME = temp3Info.BANKNAME;
                        model.Detail.ACCTHEADNO = temp3Info.ACCTHEADNO;
                        model.Detail.EXBANKNAME = temp3Info.EXBANKNAME;
                        model.Detail.ACCTEXNO = temp3Info.ACCTEXNO;
                        model.Detail.BANK_ACCTNO = temp3Info.ACCTNO;
                        break;
                }

                model.Detail.ISAGREE = temp3Info.ISAGREE;
                model.Detail.ISEMAIL = temp3Info.ISEMAIL;

                //服務單位資料
                model.Detail.UNAME = temp3Info.UNAME;
                model.Detail.INTAXNO = temp3Info.INTAXNO;
                model.Detail.SERVDEPTID = temp3Info.SERVDEPTID;
                model.Detail.ACTNAME = temp3Info.ACTNAME;
                model.Detail.ACTTYPE = temp3Info.ACTTYPE;
                model.Detail.ACTNO = temp3Info.ACTNO;
                model.Detail.ACTTEL = temp3Info.ACTTEL;
                model.Detail.ZIPCODE3 = temp3Info.ZIPCODE3;
                model.Detail.ZIPCODE3_6W = temp3Info.ZIPCODE3_6W;
                model.Detail.ZIPCODE3_2W = MyCommonUtil.GET_ZIPCODE2W(temp3Info.ZIPCODE3_6W, null);//temp3Info.ZIPCODE3_2W;
                model.Detail.ACTADDRESS = temp3Info.ACTADDRESS;
                model.Detail.JOBTITLEID = temp3Info.JOBTITLEID;

                //參訓背景資料
                model.Detail.Q1 = temp3Info.Q1;
                model.Detail.Q2_1 = temp3Info.Q2_1;
                model.Detail.Q2_1_CHECKED = ("1".Equals(temp3Info.Q2_1));
                model.Detail.Q2_2 = temp3Info.Q2_2;
                model.Detail.Q2_2_CHECKED = ("1".Equals(temp3Info.Q2_2));
                model.Detail.Q2_3 = temp3Info.Q2_3;
                model.Detail.Q2_3_CHECKED = ("1".Equals(temp3Info.Q2_3));
                model.Detail.Q2_4 = temp3Info.Q2_4;
                model.Detail.Q2_4_CHECKED = ("1".Equals(temp3Info.Q2_4));
                model.Detail.Q3 = temp3Info.Q3;
                model.Detail.Q3_OTHER = temp3Info.Q3_OTHER;
                model.Detail.Q4 = temp3Info.Q4;
                model.Detail.Q5 = temp3Info.Q5;
                model.Detail.Q61 = temp3Info.Q61;
                model.Detail.Q62 = temp3Info.Q62;
                model.Detail.Q63 = temp3Info.Q63;
                model.Detail.Q64 = temp3Info.Q64;

                model.Detail.ISPLAN28 = temp3Info.ISPLAN28;
            }
        }

        /// <summary>上傳資料驗證</summary>
        /// <param name="model"></param>
        void LoadUploadIMG(SignUpViewModel model)
        {
            var dao = new WDAIIPWEBDAO();

            SignUpDetailModel mem = dao.GetEMember();

            //上傳銀行存摺
            var getData1 = dao.GetRow(new TblE_IMG1() { IDNO = mem.IDNO, ISUSE = "Y" });
            model.Detail.ISUSE_IMG1 = (getData1 != null);
            if (getData1 != null)
            {
                try
                {
                    var v_savePath = System.Web.HttpContext.Current.Server.MapPath(getData1.FILEPATH1);
                    var v_fileNM1 = System.IO.Path.GetFileName(getData1.FILENAME1);
                    var v_fileNM1W = System.IO.Path.GetFileName(getData1.FILENAME1W);
                    var fg1xNG = !System.IO.File.Exists(string.Concat(v_savePath, v_fileNM1));
                    var fg1WxNG = !System.IO.File.Exists(string.Concat(v_savePath, v_fileNM1W));
                    model.Detail.ERR_IMG1 = (fg1xNG || fg1WxNG);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.Message, ex);
                }
            }

            //上傳身分證件驗證
            var getData2 = dao.GetRow(new TblE_IMG2() { IDNO = mem.IDNO, ISUSE = "Y" });
            model.Detail.ISUSE_IMG2 = (getData2 != null);
            if (getData2 != null)
            {
                try
                {
                    var v_savePath = System.Web.HttpContext.Current.Server.MapPath(getData2.FILEPATH1);
                    var v_fileNM1 = System.IO.Path.GetFileName(getData2.FILENAME1);
                    var v_fileNM1W = System.IO.Path.GetFileName(getData2.FILENAME1W);
                    var v_fileNM2 = System.IO.Path.GetFileName(getData2.FILENAME2);
                    var v_fileNM2W = System.IO.Path.GetFileName(getData2.FILENAME2W);
                    var fg1xNG = !System.IO.File.Exists(string.Concat(v_savePath, v_fileNM1));
                    var fg1WxNG = !System.IO.File.Exists(string.Concat(v_savePath, v_fileNM1W));
                    var fg2xNG = !System.IO.File.Exists(string.Concat(v_savePath, v_fileNM2));
                    var fg2WxNG = !System.IO.File.Exists(string.Concat(v_savePath, v_fileNM2W));
                    model.Detail.ERR_IMG2 = (fg1xNG || fg1WxNG || fg2xNG || fg2WxNG);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.Message, ex);
                }
            }

            //資料不全離開
            if (getData1 == null || !getData1.ACCTMODE.HasValue) { return; }
            model.Detail.ACCTMODE = getData1.ACCTMODE;
            if (getData1.ACCTMODE.Value == 0)
            {
                var fg1 = string.IsNullOrEmpty(model.Detail.POSTNO);
                var fg2 = string.IsNullOrEmpty(model.Detail.POST_ACCTNO);
                if (fg1) model.Detail.POSTNO = getData1.POSTNO;//請輸入局號
                if (fg2) model.Detail.POST_ACCTNO = getData1.ACCTNO;//請輸入帳號
            }
            else if (getData1.ACCTMODE.Value == 1)
            {
                var fg1 = string.IsNullOrEmpty(model.Detail.BANKNAME);
                var fg2 = string.IsNullOrEmpty(model.Detail.ACCTHEADNO);
                var fg3 = string.IsNullOrEmpty(model.Detail.EXBANKNAME);
                var fg4 = string.IsNullOrEmpty(model.Detail.ACCTEXNO);
                var fg5 = string.IsNullOrEmpty(model.Detail.BANK_ACCTNO);
                if (fg1) model.Detail.BANKNAME = getData1.BANKNAME;//請輸入總行名稱
                if (fg2) model.Detail.ACCTHEADNO = getData1.ACCTHEADNO;//請輸入總行代號
                if (fg3) model.Detail.EXBANKNAME = getData1.EXBANKNAME;//請輸入分行名稱
                if (fg4) model.Detail.ACCTEXNO = getData1.ACCTEXNO;//請輸入分行代號
                if (fg5) model.Detail.BANK_ACCTNO = getData1.ACCTNO;//請輸入銀行帳號
            }
        }

        /// <summary>
        /// 載入就業通單一簽入傳送的會員資料
        /// </summary>
        /// <param name="model"></param>
        private void LoadMember(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            SignUpDetailModel memInfo = dao.GetEMember();

            LoadMemberBase(model, memInfo, "3");

            if (memInfo == null) { return; }

            model.Detail.IDNO = memInfo.IDNO;
        }

        /// <summary> 載入會員基本資料 </summary>
        /// <param name="model"></param>
        /// <param name="newDetail"></param>
        /// <param name="dataType"></param>
        private void LoadMemberBase(SignUpViewModel model, SignUpDetailModel newDetail, string dataType)
        {
            //dataType: temp2info:1 /temp3info:2 /memInfo:3
            SignUpDetailModel detail = model.Detail;

            //a:從未參加過產投課程者() : 從台灣就業通登入後, 聯絡電話 / 行動電話 / 電子郵件三個欄位不帶入台灣就業通的資料, 請留空, 設為必填, 由民眾自行在產投網站上填入
            //如學員在報名資料維護與線上報名填寫個人資料時，未填入『聯絡電話』、『行動電話』與『電子郵件』欄位，於醒視窗內增加提醒文字請學員填寫。
            //b.之前參加過產投課程者() : 從台灣就業通登入後, 聯絡電話 / 行動電話 / 電子郵件三個欄位不帶入台灣就業通的資料, 請帶入系統內該學員資料維護中上一次參加產投課程時的資料
            //c.之前報名過產投但是沒有參訓者() : 從台灣就業通登入後, 聯絡電話 / 行動電話 / 電子郵件三個欄位不帶入台灣就業通的資料, 請帶入系統內該民眾上一次報名時的資料
            if (newDetail != null)
            {
                model.Detail.NAME = WebUtility.HtmlDecode(newDetail.NAME); //2019-01-18 修正中文編碼轉置問題(&#XXXXX;)
                model.Detail.IDNO = newDetail.IDNO;
                model.Detail.BIRTHDAY = newDetail.BIRTHDAY;

                //學歷
                if (!string.IsNullOrEmpty(newDetail.DEGREEID))
                {
                    model.Detail.DEGREEID = newDetail.DEGREEID;
                }

                //婚姻狀況  (1.已;2.未 3.暫不提供(預設))
                switch (newDetail.MARITALSTATUS)
                {
                    case 1:
                    case 2:
                        model.Detail.MARITALSTATUS = newDetail.MARITALSTATUS;
                        break;
                    default:
                        model.Detail.MARITALSTATUS = 3;
                        break;
                }

                switch (dataType)
                {
                    case "1": //temp2info:1 

                    case "2": //from stud_entertemp3 //temp3info:2
                        //性別
                        if (!string.IsNullOrEmpty(newDetail.SEX))
                        {
                            model.Detail.SEX = newDetail.SEX;
                        }

                        //身份別
                        if (newDetail.PASSPORTNO.HasValue)
                        {
                            model.Detail.PASSPORTNO = newDetail.PASSPORTNO;
                        }

                        //畢業狀況
                        if (!string.IsNullOrEmpty(newDetail.GRADID))
                        {
                            model.Detail.GRADID = newDetail.GRADID.PadLeft(2, '0');
                        }

                        //學校名稱
                        model.Detail.SCHOOLNAME = newDetail.SCHOOLNAME;
                        //科系名稱
                        model.Detail.DEPARTMENT = newDetail.DEPARTMENT;

                        //2017開始不帶就業通member資料
                        if (!string.IsNullOrEmpty(newDetail.PHONED))
                        {
                            model.Detail.PHONED = newDetail.PHONED;
                        }

                        if (!string.IsNullOrEmpty(newDetail.PHONEN))
                        {
                            model.Detail.PHONEN = newDetail.PHONEN;
                        }

                        if (!string.IsNullOrEmpty(newDetail.CELLPHONE))
                        {
                            model.Detail.CELLPHONE = newDetail.CELLPHONE;
                        }

                        model.Detail.HASMOBILE = (string.IsNullOrWhiteSpace(newDetail.CELLPHONE) ? "N" : "Y");

                        //通訊地址 ZipCode //model.Detail.ZIPCODE = newDetail.ZIPCODE;
                        //model.Detail.ZIPCODE = (!newDetail.ZIPCODE.HasValue || newDetail.ZIPCODE == 0 ? null : newDetail.ZIPCODE);
                        //model.Detail.ZIPCODE_2W = newDetail.ZIPCODE_2W;
                        model.Detail.ZIPCODE = newDetail.ZIPCODE;
                        model.Detail.ZIPCODE_6W = newDetail.ZIPCODE_6W; //MyCommonUtil.GET_ZIPCODE6W(newDetail.ZIPCODE, newDetail.ZIPCODE_2W);
                        model.Detail.ZIPCODE_2W = MyCommonUtil.GET_ZIPCODE2W(newDetail.ZIPCODE_6W, null);
                        model.Detail.ADDRESS = newDetail.ADDRESS;

                        //2017開始不帶就業通member資料
                        model.Detail.HASEMAIL = (string.IsNullOrWhiteSpace(newDetail.EMAIL) || "無".Equals(newDetail.EMAIL) ? "N" : "Y");

                        if (!string.IsNullOrEmpty(newDetail.EMAIL))
                        {
                            model.Detail.EMAIL = newDetail.EMAIL.Trim();
                        }

                        break;

                    case "3": //from member
                        //性別
                        model.Detail.SEX = newDetail.SEX;
                        //身份別
                        model.Detail.PASSPORTNO = newDetail.PASSPORTNO;
                        //'畢業狀況
                        if (string.IsNullOrEmpty(model.Detail.GRADID))
                        {
                            model.Detail.GRADID = (newDetail.GRADID != null) ? newDetail.GRADID.PadLeft(2, '0') : "";
                        }

                        //通訊地址 ZipCode
                        if (string.IsNullOrEmpty(model.Detail.ZIPCODE) && string.IsNullOrEmpty(model.Detail.ZIPCODE_6W) && string.IsNullOrEmpty(model.Detail.ADDRESS))
                        {
                            model.Detail.ZIPCODE = newDetail.ZIPCODE;
                            model.Detail.ZIPCODE_6W = newDetail.ZIPCODE_6W;
                            model.Detail.ZIPCODE_2W = MyCommonUtil.GET_ZIPCODE2W(newDetail.ZIPCODE_6W, null);
                            model.Detail.ADDRESS = newDetail.ADDRESS;
                        }
                        break;
                }
            }
        }
        #endregion

        #region 會員專區-報名記錄
        /// <summary>
        /// 查詢顯示已報名班級資訊
        /// </summary>
        /// <param name="model"></param>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        public void ShowEnterClass(ref EnterClassViewModel model, string idno, DateTime birth)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string eOCIDs = string.Empty;
            //取得-eOCIDs
            IList<TblCLASS_CLASSINFO> type2ClassList = dao.QueryStudEnterType2Class(idno, birth);
            if (type2ClassList != null)
            {
                foreach (TblCLASS_CLASSINFO item in type2ClassList)
                {
                    if (!string.IsNullOrEmpty(eOCIDs)) eOCIDs += ",";
                    eOCIDs += Convert.ToString(item.OCID);
                }
            }

            string timsOCIDs = string.Empty;
            //取得-timsOCIDs-排除eOCIDs
            IList<TblCLASS_CLASSINFO> typeClassList = dao.QueryStudEnterTypeClass(idno, birth, eOCIDs);
            if (typeClassList != null)
            {
                foreach (TblCLASS_CLASSINFO item in typeClassList)
                {
                    if (!string.IsNullOrEmpty(timsOCIDs)) timsOCIDs += ",";
                    timsOCIDs += Convert.ToString(item.OCID);
                }
            }

            //取得-eOCIDs-timsOCIDs(排除eOCIDs) queryAllEnterClass
            model.Grid = dao.QueryAllEnterClass(idno, birth, eOCIDs, timsOCIDs);
            if (model.Grid != null)
            {
                foreach (EnterClassGridModel item in model.Grid)
                {
                    if (!string.IsNullOrEmpty(item.CLASSCNAME2) && "Y".Equals(item.NOTOPENM))
                    {
                        item.CLASSCNAME2 += " 本班不開班";
                    }

                    item.SIGNNO_TEXT = "線上收件成功";
                    item.EXAMNO_TEXT = "";
                    switch (item.TPLANID)
                    {
                        case "28"://產投
                            //產投課程若無拿到報名序號表示收件失敗
                            if (!item.SIGNNO.HasValue || item.SIGNNO <= 0)
                            {
                                item.SIGNNO_TEXT = "線上收件失敗";
                            }
                            else
                            {
                                //2019-01-23 add 產投報名成功加顯示序號資訊
                                item.SIGNNO_TEXT += "(序號：" + item.SIGNNO + ")";
                            }
                            break;

                        default:
                            //case "06"://06:在職 case "70"://70:區域產業據點
                            //報名管道 (1.網;2.現;3.通;4.推)   報名: 網路、現場(含通訊)、推介
                            if (item.ENTERCHANNEL == 2)
                            {
                                //現場報名
                                item.SIGNNO_TEXT = "報名成功";
                                //從 stud_entertype 取得准考證號資料（e網報名審核通過）
                                item.EXAMNO_TEXT = (!string.IsNullOrWhiteSpace(item.EXAMNO) ? "(准考證號：" + item.EXAMNO + ")" : "");
                            }
                            else
                            {
                                //網路報名
                                //報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                                switch (item.SIGNUPSTATUS)
                                {
                                    case 1: //報名成功
                                    case 3: //正取
                                    case 4: //備取
                                    case 5: //未錄取
                                            //從 stud_entertype 取得准考證號資料（e網報名審核通過）
                                        item.EXAMNO_TEXT = (string.IsNullOrWhiteSpace(item.EXAMNO) ? "" : "(准考證號：" + item.EXAMNO + ")");
                                        break;
                                }
                            }
                            break;

                    }

                    //SIGNUPSTATUS 報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                    //SIGNUPSTATUS_NEW 1:審核情形-審核中-"報名失敗":"報名成功"3:正取4:備取5:未錄取/(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                    item.SIGNUPSTATUS_NEW = item.SIGNUPSTATUS;
                    //系統日 < 報名迄日 or 系統日 < 開訓日
                    //未錄取(審核失敗) //在職報名失敗改以未錄取顯示，但仍要顯示報名失敗的選項出來
                    switch (item.TPLANID)
                    {
                        case "28"://產投-只有產投-審核失敗改為未錄取
                            if (item.SIGNUPSTATUS.HasValue && item.SIGNUPSTATUS == 2) { item.SIGNUPSTATUS_NEW = 5; }
                            break;
                    }

                    //系統日若超過開訓日+14 且超過報名結束日 則為1 否為0 (1:啟動'未錄取 顯示功能。)
                    if (item.XDAY14 == 1 && item.XDAY1 == 1 && item.SIGNUPSTATUS.HasValue)
                    {
                        //若超過 則為1
                        //SIGNUPSTATUS 報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                        switch (item.SIGNUPSTATUS)
                        {
                            case 0: //未審/收件
                            case 1: //(審核成功)
                                item.SIGNUPSTATUS_NEW = 1;//(審核中)
                                break;
                            case 2: //(審核失敗)
                            case 5: //未錄取
                                //未審/收件 //(審核失敗) //未錄取
                                item.SIGNUPSTATUS_NEW = 5;//(未錄取)
                                break;
                            case 3: //正取
                            case 4: //備取
                                //正取 //備取
                                break;
                            default:
                                //PTYPE 網路_產投(1) 網路(e網報名)_非產投(2) 現場_現場(3)
                                //ENTERCHANNEL 1.網;2.現;3.通;4.推
                                //其餘皆納入未錄取 (0:收件完成 ,1:審核中(產投)/報名成功(在職) ,2:未錄取(產投審核失敗)/報名失敗(在職))
                                //2019-02-21 add if 非現場報名 PTYPE:3 //5:(未錄取)
                                if (!("3".Equals(item.PTYPE) && item.ENTERCHANNEL == 2)) { item.SIGNUPSTATUS_NEW = 5; }
                                break;
                        }
                    }

                    //超過開訓日14天(超過開訓日)，但沒有學員狀態
                    //SIGNUPSTATUS_NEW 1:審核情形-審核中-"報名失敗":"報名成功"3:正取4:備取5:未錄取
                    //5:(未錄取)
                    if (item.XDAY14 == 1 && item.XDAY1 == 1 && !item.STUDSTATUS.HasValue) { item.SIGNUPSTATUS_NEW = 5; }

                    //只有當報名失敗（signupstatus != 2）時才需顯示原因
                    //SIGNUPSTATUS 報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                    if (item.SIGNUPSTATUS != 2) { item.SIGNUPMEMO = string.Empty; }
                }
            }

        }

        public void ShowEnterClassDel(ref EnterClassDelViewModel model, string idno, DateTime birth)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string eOCIDs = string.Empty;

            //取得-eOCIDs
            IList<TblCLASS_CLASSINFO> type2ClassList = dao.QueryStudEnterType2ClassDel(idno, birth);
            if (type2ClassList != null)
            {
                foreach (TblCLASS_CLASSINFO item in type2ClassList)
                {
                    if (!string.IsNullOrEmpty(eOCIDs)) eOCIDs += ",";
                    eOCIDs += Convert.ToString(item.OCID);
                }
            }

            //取得-eOCIDs
            model.Grid = dao.QueryAllEnterClassDel(idno, birth, eOCIDs);
            if (model.Grid != null)
            {
                foreach (EnterClassDelGridModel item in model.Grid)
                {
                    if (!string.IsNullOrEmpty(item.CLASSCNAME2) && "Y".Equals(item.NOTOPENM)) { item.CLASSCNAME2 += " 本班不開班"; }

                    item.SIGNNO_TEXT = "線上收件成功";
                    switch (item.TPLANID)
                    {
                        case "28"://產投
                            //產投課程若無拿到報名序號表示收件失敗
                            if (!item.SIGNNO.HasValue || item.SIGNNO <= 0)
                            {
                                item.SIGNNO_TEXT = "線上收件失敗";
                            }
                            else
                            {
                                //2019-01-23 add 產投報名成功加顯示序號資訊
                                item.SIGNNO_TEXT += "(序號：" + item.SIGNNO + ")";
                            }
                            break;

                        default:
                            //case "06"://06:在職 case "70"://70:區域產業據點
                            //報名管道 (1.網;2.現;3.通;4.推)   報名: 網路、現場(含通訊)、推介
                            if (item.ENTERCHANNEL == 2)
                            {
                                //現場報名
                                item.SIGNNO_TEXT = "報名成功";
                                //從 stud_entertype 取得准考證號資料（e網報名審核通過）
                            }
                            break;
                    }

                    //SIGNUPSTATUS 報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                    //SIGNUPSTATUS_NEW 1:審核情形-審核中-"報名失敗":"報名成功"3:正取4:備取5:未錄取/(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                    item.SIGNUPSTATUS_NEW = item.SIGNUPSTATUS;
                    //系統日 < 報名迄日 or 系統日 < 開訓日
                    //未錄取(審核失敗) //在職報名失敗改以未錄取顯示，但仍要顯示報名失敗的選項出來
                    switch (item.TPLANID)
                    {
                        case "28"://產投-只有產投-審核失敗改為未錄取
                            if (item.SIGNUPSTATUS.HasValue && item.SIGNUPSTATUS == 2) { item.SIGNUPSTATUS_NEW = 5; }
                            break;
                    }

                    //系統日若超過開訓日+14 且超過報名結束日 則為1 否為0 (1:啟動'未錄取 顯示功能。)
                    if (item.XDAY14 == 1 && item.XDAY1 == 1 && item.SIGNUPSTATUS.HasValue)
                    {
                        //若超過 則為1
                        //SIGNUPSTATUS 報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                        switch (item.SIGNUPSTATUS)
                        {
                            case 0: //未審/收件
                            case 1: //(審核成功)
                                item.SIGNUPSTATUS_NEW = 1;//(審核中)
                                break;
                            case 2: //(審核失敗)
                            case 5: //未錄取
                                //未審/收件 //(審核失敗) //未錄取
                                item.SIGNUPSTATUS_NEW = 5;//(未錄取)
                                break;
                            case 3: //正取
                            case 4: //備取
                                //正取 //備取
                                break;
                            default:
                                //PTYPE 網路_產投(1) 網路(e網報名)_非產投(2) 現場_現場(3)
                                //ENTERCHANNEL 1.網;2.現;3.通;4.推
                                //其餘皆納入未錄取 (0:收件完成 ,1:審核中(產投)/報名成功(在職) ,2:未錄取(產投審核失敗)/報名失敗(在職))
                                //2019-02-21 add if 非現場報名 PTYPE:3 //5:(未錄取)
                                if (!("3".Equals(item.PTYPE) && item.ENTERCHANNEL == 2)) { item.SIGNUPSTATUS_NEW = 5; }
                                break;
                        }
                    }

                    //超過開訓日14天(超過開訓日)，但沒有學員狀態
                    //SIGNUPSTATUS_NEW 1:審核情形-審核中-"報名失敗":"報名成功"3:正取4:備取5:未錄取
                    //5:(未錄取)
                    if (item.XDAY14 == 1 && item.XDAY1 == 1 && !item.STUDSTATUS.HasValue) { item.SIGNUPSTATUS_NEW = 5; }

                    //只有當報名失敗（signupstatus != 2）時才需顯示原因
                    //SIGNUPSTATUS 報名狀態(0:收件完成  1:報名成功  2:報名失敗 3:正取 4:備取 5:未錄取)
                    if (item.SIGNUPSTATUS != 2) { item.SIGNUPMEMO = string.Empty; }
                }
            }
        }

        /// <summary>
        /// 2019-02-21 add 檢核可否允許取消報名
        /// </summary>
        /// <param name="esernum"></param>
        /// <returns></returns>
        public bool CheckCancelEnterClass(Int64 esetid, Int64 esernum, Int64 ocid1)
        {
            bool rtn = true;
            string funcName = "WDAIIPWEB.GetEnterType2CancelStatus";
            SessionModel sm = SessionModel.Get();
            BaseDAO dao = new BaseDAO();

            Hashtable parms = new Hashtable { ["ESETID"] = esetid, ["ESERNUM"] = esernum, ["OCID1"] = ocid1 };

            IList<Hashtable> list = dao.QueryForListAll<Hashtable>(funcName, parms);

            if (list == null) { return rtn; }

            foreach (Hashtable item in list)
            {
                //學員於報名截止日前，若為「備取學員」可自行從在職訓練網取消報名。
                //SIGNUPSTATUS 0:收件完成 1:報名成功 2:報名失敗 3:正取(KEY_SELRESULT) 4:備取 5:未錄取
                //!0:若已進行審核，則不允許取消報名 //!4:若非備取 ，則不允許取消報名
                //NO_CANCEL 不得取消報名 true/false;
                bool fg_NO_CANCEL = !"0".Equals(Convert.ToString(item["SIGNUPSTATUS"])) ? (!"4".Equals(Convert.ToString(item["SIGNUPSTATUS"])) ? true : false) : false;
                if (fg_NO_CANCEL)
                {
                    sm.LastResultMessage = "報名資料已進行審核，不得取消報名";
                    rtn = false;
                    break;
                }
                //若已超過報名截止日，則不允許取消報名
                if ("Y".Equals(Convert.ToString(item["ISOVERENTERDATE"])))
                {
                    sm.LastResultMessage = "此報名資料已過報名截止日，欲取消報名者，請洽訓練單位";
                    rtn = false;
                    break;
                }
            }

            return rtn;
        }
        #endregion

        #region 會員專區-取消報名

        /// <summary>
        /// 取消報名作業 tplanid, esetid.Value, esernum.Value, ocid1.Value
        /// </summary>
        /// <param name="esernum"></param>
        public void procCancelEnter2(WDAIIPWEBDAO dao, string tplanid, Int64 esetid, Int64 esernum, Int64 ocid1)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //dao.BeginTransaction();
            // 備份 stud_entertype2 to stud_entertype2deldata（民眾自行取消報名記錄）
            dao.BackUpEnterType2(tplanid, esernum);
            // 刪除 stud_entertype2
            dao.DelEnterType2(esernum);
            //dao.CommitTransaction();
        }

        /// <summary>
        ///  取消報名作業 tplanid, setid.Value, enterdate.Value, sernum.Value, ocid1.Value
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="tplanid"></param>
        /// <param name="setid"></param>
        /// <param name="enterdate"></param>
        /// <param name="sernum"></param>
        /// <param name="ocid1"></param>
        public void procCancelEnter(WDAIIPWEBDAO dao, string tplanid, decimal setid, DateTime enterdate, decimal sernum, Int64 ocid1)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //dao.BeginTransaction();
            // 備份 stud_entertype to stud_entertypedeldata（民眾自行取消報名記錄）
            dao.BackUpEnterType(tplanid, setid, enterdate, sernum);
            // 刪除 stud_entertype
            dao.DelEnterType(setid, enterdate, sernum);
            //dao.CommitTransaction();
        }

        #endregion

        public void LoadEnterData(StudOnlineViewModel model)
        {
            //2.使用Stud_EnterTemp3 報名資料維護。
            this.LoadTemp3(model);

            //3.登入成功 '採用 Member 資料
            this.LoadMember(model);
        }

        private void LoadMember(StudOnlineViewModel model)
        {
            var dao = new WDAIIPWEBDAO();
            var memInfo = dao.GetEMember();
            if (memInfo == null) { return; }

            model.Detail.IDNO = memInfo.IDNO;
            model.Detail.NAME = HttpUtility.HtmlDecode(memInfo.NAME);
            model.Detail.CONTACTPHONE = memInfo.CELLPHONE ?? memInfo.PHONED;
            if (!string.IsNullOrEmpty(memInfo.ZIPCODE)) { model.Detail.ZIPCODE1 = int.Parse(memInfo.ZIPCODE); }
            model.Detail.ZIPCODE1_6W = memInfo.ZIPCODE_6W;
            model.Detail.ZIPCODE1_2W = MyCommonUtil.GET_ZIPCODE2W(memInfo.ZIPCODE_6W, null);
            model.Detail.ADDRESS = memInfo.ADDRESS;
            model.Detail.ISAGREE = null;//memInfo.ISAGREE;
        }

        private void LoadTemp3(StudOnlineViewModel model)
        {
            var dao = new WDAIIPWEBDAO();
            var temp3Info = dao.GetStudEnterTmp3();
            if (temp3Info == null) { return; }
            //model.Detail.IsNew = false;
            //model.Detail.ESETID4 = o2.ESETID4;
            model.Detail.IDNO = temp3Info.IDNO;
            model.Detail.NAME = HttpUtility.HtmlDecode(temp3Info.NAME);
            model.Detail.CONTACTPHONE = temp3Info.CELLPHONE ?? temp3Info.PHONED;
            if (!string.IsNullOrEmpty(temp3Info.ZIPCODE)) { model.Detail.ZIPCODE1 = int.Parse(temp3Info.ZIPCODE); }
            model.Detail.ZIPCODE1_6W = temp3Info.ZIPCODE_6W;
            model.Detail.ZIPCODE1_2W = MyCommonUtil.GET_ZIPCODE2W(temp3Info.ZIPCODE_6W, null);
            model.Detail.ADDRESS = temp3Info.ADDRESS;
            model.Detail.ISAGREE = temp3Info.ISAGREE;
            //model.Detail.MODIFYACCT = o2.MODIFYACCT;
            //model.Detail.MODIFYDATE = o2.MODIFYDATE;
        }

        /// <summary>
        /// 取得銀行下拉選單
        /// </summary>
        /// <returns></returns>
        public List<BankVM> GetBankList()
        {
            IList<BankVM> daolist = dao.QueryBankList();
            List<BankVM> list = new List<BankVM>();
            try
            {

                foreach (var item in daolist)
                {
                    //BnakCode = item.Field<string>("CODE"), //銀行代碼(CODE) BankName = item.Field<string>("DESCR"), //銀行名稱(DESCR) Text = item.Field<string>("TEXT") //銀行下拉選項
                    BankVM vm = new BankVM
                    {
                        BnakCode = item.BnakCode, //銀行代碼(CODE)
                        BankName = item.BankName, //銀行名稱(DESCR)
                        Text = item.Text //銀行下拉選項
                    };
                    list.Add(vm);
                }
            }
            catch (Exception ex)
            {
                SessionModel sm = SessionModel.Get();
                sm.LastResultMessage = string.Concat("錯誤:", ex.Message);
                LOG.Error(string.Concat("ex.Message:", ex.Message), ex);
            }
            return list;
        }
        public List<BankVM> GetBranchList(string code)
        {
            IList<BankVM> daolist = dao.QueryBranchList(code);
            List<BankVM> list = new List<BankVM>();
            try
            {
                foreach (var item in daolist)
                {
                    //BnakCode = item.Field<string>("CODE"), //銀行代碼(CODE) BankName = item.Field<string>("DESCR"), //銀行名稱(DESCR) Text = item.Field<string>("TEXT") //銀行下拉選項
                    BankVM vm = new BankVM { BranchCode = item.BranchCode, BranchName = item.BranchName, Text = item.Text, BankName = item.BankName };
                    list.Add(vm);
                }
            }
            catch (Exception ex)
            {
                SessionModel sm = SessionModel.Get();
                sm.LastResultMessage = string.Concat("錯誤:", ex.Message);
                LOG.Error(string.Concat("ex.Message:", ex.Message), ex);
            }
            return list;

        }

    }
}
