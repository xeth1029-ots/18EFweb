using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;
using Turbo.Commons;
using log4net;
using Newtonsoft.Json;
using System.Collections;
using WDAIIP.WEB.Commons.Filter;

namespace WDAIIP.WEB.Controllers
{
    public class ClassMapSchController : BaseController
    {

        //using log4net;
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // GET: ClassMapSch
        /// <summary>
        /// 課程查詢報名--地圖找課程
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ClassMapSchViewModel model = new ClassMapSchViewModel();

            DateTime now = DateTime.Now;

            string dateSYr = (now.AddMonths(-1).Year - 1911).ToString();
            string dateSMon = now.AddMonths(-1).Month.ToString();
            string dateEYr = (now.AddMonths(3).Year - 1911).ToString();
            string dateEMon = now.AddMonths(3).Month.ToString();

            model.Form.STDATE_YEAR_SHOW = dateSYr;
            model.Form.STDATE_MON = dateSMon;
            model.Form.FTDATE_YEAR_SHOW = dateEYr;
            model.Form.FTDATE_MON = dateEMon;
            model.Form.hidCoursesClosed = "N";
            //model.Form.hidOpenNotSearch = "Y";

            model.Form.PlanType = model.Form.PlanType ?? "1";

            return Index(model);
        }

        /// <summary> 依條件再次查詢地圖 課程查詢報名--地圖找課程 </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ClassMapSchViewModel model)
        {
            SessionModel sm = SessionModel.Get();

            //整理查詢條件 方便查詢資料
            GetTidydata(model);

            return View("Index", model);
        }

        /// <summary> 整理查詢條件 方便查詢資料 </summary>
        /// <param name="model"></param>
        public void GetTidydata(ClassMapSchViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ClassMapSchFormModel form = model.Form;

            string s_msg = "\n #Tidydata ";
            //foreach (var v_DISTID in form.DISTID_SHOW) { s_msg += string.Format("\n,DISTID_SHOW:{0}", v_DISTID ?? "(null)"); }
            //foreach (var v_CTID in form.CTID_SHOW) { s_msg += string.Format("\n,CTID_SHOW:{0}", v_CTID ?? "(null)"); }
            //foreach (var v_CLASSCATE in form.CLASSCATE_SHOW) { s_msg += string.Format("\n,CLASSCATE_SHOW:{0}", v_CLASSCATE ?? "(null)"); }
            //foreach (var v_ORGKIND in form.ORGKIND_SHOW) { s_msg += string.Format("\n,ORGKIND_SHOW:{0}", v_ORGKIND ?? "(null)"); }
            s_msg += string.Format("\n,form.PlanType:{0}", form.PlanType ?? "(null)");
            s_msg += string.Format("\n,form.hidCoursesClosed:{0}", form.hidCoursesClosed ?? "(null)");
            s_msg += string.Format("\n,form.JOBTMID:{0}", form.JOBTMID.HasValue ? form.JOBTMID.ToString() : "(null)");
            s_msg += string.Format("\n,form.TMID:{0}", form.TMID.HasValue ? form.TMID.ToString() : "(null)");
            s_msg += string.Format("\n,form.PARENT1:{0}", form.PARENT1.HasValue ? form.PARENT1.ToString() : "(null)");
            s_msg += string.Format("\n,form.PARENT2:{0}", form.PARENT2.HasValue ? form.PARENT2.ToString() : "(null)");
            s_msg += string.Format("\n,form.DISTID:{0}", form.DISTID ?? "(null)");
            s_msg += string.Format("\n,form.CTID_KEY:{0}", form.CTID_KEY ?? "(null)");
            s_msg += string.Format("\n,form.CLASSCATE:{0}", form.CLASSCATE ?? "(null)");
            s_msg += string.Format("\n,form.ORGKIND:{0}", form.ORGKIND ?? "(null)");
            //var result = new List<IDictionary<string, object>>();
            s_msg += "\n #Tidydata.form: ";
            var dict = form.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(form, null));
            foreach (var kvp in dict) { s_msg += string.Format("\n,{0}:{1}", kvp.Key, kvp.Value ?? "[null]"); }
            //logger.Debug(s_msg);

