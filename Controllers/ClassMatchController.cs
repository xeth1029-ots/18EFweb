using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using log4net;

namespace WDAIIP.WEB.Controllers
{
    public class ClassMatchController : LoginBaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: ClassMatch
        /// <summary>
        /// 速配條件設定頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ClassMatchViewModel model = new ClassMatchViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            // 預設顯示在職進修設定內容
            model.Form.PlanType = "2";

            // 產生職類業別選項清單(產投用)
            model.TMIDGrid = GetTMIDGrid();

            // 產生通俗職類選項清單(在職用)
            model.CJobNoGrid = GetCJobNoGrid();

            // 查詢並載入已設定結果
            LoadData(model);

            return View("Index", model);
        }

        /// <summary>
        /// 儲存速配課程媒合條件
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ClassMatchViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string[] tmidAry = null;
            string[] cjobnoAry = null;

            TblTB_MEMSEARCH memSearch = dao.GetMemSearch(sm.MemSN);

            // 產生職類業別選項清單(產投用)
            model.TMIDGrid = GetTMIDGrid();
            // 載入產投設定項目
            if (!string.IsNullOrEmpty(model.Form.TMIDRESULT))
            {
                tmidAry = model.Form.TMIDRESULT.Split(',');
            }
            LoadTMIDSel(model, tmidAry);

            // 產生通俗職類選項清單(在職用)
            model.CJobNoGrid = GetCJobNoGrid();
            // 在職設定項目
            if (!string.IsNullOrEmpty(model.Form.CJOBUNKEYRESULT))
            {
                cjobnoAry = model.Form.CJOBUNKEYRESULT.Split(',');
            }
            LoadCJobNoSel(model, cjobnoAry);

            switch (model.Form.PlanType)
            {
                case "1": //產投
                    if (string.IsNullOrEmpty(model.Form.SENDMAIL28))
                    {
                        ModelState.AddModelError("ValidMsg", "請選擇 本人是否希望收到產業人才投資方案速配課程資訊");
                    }

                    //2019-01-31 add 課程地點所屬縣市別改為必填
                    if (string.IsNullOrEmpty(model.Form.CTID))
                    {
                        ModelState.AddModelError("ValidMsg", "請選擇 課程地點所屬縣市別");
                    }
                    break;
                case "2": //在職
                    if (string.IsNullOrEmpty(model.Form.SENDMAIL06))
                    {
                        ModelState.AddModelError("ValidMsg", "請選擇 本人是否希望收到分署自辦在職訓練速配課程資訊");
                    }

                    //2019-01-31 add 分署別改為必填
                    if (string.IsNullOrEmpty(model.Form.DISTID))
                    {
                        ModelState.AddModelError("ValidMsg", "請選擇 分署別");
                    }
                    break;
                case "5": //區域產業據點
                    if (string.IsNullOrEmpty(model.Form.SENDMAIL70))
                    {
                        ModelState.AddModelError("ValidMsg", "請選擇 本人是否希望收到區域產業據點速配課程資訊");
                    }

                    //2019-01-31 add 分署別改為必填
                    if (string.IsNullOrEmpty(model.Form.DISTID))
                    {
                        ModelState.AddModelError("ValidMsg", "請選擇 分署別");
                    }
                    break;

            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (memSearch == null)
                    {
                        //新增
                        dao.InsertMemSearch(model.Form, sm);
                    }
                    else
                    {
                        //修改
                        dao.UpdateMemSearch(model.Form, sm);
                    }

                    sm.LastResultMessage = "儲存成功！";
                    switch (model.Form.PlanType)
                    {
                        case "1": //產投速配結果清單
                            sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassMatchPlan28");
                            break;
                        case "2": //在職進修速配結果清單
                            sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassMatchPlan06");
                            break;
                        case "5": //區域產業據點-速配結果清單
                            sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassMatchPlan70");
                            break;
                    }

                    //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassMatchList");
                }
                catch (Exception)
                {
                    sm.LastErrorMessage = "儲存失敗";
                }
            }

            return View("Index", model);
        }

        /// <summary>
        /// 更新設定條件
        /// </summary>
        /// <param name="planType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PlanTypeChange(string planType)
        {
            if (string.IsNullOrEmpty(planType))
            {
                //throw new ArgumentNullException("plantype 不可為 null");
                LOG.Debug("[ALERT] ClassMatchController post Index (403):  planType 不可為 null");
                return new HttpStatusCodeResult(403);
            }

            switch (planType)
            {
                case "1"://產投
                case "2"://在職
                case "5"://區域產業據點
                    break;
                default:
                    LOG.Debug("[ALERT] ClassMatchController post Index (403):  planType [" + planType + "]格式錯誤");
                    return new HttpStatusCodeResult(403);
            }

            ClassMatchViewModel model = new ClassMatchViewModel();
            model.Form.PlanType = planType;

            // 產生職類業別選項清單(產投用)
            model.TMIDGrid = GetTMIDGrid();

            // 產生通俗職類選項清單(在職用)
            model.CJobNoGrid = GetCJobNoGrid();

            // 查詢並載入已設定結果
            LoadData(model);

            return View("Index", model);
        }

        /// <summary>
        /// 查詢載入速配課程已設定結果
        /// </summary>
        /// <param name="model"></param>
        public void LoadData(ClassMatchViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string tmid = string.Empty;
            string[] tmidAry = null;


            string cjobType = string.Empty;
            string cjobno = string.Empty;
            string[] cjobnoAry = null;
            //List<string> cjobnoList = null;

            TblTB_MEMSEARCH data = dao.GetMemSearch(sm.MemSN);

            if (data != null)
            {
                model.Form.SENDMAIL06 = (String.IsNullOrEmpty(data.SENDMAIL06) ? "N" : data.SENDMAIL06);
                model.Form.SENDMAIL28 = (String.IsNullOrEmpty(data.SENDMAIL28) ? "N" : data.SENDMAIL28);
                model.Form.SENDMAIL70 = (String.IsNullOrEmpty(data.SENDMAIL70) ? "N" : data.SENDMAIL70);

                cjobno = Convert.ToString(data.CJOBNO);
                if (!string.IsNullOrEmpty(cjobno))
                {
                    cjobnoAry = cjobno.Split(',');
                }

                tmid = Convert.ToString(data.TMID);
                if (!string.IsNullOrEmpty(tmid))
                {
                    tmidAry = tmid.Split(',');
                }

                LoadTMIDSel(model, tmidAry);
                model.Form.CTID = data.CTID;
                LoadCJobNoSel(model, cjobnoAry);
                model.Form.DISTID = data.DISTID;

                //載入產投設定項目
                model.Form.SENDMAIL28 = data.SENDMAIL28;
                //在職設定項目
                model.Form.SENDMAIL06 = data.SENDMAIL06;
                //區域產業據點
                model.Form.SENDMAIL70 = data.SENDMAIL70;
            }

        }

        /// <summary>
        /// 載入在職速配課程-訓練業別勾選結果
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tmidAry"></param>
        public void LoadTMIDSel(ClassMatchViewModel model, string[] tmidAry)
        {
            List<string> tmidList = null;

            if (tmidAry != null && tmidAry.Length > 0)
            {
                if (model.TMIDGrid != null)
                {
                    foreach (var lv1Item in model.TMIDGrid)
                    {
                        tmidList = new List<string>();
                        foreach (var lv2Item in lv1Item.keyMap_list)
                        {
                            if (Array.IndexOf(tmidAry, lv2Item.CODE) > -1)
                            {
                                tmidList.Add(lv2Item.CODE);
                            }
                        }

                        lv1Item.TMIDLv2SelValue = tmidList.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 載入產投速配課程-通俗職類勾選結果
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cjobnoAry"></param>
        public void LoadCJobNoSel(ClassMatchViewModel model, string[] cjobnoAry)
        {
            List<string> cjobnoList = null;

            CJobNoGridModel lv1Item = null;
            CJobNOLv2GridModel lv2Item = null;

            //載入勾選結果
            if (cjobnoAry != null && cjobnoAry.Length > 0)
            {

                if (model.CJobNoGrid != null)
                {
                    for (int i = 0; i < model.CJobNoGrid.Count; i++)
                    {
                        lv1Item = model.CJobNoGrid[i];
                        for (int j = 0; j < lv1Item.CJOBNOLv2ListItem.Count; j++)
                        {
                            lv2Item = lv1Item.CJOBNOLv2ListItem[j];
                            cjobnoList = new List<string>();

                            for (int k = 0; k < lv2Item.keyMap_list.Count; k++)
                            {
                                var lv3Item = lv2Item.keyMap_list[k];
                                if (Array.IndexOf(cjobnoAry, lv3Item.CODE) > -1)
                                {
                                    cjobnoList.Add(lv3Item.CODE);
                                }
                            }

                            //model.CJobNoGrid[i].CJOBNOLv2ListItem[j].CJOBNOLv3Sel = data.CJOBNO;
                            model.CJobNoGrid[i].CJOBNOLv2ListItem[j].CJOBNOLv3SelValue = cjobnoList.ToArray();
                            /*model.CJobNoGrid[i].CJOBNOLv2ListItem[j].CJOBNOLv3SelValue = new string[cjobnoList.Count];
                            for (int x = 0; x < cjobnoList.Count; x++)
                            {
                                model.CJobNoGrid[i].CJOBNOLv2ListItem[j].CJOBNOLv3SelValue[x] = cjobnoList[x];
                            }*/
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 產生通俗職類清單選項（在職用）
        /// </summary>
        /// <returns></returns>
        public IList<CJobNoGridModel> GetCJobNoGrid()
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            IList<KeyMapModel> lv1List = null;
            IList<Hashtable> lv2List = null;
            IList<Hashtable> lv3List = null;
            IList<CJobNoGridModel> cjobNoList = new List<CJobNoGridModel>();
            CJobNoGridModel lv1Item = null;
            CJobNOLv2GridModel lv2Item = null;

            lv1List = keyDao.GetCJobNoItemLv1List(); //查詢第一層選項
            lv2List = dao.GetCJobNoItemLv2List(); //查詢第二層選項
            lv3List = dao.GetCJobNoItemLv3List(); //查詢第三層選項
            IList<Hashtable> tmpLv2List = null;
            IList<Hashtable> tmpLv3List = null;

            KeyMapModel keyModel = null;
            IList<KeyMapModel> keyList = new List<KeyMapModel>();

            if (lv1List != null)
            {
                //產生第一層選項資料
                foreach (var item1 in lv1List)
                {
                    lv1Item = new CJobNoGridModel();
                    lv1Item.CJOBTYPE = item1.CODE;
                    lv1Item.CJOBNAME = item1.TEXT;

                    if (lv2List != null)
                    {
                        //查詢第二層選項
                        //lv2List = dao.GetCJobNoItemLv2List(lv1Item.CJOBTYPE);
                        tmpLv2List = new List<Hashtable>();
                        tmpLv2List = lv2List.Where(m => m["CJOB_TYPE"].ToString() == lv1Item.CJOBTYPE).OrderBy(m => m["CJOB_UNKEY"].ToString()).ToList();

                        if (tmpLv2List != null)
                        {
                            lv1Item.CJOBNOLv2ListItem = new List<CJobNOLv2GridModel>();

                            //產生第二層選項資料
                            foreach (var item2 in tmpLv2List)
                            {
                                lv2Item = new CJobNOLv2GridModel();
                                lv2Item.CJOBNO = Convert.ToString(item2["CJOB_UNKEY"]);
                                lv2Item.CJOBNAME = Convert.ToString(item2["JOBNAME"]);

                                //查詢第三層選項資料
                                //lv3List = dao.GetCJobNoItemLv3List(lv2Item.CJOBNO);

                                tmpLv3List = new List<Hashtable>();
                                tmpLv3List = lv3List.Where(m => m["CJOB_NO"].ToString() == lv2Item.CJOBNO).ToList();

                                if (tmpLv3List != null)
                                {
                                    keyList = new List<KeyMapModel>();
                                    foreach (Hashtable item3 in tmpLv3List)
                                    {
                                        keyModel = new KeyMapModel();
                                        keyModel.CODE = Convert.ToString(item3["CJOB_UNKEY"]);
                                        keyModel.TEXT = Convert.ToString(item3["JOBNAME"]);

                                        keyList.Add(keyModel);
                                    }

                                    lv2Item.keyMap_list = keyList;
                                    lv2Item.CJOBNO_list = MyCommonUtil.ConvertCheckBoxItems(keyList);
                                }

                                lv1Item.CJOBNOLv2ListItem.Add(lv2Item);
                            }
                        }
                    }

                    cjobNoList.Add(lv1Item);
                }
            }

            return cjobNoList;
        }

        /// <summary>
        /// 產生訓練業別清單選項（產投用）
        /// </summary>
        /// <returns></returns>
        public IList<TMIDGridModel> GetTMIDGrid()
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            IList<KeyMapModel> lv1List = null;
            IList<Hashtable> lv2List = null;
            IList<TMIDGridModel> tmidList = new List<TMIDGridModel>();
            TMIDGridModel lv1Item = null;

            lv1List = keyDao.GetTMIDItemLv1List(); //查詢第一層選項
            lv2List = dao.GetTMIDItemLv2List(); //查詢第二層選項
            IList<Hashtable> tmpLv2List = null;

            KeyMapModel keyModel = null;
            IList<KeyMapModel> keyList = new List<KeyMapModel>();

            if (lv1List != null)
            {
                //產生第一層選項資料
                foreach (var item1 in lv1List)
                {
                    lv1Item = new TMIDGridModel();
                    lv1Item.JOBTMID = item1.CODE;
                    lv1Item.JOBNAME = item1.TEXT;

                    tmpLv2List = new List<Hashtable>();
                    tmpLv2List = lv2List.Where(m => m["JOBTMID"].ToString() == lv1Item.JOBTMID).ToList();

                    if (tmpLv2List != null)
                    {
                        keyList = new List<KeyMapModel>();
                        foreach (Hashtable item2 in tmpLv2List)
                        {
                            keyModel = new KeyMapModel();
                            keyModel.CODE = Convert.ToString(item2["TMID"]);
                            keyModel.TEXT = Convert.ToString(item2["TRAINNAME"]);

                            keyList.Add(keyModel);
                        }

                        lv1Item.keyMap_list = keyList;
                        lv1Item.TMID_list = MyCommonUtil.ConvertCheckBoxItems(keyList);
                    }

                    tmidList.Add(lv1Item);
                }
            }

            return tmidList;
        }
    }
}