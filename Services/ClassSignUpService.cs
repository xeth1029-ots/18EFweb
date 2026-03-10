using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Hosting;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.DataLayers;
using Turbo.Commons;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using log4net;
using System.Diagnostics;

namespace WDAIIP.WEB.Services
{
    /// <summary> 課程報名相關邏輯 </summary>
    public class ClassSignUpService
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 產投課程報名-報名作業

        /// <summary>
        /// 檢測此學員是否 可參訓 產業人才投資方案 (大於15歲者)
        /// 以班的開訓日期為主，比對生日後若滿於等於15年，則 可參訓 產業人才投資方案
        /// <para>此 method 設計用於 View Model Validation</para>
        /// </summary>
        /// <param name="birthday"></param>
        /// <param name="STDATE"></param>
        /// <returns></returns>
        public bool CheckYearsOld15(DateTime birthday, DateTime? STDATE)
        {
            bool rtn = false; //此學員無中高齡者參訓身分
            const int cstYrOld = -15;

            // 出生日期為主
            DateTime? date1 = birthday;
            // 有開訓日期採 開訓日期 計算,沒有開訓日期採當日計算
            DateTime? date2 = (STDATE.HasValue ? STDATE : DateTime.Now.Date);

            date2 = date2.Value.AddYears(cstYrOld);
            if (DateTime.Compare(date1.Value, date2.Value) <= 0)
            {
                //此學員有15歲以上，符合參訓身份
                rtn = true;
            }

            return rtn;
        }

        /// <summary>
        /// 取得e網報名資料暫存檔序號(Stud_EnterTemp2)
        /// </summary>
        /// <param name="idno"></param>
        /// <returns></returns>
        public static Int64 GetESETIDMaxID(string idno, WDAIIPWEBDAO dao)
        {
            Int64 eSETID = 0;
            TblSTUD_ENTERTEMP2 temp2Item = dao.GeteSETID(idno);

            if (temp2Item != null)
            {
                eSETID = temp2Item.ESETID.Value;
                return eSETID;
            }

            do
            {
                // 取得最新的Key值（max + 1,  記入 sys_autonum 控管）
                eSETID = dao.GetNewId("E_ENTERTEMP2_ESETID_SEQ,STUD_ENTERTEMP2,ESETID").Value;

                // 再次確認Key值有無被使用
                temp2Item = dao.GetEnterTemp2ByeSETID(eSETID);
            } while (temp2Item != null);

            return eSETID;
        }

        /// <summary>
        /// 產投課程報名處理作業 (注意: 此為背景非同步作業, 不會立即得到處理結果)
        /// <para>1.儲存e網報名暫存資料（STUD_ENTERTEMP2）</para>
        /// <para>2.儲存班級報名資料（STUD_ENTERTYPE2, STUD_ENTERTRAIN2）</para>
        /// <para>3.報名時會額外產生一組報名結果序號</para>
        /// </summary>
        /// <param name="model"></param>
        public void ProcessEnter(SignUpViewModel model)
        {
            (new ProcessEnterWorker()).QueueWorkItem(model);
        }


