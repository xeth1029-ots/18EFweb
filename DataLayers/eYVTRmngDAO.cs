using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using log4net;
using Omu.ValueInjecter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer.RowOpExtension;
using Turbo.Commons;
using WDAIIP.WEB.Areas.Intra.Models;

namespace WDAIIP.WEB.DataLayers
{
    public class eYVTRmngDAO : BaseDAO
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 系統登入相關處理作業

        /// <summary>
        /// 以指定的 UserNO 取得使用者帳號資料
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public eYVTRmngUser GetUser(string userNo, string userPwd)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            parms["USERPWD"] = userPwd;
            return (eYVTRmngUser)base.QueryForObject("eYVTRmng.geteYVTRmngUser",parms);
        }

        /// <summary>
        /// 取得使用者群組清單
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public IList<eYVTRmngUserGroup> GetUserGroups(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return base.QueryForListAll<eYVTRmngUserGroup>("eYVTRmng.geteYVTRmngUserGroups", parms);
        }

        /// <summary>
        /// 取得登入者角色功能清單資料
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public IList<eYVTRmngRoleFunc> GetRoleFuncs(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return base.QueryForListAll<eYVTRmngRoleFunc>("eYVTRmng.geteYVTRmngRoleFuncs", parms);
        }

        /// <summary>
        /// 鎖定使用者帳號
        /// </summary>
        /// <param name="userNo"></param>
        public void UpdateUserEUsed(string userNo)
        {
            TblSYS_USER where = new TblSYS_USER { USR_ID = userNo };
            TblSYS_USER upd = new TblSYS_USER { USR_ID = userNo, USR_EUSED = "N" };

            base.Update(upd, where, where);
        }

        /// <summary>
        /// 記錄使用者登入時間
        /// </summary>
        /// <param name="userNo"></param>
        public void UpdateUserELoginDate(string userNo)
        {
            TblSYS_USER where = new TblSYS_USER { USR_ID = userNo };
            TblSYS_USER upd = new TblSYS_USER { USR_ID = userNo, USR_ELOGINDATE = DateTime.Now };

            base.Update(upd, where, where);
        }

        /// <summary>
        /// 取得系統已啟用的全部功能清單模組
        /// </summary>
        /// <returns></returns>
        public IList<eYVTRmngRoleFunc> geteYVTRmngFuncModules()
        {
            return base.QueryForListAll<eYVTRmngRoleFunc>("eYVTRmng.geteYVTRmngFuncModules", null);
        }

        /// <summary>
        /// 取得角色群組權限功能清單
        /// </summary>
        /// <param name="userNo">使用者ID</param>
        /// <param name="netID"></param>
        /// <returns></returns>
        public IList<eYVTRmngRoleFunc> GetRoleFuncs(string userNo, string netID)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;

            return base.QueryForListAll<eYVTRmngRoleFunc>("eYVTRmng.geteYVTRmngRoleFuncs", parms);
        }

        /// <summary>
        /// 取得系統已啟用的全部功能清單
        /// </summary>
        /// <returns></returns>
        public IList<TblE_FUN> GeteYVTRmngFuncsAll()
        {
            return base.QueryForListAll<TblE_FUN>("eYVTRmng.geteYVTRmngFuncsAll", null);
        }

        /// <summary>
        /// 取得系統功能項目所在的功能表階層項目資料集合
        /// </summary>
        /// <param name="funPage2"></param>
        /// <returns></returns>
        public IList<TblE_FUN> QueryFuncMenuSet( string funPage2)
        {
            var parms = new TblE_FUN { FUN_PAGE2 = funPage2 };
            return base.QueryForListAll<TblE_FUN>("eYVTRmng.queryFuncMenuSet", parms);
        }

        #endregion

        #region 首頁-變更密碼

        /// <summary>
        /// 首頁-變更密碼
        /// </summary>
        /// <param name="userNo"></param>
        /// <param name="pwd"></param>
        public void UpdateUsrPwd(string userNo, string pwd)
        {
            TblSYS_USER where = new TblSYS_USER { USR_ID = userNo };
            TblSYS_USER upd = new TblSYS_USER
            {
                USR_ID = userNo,
                USR_PWD = pwd,
                USR_CHGDATE = DateTime.Now
            };

            base.Update(upd, where, where);
        }

        #endregion

        #region 附件處理

        /// <summary>
        /// 取得已上傳檔案列表
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public IList<TblE_FILE> QueryUploadFiles(TblE_FILE parms)
        {
            //return base.GetRowList<TblE_FILE>(parms); //有欄位大小寫判別問題
            return base.QueryForListAll<TblE_FILE>("eYVTRmng.queryFileUploadsByGID", parms);
        }

        /// <summary>
        /// 新增單筆上傳紀錄 E_FILE
        /// </summary>
        /// <param name="data"></param>
        public Int64? AppendEFILE(TblE_FILE data)
        {
            //base.Insert<TblE_FILE>(data);
            return base.Insert("eYVTRmng.InsertEFile", data);
        }

        /// <summary>
        /// 單筆刪除上傳紀錄 E_FILE
        /// </summary>
        /// <param name="where"></param>
        public void DeleteEFILE(TblE_FILE where)
        {
            base.Delete<TblE_FILE>(where);
        }

        /// <summary>
        /// 批次刪除上傳紀錄 E_FILE
        /// </summary>
        /// <param name="list"></param>
        public void DeleteEFILE(IList<TblE_FILE> list)
        {
            TblE_FILE where = null;

            try
            {
                base.BeginTransaction();

                foreach (TblE_FILE item in list)
                {
                    where = new TblE_FILE
                    {
                        FILE_ID = item.FILE_ID
                    };

                    base.Delete<TblE_FILE>(where);
                }

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("DeleteEFILE failed: " + ex.Message, ex);
            }
        }

        #endregion

        #region Intra/NewsSetting 訊息發佈設定

        /// <summary>
        /// 訊息發佈設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<NewsSettingGridModel> QueryNewsSetting(NewsSettingFormModel form)
        {
            return base.QueryForList<NewsSettingGridModel>("eYVTRmng.queryNewsSetting", form);
        }

        /// <summary>
        /// 以 PK 欄位(NEW_ID) 為條件,
        /// 取得 訊息發佈檔資料
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public NewsSettingDetailModel GetNewsSetting(TblE_NEW parms)
        {
            //TblE_NEW eNew = base.GetRow<TblE_NEW>(where); //有欄位大小寫判別問題
            TblE_NEW eNew = (TblE_NEW)base.QueryForObject("eYVTRmng.getNewsSetting", parms);
            NewsSettingDetailModel detail = new NewsSettingDetailModel();
            detail.InjectFrom(eNew);

            return detail;
        }

        /// <summary>
        /// 訊息發佈設定-新增
        /// </summary>
        /// <param name="detail"></param>
        public void AppendNewsSetting(NewsSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_NEW eNew = new TblE_NEW();

            try
            {
                //base.BeginTransaction();

                // 訊息發佈檔-新增
                eNew.InjectFrom(detail);
                eNew.NEW_DIS_ID = sm.UserInfo.AppUser.UsrOrg;
                eNew.NEW_BROWSE = 0;
                eNew.NEW_CUSER = sm.UserInfo.AppUser.UsrID;
                eNew.NEW_MUSER = sm.UserInfo.AppUser.UsrID;
                //base.Insert(eNew);
                base.Insert("eYVTRmng.InsertNewsSetting", eNew);

                // 附件
                //base.CommitTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                LOG.Error("AppendNewsSetting failed: " + ex.Message);
                throw new Exception("AppendNewsSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 訊息發佈設定-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateNewsSetting(NewsSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_NEW eNew = new TblE_NEW();
            TblE_NEW where = null;

            try
            {
                // 訊息發佈檔-修改
                eNew.InjectFrom(detail);
                eNew.NEW_MDATE = DateTime.Now;
                eNew.NEW_MUSER = sm.UserInfo.AppUser.UsrID;

                where = new TblE_NEW
                {
                    NEW_ID = detail.NEW_ID
                };

                base.Update(eNew, where);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateNewsSetting failed: " + ex.Message);
                throw new Exception("UpdateNewsSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 記錄上傳附件群組代碼
        /// </summary>
        /// <param name="newID"></param>
        /// <param name="fileGID"></param>
        public void UpdateNewsSettingFileGID(Int64 newID, Int64 fileGID)
        {
            TblE_NEW where = new TblE_NEW { NEW_ID = newID};
            TblE_NEW upd = new TblE_NEW { NEW_ID = newID, NEW_FILE_GID = fileGID};

            base.Update(upd, where, where);
        }

        /// <summary>
        /// 訊息發佈設定-刪除(停用)
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteNewsSetting(NewsSettingDetailModel detail)
        {
            //實際是設定成停用
            TblE_NEW where = new TblE_NEW { NEW_ID = detail.NEW_ID };
            TblE_NEW upd = new TblE_NEW { NEW_ID = detail.NEW_ID, NEW_USED = "N"};

            base.Update<TblE_NEW>(upd, where, where);
        }

        #endregion

        #region Intra/NewsTypeSetting 訊息類型設定

        /// <summary>
        /// 訊息類型設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<NewsTypeSettingGridModel> QueryNewsTypeSetting(NewsTypeSettingFormModel form)
        {
            return base.QueryForList<NewsTypeSettingGridModel>("eYVTRmng.queryNewsTypeSetting", form);
        }

        /// <summary>
        /// 訊息類型設定-傳回訊息類別名稱資料是否存在。 若資料存在時回傳 true , 否則回傳 false
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool ExistNewsTypeSetting(NewsTypeSettingDetailModel where)
        {
            var data = (TblE_NEWSTYPE)base.QueryForObject("eYVTRmng.isNewsTypeSettingFound", where);
            return (data != null);
        }

        /// <summary>
        /// 以 PK 欄位(NEW_ID) 為條件, 取得 訊息類別設定檔資料
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public NewsTypeSettingDetailModel GetNewsTypeSetting(TblE_NEWSTYPE parms)
        {
            TblE_NEWSTYPE newsType = (TblE_NEWSTYPE)base.QueryForObject("eYVTRmng.getNewsTypeSetting", parms);
            NewsTypeSettingDetailModel detail = new NewsTypeSettingDetailModel();
            detail.InjectFrom(newsType);

            return detail;
        }

        /// <summary>
        /// 訊息類型設定-新增
        /// </summary>
        /// <param name="detail"></param>
        public void AppendNewsTypeSetting(NewsTypeSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_NEWSTYPE newsType = new TblE_NEWSTYPE();

            try
            {
                newsType.InjectFrom(detail);
                newsType.NST_CUSER = sm.UserInfo.AppUser.UsrID;
                newsType.NST_MUSER = sm.UserInfo.AppUser.UsrID;
                base.Insert("eYVTRmng.InsertNewsTypeSetting", newsType);
            }
            catch (Exception ex)
            {
                LOG.Error("AppendNewsTypeSetting failed: " + ex.Message);
                throw new Exception("AppendNewsTypeSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 訊息類型設定-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateNewsTypeSetting(NewsTypeSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_NEWSTYPE newsType = new TblE_NEWSTYPE();
            TblE_NEWSTYPE where = null;

            try
            {
                newsType.InjectFrom(detail);
                newsType.NST_MUSER = sm.UserInfo.AppUser.UsrID;

                where = new TblE_NEWSTYPE { NST_ID = detail.NST_ID};

                base.Update(newsType, where, where);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateNewsTypeSetting failed: " + ex.Message);
                throw new Exception("UpdateNewsTypeSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 訊息類型設定-刪除
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteNewsTypeSetting(NewsTypeSettingDetailModel detail)
        {
            //實際是設定成停用
            TblE_NEWSTYPE where = new TblE_NEWSTYPE { NST_ID = detail.NST_ID };
            TblE_NEWSTYPE upd = new TblE_NEWSTYPE { NST_ID = detail.NST_ID, NST_USED = "N" };

            base.Update<TblE_NEWSTYPE>(upd, where, where);
        }

        #endregion

        #region Intra/DownloadSetting 下載資料設定
        /// <summary>
        /// 下載資料設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<DownloadSettingGridModel> QueryDownloadSetting(DownloadSettingFormModel form)
        {
            return base.QueryForList<DownloadSettingGridModel>("eYVTRmng.queryDownloadSetting", form);
        }

        /// <summary>
        ///  以 PK 欄位(DLD_ID) 為條件,取得特定一筆下載資料檔資訊
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public DownloadSettingDetailModel GetDownloadSetting(TblE_DOWNLOAD parms)
        {
            TblE_DOWNLOAD data = (TblE_DOWNLOAD)base.QueryForObject("eYVTRmng.getDownloadSetting", parms);
            DownloadSettingDetailModel detail = new DownloadSettingDetailModel();
            detail.InjectFrom(data);

            return detail;
        }

        /// <summary>
        /// 下載資料設定-刪除(停用)
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteDownloadSetting(DownloadSettingDetailModel detail)
        {
            TblE_DOWNLOAD where = new TblE_DOWNLOAD { DLD_ID = detail.DLD_ID };
            TblE_DOWNLOAD upd = new TblE_DOWNLOAD { DLD_ID = detail.DLD_ID, DLD_USED = "N" };

            base.Update<TblE_DOWNLOAD>(upd, where, where);
        }

        /// <summary>
        /// 下載資料設定-新增
        /// </summary>
        /// <param name="detail"></param>
        public void AppendDownloadSetting(DownloadSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_DOWNLOAD data = new TblE_DOWNLOAD();

            try
            {
                data.InjectFrom(detail);
                data.DLD_DIS_ID = sm.UserInfo.AppUser.UsrOrg;
                data.DLD_BROWSE = 0;
                data.DLD_CUSER = sm.UserInfo.AppUser.UsrID;
                data.DLD_MUSER = sm.UserInfo.AppUser.UsrID;

                base.Insert("eYVTRmng.InsertDownloadSetting", data);
            }
            catch (Exception ex)
            {
                LOG.Error("AppendDownloadSetting failed: " + ex.Message);
                throw new Exception("AppendDownloadSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 下載資料設定-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateDownloadSetting(DownloadSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_DOWNLOAD data = new TblE_DOWNLOAD();
            TblE_DOWNLOAD where = null;

            try
            {
                data.InjectFrom(detail);
                data.DLD_MDATE = DateTime.Now;
                data.DLD_MUSER = sm.UserInfo.AppUser.UsrID;

                where = new TblE_DOWNLOAD { DLD_ID = detail.DLD_ID };

                base.Update(data,where);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateDownloadSetting failed: " + ex.Message);
                throw new Exception("UpdateDownloadSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 下載資料設定-記錄上傳附件群組代碼
        /// </summary>
        /// <param name="dldID"></param>
        /// <param name="fileGID"></param>
        public void UpdateDownloadSettingFileGID(Int64 dldID, Int64 fileGID)
        {
            TblE_DOWNLOAD where = new TblE_DOWNLOAD { DLD_ID = dldID };
            TblE_DOWNLOAD upd = new TblE_DOWNLOAD { DLD_ID = dldID, DLD_FILE_GID = fileGID };

            base.Update(upd, where, where);
        }
        #endregion

        #region Intra/ActionsSetting 活動花絮設定
        /// <summary>
        /// 活動花絮設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ActionsSettingGridModel> QueryActionsSetting(ActionsSettingFormModel form)
        {
            return base.QueryForList<ActionsSettingGridModel>("eYVTRmng.queryActionsSetting", form);
        }

        /// <summary>
        ///  以 PK 欄位(ACT_ID) 為條件,取得特定一筆下載資料檔資訊
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public ActionsSettingDetailModel GetActionsSetting(TblE_ACTIONS parms)
        {
            TblE_ACTIONS data = (TblE_ACTIONS)base.QueryForObject("eYVTRmng.getActionsSetting", parms);
            ActionsSettingDetailModel detail = null;

            if (data != null)
            {
                detail = new ActionsSettingDetailModel();
                detail.InjectFrom(data);
            }

            return detail;
        }

        /// <summary>
        /// 活動花絮設定-刪除(停用)
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteActionsSetting(ActionsSettingDetailModel detail)
        {
            TblE_ACTIONS where = new TblE_ACTIONS { ACT_ID = detail.ACT_ID };
            TblE_ACTIONS upd = new TblE_ACTIONS { ACT_ID = detail.ACT_ID, ACT_USED = "N" };

            base.Update<TblE_ACTIONS>(upd, where, where);
        }

        /// <summary>
        /// 活動花絮設定-新增
        /// </summary>
        /// <param name="detail"></param>
        public void AppendActionsSetting(ActionsSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_ACTIONS data = new TblE_ACTIONS();

            try
            {
                data.InjectFrom(detail);
                data.ACT_DIS_ID = sm.UserInfo.AppUser.UsrOrg;
                data.ACT_OBJECT = " ";
                data.ACT_BROWSE = 0;
                data.ACT_CUSER = sm.UserInfo.AppUser.UsrID;
                data.ACT_MUSER = sm.UserInfo.AppUser.UsrID;

                base.Insert("eYVTRmng.InsertActionsSetting", data);
            }
            catch (Exception ex)
            {
                LOG.Error("AppendActionsSetting failed: " + ex.Message);
                throw new Exception("AppendActionsSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 活動活絮設定-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateActionsSetting(ActionsSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_ACTIONS data = new TblE_ACTIONS();
            TblE_ACTIONS where = null;

            try
            {
                data.InjectFrom(detail);
                data.ACT_MDATE = DateTime.Now;
                data.ACT_MUSER = sm.UserInfo.AppUser.UsrID;

                where = new TblE_ACTIONS { ACT_ID = detail.ACT_ID };

                base.Update(data, where);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateActionsSetting failed: " + ex.Message);
                throw new Exception("UpdateActionsSetting failed: " + ex.Message, ex);
            }
        }
        #endregion

        #region Intra/FaqSetting FAQ設定
        /// <summary>
        /// FAQ設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<FaqSettingGridModel> QueryFaqSetting(FaqSettingFormModel form)
        {
            return base.QueryForList<FaqSettingGridModel>("eYVTRmng.queryFaqSetting", form);
        }

        /// <summary>
        /// FAQ設定-依計畫,FAQ類別查詢
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public IList<FaqSettingGroupGridModel> QueryFaqSettingList(TblE_FAQ parms)
        {
            return base.QueryForListAll<FaqSettingGroupGridModel>("eYVTRmng.queryFaqSettingList", parms);
        }
        #endregion

        #region Intra/FunctionSetting 網站功能設定

        /// <summary>
        /// 網站功能設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<FunctionSettingGridModel> QueryFunctionSetting(FunctionSettingFormModel form)
        {
            return base.QueryForList<FunctionSettingGridModel>("eYVTRmng.queryFunctionSetting", form);
        }

        /// <summary>
        /// 以PK欄位(FUN_ID)為條件,
        /// 取得 網站功能設定 資料
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public FunctionSettingDetailModel GetFunctionSetting(TblE_FUN parms)
        {
            TblE_FUN func = (TblE_FUN)base.QueryForObject("eYVTRmng.getFunctionSetting", parms);
            FunctionSettingDetailModel detail = new FunctionSettingDetailModel();
            detail.InjectFrom(func);

            return detail;
        }

        /// <summary>
        /// 判斷『外網功能權限』是否給予此功能項目刪除功能
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public string GetFunctionSettingAuthority(string parm)
        {
            string FUN_ID = parm;
            Int32 myData = Convert.ToInt32(base.QueryForObject<long>("eYVTRmng.getFunctionAuthority", FUN_ID).ToString().Trim());

            if (myData == 0)
                return "Y";
            else
                return "N";
        }

        /// <summary>
        /// 網站功能設定-新增
        /// </summary>
        /// <param name="detail"></param>
        public void InsertFunctionSetting(FunctionSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_FUN func = new TblE_FUN();
            DateTime thisTime = DateTime.Now;

            try
            {
                //base.BeginTransaction();

                func.InjectFrom(detail);
                func.FUN_CDATE = thisTime;
                func.FUN_CUSER = sm.UserInfo.AppUser.UsrID;
                func.FUN_MDATE = thisTime;
                func.FUN_MUSER = sm.UserInfo.AppUser.UsrID;

                base.Insert("eYVTRmng.InsertEFUN", func);   //『對應 sqlMaps/EYVTRmng.xml/InsertEFUN 方法』
                //base.CommitTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                LOG.Error("InsertFunctionSetting failed: " + ex.Message);
                throw new Exception("InsertFunctionSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 網站功能設定-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateFunctionSetting(FunctionSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_FUN func = new TblE_FUN();
            TblE_FUN funcWhere = null;

            try
            {
                func.InjectFrom(detail);
                func.FUN_MDATE = DateTime.Now;
                func.FUN_MUSER = sm.UserInfo.AppUser.UsrID;

                funcWhere = new TblE_FUN
                {
                    FUN_ID = detail.FUN_ID
                };

                base.Update(func, funcWhere);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateFunctionSetting failed: " + ex.Message);
                throw new Exception("UpdateFunctionSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 網站功能設定-刪除
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteFunctionSetting(FunctionSettingDetailModel detail)
        {
            //實際是設定成停用
            TblE_FUN where = new TblE_FUN { FUN_ID = detail.FUN_ID };
            TblE_FUN updateModel = new TblE_FUN { FUN_ID = detail.FUN_ID, FUN_USED = "N" };

            base.Update<TblE_FUN>(updateModel, where, where);
        }

        #endregion

        #region Intra/GroupSetting 權限設定

        /// <summary>
        /// 權限設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<GroupSettingGridModel> QueryGroupSetting(GroupSettingFormModel form)
        {
            return base.QueryForList<GroupSettingGridModel>("eYVTRmng.queryGroupSetting", form);
        }

        /// <summary>
        /// 以PK欄位(FUN_ID)為條件,
        /// 取得 權限設定資料
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public GroupSettingDetailModel GetGroupSetting(TblE_GROUP parms)
        {
            TblE_GROUP func = (TblE_GROUP)base.QueryForObject("eYVTRmng.getGroupSetting", parms);
            GroupSettingDetailModel detail = new GroupSettingDetailModel();
            detail.InjectFrom(func);

            return detail;
        }

        /// <summary>
        /// 取得外網群組對應外網網站功能項資訊
        /// </summary>
        /// <param name="GRP_ID"></param>
        /// <returns></returns>
        public IList<GroupSettingFunctionItem> GetGroupSettingFunctionItem(string GRP_ID)
        {
            Hashtable parm = new Hashtable();
            parm.Add("GRP_ID", GRP_ID);

            //(因為選項清單是不分頁的,所以要用QueryForListAll,而不是QueryForList)
            return base.QueryForListAll<GroupSettingFunctionItem>("eYVTRmng.getGroupSettingFunctionItem", parm);
        }

        /// <summary>
        /// 權限設定-傳回群組名稱是否存在。(若資料存在時回傳true,否則回傳false)
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool ExistGroupSetting(GroupSettingDetailModel where)
        {
            var data = (TblE_GROUP)base.QueryForObject("eYVTRmng.isGroupSettingFound", where);
            return (data != null);
        }

        /// <summary>
        /// 權限設定(1)-新增
        /// </summary>
        /// <param name="detail"></param>
        public Int64 InsertGroupSetting(GroupSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_GROUP eGroup = new TblE_GROUP();
            DateTime thisTime = DateTime.Now;

            try
            {
                //base.BeginTransaction();

                eGroup.InjectFrom(detail);
                eGroup.GRP_CDATE = thisTime;
                eGroup.GRP_CUSER = sm.UserInfo.AppUser.UsrID;
                eGroup.GRP_MDATE = thisTime;
                eGroup.GRP_MUSER = sm.UserInfo.AppUser.UsrID;

                //base.Insert("eYVTRmng.InsertEGROUP", eGroup);   //『對應 sqlMaps/EYVTRmng.xml/InsertEGROUP 方法』
                return base.Insert("eYVTRmng.InsertEGROUP", eGroup);
                //base.CommitTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                LOG.Error("InsertGroupSetting failed: " + ex.Message);
                throw new Exception("InsertGroupSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 權限設定(1)-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateGroupSetting(GroupSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_GROUP func = new TblE_GROUP();
            TblE_GROUP funcWhere = null;

            try
            {
                func.InjectFrom(detail);
                func.GRP_MDATE = DateTime.Now;
                func.GRP_MUSER = sm.UserInfo.AppUser.UsrID;

                funcWhere = new TblE_GROUP
                {
                    GRP_ID = detail.GRP_ID
                };

                base.Update(func, funcWhere);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateGroupSetting failed: " + ex.Message);
                throw new Exception("UpdateGroupSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 權限設定(2)-刪除
        /// </summary>
        public void DeleteAuthority(TblE_AUTHORITY grpId_Authority)
        {
            //BeginTransaction();

            TblE_AUTHORITY deleteModel = (TblE_AUTHORITY)new TblE_AUTHORITY().InjectFrom(grpId_Authority);
            base.Delete<TblE_AUTHORITY>(deleteModel);

            //RollBackTransaction();
            //CommitTransaction();
        }

        /// <summary>
        /// 權限設定(2)-新增
        /// </summary>
        /// <param name="authGRP_ID"></param>
        /// <param name="authFuncId"></param>
        /// <returns></returns>
        public void InsertAuthority(string authGRP_ID, string authFuncId)
        {
            TblE_AUTHORITY eAuthority = new TblE_AUTHORITY { AUTH_GRP_ID = Convert.ToInt64(authGRP_ID), AUTH_FUN_ID = Convert.ToInt64(authFuncId) };

            try
            {
                //base.BeginTransaction();
                base.Insert("eYVTRmng.InsertAuthority", eAuthority);
                //base.CommitTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                LOG.Error("InsertAuthority failed: " + ex.Message);
                throw new Exception("InsertAuthority failed: " + ex.Message, ex);
            }
        }

        #endregion

        #region Intra/AccountSetting 帳號設定

        /// <summary>
        /// 帳號設定-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<AccountSettingGridModel> QueryAccountSetting(AccountSettingFormModel form)
        {
            return base.QueryForList<AccountSettingGridModel>("eYVTRmng.queryAccountSetting", form);
        }

        /// <summary>
        /// 以PK欄位(FUN_ID)為條件,
        /// 取得 帳號設定 資料
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public AccountSettingDetailModel GetAccountSetting(TblSYS_USER parms)
        {
            TblSYS_USER sysUsr = (TblSYS_USER)base.QueryForObject("eYVTRmng.getAccountSetting", parms);
            AccountSettingDetailModel detail = new AccountSettingDetailModel();
            detail.InjectFrom(sysUsr);

            return detail;
        }

        /// <summary>
        /// 檢核身份證號碼是否已經存在
        /// </summary>
        /// <param name="GRP_ID"></param>
        /// <returns></returns>
        public IList<AccountSettingDetailModel> ExistAccountSettingIdno(string USR_IDNO)
        {
            Hashtable parm = new Hashtable();
            parm.Add("USR_IDNO", USR_IDNO);

            //(因為選項清單是不分頁的,所以要用QueryForListAll,而不是QueryForList)
            return base.QueryForListAll<AccountSettingDetailModel>("eYVTRmng.isAccountSettingIdnoFound", parm);
        }

        /// <summary>
        /// 帳號設定-傳回帳號是否存在。(若資料存在時回傳true,否則回傳false)
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool ExistAccountSetting(AccountSettingDetailModel where)
        {
            var data = (TblSYS_USER)base.QueryForObject("eYVTRmng.isAccountSettingFound", where);
            return (data != null);
        }

        /// <summary>
        /// 帳號設定-新增
        /// </summary>
        /// <param name="detail"></param>
        public void InsertAccountSetting(AccountSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblSYS_USER acc = new TblSYS_USER();
            DateTime thisTime = DateTime.Now;

            try
            {
                //base.BeginTransaction();

                acc.InjectFrom(detail);
                acc.USR_CDATE = thisTime;
                acc.USR_CUSER = sm.UserInfo.AppUser.UsrID;
                acc.USR_MDATE = thisTime;
                acc.USR_MUSER = sm.UserInfo.AppUser.UsrID;

                base.Insert("eYVTRmng.InsertAccountSetting", acc);
                //base.CommitTransaction();
            }
            catch (Exception ex)
            {
                //base.RollBackTransaction();
                LOG.Error("InsertAccountSetting failed: " + ex.Message);
                throw new Exception("InsertAccountSetting failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 帳號設定-修改
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateAccountSetting(AccountSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblSYS_USER acc = new TblSYS_USER();
            TblSYS_USER accWhere = null;

            try
            {
                acc.InjectFrom(detail);
                acc.USR_MDATE = DateTime.Now;
                acc.USR_MUSER = sm.UserInfo.AppUser.UsrID;

                accWhere = new TblSYS_USER
                {
                    USR_ID = detail.USR_ID
                };

                base.Update(acc, accWhere);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateAccountSetting failed: " + ex.Message);
                throw new Exception("UpdateAccountSetting failed: " + ex.Message, ex);
            }
        }

        #endregion

        #region Intra/MemberSetting 網站會員管理

        /// <summary>
        /// 網站會員管理-查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<MemberSettingGridModel> QueryMemberSetting(MemberSettingFormModel form)
        {
            return base.QueryForList<MemberSettingGridModel>("eYVTRmng.queryMemberSetting", form);
        }

        /// <summary>
        /// 以PK欄位(FUN_ID)為條件,
        /// 取得 網站會員管理資料
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public MemberSettingDetailModel GetMemberSetting(TblE_MEMBER parms)
        {
            TblE_MEMBER mem = (TblE_MEMBER)base.QueryForObject("eYVTRmng.getMemberSetting", parms);
            MemberSettingDetailModel detail = new MemberSettingDetailModel();
            detail.InjectFrom(mem);

            return detail;
        }

        /// <summary>
        /// 網站會員管理-帳號啟用
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateMemberTurnOn(MemberSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_MEMBER mem = new TblE_MEMBER();
            TblE_MEMBER memWhere = null;

            try
            {
                mem.InjectFrom(detail);
                mem.MEM_STATUS = "Y";
                mem.MEM_MDATE = DateTime.Now;
                mem.MEM_MUSER = sm.UserInfo.AppUser.UsrID;

                memWhere = new TblE_MEMBER
                {
                    MEM_ID = detail.MEM_ID
                };

                base.Update(mem, memWhere);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateMemberTurnOn failed: " + ex.Message);
                throw new Exception("UpdateMemberTurnOn failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 網站會員管理-帳號停用
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateMemberTurnOff(MemberSettingDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            TblE_MEMBER mem = new TblE_MEMBER();
            TblE_MEMBER memWhere = null;

            try
            {
                mem.InjectFrom(detail);
                mem.MEM_STATUS = "N";
                mem.MEM_MDATE = DateTime.Now;
                mem.MEM_MUSER = sm.UserInfo.AppUser.UsrID;

                memWhere = new TblE_MEMBER
                {
                    MEM_ID = detail.MEM_ID
                };

                base.Update(mem, memWhere);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateMemberTurnOff failed: " + ex.Message);
                throw new Exception("UpdateMemberTurnOff failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 網站會員管理-重設密碼
        /// </summary>
        /// <param name="detail"></param>
        public void UpdateMemberPwd(MemberSettingDetailModel detail, string strDefaultPwd)
        {
            SessionModel sm = SessionModel.Get();
            TblE_MEMBER mem = new TblE_MEMBER();
            TblE_MEMBER memWhere = null;

            try
            {
                mem.InjectFrom(detail);
                mem.MEM_PWD = strDefaultPwd;
                mem.MEM_MDATE = DateTime.Now;
                mem.MEM_MUSER = sm.UserInfo.AppUser.UsrID;

                memWhere = new TblE_MEMBER
                {
                    MEM_ID = detail.MEM_ID
                };

                base.Update(mem, memWhere);
            }
            catch (Exception ex)
            {
                LOG.Error("UpdateMemberPwd failed: " + ex.Message);
                throw new Exception("UpdateMemberPwd failed: " + ex.Message, ex);
            }
        }

        #endregion
    }
}