            //查詢類別("1":產業人才投資方案、"2":在職進修訓練、"5":區域產業據點)
            //產業人才投資方案
            model.Grid1 = dao.QueryClassMapSchList_G1(form);

            //ArrayList data = new ArrayList();
            MapSearchBounds jobBounds = null;
            if (jobBounds == null)
            {
                // 找出座標分佈的邊界
                jobBounds = new MapSearchBounds();
                jobBounds.left = double.MaxValue;
                jobBounds.right = 0;
                jobBounds.top = 0;
                jobBounds.bottom = double.MaxValue;
                foreach (var rItem in model.Grid1)
                {
                    double x = rItem.REAL_TWD97_X.HasValue ? rItem.REAL_TWD97_X.Value : 0;
                    double y = rItem.REAL_TWD97_Y.HasValue ? rItem.REAL_TWD97_Y.Value : 0;
                    jobBounds.left = Math.Min(jobBounds.left, x);
                    jobBounds.right = Math.Max(jobBounds.right, x);
                    jobBounds.bottom = Math.Min(jobBounds.bottom, y);
                    jobBounds.top = Math.Max(jobBounds.top, y);

                    //Hashtable station = new Hashtable();
                    //station.Add("STATION_ID", rItem.OCID);// Convert.ToString(dr["STATION_ID"]));
                    //station.Add("STATION_CITY", rItem.OCID);// Convert.ToString(dr["STATION_CITY"]));
                    //station.Add("STATION_ZIP", rItem.TADDRESS);//Convert.ToString(dr["STATION_ZIP"]));
                    //station.Add("STATION_ADDRESS", rItem.TADDRESS);//Convert.ToString(dr["STATION_ADDRESS"]));
                    //station.Add("STATION_NAME", rItem.CLASSCNAME);//Convert.ToString(dr["STATION_NAME"]));
                    //station.Add("STATION_TEL1", rItem.OCID);//Convert.ToString(dr["STATION_TEL1"]));
                    //station.Add("STATION_FAX", rItem.OCID);//Convert.ToString(dr["STATION_FAX"]));
                    //station.Add("STATION_TYPE", rItem.OCID);//Convert.ToString(dr["STATION_TYPE"]));
                    //station.Add("STATION_INFO", rItem.OCID);//Convert.ToString(dr["STATION_INFO"]).Replace("'", "’"));
                    //station.Add("MAP_X", x);//Convert.ToString(dr["MAP_X"]));
                    //station.Add("MAP_Y", y);//Convert.ToString(dr["MAP_Y"]));
                    //data.Add(station);
                }

                logger.Debug("showData: jobBounds " + Newtonsoft.Json.JsonConvert.SerializeObject(jobBounds));

                // 放大分佈座標邊界, 讓邊緣的點位顯示效果更好
                //jobBounds.left = Math.Max(jobBounds.left - 0.2, 147725.0);
                //jobBounds.right = Math.Min(jobBounds.right + 15, 355260.0);
                jobBounds.bottom = Math.Max(jobBounds.bottom, 22.0);
                jobBounds.top = Math.Min(jobBounds.top + 0.1, 25.5);

                //轉成 JSON 字串    
                model.Form.hidJobBounds = Newtonsoft.Json.JsonConvert.SerializeObject(jobBounds);
                //model.Form.liSDM = JsonConvert.SerializeObject(data);
            }
        }

        /// <summary> 切換計畫別重取查詢條件 </summary>
        /// <param name="PlanType"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCondition(string PlanType, string keywords, string PARENT1, string PARENT2, string JOBTMID, string TMID, string ContainsOver)
        {
            ClassMapSchViewModel model = new ClassMapSchViewModel();

            long iPARENT1 = default(long); bool flag_PARENT1 = (PARENT1 != null ? long.TryParse(PARENT1, out iPARENT1) : false);
            long iPARENT2 = default(long); bool flag_PARENT2 = (PARENT2 != null ? long.TryParse(PARENT2, out iPARENT2) : false);
            long iJOBTMID = default(long); bool flag_JOBTMID = (JOBTMID != null ? long.TryParse(JOBTMID, out iJOBTMID) : false);
            long iTMID = default(long); bool flag_TMID = (TMID != null ? long.TryParse(TMID, out iTMID) : false);

            string s_msg = "\n #GetCondition ";
            s_msg += string.Format("\n,PlanType:{0}", PlanType);
            s_msg += string.Format("\n,PARENT1:{0}", PARENT1 ?? "(null)");
            s_msg += string.Format("\n,PARENT2:{0}", PARENT2 ?? "(null)");
            s_msg += string.Format("\n,JOBTMID:{0}", JOBTMID ?? "(null)");
            s_msg += string.Format("\n,TMID:{0}", TMID ?? "(null)");
            s_msg += string.Format("\n,ContainsOver:{0}", ContainsOver ?? "(null)");
            //logger.Debug(s_msg);

            model.Form.PlanType = PlanType;
            model.Form.KEYWORDS = keywords;
            model.Form.PARENT1 = flag_PARENT1 ? (long?)iPARENT1 : model.Form.PARENT1;
            model.Form.PARENT2 = flag_PARENT2 ? (long?)iPARENT2 : model.Form.PARENT2;
            model.Form.JOBTMID = flag_JOBTMID ? (long?)iJOBTMID : model.Form.JOBTMID;
            model.Form.TMID = flag_TMID ? (long?)iTMID : model.Form.TMID;
            model.Form.ContainsOverEnter = ContainsOver.Equals("true") ? true : false;

            return PartialView("_Condition", model);
        }

        /// <summary> 單筆課程收藏 </summary>
        /// <returns></returns>
        [LoginRequired]
        [HttpPost]
        public ActionResult AddClassTrace(string PlanType, string OCID, string ADDCHK)
        {
            var result = new AjaxResultStruct();
            string msg = string.Empty;
            result.status = false;

            Int64? iOCID = null;
            if (MyCommonUtil.isUnsignedInt(OCID)) { iOCID = Convert.ToInt64(OCID); }
            if (iOCID == null)
            {
                ArgumentNullException ex = new ArgumentNullException("OCID");
                logger.Warn(ex.Message, ex);
                //throw ex;
                return base.SetPageNotFound();
            }
            if (string.IsNullOrEmpty(PlanType))
            {
                ArgumentNullException ex = new ArgumentNullException("PlanType");
                logger.Warn(ex.Message, ex);
                //throw ex;
                return base.SetPageNotFound();
            }

            SessionModel sm = SessionModel.Get();
            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                logger.Warn("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);
                //throw ex;
                return base.SetPageNotFound();
                //rtn = RedirectToAction("Login", "Member");
                //return rtn;

            }

            var s_msg = "";
            s_msg += string.Format("\n PlanType: {0}", PlanType);
            s_msg += string.Format("\n OCID: {0}", OCID);
            //ADDCHK: 0:取消收藏 1：增加收藏
            s_msg += string.Format("\n ADDCHK: {0}", ADDCHK);
            logger.Debug(s_msg);

            //ActionResult rtn = null;
            //string alertMsg = string.Empty;

            IList<Int64?> classlist = new List<Int64?>();
            classlist.Add(iOCID);

            result.status = true;

            try
            {
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
                bool isExists = dao.IsClassTraceExists(PlanType, iOCID.Value, sm);
                if (ADDCHK.Equals("1"))
                {
                    if (isExists)
                    {
                        msg = "課程收藏清單已有此課程！";
                        result.data = msg;
                        return Content(result.Serialize(), "application/json");
                    }

                    //批次加入課程收藏
                    dao.AddClassTrace(PlanType, classlist, sm);
                    msg = "已新增至課程收藏清單。";
                    result.data = msg;
                    return Content(result.Serialize(), "application/json");
                }

                dao.DelClassTrace(classlist, sm);
                msg = "已刪除選取課程收藏清單。";
                result.data = msg;
                return Content(result.Serialize(), "application/json");
            }
            catch (Exception ex)
            {
                //rtn = Detail(PlanType, ProvideLocation, Convert.ToString(iOCID), "加入課程收藏失敗。", true);
                logger.Warn(ex.Message, ex);
                result.status = false;
                msg = "課程收藏失敗!!";
                sm.LastErrorMessage = msg;
                result.data = msg;
            }
            return Content(result.Serialize(), "application/json");
        }

    }

}