        /// <summary> 刪除異常的報名資料 stud_entertype2, stud_entertrain2 </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <param name="signno"></param>
        public void _DeleteEnterErr(decimal ocid, string idno, decimal signno)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            dao.BeginTransaction();
            try
            {
                // 刪除stud_entertype2 的異常資料
                this.DeleteEnterType2Err(ocid, idno, dao);

                // 備份&刪除 stud_entertrain2 資料
                this.DeleteEnterTrain2Err(ocid, idno, signno, dao);

                dao.CommitTransaction();
            }
            catch (Exception ex)
            {
                dao.RollBackTransaction();
                logger.Error($"DeleteEnterErr ex:{ex.Message}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// stud_entertype2 有異常資料,刪除無序號資料(剛除異常筆數共小於5筆的資料)
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        private void DeleteEnterType2Err(decimal ocid, string idno, WDAIIPWEBDAO dao)
        {
            //有異常時會有資料
            IList<Hashtable> type2Err1List = dao.QueryEnterTypeErr1List(ocid, idno);

            //等待30秒刪除沒有序號的資料 //int iSleep = 1000;
            Int64 eSETID = 0;

            if (type2Err1List != null && type2Err1List.Count > 0)
            {
                //eric, Sleep 邏輯是否有必要???// 若真有必要, 要改寫這一段, 不能直接在 IIS Request Thread Sleep// 會造成 IIS 效能下降
                //有異常資料 '等待10秒 //iSleep = 1000 * 10; //Thread.Sleep(iSleep);

                var type2Info = type2Err1List[0];

                Int64.TryParse($"{type2Info["ESETID"]}", out eSETID);

                if (eSETID > 0)
                {
                    //判斷資料筆數 超過5筆則停止刪除作業
                    IList<Hashtable> type2Err2List = dao.QueryEnterTypeErr2List(ocid, eSETID);

                    if (type2Err2List != null && type2Err2List.Count < 5) { dao.DeleteEnterType2Err(ocid, eSETID); }
                }

                //等待30秒刪除沒有序號的資料 //iSleep = 1000 * 30; //Thread.Sleep(iSleep);
                dao.DeleteEnterType2Err(ocid, eSETID);
            }
        }

        /// <summary>
        /// stud_entertrain2 報名成功，檢查異常資料並刪除。
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="idno"></param>
        /// <param name="signno"></param>
        private void DeleteEnterTrain2Err(decimal ocid, string idno, decimal signno, WDAIIPWEBDAO dao)
        {
            // 找出 stud_entertype2  6小時內 第1筆序號
            TblSTUD_ENTERTYPE2 type2Info = dao.GetEnterType2MinESERNUM(ocid, idno, signno);

            if (type2Info == null) return;
            Int64 iESERNUM = type2Info.ESERNUM.Value;

            // 找出 stud_entertrain2 最大重複資料序號
            TblSTUD_ENTERTRAIN2 train2Info = dao.GetEnterTrain2MaxSEID(iESERNUM);

            if (train2Info == null) return;
            Int64 iSEID = train2Info.SEID.Value;

            //執行刪除 stud_entertrain2
            dao.DeleteEnterTrain2(idno, iSEID);
        }
        #endregion

        #region 在職課程報名-報名作業

        /// <summary>
        /// 課程報名-報名作業, 
        /// 06:在職 /28:產投 /70:區域產業據點 課程報名作業
        /// 作業結果回寫 model 中下列欄位: 
        /// <para>model.SignUpErrMsg: 報名結果錯誤訊息</para>
        /// </summary>
        /// <param name="model"></param>
        public void ProcessEnter2(SignUpViewModel model, SessionModel sm)
        {
            decimal ocid = model.Form.OCID.Value;
            string idno = model.Detail.IDNO;
            DateTime birth = model.Detail.BIRTHDAY.Value;

            //取得學員資料 Convert_E_Member
            EMemberExtModel memInfo = this.GetEMember(idno, birth);
            if (memInfo == null)
            {
                model.SignUpErrMsg = "找不到會員基本資料(E_MEMBER)";
                return;
            }

            //取得班級資料 Check_OCID
            ClassClassInfoExtModel classInfo = this.CheckOCID(model);
            if (!string.IsNullOrEmpty(model.SignUpErrMsg)) return;

            model.ClassInfo = classInfo;

            //取得學員報名此班狀況 Join_Class (我要報名 check Class_ClassInfo e_member)
            this.JoinClass(model, memInfo);
            if (!string.IsNullOrEmpty(model.SignUpErrMsg)) return;

            //TODO: 轉換資料 '線上報名資料補充2 Convert_EnterSubData2

            //檢查e網前台 報名重複資料 MEM_060 依 mem_idno 單1身份證號(排除同身份證號)
            this.CheckClsTrace(model);
            if (!string.IsNullOrEmpty(model.SignUpErrMsg)) return;

            //再檢查報名資料其他狀況2 依 mem_idno 單1身份證號(排除同身份證號)
            this.CheckEClsTrace2(model, memInfo);
            if (!string.IsNullOrEmpty(model.SignUpErrMsg)) return;

            //檢查通過後 儲存報名資料
            this.SaveEnterTempA(model, memInfo, sm);
        }

        /// <summary>
        /// 取得指定 身分證+生日 的會員(E_MAMBER)資料, 
        /// 若找不到則回傳 null
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        private EMemberExtModel GetEMember(string idno, DateTime birth)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            EMemberExtModel memInfo = dao.GetEMember(idno, birth);

            if (memInfo != null)
            {
                memInfo.MEM_IDNO = memInfo.MEM_IDNO.ToUpper();

                //學歷背景
                if (!string.IsNullOrEmpty(memInfo.MEM_EDU))
                {
                    memInfo.MEM_EDU = memInfo.MEM_EDU.ToUpper();
                }

                //身份別
                if (!string.IsNullOrEmpty(memInfo.MEM_FOREIGN))
                {
                    memInfo.MEM_FOREIGN = memInfo.MEM_FOREIGN.ToUpper();
                }

                //兵役狀況
                if (!string.IsNullOrEmpty(memInfo.MEM_MILITARY))
                {
                    memInfo.MEM_MILITARY = memInfo.MEM_MILITARY.ToUpper();
                }

                //婚姻狀況
                if (!string.IsNullOrEmpty(memInfo.MEM_MARRY))
                {
                    memInfo.MEM_MARRY = memInfo.MEM_MARRY.ToUpper();
                }

                //畢業狀況
                if (!string.IsNullOrEmpty(memInfo.MEM_GRADUATE))
                {
                    memInfo.MEM_GRADUATE = memInfo.MEM_GRADUATE.ToUpper();
                }

                if ("00".Equals(memInfo.HANDTYPEID)
                    || string.IsNullOrWhiteSpace(memInfo.HANDTYPEID)
                    || "01".Equals(memInfo.HANDLEVELID)
                    || string.IsNullOrWhiteSpace(memInfo.HANDLEVELID))
                {
                    //異常資料代表為非障礙。
                    memInfo.HANDTYPEID = "";
                    memInfo.HANDLEVELID = "";
                }

                if (string.IsNullOrWhiteSpace(memInfo.HANDTYPEID2)
                    || string.IsNullOrWhiteSpace(memInfo.HANDLEVELID2))
                {
                    //異常資料代表為非障礙
                    memInfo.HANDTYPEID2 = "";
                    memInfo.HANDLEVELID2 = "";
                }

                memInfo.HANDTYPEYN = "N";
                if (!string.IsNullOrWhiteSpace(memInfo.HANDTYPEID)
                    || !string.IsNullOrWhiteSpace(memInfo.HANDLEVELID))
                {
                    memInfo.HANDTYPEYN = "Y";
                }

                if (!string.IsNullOrWhiteSpace(memInfo.HANDTYPEID2)
                    || !string.IsNullOrWhiteSpace(memInfo.HANDLEVELID2))
                {
                    memInfo.HANDTYPEYN = "Y";
                }

                memInfo.HANDTYPENUM = "2";
                if (!string.IsNullOrWhiteSpace(memInfo.HANDTYPEID))
                {
                    //以新制為準 舊制為輔
                    memInfo.HANDTYPENUM = "1";
                }

                if (!string.IsNullOrEmpty(memInfo.MEM_OPENSEC))
                {
                    //個資使用是否同意
                    memInfo.MEM_OPENSEC = memInfo.MEM_OPENSEC.ToUpper();
                }

                if (!string.IsNullOrEmpty(memInfo.EPAPER))
                {
                    //訂閱電子報
                    memInfo.EPAPER = memInfo.EPAPER.ToUpper();
                }

                if (!string.IsNullOrEmpty(memInfo.MEM_SEX))
                {
                    //性別
                    switch (memInfo.MEM_SEX)
                    {
                        case "1":
                            memInfo.MEM_SEX = "M";
                            break;
                        case "2":
                            memInfo.MEM_SEX = "F";
                            break;
                    }
                }
            }

            return memInfo;
        }

        /// <summary>
        /// 根據傳入的班級代碼(model.Detail.OCID) 檢查班級狀況, 
        /// 若檢核成功(可報名)則回傳班級資料(ClassClassInfoExtModel),
        /// 若檢核失敗(不可報名)則回傳 null, 並將檢核結果訊息回寫 model.SignUpErrMsg
        /// </summary>
        /// <param name="model"></param>
        private ClassClassInfoExtModel CheckOCID(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            DateTime aNow = new MyKeyMapDAO().GetSysDateNow();
            ClassClassInfoExtModel classInfo = null;

            long? ocid = model.Detail.OCID;
            string chkMsg = "";

            model.SignUpErrMsg = "";
            if (!ocid.HasValue)
            {
                chkMsg = "輸入有誤，班級代號為空。";
                model.SignUpErrMsg = chkMsg;
                return classInfo;
            }

            try
            {
                classInfo = dao.GetClassInfoByOCID(ocid.Value);
            }
            catch (Exception ex)
            {
                logger.Error($"CheckOCID ex:{ex.Message}", ex);
                chkMsg = $"程式錯誤!，班級代號: {ocid}, ex:{ex.Message}";
                model.SignUpErrMsg = chkMsg;
                return classInfo;
            }

            if (classInfo == null)
            {
                chkMsg = $"輸入有誤，班級代號 {ocid} 查無此班。";
                model.SignUpErrMsg = chkMsg;
                return classInfo;
            }
            if (string.IsNullOrEmpty(classInfo.TPLANID))
            {
                chkMsg = $"輸入有誤，班級代號 {ocid} 查無此班。";
                model.SignUpErrMsg = chkMsg;
                return classInfo;
            }

            //sType: 0:限定報名期間 1:不限定報名期間
            string sType = "0";
            if ("0".Equals(sType))
            {
                if (classInfo.FENTERDATE.HasValue)
                {
                    //課程已結束報名
                    if (DateTime.Compare(aNow, classInfo.FENTERDATE.Value) > 0)
                    {
                        chkMsg = $"{ocid}該課程已結束報名(依報名截止日期時間)";
                        model.SignUpErrMsg = chkMsg;
                        return classInfo;
                    }
                }
                else
                {
                    //情況為不可能(通尚該日為必填)
                    chkMsg = $"{ocid}該課程已結束報名(無截止日)";
                    model.SignUpErrMsg = chkMsg;
                    return classInfo;
                }

                if (classInfo.SENTERDATE.HasValue)
                {
                    //課程報名尚未開始
                    if (DateTime.Compare(aNow, classInfo.SENTERDATE.Value) < 0)
                    {
                        chkMsg = $"{ocid}該課程報名尚未開始(依報名開始日期時間)";
                        model.SignUpErrMsg = chkMsg;
                        return classInfo;
                    }
                }
                else
                {
                    //情況為不可能(通尚該日為必填)
                    chkMsg = $"{ocid}該課程報名尚未開始(無開始日)";
                    model.SignUpErrMsg = chkMsg;
                    return classInfo;
                }

                if ("Y".Equals(Convert.ToString(classInfo.NOTOPEN).ToUpper()))
                {
                    //此訓練班級為不開班,無法報名!
                    chkMsg = $"{ocid}此訓練班級為不開班,無法報名!";
                    model.SignUpErrMsg = chkMsg;
                    return classInfo;
                }
            }

            model.SignUpErrMsg = chkMsg;
            return classInfo;
        }

        /// <summary>
        /// 檢核會員報名班級(model.ClassInfo)的各項限制條件(目前只有檢核報名年齡條件),
        /// 檢核結果回寫 model.SignUpErrMsg
        /// </summary>
        /// <param name="model"></param>
        /// <param name="memInfo"></param>
        private void JoinClass(SignUpViewModel model, EMemberExtModel memInfo)
        {
            ClassClassInfoExtModel classInfo = model.ClassInfo;

            if (classInfo.CAPAGE1.HasValue)
            {
                if (DateTime.Compare(memInfo.MEM_BIRTH.Value.AddYears(classInfo.CAPAGE1.Value), classInfo.STDATE.Value) > 0)
                {
                    model.SignUpErrMsg = $"您的年齡不符合此課程報名資格的最小年齡要求！({classInfo.CAPAGE1})";
                }
            }
        }

        /// <summary>
        /// 檢查e網前台 報名重複資料 MEM_060
        /// <para>根據 model.Form.OCID 及 model.Form.IDNO</para>
        /// <para>檢核結果回寫 model.SignUpErrMsg</para>
        /// </summary>
        /// <param name="model"></param>
        private void CheckClsTrace(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            decimal ocid = model.Form.OCID ?? 0;
            string idno = model.Form.IDNO;

            IList<ClassClassInfoExtModel> classList = dao.QueryClassOrgInfoByOCID(ocid);
            string sTProperty = string.Empty;
            string sTPropertyType = string.Empty;
            string ocids = string.Empty;
            string chkMsg = string.Empty;

            if (classList != null && classList.Count > 0)
            {
                // TODO: 產投系統不會再有職前課程報名記錄, 實際檢核作法待確認 

                // start 訓練性質判斷(在職跟職前不能一起報名) 
                //foreach (var item in classList)
                //{
                //    sTProperty += Convert.ToString(item.TPROPERTYID);
                //    if (sTProperty.IndexOf("0") >= 0 && sTProperty.IndexOf("1") >= 0)
                //    {
                //        chkMsg = "不能同時報名訓練性質為 在職跟職前的班級";
                //        break;
                //    }
                //}
                //if (string.IsNullOrEmpty(chkMsg) && !string.IsNullOrEmpty(sTProperty))
                //{
                //    //判斷報名班級是1: 在職或是0: 職前的
                //    if (sTProperty.IndexOf("0") >= 0)
                //    {
                //        sTPropertyType = "0"; //0職前
                //    }

                //    if (sTProperty.IndexOf("1") >= 0)
                //    {
                //        sTPropertyType = "1"; //1在職
                //    }
                //}
                // end 訓練性質判斷(在職跟職前不能一起報名) 


                //[已拿掉] 狀況二(當本次報名課程清單中,與過去已報名課程,為同一培訓單位
                //,且甄試日期為同一天的報名課程,顯示訊息如下

                //您所選擇由XXXXX(訓練單位名稱)開訓之課程XXXXX(課程名稱)
                //，與您日前已完成報名之XXXXX(課程名稱)甄試作業為同一天舉辦
                //，因故無法受理您此次報名需求，請見諒。

                //[已拿掉] 狀況三(當本次報名課程清單中,與過去已報名課程,為同一培訓單位
                //,且甄試日期為同一天的報名課程,但已報名課程之報名資料被審核失敗,則容許報名本次)
                //(e網審核成功,錄取作業為未選擇,正取與備取者.)
                //請查閱： Get_Stud_EnterType2_OCIDs


                //狀況四(當本次報名課程清單中,與過去已報名課程,為同一培訓單位
                //,但已報名課程之報名資料被審核失敗,則容許報名本次，否則不可報名)
                IList<ClassClassInfoExtModel> chkClassList = null;
                if (string.IsNullOrEmpty(chkMsg))
                {
                    //查詢收件完成（審核中）及報名成功的班
                    IList<TblSTUD_ENTERTYPE2> type2List = dao.QueryStudEnterType2ByIDNO(idno);

                    if (type2List != null)
                    {
                        foreach (var type2Item in type2List)
                        {
                            if (ocids.IndexOf($"{type2Item.OCID1}") < 0)
                            {
                                if (!string.IsNullOrEmpty(ocids)) ocids += ",";
                                ocids += $"{type2Item.OCID1}";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(ocids))
                    {
                        //要報名的班
                        foreach (var classItem in classList)
                        {
                            //查詢要報的班是否已在報名成功階段，若是則停止報名
                            chkClassList = dao.QueryClassOrgInfoByOCIDs(ocids, classItem.ORGID.Value, classItem.OCID.Value);

                            if (chkClassList != null && chkClassList.Count > 0)
                            {
                                var chkClassItem = chkClassList[0];

                                chkMsg = "您所選擇由 「{0}」 ({1})(培訓單位)";
                                chkMsg += "開訓之課程 報名課程1: 「{2}」 ({3}) <br>";
                                chkMsg += "與您日前已完成報名之 報名課程2: 「{4}」({5}) ";
                                chkMsg += "已有報名資料，尚在審核中或報名成功，因故無法受理您此次報名需求，請見諒。";

                                chkMsg = string.Format(chkMsg, classItem.ORGNAME, Convert.ToString(classItem.ORGID), classItem.CLASSNAME, Convert.ToString(classItem.OCID), chkClassItem.CLASSNAME, Convert.ToString(chkClassItem.OCID));
                                break;
                            }
                        }
                    }
                }

                //狀況五(已在內網有報名資料)
                ocids = "";
                chkClassList = null;
                if (string.IsNullOrEmpty(chkMsg))
                {
                    IList<TblSTUD_ENTERTYPE> typeList = dao.QueryStudEnterTypeByIDNO(idno);
                    if (typeList != null)
                    {
                        foreach (var typeItem in typeList)
                        {
                            if (ocids.IndexOf($"{typeItem.OCID1}") < 0)
                            {
                                if (!string.IsNullOrEmpty(ocids)) ocids += ",";
                                ocids += $"{typeItem.OCID1}";
                            }
                        }

                        if (!string.IsNullOrEmpty(ocids))
                        {
                            //要報名的班
                            foreach (var classItem in classList)
                            {
                                chkClassList = dao.QueryClassOrgInfoByOCIDs2(ocids, classItem.ORGID.Value, classItem.OCID.Value);

                                if (chkClassList != null && chkClassList.Count > 0)
                                {
                                    var chkClassItem = chkClassList[0];

                                    chkMsg = "您所選擇由 「{0}」 ({1})(培訓單位)";
                                    chkMsg += "開訓之課程 報名課程1: 「{2}」 ({3})<br>";
                                    chkMsg += "與您日前已完成報名之 報名課程2: 「{4}」 ({5}) ";
                                    chkMsg += "已在內網有報名資料，尚在審核中或報名成功，因故無法受理您此次報名需求，請見諒。";

                                    chkMsg = string.Format(chkMsg, classItem.ORGNAME, Convert.ToString(classItem.ORGID), classItem.CLASSNAME, Convert.ToString(classItem.OCID), chkClassItem.CLASSNAME, Convert.ToString(chkClassItem.OCID));
                                    break;
                                }
                            }
                        }
                    }
                }

                //狀況五-2(已在內網有報名資料)(甄試作業為同一天舉辦) --> 已拿掉
            }

            model.SignUpErrMsg = chkMsg;
        }

        /// <summary>
        /// 檢核要報名的班級限制資料 CapDegree CapAge
        /// <para>檢核結果回寫 model.SignUpErrMsg</para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="memInfo"></param>
        private void CheckEClsTrace2(SignUpViewModel model, EMemberExtModel memInfo)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            /* , string capdegreeName */
            string chkMsg = string.Empty;

            decimal ocid = model.Detail.OCID ?? 0;
            DateTime birth = model.Detail.BIRTHDAY.Value;
            string memDegreeID = memInfo.MEM_EDU;


            //查詢要報名的班級限制資料 CapDegree CapAge(此欄位都為必填)
            IList<ClassClassInfoExtModel> classList = dao.QueryClassOrgInfoByOCIDs3(ocid);
            string capDegreeName = string.Empty;
            string age = string.Empty;
            DateTime zeroTime = new DateTime(1, 1, 1);
            int years = 0;
            string capdegree = string.Empty;

            // 會員學歷代碼(memDegreeID)對應名稱
            Dictionary<string, string> memDegreeNames = new Dictionary<string, string>()
            {
                { "00", "不限" },
                { "01", "國中(含)以下" },
                { "02", "高中/職" },
                { "03", "專科" },
                { "04", "大學(大學)" },
                { "05", "碩士" },
                { "06", "博士" },
                { "07", "國中(含)以上" },
                { "08", "高中/職(含)以上" },
                { "09", "專科(含)以上" },
                { "10", "大學(含)以上" },
                { "11", "碩士(含)以上" },
                { "12", "大學(二技)" },
                { "13", "大學(四技)" },
            };

            string memDegreeName = memDegreeNames[memDegreeID];

            if (classList != null)
            {
                foreach (var classItem in classList)
                {
                    capDegreeName = classItem.CAPDEGREE_TEXT; //文字顯示班級限制的學歷

                    //DateDiff(DateInterval.Year, CDate(sBirthday), CDate(dr("STDate")))
                    TimeSpan span = classItem.STDATE.Value - birth;
                    years = (zeroTime + span).Year - 1;
                    age = years.ToString();

                    if (DateTime.Compare(birth.AddYears(classItem.CAPAGE1.Value), classItem.STDATE.Value) > 0)
                    {
                        chkMsg += string.Format("[{0}] 您的年齡 ( {1} ) 不符合此課程報名資格的年齡要求!( {2} ~ {3} ) 依開訓日期 ({4})<br>"
                            , Convert.ToString(classItem.OCID)
                            , age
                            , Convert.ToString(classItem.CAPAGE1)
                            , Convert.ToString(classItem.CAPAGE2)
                            , classItem.STDATE_TW);
                    }


                    switch (Convert.ToString(classItem.CAPDEGREE))
                    {
                        case "00": //不限
                            break;
                        case "14": //國小(含)以上
                            break;
                        default:
                            capdegree = Convert.ToString(classItem.CAPDEGREE);
                            if (string.Compare(Convert.ToString(capdegree), "07") >= 0
                                && string.Compare(Convert.ToString(capdegree), "13") <= 0)
                            {
                                //e_member.mem_edu
                                switch (memDegreeID)
                                {
                                    case "01": //國中(含)以下
                                        if (string.Compare(Convert.ToString(capdegree), "07") > 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "02": //高中/職
                                        if (string.Compare(Convert.ToString(capdegree), "08") > 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "03": //專科
                                        if (string.Compare(Convert.ToString(capdegree), "09") > 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "04": //大學(大學)
                                        if (string.Compare(Convert.ToString(capdegree), "11") == 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "05": //碩士
                                    case "06": //博士
                                        break;
                                }
                            }
                            else
                            {
                                //e_member.mem_edu
                                switch (memDegreeID)
                                {
                                    case "01": //國中(含)以下
                                        if (string.Compare(Convert.ToString(capdegree), "01") > 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "02": //高中/職
                                        if (string.Compare(Convert.ToString(capdegree), "02") > 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "03": //專科
                                        if (string.Compare(Convert.ToString(capdegree), "03") > 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "04": //大學(大學)
                                        if (string.Compare(Convert.ToString(capdegree), "04") == 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "05": //碩士
                                        if (string.Compare(Convert.ToString(capdegree), "04") == 0)
                                        {
                                            chkMsg += string.Format("[{0}] 您的學歷 ({1}) 不符合此課程報名資格的學歷 ({2}) 要求!"
                                                , Convert.ToString(classItem.OCID), memDegreeName, capDegreeName);
                                        }
                                        break;
                                    case "06": //博士
                                        break;
                                }

                            }

                            break;
                    }
                }
            }

            model.SignUpErrMsg = chkMsg;
        }

        /// <summary> 報名資料儲存作業（mem_ws.saveEnterTemp2） </summary>
        /// <param name="idno">身分證字號</param>
        /// <param name="birth">出生日期</param>
        /// <param name="ocid">班級代碼</param>
        /// <param name="chkMsg">提示訊息</param>
        private void SaveEnterTempA(SignUpViewModel model, EMemberExtModel memInfo, SessionModel sm)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string idno = model.Detail.IDNO;
            DateTime birth = model.Detail.BIRTHDAY.Value;
            decimal dec_OCID = model.Form.OCID.Value;
            string strSCHOOLNAME = model.Detail.SCHOOLNAME;
            string strDEPARTMENT = model.Detail.DEPARTMENT;
            Int64? i_SETID = null;
            Int64? i_eSETID = null;

            ClassClassInfoExtModel classInfo = model.ClassInfo;
            string chkMsg = "";

            //全範圍搜尋IDNO '有資料(修改模式) 'ALL '沒有資料(新增模式)
            TblSTUD_ENTERTEMP2 temp2Info = dao.GetStudEnterTemp2ByIDNO(idno, birth);
            bool blExistsTemp2 = false;
            bool flag_Temp2_idno_birth = false;
            bool flag_Temp2_idno = false;
            if (temp2Info != null)
            {
                flag_Temp2_idno_birth = true;
                blExistsTemp2 = true;
            }
            if (!blExistsTemp2)
            {
                //試著用 idno 再找一次(排除生日)
                temp2Info = dao.GetStudEnterTemp2ByIDNO(idno);
                if (temp2Info != null)
                {
                    //有找到 表示生日與[就業通]不同，依就業通生日為準
                    flag_Temp2_idno = true;
                    blExistsTemp2 = true;
                }
            }

            //找出 可能的 SETID '沒有為空
            TblSTUD_ENTERTEMP enterTempInfo = dao.GetEnterTempMaxSETID(idno, birth);
            bool flag_esetid_exists = true;
            if (enterTempInfo == null) { flag_esetid_exists = false; }
            if (enterTempInfo != null && enterTempInfo.SETID == null) { flag_esetid_exists = false; }
            //試著用 idno 再找一次(排除生日)
            if (!flag_esetid_exists) { enterTempInfo = dao.GetEnterTempMaxSETID(idno); }
            if (enterTempInfo != null && enterTempInfo.SETID != null) { i_SETID = enterTempInfo.SETID; }

            if (!string.IsNullOrEmpty(model.Detail.ZIPCODE2_6W))
            {
                bool flag_ZIPCODE2_6W_NG = false;
                if (model.Detail.ZIPCODE2_6W.Length != 2 && model.Detail.ZIPCODE2_6W.Length != 3) { flag_ZIPCODE2_6W_NG = true; }
                if (!flag_ZIPCODE2_6W_NG)
                {
                    int iZIPCODE2_6W = int.TryParse(model.Detail.ZIPCODE2_6W, out iZIPCODE2_6W) ? iZIPCODE2_6W : -1;
                    if (iZIPCODE2_6W < 0 || iZIPCODE2_6W > 999) { model.Detail.ZIPCODE2_6W = null; }
                }
                if (flag_ZIPCODE2_6W_NG) { model.Detail.ZIPCODE2_6W = null; }
            }

            //查詢會員基本資料 (基本上一定有資料!沒資料就不用玩了)
            TblE_MEMBER mem = dao.GetEMemberByIDNO(idno, birth);
            mem.MEM_GRADUATE = model.Detail.GRADID; /* 畢業狀況 */
            mem.MEM_SCHOOL = strSCHOOLNAME;
            mem.MEM_DEPART = strDEPARTMENT;
            mem.MEM_TEL = model.Detail.PHONED; /* 聯絡電話-日 */
            mem.MEM_TELN = model.Detail.PHONEN; /* 聯絡電話-夜 */
            mem.MEM_MOBILE = model.Detail.CELLPHONE; /* 聯絡電話-行動 */
            mem.MEM_ZIP = model.Detail.ZIPCODE; // Convert.ToString(model.Detail.ZIPCODE.Value);
            mem.MEM_ZIP6W = model.Detail.ZIPCODE2_6W;
            mem.MEM_ADDR = model.Detail.ADDRESS;
            mem.MEM_EMAIL = model.Detail.EMAIL;
            //2019-02-28 再convert一次性別
            if (!string.IsNullOrEmpty(mem.MEM_SEX))
            {
                switch (mem.MEM_SEX)
                {
                    case "1":
                        mem.MEM_SEX = "M";
                        break;
                    case "2":
                        mem.MEM_SEX = "F";
                        break;
                }
            }
            MyCommonUtil.HtmlDecode(mem); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）
            // 記錄登入者資訊 (會員資料同步到 E_MEMBER 中)
            dao.saveToEMember(mem);

            dao.BeginTransaction();
            try
            {
                //儲存報名基本資料（STUD_ENTERTEMP3）
                (new ProcessEnterWorker()).SaveEnterTemp3(model, dao);

                bool flag_insert_1 = false; //確認是否可以用新增，非新增就是修改
                TblSTUD_ENTERTEMP2 newTemp2Info = null;
                if (flag_Temp2_idno)
                {
                    //修改模式
                    newTemp2Info = dao.GetStudEnterTemp2ByIDNO(idno);
                    i_eSETID = newTemp2Info.ESETID;
                }
                if (flag_Temp2_idno_birth)
                {
                    //修改模式
                    newTemp2Info = dao.GetStudEnterTemp2ByIDNO(idno, birth);
                    i_eSETID = newTemp2Info.ESETID;
                }
                if (newTemp2Info == null)
                {
                    //上述都查無資料-產生一個新的-eSETID
                    i_eSETID = GetESETIDMaxID(idno, dao);
                    flag_insert_1 = true;
                }
                model.Detail.ESETID = i_eSETID;

                //新增模式 //查詢 len(eSETID) < 8
                //newTemp2Info = dao.GetStudEnterTemp2ByESETIDLen(idno, birth);
                if (flag_insert_1)
                {
                    //新增
                    dao.InsertStudEnterTemp2(mem, i_SETID, i_eSETID.Value);
                }
                else
                {
                    //修改
                    dao.UpdateStudEnterTemp2(mem, i_eSETID.Value);
                }

                // 記錄線上報名資料 stud_entertype2
                DateTime aNow = new MyKeyMapDAO().GetSysDateNow();
                // 取得資料庫系統時間
                model.SignUpTime = aNow;

                // 查詢課程報名結果資料（by當天） //TblSTUD_ENTERTYPE2 newType2Info = dao.GetStudEnterType2ByEnterDate(eSETID.Value, ocid, aNow.Date);
                TblSTUD_ENTERTYPE2 newType2Info = null;
                //2019-02-11 fix 報名在職課程：改成若要報同一班，上一筆報名資料(entertype2)需為審核失敗才能允許再次報名
                IList<TblSTUD_ENTERTYPE2> type2List = dao.QueryStudEnterType2ByOCID(i_eSETID.Value, dec_OCID);

                if (type2List != null && type2List.Count > 0)
                {
                    type2List = type2List.Where(x => x.SIGNUPSTATUS != 2).ToList();
                }

                string s_MIDENTITYID = (model.Detail.MIDENTITYID ?? "01");
                if (s_MIDENTITYID.Equals("01")) { s_MIDENTITYID = ("Y".Equals(memInfo.HANDTYPEYN) ? "06" : s_MIDENTITYID); }

                //if (newType2Info == null)
                if (type2List == null || type2List.Count == 0)
                {
                    //2019-02-11 add 多判斷entertype表中是否已有同一門課的報名資料，有則不允許再次報名（屬現場報名）
                    IList<TblSTUD_ENTERTYPE> typeList = dao.QueryStudEnterTypeNotE(sm.ACID, dec_OCID);

                    if (typeList == null || typeList.Count == 0)
                    {
                        // 取得最新sernum
                        Int64 newSERNUM = dao.GetNewSERNUM(i_eSETID.Value, aNow.Date);

                        // 取得最新esernum (len(esernum) < 8 for 在職用)
                        Int64 newESERNUM = dao.GetNewESERNUMBByLen();
                        // 不使用 產投產生序號方式 //Int64 newESERNUM = dao.GetNewId("STUD_ENTERTYPE2_ESERNUM_SEQ,STUD_ENTERTYPE2,ESERNUM").Value;
                        if (newESERNUM <= 0)
                        {
                            dao.RollBackTransaction();
                            chkMsg = "ESERNUM ERROR! 資料庫作業異常，送出失敗，如問題持續請聯絡系統管理人員!";
                            model.SignUpErrMsg = chkMsg;
                            return;
                        }

                        aNow = new MyKeyMapDAO().GetSysDateNow();

                        //新增
                        newType2Info = new TblSTUD_ENTERTYPE2
                        {
                            //eSerNum_MaxID
                            ESERNUM = newESERNUM,
                            ESETID = i_eSETID,
                            SETID = i_SETID,
                            ENTERDATE = aNow.Date,/*填寫日期*/
                            SERNUM = newSERNUM,/*序號*/
                            RELENTERDATE = aNow,/*報名日期*/
                            OCID1 = Convert.ToInt64(classInfo.OCID),
                            TMID1 = Convert.ToInt64(classInfo.TMID),
                            IDENTITYID = s_MIDENTITYID,/*06:身心障礙者 , 01:一般身份者(直接加入)*/
                            RID = classInfo.RID,
                            PLANID = Convert.ToInt64(classInfo.PLANID),
                            SIGNUPSTATUS = 0,/* 0:收件 */
                            SIGNUPMEMO = string.Empty,/*報名備註-memInfo.MEM_MEMO*/
                            ISOUT = 0,/*暫定規則 */
                            MODIFYACCT = idno,
                            MODIFYDATE = aNow,
                            ENTERPATH = "E",/*報名路徑，來自E網*/
                            MIDENTITYID = s_MIDENTITYID
                        };
                        dao.Insert(newType2Info);

                        // 新增 stud_entertrain2
                        model.Detail.ESERNUM = newESERNUM;
                        // 新增線上報名資料 - stud_entertrain2
                        this.StoreEnterTrain2Model(dao, model);

                        //insert/update entertemp3
                        IList<TblSTUD_ENTERTEMP3> temp3List = dao.GetRowList(new TblSTUD_ENTERTEMP3 { IDNO = model.Detail.IDNO, BIRTHDAY = model.Detail.BIRTHDAY });
                        if (temp3List == null || temp3List.Count == 0)
                        {
                            //新增
                            dao.InsertEnterTemp3(model.Detail);
                        }
                        else
                        {
                            //修改
                            dao.UpdateEnterTemp3(model.Detail);
                        }

                        dao.CommitTransaction();
                    }
                    else
                    {
                        chkMsg = $"({dec_OCID})已有報名資料，請確認!(非網路)";
                        //有異常, 資料倒回
                        dao.RollBackTransaction();
                    }
                }
                else
                {
                    /*IList<TblSTUD_ENTERTYPE2> notEEnterList = dao.QueryStudEnterType2NotE(eSETID.Value, ocid, aNow);
                     * ,if (notEEnterList != null && notEEnterList.Count > 0),{,chkMsg = "({0})當天已有報名資料，請確認!(非E網)";,}
                     * ,else,{,chkMsg = "({0})當天已有(E網)報名資料，請確認!";,}*/

                    chkMsg = $"({dec_OCID})已有(網路)報名資料，請確認!";
                    //有異常, 資料倒回
                    dao.RollBackTransaction();
                }
            }
            catch (Exception ex)
            {
                dao.RollBackTransaction();
                logger.Error($"WDAIIPService SaveEnterTempA ex:{ex.Message}", ex);
                chkMsg = "資料庫作業異常，送出失敗，請重新選擇送出，如問題持續請聯絡系統管理人員";
            }
            model.SignUpErrMsg = chkMsg;
        }

        /// <summary>儲存 STUD_ENTERTRAIN2 資料操作
        /// <para>會以 data.ESERNUM 判斷資料是否存在, 只有當資料不存在時才會新增,若資料已存在, 則不會異動</para>
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="model"></param>
        private void StoreEnterTrain2Model(WDAIIPWEBDAO dao, SignUpViewModel model)
        {
            var data = model.Detail;

            // 無報名資料序號不寫入
            if (!data.ESERNUM.HasValue || data.ESERNUM <= 0)
            {
                logger.Warn("StoreEnterTrain2Model: 報名資料序號(ESERNUM) <= 0");
                throw new Exception("StoreEnterTrain2Model: 報名資料序號(ESERNUM) <= 0");
            }

            // 若查到已有資料了就不再寫入
            TblSTUD_ENTERTRAIN2 train2Info = dao.GetStudEnterTrain2ByESERNUM(data.ESERNUM.Value);
            if (train2Info != null)
            {
                logger.Info("StoreEnterTrain2Model: 資料已存在, ESERNUM=" + data.ESERNUM.Value);
                return;
            }

            // 新增 STUD_ENTERTRAIN2
            dao.InsertStudEnterTrain2(model);
        }

        #endregion
    }

    /// <summary>
    /// 產投報名背景執行工作封裝
    /// </summary>
    public class ProcessEnterWorker
    {
        private static ILog logger = LogManager.GetLogger("ProcessEnterWorker");
        private static ILog traceLogger = LogManager.GetLogger(typeof(ProcessEnterWorker));

        /// <summary>
        /// 報名處理排隊等待最長時間(秒), 
        /// 若大於這個值還無法進行報名處理, 則應結束等待返回
        /// <para>這個值可由 appSetting ProcessEnterWorkerWaitTimeout 調整, 
        /// 但只會讀取1次, 若要調整該設定則要重新啟動 IIS 才會生效</para>
        /// </summary>
        private static readonly long WAIT_TIMEOUT_MILLISECONDS = ConfigModel.ProcessEnterWorkerWaitTimeout * 1000;

        /// <summary>
        /// 當作業中的報名處理個數達上限時的等待時間( 500 毫秒),
        /// 新的報名處理要求將等待這個秒數後, 再次檢查作業中的處理個數
        /// </summary>
        private static readonly int WAIT_LOOP_MILLISECONDS = 500;
        private static readonly int WAIT_LOOP_MILLISECONDS2 = 630;
        private static readonly int WAIT_LOOP_MILLISECONDS3 = 760;

        /// <summary>啟動產投報名背景處理程序<para>當作業中的報名處理個數達上限時, 會自動等待排隊(Queue Wait, FIFO)</para>
        /// </summary>
        /// <param name="model"></param>
        public void QueueWorkItem(SignUpViewModel model)
        {
            string host = System.Web.HttpContext.Current.Server.MachineName;
            int MaxThreads = ConfigModel.ProcessEnterWorkerMaxThreads;
            SessionModel sm = SessionModel.Get();
            ClassSignUpCtl ctl = new ClassSignUpCtl(host);

            // 報名作業排隊取號 // 這個不是產投報名結果序號, 只是 Queing 系統的處理順序
            int seq = ctl.GetWorkerSeq(MaxThreads);
            string idno = model.Detail.IDNO;
            long ocid = Convert.ToInt64(model.ClassInfo.OCID.Value);
            string strInfo = $"<{sm.SessionID}:{seq}>: IDNO= {idno}, OCID={ocid}";
            logger.Info($"QueueWorkItem({strInfo})");

            try
            {
                // 起始報名處理狀態
                ClassSignUpStatus signUpInfo = ClassSignUpStatus.GetSignUpInfo(host, sm.SessionID); //啟動產投報名背景處理程序--起始狀態
                signUpInfo.Host = host;
                signUpInfo.QueueSeq = seq;
                signUpInfo.IDNO = idno;
                signUpInfo.OCID = ocid;
                signUpInfo.Status = -1;
                signUpInfo.PreStatus = -1;
                signUpInfo.QueueTime = null;
                signUpInfo.StartTime = null;
                signUpInfo.Timeout = false;
                signUpInfo.FinishTime = null;
                // 起始
                ClassSignUpStatus.SetSignUpInfo(host, sm.SessionID, signUpInfo);  // 起始 
                // 啟動產投報名背景處理程序(排入 Thread Pool)
                Action<CancellationToken> action = token => DoProcess(token, ctl, sm.SessionID, model, seq, MaxThreads);
                // 背景處理程序
                HostingEnvironment.QueueBackgroundWorkItem(action);
            }
            catch (Exception ex)
            {
                logger.Error($"QueueWorkItem({strInfo}): ex:{ex.Message}", ex);
                try
                {
                    // 報名處理異常, 清除佔用的處理計數 WORKER_ITEM_COUNTER
                    ctl.DecWorkerCounter(ctl.HasAllocWorkerCounter, false);
                }
                catch (Exception e)
                {
                    logger.Error($"QueueWorkItem({strInfo}): 清除佔用的處理計數 WORKER_ITEM_COUNTER ex:{e.Message}", e);
                }
            }

        }

        /// <summary>
        /// 產投報名實際處理邏輯 (這個 function 是由背景 Thread 所執行, 不在 MVC main thread 中)
        /// <para>作業狀態及結果將記錄在 ClassSignUpStatus 中</para>
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcess(CancellationToken cancellationToken, ClassSignUpCtl ctl, string sessionId, SignUpViewModel model, int workerSeq, int MaxThreads)
        {
            string idno = model.Detail.IDNO;
            decimal ocid = model.ClassInfo.OCID.Value;
            string strInfo = $"<{sessionId}:{workerSeq}>: IDNO={idno}, OCID={ocid}";

            ClassSignUpStatus signUpInfo = ClassSignUpStatus.GetSignUpInfo(ctl.Host, sessionId); //DoProcess 產投報名實際處理
            signUpInfo.ThreadID = Thread.CurrentThread.ManagedThreadId;
            signUpInfo.QueueTime = DateTime.Now;
            signUpInfo.Status = 0; // 排隊中
            signUpInfo.ErrMsg = "";
            ClassSignUpStatus.SetSignUpInfo(ctl.Host, sessionId, signUpInfo);  // 排隊等候

            //string s_errMsg = string.Empty;
            bool canDo = false;
            //int maxThreads = ConfigModel.ProcessEnterWorkerMaxThreads;
            Stopwatch sw = Stopwatch.StartNew(); // 使用 Stopwatch 精確計時

            try
            {
                while (!canDo)
                {
                    // 檢查 CancellationToken (IIS 停止或 Request 取消)
                    if (cancellationToken.IsCancellationRequested)
                    {
                        string s_errMsg = "系統正在關閉或請求已取消";
                        logger.Warn($"DoProcess({strInfo}):{s_errMsg}");
                        throw new OperationCanceledException(s_errMsg);
                    }

                    // 1. 取得當前控制台快照，減少後續多次讀取 DB 的開銷
                    var ctlStatus = ctl.GetSignUpCtl();
                    int currentSeq = ctlStatus.CURSEQ ?? 0;
                    int runningCount = ctlStatus.PCOUNT ?? 0;
                    logger.Info($"DoProcess({strInfo}): INIT. currentSeq:{currentSeq}. PCOUNT:{runningCount}.");

                    // 2. 核心判斷：是否輪到我且容量充足,runningCount < maxThreads &&
                    if (currentSeq + 1 == workerSeq)
                    {
                        // 查是否已輪到當前的排隊號碼,嘗試正式取得權限 (此方法內應含同步鎖或原子更新)
                        canDo = ctl.CheckWorkerSeq(workerSeq, MaxThreads);

                        if (canDo) break;
                    }
                    // 5. 【關鍵優化】自適應等待邏輯 (Adaptive Backoff),根據排在前面的人數來決定,加入隨機抖動 (Jitter) 避免所有 Thread 同時喚醒衝撞 DB
                    int queueGap = workerSeq - currentSeq;
                    int baseSleep = (queueGap > MaxThreads * 2) ? WAIT_LOOP_MILLISECONDS3 : ((queueGap > MaxThreads) ? WAIT_LOOP_MILLISECONDS2 : WAIT_LOOP_MILLISECONDS); //基本0.5 秒,差距兩倍以上n秒,差距一倍以上1秒
                    int basejitter = queueGap + baseSleep + (new Random().Next(-100, 101));

                    // 4.異常跳號判定,當前號碼已超過我的號碼，說明中間發生跳號 或是人太多
                    if (currentSeq > workerSeq || queueGap > MaxThreads * 3)
                    {
                        string skipMsg = $"因報名人數眾多系統無法即時處理(3), 請稍後再試.{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{workerSeq},{currentSeq}";
                        //string skipMsg = $"排隊號碼異常已失效，請重新報名。";
                        signUpInfo.WaitTimes = (long)sw.ElapsedMilliseconds;
                        HandleProcessEnd(ctl, sessionId, signUpInfo, 9, skipMsg, true, null, strInfo);
                        logger.Warn($"DoProcess({strInfo}): Seq Skipped(排隊號碼異常已失效).{skipMsg}:{sw.ElapsedMilliseconds}ms,WorkerSeq:{workerSeq}, CurrentSeq:{currentSeq}");
                        return;
                    }

                    // 3. 逾時判定(保留時間判定)
                    if (sw.ElapsedMilliseconds >= WAIT_TIMEOUT_MILLISECONDS)
                    {
                        string timeoutMsg = $"等待逾時({WAIT_TIMEOUT_MILLISECONDS / 1000}秒), 因報名人數眾多系統無法即時處理(2), 請稍後再試.{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{workerSeq},{currentSeq}";
                        signUpInfo.WaitTimes = (long)sw.ElapsedMilliseconds;
                        HandleProcessEnd(ctl, sessionId, signUpInfo, 3, timeoutMsg, true, null, strInfo);
                        logger.Warn($"DoProcess({strInfo}): TIMEOUT.{timeoutMsg} Elapsed:{sw.ElapsedMilliseconds}ms,WorkerSeq:{workerSeq}, CurrentSeq:{currentSeq}");
                        return;
                    }

                    // 5.
                    Thread.Sleep(basejitter);
                }

                // 報名處理邏輯開始 // 遞增 Worker Counter,進入正式報名程序
                ctl.IncWorkerCounter(true);

                signUpInfo.StartTime = DateTime.Now;
                //signUpInfo.PreStatus = signUpInfo.Status;
                signUpInfo.Status = 1; // 處理中
                signUpInfo.WaitTimes = (long)sw.ElapsedMilliseconds;

                ClassSignUpStatus.SetSignUpInfo(ctl.Host, sessionId, signUpInfo);

                logger.Info($"DoProcess({strInfo}): START processing after {sw.ElapsedMilliseconds}ms wait.");

                // 執行實際報名邏輯
                _DoProcess(model);

                // 成功處理
                HandleProcessEnd(ctl, sessionId, signUpInfo, 2, "", false, model.SIGNNO, strInfo);
            }
            catch (SignUpException se)
            {
                HandleProcessEnd(ctl, sessionId, signUpInfo, 3, se.Message, false, null, strInfo);
                string s_errMsg = $"DoProcess({strInfo}): SignUpException se,Fail-{se.Message}";
                logger.Info(s_errMsg);
                traceLogger.Debug(s_errMsg, se);
            }
            catch (Exception e)
            {
                HandleProcessEnd(ctl, sessionId, signUpInfo, 9, $"系統繁忙或異常: {e.Message}", false, null, strInfo);
                string s_errMsg = $"DoProcess({strInfo}): 系統繁忙或異常,Internal Error-{e.Message}";
                logger.Error(s_errMsg, e);
                traceLogger.Debug(s_errMsg, e);
            }
        }

        /// <summary>
        /// 統一處理結束狀態與資源釋放
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="sessionId"></param>
        /// <param name="info"></param>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        /// <param name="isTimeout"></param>
        /// <param name="signNo"></param>
        /// <param name="strInfo"></param>
        private void HandleProcessEnd(ClassSignUpCtl ctl, string sessionId, ClassSignUpStatus info, int status, string msg, bool isTimeout, long? signNo, string strInfo)
        {
            info.FinishTime = DateTime.Now;
            info.PreStatus = info.Status;
            info.Status = status; //-1:初始狀態,0:排隊中,1:處理中,2:報名成功,3:報名失敗(逾時),9:系統繁忙或異常(逾時)
            info.ErrMsg = msg;
            info.Timeout = isTimeout;
            if (signNo.HasValue) info.SignNo = signNo.Value;
            if (info.StartTime.HasValue) info.ProcessTimes = (long)(info.FinishTime.Value - info.StartTime.Value).TotalMilliseconds;

            ClassSignUpStatus.SetSignUpInfo(ctl.Host, sessionId, info);

            try
            {
                // 確保計數器一定會釋放,,報名處理結束, 清除佔用的處理計數 WORKER_ITEM_COUNTER
                ctl.DecWorkerCounter(ctl.HasAllocWorkerCounter, isTimeout);
            }
            catch (Exception e)
            {
                logger.Error($"HandleProcessEnd({strInfo}): 清除佔用的處理計數 WORKER_ITEM_COUNTER: ex:{e.Message}", e);
            }
            logger.Info($"DoProcess({strInfo}): FINISHED. [{(status == 2 ? "Success" : (status == 9 ? "Error" : "Fail"))}] [WaitTimes={info.WaitTimes}, ProcessTimes={info.ProcessTimes}]");
        }

        #region 產投課程報名作業邏輯

        /// <summary>實際產產投報名處理邏輯</summary>
        /// <param name="model"></param>
        private void _DoProcess(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            ClassSignUpService serv = new ClassSignUpService();

            string errMsg = "";
            string straIDNO = model.Detail.IDNO;

            // 檢核重疊時段的課程比對機制
            if ("Y".Equals(ConfigModel.DoubleStopEnter2))
            {
                errMsg = this.CheckDoubleClsX1(model.ClassInfo, straIDNO);
                //檢核失敗, 直接丟出 SignUpException
                if (!string.IsNullOrEmpty(errMsg))
                {
                    throw new SignUpException(errMsg);
                }
            }

            //取得資料庫系統時間
            model.SignUpTime = new MyKeyMapDAO().GetSysDateNow();

            //2019-02-28 修正性別再多判斷一次
            model.Detail.SEX = model.Detail.GET_SEX_MF;

            string str_em1 = "報名失敗，該學員報名資料異常 或 資料庫異常，請重試!!<br>";
            string str_em2 = "請再試一次，造成您不便之處，還請見諒。<br>";
            string str_em3 = "(若持續出現此問題，請聯絡系統管理者)!!!";

            dao.BeginTransaction();
            try
            {
                // 儲存報名基本資料（STUD_ENTERTEMP3）
                this.SaveEnterTemp3(model, dao);

                // step.1 儲存e網報名暫存資料（STUD_ENTERTEMP2）
                this.StoreEnterTemp2Model(model, dao);

                if (!model.Detail.ESETID.HasValue || model.Detail.ESETID == 0)
                {
                    // 這個判斷式是舊產投網站程式邏輯, 不太合理, 暫時保留, 後續再檢討
                    errMsg = $"{str_em1}{str_em2}{str_em3}";
                    logger.Warn("_DoProcess: StoreEnterTemp2Model() 後 STUD_ENTERTEMP2.ESETID 沒有值!");
                    throw new SignUpException(errMsg);
                }

                // step.2 儲存班級報名資料（STUD_ENTERTYPE2, STUD_ENTERTRAIN2）// insert or update STUD_ENTERTYPE2
                this.SaveEnterType2(dao, model);

                // 一旦儲存成功, 一定會有 SIGNNO, 要不然就會有 Exception // 不用再考慮取號成功與否的情況
                // 報名相關資料儲存完畢, 沒有異常, 資料 commit
                dao.CommitTransaction();

                #region 跟報名效力無關的資料異動, 不要放在同一個 DAO Transaction 中

                try
                {
                    // step.3.更新學員資料
                    // update STUD_STUDENTINFO.ISAGREE「是否同意」
                    switch (model.Detail.ISAGREE)
                    {
                        case "Y":
                        case "N":
                            break;
                        default:
                            //預設不同意
                            model.Detail.ISAGREE = "N";
                            break;
                    }
                    dao.UpdateStudIsAgree(model.Detail);

                    // step.4 異動維護 STUD_ENTERTEMP3
                    if (model.Detail.ESETID3.HasValue)
                    {
                        if (!model.Detail.NAME.Equals(model.Detail.NAME3))
                        {
                            // 姓名有異動時要備份 stud_entertemp3
                            dao.BackUpEnterTemp3(model.Detail.IDNO, model.Detail.ESETID3.Value);
                        }

                        // 儲存 stud_entertemp3 異動結果
                        dao.UpdateEnterTemp3(model.Detail);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"_DoProcess: UpdateStudIsAgree() or UpdateEnterTemp3() Exception (不影響報名), ex:{e.Message}", e);
                    // 這些作業不影響報名效力, 不往外丟 Exception
                }
                #endregion

            }
            catch (SignUpException se)
            {
                // 有異常, 資料倒回
                dao.RollBackTransaction();
                logger.Error($" catch (SignUpException se)-dao.RollBackTransaction(): ex:{se.Message}", se);
                throw se;
            }
            catch (Exception ex)
            {
                // 有異常, 資料倒回
                dao.RollBackTransaction();
                logger.Error($" catch (Exception ex)-dao.RollBackTransaction(): ex:{ex.Message}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 儲存會員報名資料 StudEnterTemp3,有異常時直接丟出 Exception
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dao"></param>
        public void SaveEnterTemp3(SignUpViewModel model, WDAIIPWEBDAO dao)
        {
            try
            {
                // 查詢產投報名暫存資料(stud_entertemp3)
                IList<TblSTUD_ENTERTEMP3> enterTemp3List = dao.GeteSETID3(model.Detail.IDNO);

                // 報名資料維護
                model.Detail.DB_ACTION = (enterTemp3List != null && enterTemp3List.Count > 0) ? "UPDATE" : "CREATE";

                dao.SaveCaseData("StudEnterTemp3", model.Detail, false);
            }
            catch (Exception ex)
            {
                Int64 iESETID3 = (model.Detail != null && model.Detail.ESETID3.HasValue) ? model.Detail.ESETID3.Value : -1;
                string msg = $"#SaveEnterTemp3: 發生錯誤,iESETID3={iESETID3}, ex:{ex.Message}";
                logger.Error(msg, ex);
                throw new Exception(msg, ex);
            }
        }

        /// <summary>
        /// 儲存e網報名暫存資料檔(STUD_ENTERTEMP2), 同時會回寫 model.Detail.ESETID
        /// </summary>
        /// <param name="model"></param>
        private void StoreEnterTemp2Model(SignUpViewModel model, WDAIIPWEBDAO dao)
        {
            string straIDNO = model.Detail.IDNO;

            //DateTime today = new MyKeyMapDAO().GetSysDateNow();
            TblSTUD_ENTERTEMP2 temp2 = dao.GeteSETID(straIDNO);

            try
            {
                if (temp2 == null || !temp2.ESETID.HasValue || temp2.ESETID == 0)
                {
                    // 資料不存在, 新增
                    model.Detail.DB_ACTION = "CREATE";

                    // 沒資料時取得最新的eSETID（stud_entertemp2.esetid ,同 E_ENTERTEMP2_ESETID_SEQ.NEXTVAL）
                    model.Detail.ESETID = ClassSignUpService.GetESETIDMaxID(straIDNO, dao);

                    dao.InsertEnterTemp2(model);
                }
                else
                {
                    // 資料存在, 更新
                    model.Detail.DB_ACTION = "UPDATE";

                    model.Detail.ESETID = temp2.ESETID.Value;

                    //有不一樣的資料採備份機制
                    bool flag_BackUp1 = (Convert.ToString(temp2.NAME) != Convert.ToString(model.Detail.NAME)
                        || DateTime.Compare(temp2.BIRTHDAY.Value, model.Detail.BIRTHDAY.Value) != 0);
                    bool flag_BackUp2 = (Convert.ToString(temp2.PHONE1) != Convert.ToString(model.Detail.PHONED)
                        || Convert.ToString(temp2.PHONE2) != Convert.ToString(model.Detail.PHONEN)
                        || Convert.ToString(temp2.CELLPHONE) != Convert.ToString(model.Detail.CELLPHONE));

                    //啟用備份機制條件==>不是同名，也不同生日
                    if (flag_BackUp1 || flag_BackUp2)
                    {
                        // 啟用備份(copy stud_entertemp2 to stud_entertemp2deldata)
                        dao.BackUpEnterTemp2(temp2);
                    }

                    // 異動個人資料(stud_entertemp2)
                    dao.UpdateEnterTemp2(model, temp2);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"StoreEnterTemp2Model: ex:{ex.Message}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// 儲存班級報名資料（STUD_ENTERTYPE2, STUD_ENTERTRAIN2）
        /// <para>會先以 e網報名暫存資料KEY(STUD_ENTERTEMP2.ESETID 存在model.Detail.ESETID)以及 OCID,
        /// 檢核班級報名資料是否已存在, 若存在(重複寫入)則丟出異常.</para>
        /// <para>若作業結果成功, 會將產投報名結果序號回寫 model.SIGNNO</para>
        /// <para>若作業結果失敗則會將原因以 SignUpException 丟出</para>
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="model"></param>
        private void SaveEnterType2(WDAIIPWEBDAO dao, SignUpViewModel model)
        {
            string errMsg = "";
            try
            {
                model.SIGNNO = null;

                // 線上報名資料寫入 STUD_ENTERTYPE2 中

                // 查詢 stud_entertype2 資料是否已存在
                string eTypeInfo = $"ESETID={model.Detail.ESETID.Value}, OCID={model.Form.OCID.Value}";
                TblSTUD_ENTERTYPE2 type2Info = dao.GetStudEnterType2ByESETID(model.Detail.ESETID.Value, model.Form.OCID.Value);

                if (type2Info == null)
                {
                    // 新增作業
                    traceLogger.Debug($"TblSTUD_ENTERTYPE2({eTypeInfo}) NOT EXISTS,CREATE");

                    // 新增e網報名資料 (STUD_ENTERTYPE2)
                    model.Detail.DB_ACTION = "CREATE";
                    this.StoreEnterType2Model(dao, model);

                    // 新增線上報名資料 - STUD_ENTERTRAIN2
                    this.StoreEnterTrain2Model(dao, model);
                }
                else
                {
                    // 修改作業
                    traceLogger.Debug($"TblSTUD_ENTERTYPE2({eTypeInfo}) EXISTS, ESERNUM={type2Info.ESERNUM}, OCID={type2Info.OCID1}, SIGNNO={type2Info.SIGNNO}");

                    model.Detail.ESERNUM = type2Info.ESERNUM.Value;
                    model.SIGNNO = type2Info.SIGNNO;

                    errMsg = $"報名失敗，您已經有報名此班級了!!! (班級代碼: {type2Info.OCID1}, 報名序號: {type2Info.SIGNNO})";
                }
            }
            catch (Exception ex)
            {
                string sESERNUM = (model.Detail != null && model.Detail.ESERNUM.HasValue) ? model.Detail.ESERNUM.Value.ToString() : "null";
                string msg = $"SaveEnterType2: 發生錯誤, iESERNUM={sESERNUM}, ex:{ex.Message}";
                logger.Error(msg, ex);
                traceLogger.Debug(msg, ex);
                throw new Exception(msg, ex);
            }

            if (!string.IsNullOrEmpty(errMsg)) { throw new SignUpException(errMsg); }
        }

        /// <summary>
        /// 根據 model.Detail.DB_ACTION(CREATE 新增, UPDATE 異動),
        /// 更新 STUD_ENTERTYPE2 資料操作但不會 commit, 要由呼叫者控管 Transaction
        /// <para>會同時產生報名結果序號 SIGNNO 並回寫 model.SIGNNO</para>
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="model"></param>
        private void StoreEnterType2Model(WDAIIPWEBDAO dao, SignUpViewModel model)
        {
            TblSTUD_ENTERTYPE2 newType2 = new TblSTUD_ENTERTYPE2();

            // TODO: 確認 STUD_ENTERTYPE2.SERNUM 的功能目的, 是否有必要存在
            model.Detail.SERNUM = dao.GetIntSerNum(model.Detail.ESETID.Value);

            // 取得報名資料檔的流水號
            //model.Detail.ESERNUM = (new WDAIIPWEBDAO()).GetNewId("STUD_ENTERTYPE2_ESERNUM_SEQ,STUD_ENTERTYPE2,ESERNUM").Value;
            model.Detail.ESERNUM = dao.GetNewId("STUD_ENTERTYPE2_ESERNUM_SEQ,STUD_ENTERTYPE2,ESERNUM").Value;

            // 取得產投課程報名結果序號 SIGNNO
            model.SIGNNO = dao.GetSignNoxEnterType3(model.Form.OCID.Value);

            var classInfo = model.ClassInfo;
            var type2Info = model.Detail;

            newType2.ESERNUM = type2Info.ESERNUM; //stud_entertype2.esernum (pk)
            newType2.ESETID = type2Info.ESETID; //ref stud_entertemp2.esetid 
            newType2.SETID = type2Info.SETID; //ref stud_entertemp.setid
            newType2.SERNUM = type2Info.SERNUM; //stud_entertype2.sernum
            newType2.ENTERDATE = model.SignUpTime.Value.Date; //truncate time
            newType2.RELENTERDATE = model.SignUpTime;
            newType2.SIGNNO = model.SIGNNO;

            // 填入學員報名職類基本資料
            newType2.OCID1 = model.Form.OCID;
            newType2.TMID1 = Convert.ToInt64(classInfo.TMID);
            newType2.RID = classInfo.RID;
            newType2.PLANID = Convert.ToInt64(classInfo.PLANID);
            newType2.IDENTITYID = type2Info.MIDENTITYID;
            newType2.ENTERPATH = "O"; //產投外網 by AMU 201212
            newType2.MODIFYACCT = type2Info.IDNO;
            newType2.MODIFYDATE = model.SignUpTime;

            newType2.SIGNUPSTATUS = 0;
            newType2.ISOUT = 0;

            switch (model.Detail.DB_ACTION)
            {
                case "CREATE":
                    //新增 stud_entertype2
                    dao.InsertStudEnterType2(newType2);
                    break;
                case "UPDATE":
                    //修改 stud_entertype2
                    dao.UpdateStudEnterType2(newType2);
                    break;
            }
        }

        /// <summary>
        /// 儲存 STUD_ENTERTRAIN2 資料操作
        /// <para>會以 data.ESERNUM 判斷資料是否存在, 只有當資料不存在時才會新增, 若資料已存在, 則不會異動</para>
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="model"></param>
        private void StoreEnterTrain2Model(WDAIIPWEBDAO dao, SignUpViewModel model)
        {
            var data = model.Detail;

            // 無報名資料序號不寫入
            if (!data.ESERNUM.HasValue || data.ESERNUM <= 0)
            {
                throw new Exception("StoreEnterTrain2Model: 報名資料序號(ESERNUM) <= 0");
            }

            // 若查到已有資料了就不再寫入
            TblSTUD_ENTERTRAIN2 train2Info = dao.GetStudEnterTrain2ByESERNUM(data.ESERNUM.Value);
            if (train2Info != null)
            {
                logger.Info($"StoreEnterTrain2Model: 資料已存在, ESERNUM={data.ESERNUM.Value}");
                return;
            }

            // 新增 stud_entertrain2
            dao.InsertStudEnterTrain2(model);
        }

        #endregion

        #region 產投報名, 檢核課程時段重疊之情形
        /// <summary>
        /// 產投報名, 檢核課程時段是否重疊，有則顯示相關提示訊息
        /// </summary>
        /// <param name="classInfo"></param>
        /// <param name="idno"></param>
        /// <returns></returns>
        private string CheckDoubleClsX1(ClassClassInfoExtModel classInfo, string idno)
        {
            string rtn = string.Empty;
            string errMsg = "檢核課程時段重疊作業異常: ";
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            bool blFlag = false;

            if (classInfo == null || string.IsNullOrEmpty(idno))
            {
                errMsg += "參數 班級資訊(classInfo) 及 身分證號(IDNO) 不能為空";
                return errMsg;
            }

            // 1.查詢民眾已報名的班(尚未開訓)
            IList<ClassClassInfoExtModel> enterClassList = dao.QueryEnterClassByIDNO(idno, classInfo.OCID.Value);

            if (enterClassList != null)
            {
                foreach (var enterItem in enterClassList)
                {
                    // 檢核課程時段是否重疊（true:有重疊）
                    // 有任何一門課程重疊, 即停止檢核
                    blFlag = this.CheckClsDouble(classInfo.OCID.Value, enterItem.OCID.Value, dao);

                    if (blFlag)
                    {
                        rtn = GetDoubleMsg(classInfo.CLASSNAME2, enterItem.CLASSNAME2);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(rtn))
            {
                //2.查詢民眾已參訓課程(尚未結訓)
                IList<ClassClassInfoExtModel> trainClassList = dao.QueryTrainClassByIDNO(idno, classInfo.OCID.Value);

                if (trainClassList != null)
                {
                    foreach (var enterItem in trainClassList)
                    {
                        //檢核課程時段是否重疊（true:有重疊）
                        blFlag = this.CheckClsDouble(classInfo.OCID.Value, enterItem.OCID.Value, dao);

                        if (blFlag)
                        {
                            rtn = GetDoubleMsg(classInfo.OCID + classInfo.CLASSNAME2, enterItem.OCID + enterItem.CLASSNAME2);
                            break;
                        }
                    }
                }
            }
            return rtn;
        }

        /// <summary>
        /// 回傳課程時段重疊提示訊息
        /// </summary>
        /// <param name="className1"></param>
        /// <param name="className2"></param>
        /// <returns></returns>
        private string GetDoubleMsg(string className1, string className2)
        {
            string content = "您本次報名的課程: {0}，與已報名的課程: {1}，有時段重疊之情形，請擇一參訓。";
            return string.Format(content, className1, className2);
        }

        /// <summary>
        /// 檢核報名課程時段有無重疊
        /// (true:有課程時段重疊之情形 false:沒有)
        /// </summary>
        /// <param name="ocid1"></param>
        /// <param name="ocid2"></param>
        /// <returns></returns>
        private bool CheckClsDouble(decimal ocid1, decimal ocid2, WDAIIPWEBDAO dao)
        {
            bool rtn = false;

            IList<Hashtable> list = null;

            list = dao.QueryClassDescDouble(ocid1, ocid2); //比對1
            if (list != null && list.Count > 0)
            {
                rtn = true;
            }
            else
            {
                list.Clear();
                list = dao.QueryClassDescDouble(ocid2, ocid1); //比對2 交換比對
                if (list != null && list.Count > 0)
                {
                    rtn = true;
                }
            }

            return rtn;
        }
        #endregion

        #region 排隊控制相關 Methods

        //private static string AppDataPath = "";

        //報名作業的排隊號碼計數器日期
        //private static string WORKER_ITEM_DATE_file = "WORKER_ITEM_DATE.txt";

        //報名作業的排隊號碼計數器 (由1開始)
        //private static string WORKER_ITEM_SEQ_file = "WORKER_ITEM_SEQ.txt";
        //private static int WORKER_ITEM_SEQ = 0;

        //報名作業當前輪到的號碼 (由1開始)
        //private static string WORKER_ITEM_CURRENT_SEQ_file = "WORKER_ITEM_CURRENT_SEQ.txt";
        //private static int WORKER_ITEM_CURRENT_SEQ = 1;

        #endregion

    }

    /// <summary>
    /// 代表報名(邏輯檢核等)失敗的 Exception
    /// </summary>
    class SignUpException : Exception
    {
        /// <summary>
        /// 以報名失敗的錯誤訊息建構 SignUpException
        /// </summary>
        /// <param name="msg"></param>
        public SignUpException(string msg) : base(msg)
        {
        }
    }


    /// <summary>
    /// 產投課程報名處理狀態
    /// <para>由背景處理程序將狀態寫回供查詢之用</para>
    /// </summary>
    public class ClassSignUpStatus : TblSYS_SIGNUP_STATUS
    {
        #region 報名處理狀態的全域靜態暫存存取相關

        /* 20190125, Eric, 
         * 修改 ClassSignUpStatus 存放的方式, 改用 DB 去存放, 
         * 以解決負載平衡環境下無法正確取得狀態之問題
        /// <summary>
        /// 用來存放報名處理狀態的全域靜態變數,
        /// 字典索引值必須為 SessionModel 的 SessionID
        /// </summary>
        private static Dictionary<string, ClassSignUpStatus> signUpInfos = new Dictionary<string, ClassSignUpStatus>();
        */

        /// <summary>取得指定 SeesionID 的 ClassSignUpStatus,若不存在則回傳一個空的 ClassSignUpStatus.</summary>
        /// <param name="host"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static ClassSignUpStatus GetSignUpInfo(string host, string sessionId)
        {
            // 2019.01.29, Eric,修訂 SignUpStatus 的索引 Key 為單一 SessionID (不要加上 host 資訊),以確保在 Random 負載平衡環境下能正確運作

            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException("sessionId 不能為 null");

            //20190125, Eric, 改為 DB 儲存 ClassSignUpStatus,以 Host + SessionID 為 Key 值,這種儲存方式在取存時,已不需要 lock 操作
            //object obj = new object();ClassSignUpStatus where = new ClassSignUpStatus() { SessionID = sessionId };

            BaseDAO dao = new BaseDAO();

            ClassSignUpStatus signUpInfo = dao.GetRow<ClassSignUpStatus>(new ClassSignUpStatus() { SessionID = sessionId });
            //測試報名逾時 
            if (signUpInfo != null && ConfigModel.IsTestSignUpTimeout) { signUpInfo.Status = 1; }

            // ClassSignUpStatus 資料不存在, 回傳一個空的
            return signUpInfo ?? new ClassSignUpStatus() { SessionID = sessionId };
        }

        /// <summary>
        /// 設定/更新 指定 SeesionID 的 ClassSignUpStatus
        /// <para>若 signUpInfo 為 null, 則會由全域暫存中清除</para>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="signUpInfo"></param>
        public static void SetSignUpInfo(string host, string sessionId, ClassSignUpStatus signUpInfo)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId 不能為 null");
            }

            //20190125, Eric, 改為 DB 儲存 ClassSignUpStatus,以 Host + SessionID 為 Key 值,這種儲存方式在取存時,已不需要 lock 操作
            //20190129, Eric, 改為只用 SessionID 為 Key 值, 存取 ClassSignUpStatus, 但存取 ClassSignUpStatistic 仍需要 Host 值

            ClassSignUpStatus where = new ClassSignUpStatus() { SessionID = sessionId };

            BaseDAO dao = new BaseDAO();

            //signUpInfo == null 移除 sessionId 對應的 ClassSignUpStatus
            if (signUpInfo == null)
            {
                // 移除 sessionId 對應的 ClassSignUpStatus
                dao.Delete<ClassSignUpStatus>(where);
            }
            else
            {
                //signUpInfo != null 持續更新或新增目前狀態
                ClassSignUpStatus signupStatus = dao.GetRow<ClassSignUpStatus>(where);
                if (signupStatus != null)
                {
                    // Update 更新狀態
                    dao.Update<ClassSignUpStatus>(signUpInfo, where);
                }
                else
                {
                    // ClassSignUpStatus 資料不存在, 新增
                    signUpInfo.Host = host;
                    signUpInfo.SessionID = where.SessionID;
                    dao.Insert<ClassSignUpStatus>(signUpInfo);
                }

                // 更新 SignUp 等待及處理 平均時間
                ClassSignUpStatistic.CalcAvgTime(host, signUpInfo);
                // 更新 SignUp 當前處理數量 //ClassSignUpStatistic.RefreshProcessCount(host);
                // 更新 SignUp 處理人數統計
                /*case 0:,ClassSignUpStatistic.Inc(host, ClassSignUpStatisticField.Wait);,break;,case 1:,ClassSignUpStatistic.Inc(ClassSignUpStatisticField.Process);,if (signUpInfo.PreStatus == 0) ClassSignUpStatistic.Dec(host, ClassSignUpStatisticField.Wait);,break;*/
                switch (signUpInfo.Status)
                {
                    case 2:
                        ClassSignUpStatistic.Inc(host, ClassSignUpStatisticField.Success);
                        //ClassSignUpStatistic.Dec(ClassSignUpStatisticField.Process);
                        break;
                    case 3:
                    case 9:
                        if (signUpInfo.Timeout.HasValue && signUpInfo.Timeout.Value)
                        {
                            ClassSignUpStatistic.Inc(host, ClassSignUpStatisticField.Timeout);
                        }
                        else if (signUpInfo.Status == 9)
                        {
                            ClassSignUpStatistic.Inc(host, ClassSignUpStatisticField.Error);
                        }
                        else
                        {
                            ClassSignUpStatistic.Inc(host, ClassSignUpStatisticField.Fail);
                        }
                        //if (signUpInfo.PreStatus == 0) { ClassSignUpStatistic.Dec(host, ClassSignUpStatisticField.Wait); }
                        //else,{,ClassSignUpStatistic.Dec(host, ClassSignUpStatisticField.Process);,},
                        break;
                }
            }

        }
        #endregion

        /// <summary>
        /// Serialize this JsonResultStruct to JSON string
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            // 建立客制化的 DateTime 格式 (取到千分之1秒)
            MyJsonDateTimeConverter dateTimeConvert = new MyJsonDateTimeConverter("yyyy'/'MM'/'dd' 'HH':'mm':'ss'.'fff");

            // Serialize 本身並指定套用在 DateTime 物件的格式
            string jsonStr = JsonConvert.SerializeObject(this, dateTimeConvert);

            return jsonStr;
        }
    }

    /// <summary> 產投課程報名處理狀態統計欄位 </summary>
    public enum ClassSignUpStatisticField
    {
        /*<summary>處理中人數</summary>Process,<summary> 等待人數</summary>Wait*/
        /// <summary>等待逾時人數</summary>
        Timeout,
        /// <summary>報名失敗人數</summary>
        Fail,
        /// <summary>報名收件成功人數</summary>
        Success,
        /// <summary>報名系統異常人數</summary>
        Error
    }

    /// <summary> 產投課程報名處理狀態統計 </summary>
    public class ClassSignUpStatistic : TblSYS_SIGNUP_STATISTICS
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /* 20190125, Eric, 將統計的計數器改為DB儲存 */

        /// <summary>60分鐘統計循環計數器</summary>
        private static ClassSignUpStatistic[] statistics = null;

        private static readonly int MAX_STATISTICS = 60;

        private static readonly object lockObj = new object();

        private static readonly object get_mutex = new object();

        private static readonly object ins_mutex = new object();

        #region 統計計數器操作 methods

        /// <summary>取得 60分鐘統計循環計數器(每1分鐘為1級距), <seealso cref="ClassSignUpStatistic"/></summary>
        /// <returns></returns>
        public static ClassSignUpStatistic[] GetStatistics()
        {
            string host = System.Web.HttpContext.Current.Server.MachineName;

            if (statistics != null)
            {
                // 暫存統計資料存在, 直接取用, 以避免重覆抓取相同資料, 影響效能
            }
            else
            {
                // 重新由資料庫中抓取
                ClassSignUpStatistic where = new ClassSignUpStatistic() { Host = host };

                IList<ClassSignUpStatistic> list = (new BaseDAO()).GetRowList<ClassSignUpStatistic>(where);
                statistics = new ClassSignUpStatistic[MAX_STATISTICS];
                foreach (var stat in list)
                {
                    if (stat.Idx.HasValue && stat.Idx.Value >= 0 && stat.Idx.Value < MAX_STATISTICS)
                    {
                        statistics[stat.Idx.Value] = stat;
                    }
                }
                for (int i = 0; i < statistics.Length; i++)
                {
                    if (statistics[i] == null)
                    {
                        statistics[i] = new ClassSignUpStatistic();
                    }
                }
            }

            // 更新當前處理程序個數
            for (int i = 0; i < statistics.Length; i++)
            {
                statistics[i].IsCurrent = false;
            }
            int idx = DateTime.Now.Minute;
            statistics[idx].IsCurrent = true;
            statistics[idx].Process = (new ClassSignUpCtl(host)).GetSignUpCtl().PCOUNT;

            // 去除不正常(不連續)的統計級距
            bool cleanBegin = false;
            for (int i = idx - 1; i >= 0; i--)
            {
                if (statistics[i] == null || !statistics[i].Idx.HasValue)
                {
                    cleanBegin = true;
                }
                if (cleanBegin)
                {
                    statistics[i] = new ClassSignUpStatistic();
                }
            }
            for (int i = statistics.Length - 1; i > idx; i--)
            {
                if (!statistics[i].Idx.HasValue)
                {
                    cleanBegin = true;
                }
                if (cleanBegin)
                {
                    statistics[i] = new ClassSignUpStatistic();
                }
            }

            return statistics;
        }

        /*更新當前處理程序個數public static void RefreshProcessCount(string host){lock (lockObj)
         * {ClassSignUpStatistic stat = GetCurrentStat(host);stat.Process = ProcessEnterWorker.GetCurrentWorkerCount();}} */

        /// <summary>將當前統計(每1分鐘為1級距)中指定欄位計數加1 
        /// <para>最大值=Int.MaxValue</para>
        /// </summary>
        /// <param name="field"><see cref="ClassSignUpStatisticField"/></param>
        public static void Inc(string host, ClassSignUpStatisticField field)
        {
            ClassSignUpStatistic stat = GetCurrentStat(host);
            ClassSignUpStatistic statNew = new ClassSignUpStatistic();
            /*case ClassSignUpStatisticField.Process:statNew.Process=_Inc(stat.Process);break;case ClassSignUpStatisticField.Wait:,statNew.Wait = _Inc(stat.Wait);,break;*/
            switch (field)
            {
                case ClassSignUpStatisticField.Timeout:
                    statNew.Timeouts = _Inc(stat.Timeouts);
                    break;
                case ClassSignUpStatisticField.Error:
                    statNew.Error = _Inc(stat.Error);
                    break;
                case ClassSignUpStatisticField.Fail:
                    statNew.Fail = _Inc(stat.Fail);
                    break;
                case ClassSignUpStatisticField.Success:
                    statNew.Success = _Inc(stat.Success);
                    break;
            }

            // 清除暫存統計資料
            statistics = null;

            BaseDAO dao = new BaseDAO();
            ClassSignUpStatistic where = new ClassSignUpStatistic() { Host = stat.Host, Idx = stat.Idx };
            lock (lockObj)
            {
                dao.Update<ClassSignUpStatistic>(statNew, where);
            }
        }

        /// <summary>
        /// 將當前統計(每1分鐘為1級距)中指定欄位計數 減1 
        /// <para>最小值=0</para>
        /// </summary>
        /// <param name="field"><see cref="ClassSignUpStatisticField"/></param>
        public static void Dec(string host, ClassSignUpStatisticField field)
        {
            ClassSignUpStatistic stat = GetCurrentStat(host);
            ClassSignUpStatistic statNew = new ClassSignUpStatistic();
            /*case ClassSignUpStatisticField.Process:statNew.Process = _Dec(stat.Process);break;case ClassSignUpStatisticField.Wait:,statNew.Wait = _Dec(stat.Wait);,break;*/
            switch (field)
            {
                case ClassSignUpStatisticField.Timeout:
                    statNew.Timeouts = _Dec(stat.Timeouts);
                    break;
                case ClassSignUpStatisticField.Error:
                    statNew.Error = _Dec(stat.Error);
                    break;
                case ClassSignUpStatisticField.Fail:
                    statNew.Fail = _Dec(stat.Fail);
                    break;
                case ClassSignUpStatisticField.Success:
                    statNew.Success = _Dec(stat.Success);
                    break;
            }

            // 清除暫存統計資料
            statistics = null;

            BaseDAO dao = new BaseDAO();
            ClassSignUpStatistic where = new ClassSignUpStatistic() { Host = stat.Host, Idx = stat.Idx };
            lock (lockObj)
            {
                dao.Update<ClassSignUpStatistic>(statNew, where);
            }
        }

        /// <summary>
        /// 更新計算當前統計(每1分鐘為1級距)中平均等待時間及平均處理時間 
        /// <para>必須在呼叫 Inc 及 Dec 之前執行, 才會正確</para>
        /// </summary>
        /// <param name="status"></param>
        public static void CalcAvgTime(string host, ClassSignUpStatus status)
        {
            if (status.Status != 2 && status.Status != 3 && status.Status != 9) return;

            // 只計算已經結束作業的平均處理及等待時間
            ClassSignUpStatistic stat = GetCurrentStat(host);

            int Success = stat.Success ?? 0;
            int Fail = stat.Fail ?? 0;
            int Timeout = stat.Timeouts ?? 0;
            int Error = stat.Error ?? 0;
            long AvgWaitTime = stat.AvgWaitTime ?? 0;
            long AvgProcessTime = stat.AvgProcessTime ?? 0;

            // 更新平均等待時間 // 總人數 = 成功人數 + 失敗人數 + Timeout人數 + Error 人數
            int pCount = Success + Fail + Timeout + Error;
            if (!status.WaitTimes.HasValue) status.WaitTimes = 0;
            long avgWait = (AvgWaitTime * pCount + status.WaitTimes.Value) / (pCount + 1);
            stat.AvgWaitTime = avgWait;

            if (!status.Timeout.HasValue || !status.Timeout.Value)
            {
                // 更新平均作業時間 , 總人數 = 成功人數 + 失敗人數
                pCount = Success + Fail;
                if (!status.ProcessTimes.HasValue) status.ProcessTimes = 0;
                long avgProcess = (AvgProcessTime * pCount + status.ProcessTimes.Value) / (pCount + 1);
                stat.AvgProcessTime = avgProcess;
            }

            // 寫入DB
            BaseDAO dao = new BaseDAO();
            ClassSignUpStatistic where = new ClassSignUpStatistic() { Host = stat.Host, Idx = stat.Idx };
            ClassSignUpStatistic statNew = new ClassSignUpStatistic() { AvgWaitTime = stat.AvgWaitTime, AvgProcessTime = stat.AvgProcessTime };
            lock (lockObj)
            {
                dao.Update<ClassSignUpStatistic>(statNew, where);
            }
        }

        private static int _Inc(int? value)
        {
            int val = value ?? 0;
            return (val >= int.MaxValue) ? val : (val + 1);
        }

        private static int _Dec(int? value)
        {
            int val = value ?? 0;
            return (val <= 0) ? 0 : (val - 1);
        }

        private static ClassSignUpStatistic GetCurrentStat(string host)
        {
            ClassSignUpStatistic stat = null;
            lock (get_mutex) { stat = Get_stat(host); }
            return stat;
        }

        /// <summary>清空(刪除)前一日統計資料</summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private static ClassSignUpStatistic Get_stat(string host)
        {
            BaseDAO dao = new BaseDAO();
            DateTime dtNow = DateTime.Now;

            int idx = dtNow.Minute;
            ClassSignUpStatistic where = new ClassSignUpStatistic() { Host = host, Idx = idx };
            ClassSignUpStatistic stat = dao.GetRow<ClassSignUpStatistic>(where);

            // 判斷是否跨天, 跨天歸零重新計算
            string date = HelperUtil.DateTimeToString(dtNow, "");
            bool reset = false;
            if (stat != null && !date.Equals(stat.StatDate))
            {
                // 刪除暫存資料,清空(刪除)前一日統計資料
                statistics = null;
                ClassSignUpStatistic whereD = new ClassSignUpStatistic() { Host = host };
                dao.Delete<ClassSignUpStatistic>(whereD);
                stat = null;
                reset = true;
            }

            //Current級距Stat資料存在,直接回傳
            if (stat != null) { return stat; }

            // 複制前一個統計級距, 並清空下一個統計級距
            int idx0 = idx - 1;
            int idx1 = idx + 1;
            idx0 = (idx0 < 0) ? MAX_STATISTICS - 1 : idx0;
            idx1 = (idx1 >= MAX_STATISTICS) ? 0 : idx1;
            ClassSignUpStatistic where0 = new ClassSignUpStatistic() { Host = host, Idx = idx0 };

            // 若為跨日歸零時,就不用抓前一個級距來複制
            if (!reset) { stat = dao.GetRow<ClassSignUpStatistic>(where0); }

            if (stat != null)
            {
                // 前一個級距stat資料存在, 移動 Current 指標
                stat.IsCurrent = false;
                dao.Update<ClassSignUpStatistic>(stat, where0);
            }
            else
            {
                // 前一個級距資料不存在, 新建一個
                stat = new ClassSignUpStatistic()
                {
                    StatDate = date,
                    Host = where.Host,
                    Idx = where.Idx,
                    Wait = 0,
                    //Process = 0,
                    Success = 0,
                    Fail = 0,
                    Error = 0,
                    Timeouts = 0,
                    AvgProcessTime = 0,
                    AvgWaitTime = 0,
                    IsCurrent = true
                };
            }

            // Current 級距新增寫入資料庫 (新增前試著抓取)
            stat.IsCurrent = true;
            stat.Idx = where.Idx;
            ClassSignUpStatistic where3 = new ClassSignUpStatistic() { Host = stat.Host, Idx = stat.Idx };
            ClassSignUpStatistic stat3 = dao.GetRow<ClassSignUpStatistic>(where3);
            //新增失敗 查詢後返回
            if (stat3 == null && !Insert_stat(dao, stat))
            {
                stat3 = dao.GetRow<ClassSignUpStatistic>(where3);
                return stat3;
            }
            if (stat3 != null) { return stat3; }

            // 清空(刪除)下一個統計級距(用來作為統計循環辨識用)
            ClassSignUpStatistic where1 = new ClassSignUpStatistic() { Host = host, Idx = idx1 };
            dao.Delete<ClassSignUpStatistic>(where1);

            return stat;
        }

        private static bool Insert_stat(BaseDAO dao, ClassSignUpStatistic stat)
        {
            try
            {
                lock (ins_mutex)
                {
                    dao.Insert<ClassSignUpStatistic>(stat);
                }
            }
            catch (Exception)
            {
                //logger.Warn($"#Insert_stat ex:{ex.Message}", ex);
                return false; //throw;
            }
            return true;
        }

        /// <summary>
        /// 更新統計監控圖資料(尤其是每分鐘統計級距切換, 確保統曲線的連續性)
        /// </summary>
        /// <param name="host"></param>
        public static void RefreshStatistics(string host)
        {
            // 刪除暫存 statistics 以重新產生統計監控圖所需的暫存資料
            statistics = null;

            ClassSignUpStatistic stat = GetCurrentStat(host);
            //lock (lockObj),{,},
        }

        public static void ResetStatistics(string host)
        {
            // 刪除暫存資料
            statistics = null;

            // 清空(刪除)所有統計級距
            BaseDAO dao = new BaseDAO();
            ClassSignUpStatistic where1 = new ClassSignUpStatistic() { Host = host };
            lock (lockObj)
            {
                dao.Delete<ClassSignUpStatistic>(where1);
            }
        }

        public ClassSignUpStatistic Clone()
        {
            ClassSignUpStatistic obj = new ClassSignUpStatistic();
            obj.InjectFrom(this);
            return obj;
        }
        #endregion

    }

    public class ClassSignUpMonitor
    {
        private static ILog logger = LogManager.GetLogger(typeof(ClassSignUpMonitor));
        private static bool ThreadIsAlive = false;

        /// <summary>
        /// 如果 統計資訊 背景更新 Thread 尚未啟動則啟動它, 若已啟動則直接返回
        /// </summary>
        public void StartRefresher(string host)
        {
            if (ThreadIsAlive) { return; }

            //  啟動產投報名統計資訊背景更新程序
            logger.Info("StartRefresher: 啟動產投報名統計資訊背景更新程序");

            Action<CancellationToken> action = token => StatisticsRefresher(token, host);

            HostingEnvironment.QueueBackgroundWorkItem(action);

        }

        /// <summary>啟動 產投報名監控統計資訊 背景更新 Thread</summary>
        /// <param name="cancellationToken"></param>
        /// <param name="host"></param>
        private void StatisticsRefresher(CancellationToken cancellationToken, string host)
        {
            if (ThreadIsAlive) { return; }
            //logger.Info("StatisticsRefresher: Start");
            try
            {
                if (ThreadIsAlive) { return; }
                while (true)
                {
                    logger.Info("StatisticsRefresher: Execute");
                    ThreadIsAlive = true;

                    ClassSignUpStatistic.RefreshStatistics(host);

                    Thread.Sleep(60000);   // 每1分鐘跑一次
                }

            }
            catch (ThreadAbortException ex)
            {
                logger.Warn("StatisticsRefresher: ThreadAbortException", ex);
            }
            catch (Exception ex)
            {
                // 攔下 ThreadAbortException 以外的所有 Exception ,以確保 StatisticsRefresher 背景 thread 不會意外停掉   
                logger.Warn($"StatisticsRefresher: ex:{ex.Message}", ex);
            }
            ThreadIsAlive = false;
            //logger.Info("StatisticsRefresher: Thread End");
        }
    }

    /// <summary>
    /// 排隊控制器
    /// </summary>
    public class ClassSignUpCtl
    {
        //private static ILog logger = LogManager.GetLogger("ProcessEnterWorker");
        protected static readonly ILog logger = LogManager.GetLogger("ProcessEnterWorker");

        /// <summary>排隊號碼存取同步 lock</summary>
        private static readonly object WORKER_ITEM_SEQ_lock = new object();

        /// <summary>處理程序 lock</summary>
        private static readonly object WORKER_ITEM_COUNTER_lock = new object();

        private string _Host = null;

        /// <summary> 是否已有佔用 Worker Item Counter </summary>
        private bool _allocWorkerCounter = false;

        /// <summary>
        /// 排隊控制器 建構子
        /// </summary>
        /// <param name="Host">主機IP/名稱</param>
        public ClassSignUpCtl(string Host)
        {
            if (Host == null) throw new ArgumentNullException("Host");
            this._Host = Host;
        }

        /// <summary>
        /// 主機 IP/名稱
        /// </summary>
        public string Host => this._Host;

        /// <summary>
        /// 是否已有佔用 Worker Item Counter
        /// </summary>
        public bool HasAllocWorkerCounter => this._allocWorkerCounter;

        /// <summary> (取得或新增1筆)號碼牌,控制內容</summary>
        /// <returns></returns>
        public TblSYS_SIGNUP_CTL GetSignUpCtl()
        {
            BaseDAO dao = new BaseDAO();
            TblSYS_SIGNUP_CTL ctl = null;
            ctl = dao.GetRow<TblSYS_SIGNUP_CTL>(new TblSYS_SIGNUP_CTL() { HOST = this._Host });
            if (ctl != null) { return ctl; }

            // 不存在, 起始1筆資料 if (ctl == null) { }
            ctl = new TblSYS_SIGNUP_CTL() { HOST = this._Host, QDAY = DateTime.Now.Day, SEQ = 0, CURSEQ = 0, PCOUNT = 0, NCOUNT = 0 };
            dao.Insert<TblSYS_SIGNUP_CTL>(ctl);
            return ctl;
        }

        /// <summary> UP設定號碼牌控制狀態 </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        public int SetSignUpCtl(TblSYS_SIGNUP_CTL ctl)
        {
            int rst = -1;
            BaseDAO dao = new BaseDAO();
            TblSYS_SIGNUP_CTL where = new TblSYS_SIGNUP_CTL() { HOST = this._Host };
            rst = dao.Update<TblSYS_SIGNUP_CTL>(ctl, where);
            return rst;
        }

        /// <summary>重設號碼牌控制狀態</summary>
        /// <returns></returns>
        public int ResetSignUpCtl()
        {
            int rst = -1;
            TblSYS_SIGNUP_CTL ctl = new TblSYS_SIGNUP_CTL() { HOST = this._Host, QDAY = DateTime.Now.Day, SEQ = 0, CURSEQ = 0, PCOUNT = 0, NCOUNT = 0 };
            rst = SetSignUpCtl(ctl);
            return rst;
        }

        /// <summary>從狀態檔中取排隊號碼計數器值 (回傳結果為累加後的值, 由1開始)</summary>
        /// <returns></returns>
        private static int _GetWorkerSeq(TblSYS_SIGNUP_CTL ctl) => ctl.SEQ.HasValue ? ctl.SEQ.Value + 1 : 1;

        /// <summary>從狀態檔中取當前輪到的號碼 (回傳結果為累加後的值, 由1開始)</summary>
        /// <returns></returns>
        private int _GetWorkerCurrentSeq(TblSYS_SIGNUP_CTL ctl) => ctl.CURSEQ.HasValue ? ctl.CURSEQ.Value + 1 : 1;

        /// <summary> 抽取一個報名作業的排隊號碼 (回傳結果由1開始) </summary>
        /// <returns></returns>
        public int GetWorkerSeq(int MaxThreads)
        {
            //int workerSeq = 1;
            lock (WORKER_ITEM_SEQ_lock)
            {
                TblSYS_SIGNUP_CTL ctl = this.GetSignUpCtl();
                int workerSeq = _GetWorkerSeq(ctl);

                // 檢查當前報名 worker thread 數量, 若跨天 或 當前並無 worker thread ,表示重新開始另一輪的排隊, 先起始(重設/較準)排隊計數器 
                int systemDay = DateTime.Now.Day;
                int workerSeqDay = ctl.QDAY.Value;

                if (workerSeqDay != systemDay || ctl.PCOUNT > ctl.NCOUNT || ctl.CURSEQ > ctl.SEQ)
                {
                    //非同日或ctl.PCOUNT > ctl.NCOUNT(極端異常),ctl.CURSEQ > ctl.SEQ(極端異常),重設排隊計數器
                    logger.Info($"GetWorkerSeq: 重設排隊計數器. (workerSeq={ctl.SEQ}/1, CurrentSeq={ctl.CURSEQ}, workerSeqDay={workerSeqDay}).1");
                    this.ResetSignUpCtl();
                    workerSeq = 1;
                }

                // 回寫 WORKER_ITEM_SEQ_file
                TblSYS_SIGNUP_CTL upd = new TblSYS_SIGNUP_CTL() { SEQ = workerSeq };
                this.SetSignUpCtl(upd);

                // 等待 worker thread 計數加1 // 由 DoProcess() 中拉到這裡, 雖然跟實際 Worker Process 創建時間點分離(提早)
                // 但為了確保不會有 race condition 的問題, 還是拉到這裡, 讓單一 WORKER_ITEM_SEQ_lock scope 能作用 
                this.IncWorkerCounter(false);

                return workerSeq;
            }
        }

        /// <summary> 檢查 報名作業的排隊號碼 是否已可進行作業 </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public bool CheckWorkerSeq(int MySeq, int MaxThreads)
        {
            //bool canDo = false;
            lock (WORKER_ITEM_SEQ_lock)
            {
                // 檢查 WORKER_ITEM_COUNTER 是否小於 MAX_PROCESS
                TblSYS_SIGNUP_CTL ctl = this.GetSignUpCtl();

                // 當前報名處理 thread 個數小於上限, 可以處理報名,檢查是否已輪到 傳入的排隊號碼(WorkerSeq) 
                if (ctl.PCOUNT < MaxThreads && _GetWorkerCurrentSeq(ctl) == MySeq)
                {
                    // 已輪到指定的 排隊號碼,canDo = true;回寫 
                    TblSYS_SIGNUP_CTL upd = new TblSYS_SIGNUP_CTL() { CURSEQ = MySeq };
                    this.SetSignUpCtl(upd);

                    return true;
                }
            }
            return false;
        }

        /// <summary>報名處理開始，原子化增加佔用的 WORKER_ITEM_COUNTER</summary>
        public void IncWorkerCounter(bool incPCount)
        {
            // 這裡甚至可以不需要 C# 的 lock，因為資料庫層級已經保證原子性
            string targetCol = incPCount ? "PCOUNT" : "NCOUNT";

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            dao.UpdateCounterAtomic(this._Host, targetCol, true);

            this._allocWorkerCounter = true;
        }

        /// <summary>報名處理結束，原子化清除佔用的 WORKER_ITEM_COUNTER</summary>
        public void DecWorkerCounter(bool HasAllocWorkerCounter, bool isTimeout)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            // 同時更新 PCOUNT 與 NCOUNT 的原子操作建議分開或寫成複合 SQL
            if (HasAllocWorkerCounter)
            {
                dao.UpdateCounterAtomic(this._Host, "PCOUNT", false);
                dao.UpdateCounterAtomic(this._Host, "NCOUNT", false);
            }
            else if (isTimeout)
            {
                dao.UpdateCounterAtomic(this._Host, "NCOUNT", false);
            }
            this._allocWorkerCounter = false;
        }
    }
}
