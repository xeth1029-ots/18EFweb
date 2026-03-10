using eYVTR_mng_n.Models;
using eYVTR_mng_n.Models.Entities;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using log4net;
using eYVTR_mng_n.Areas.AM.Models;
using Turbo.DataLayer;
using Omu.ValueInjecter;
using Turbo.DataLayer.RowOpExtension;
using Turbo.Commons;

namespace eYVTR_mng_n.DataLayers
{
    public class ClamDAO : BaseDAO
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Services/ClamService 取得系統功能項目所在的功能表階層項目資料集合
        /// <summary>
        /// 取得系統功能項目所在的功能表階層項目資料集合
        /// </summary>
        /// <param name="prgid">技能檢定系統 1.0 使用的功能編號（即要在頁面「程式功能模組路徑及功能名稱」區塊顯示的功能編號）</param>
        /// <param name="prgid2">（非必填）技能檢定系統 2.0 使用的功能編號。若僅單純指定 prgid 參數值會傳回非預期的功能表階層項目資料集合時，請務必再指定 prgid2 參數值</param>
        /// <returns></returns>
        public IList<TblCLAMFUNCM> QueryFuncMenuSet(string prgid, string prgid2 = null)
        {
            var parms = new { PRGID = prgid,  PRGID2 = prgid2 };
            return base.QueryForList<TblCLAMFUNCM>("CLAM.queryFuncMenuSet", parms);
        }
        #endregion
        
        #region AM/C101M 系統功能維護 系統名稱-查詢
        public IList<KeyMapModel> QueryC101MSysidlist()
        {
            return base.QueryForList<KeyMapModel>("CLAM.getC101MSysidList", null);
        }
        #endregion

        #region AM/C101M 系統功能維護 模組代號-查詢
        public IList<KeyMapModel> QueryC101MModuleslist(C101MFormModel parms)
        {
            return base.QueryForList<KeyMapModel>("CLAM.getC101MModulesList", parms);
        }
        #endregion

        #region AM/C101M 系統功能維護 查詢
        /// <summary>
        /// 系統功能維護-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<C101MGridModel> QueryC101M(C101MFormModel parms)
        {
            return base.QueryForList<C101MGridModel>("CLAM.queryC101M", parms);
        }
        #endregion

        #region AM/C101M 新增 系統功能維護(TblCLAMFUNCM) C101MAppendClamfuncm
        /// <summary>
        /// 新增 系統功能維護(TblCLAMFUNCM)
        /// </summary>
        /// <param name="clamfuncm"></param>
        public void C101MAppendClamfuncm(TblCLAMFUNCM clamfuncm)
        {
            //預設資料
            clamfuncm.PRGID = " ";
            clamfuncm.OPENAUTH = " ";
            clamfuncm.SHOWMENU = "1";

            TblCLAMFUNCM clamfuncm2 = new TblCLAMFUNCM();
            
            clamfuncm2.InjectFrom(clamfuncm);
            //寫入資料
            base.Insert<TblCLAMFUNCM>(clamfuncm2);
        }
        #endregion

        #region AM/C101M 修改 系統功能維護(TblCLAMFUNCM) C101MAppendClamfuncm
        /// <summary>
        /// 修改 系統功能維護(TblCLAMFUNCM)
        /// </summary>
        /// <param name="clamfuncm"></param>
        public void C101MUpdateClamfuncm(TblCLAMFUNCM clamfuncm)
        {
            TblCLAMFUNCM clamfuncm2 = new TblCLAMFUNCM();

            clamfuncm2.InjectFrom(clamfuncm);

            clamfuncm2.PRGID = " ";

            this.Find<TblCLAMFUNCM>()
                .IsEqual(x => x.SYS_ID, clamfuncm2.SYS_ID)
                .IsEqual(x => x.MODULES, clamfuncm2.MODULES)
                .IsEqual(x => x.SUBMODULES, clamfuncm2.SUBMODULES)
                .IsEqual(x => x.PRGID, clamfuncm2.PRGID)
                .Update(clamfuncm2, this);
        }
        #endregion

        #region AM/C101M 系統功能維護-查詢明細 GetC101MDetail
        public C101MDetailModel getC101MDetail(TblCLAMFUNCM where)
        {
            TblCLAMFUNCM clamfuncm = base.GetRow<TblCLAMFUNCM>(where);
            C101MDetailModel detail = new C101MDetailModel(clamfuncm);
            return detail;
        }
        #endregion

        #region AM/C101M 刪除 系統功能維護(TblCLAMFUNCM) C101MDeleteClamfuncm
        /// <summary>
        /// 刪除 系統功能維護(TblCLAMFUNCM),
        /// (根據 PK 欄位(SYS_ID,MODULES,SUBMODULES,PRGID)
        /// </summary>
        /// <param name="clamfuncm"></param>
        public void C101MDeleteClamfuncm(TblCLAMFUNCM clamfuncm)
        {
            try
            {
                base.BeginTransaction();

                base.Delete<TblCLAMFUNCM>(clamfuncm);

                TblCLAMGMAPM clamgmapmwhere = new TblCLAMGMAPM();
                clamgmapmwhere.InjectFrom(clamfuncm);

                base.Delete<TblCLAMGMAPM>(clamgmapmwhere);

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("C101MDeleteClamfuncm failed: " + ex.Message, ex);
            }
        }
        #endregion

        #region AM/C603R1 權限資料列印-帳號 查詢
        /// <summary>
        /// 權限資料列印-帳號-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<C603R1GridModel> QueryC603R1(C603R1FormModel parms)
        {
            return base.QueryForList<C603R1GridModel>("CLAM.queryC603R1", parms);
        }
        #endregion

        #region AM/C603R2 權限資料列印-程式 查詢
        /// <summary>
        /// 權限資料列印-程式-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<C603R2GridModel> QueryC603R2(C603R2FormModel parms)
        {
            return base.QueryForList<C603R2GridModel>("CLAM.queryC603R2", parms);
        }
        #endregion


        /// <summary>
        /// 以指定的 UserNO 取得使用者帳號資料
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public ClamUser GetUser(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return (ClamUser)base.QueryForObject("CLAM.getClamUser", parms);
        }

        /// <summary>
        /// 以指定的 User PKI No 取得使用者帳號資料
        /// </summary>
        /// <param name="userPkiNo"></param>
        /// <returns></returns>
        public TblCLAMDBURM GetUserByPKI(string userPkiNo)
        {
            TblCLAMDBURM where = new TblCLAMDBURM { PKINO = userPkiNo };
            return base.GetRow<TblCLAMDBURM>(where);
        }

        
        /// <summary>
        /// 取得使用者群組清單
        /// </summary>
        /// <param name="userNo">使用者ID</param>
        /// <param name="netID"></param>
        /// <returns></returns>
        public IList<ClamUserGroup> GetUserGroups(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return base.QueryForListAll<ClamUserGroup>("CLAM.getClamUserGroups", parms);
        }

        /// <summary>
        /// 取得角色群組權限功能清單
        /// </summary>
        /// <param name="userNo">使用者ID</param>
        /// <param name="netID"></param>
        /// <returns></returns>
        public IList<ClamRoleFunc> GetRoleFuncs(string userNo, string netID)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            parms["NETID"] = netID;
            return base.QueryForListAll<ClamRoleFunc>("CLAM.getClamRoleFuncs", parms);
        }

        /// <summary>
        /// 取得系統已啟用的全部功能清單
        /// </summary>
        /// <returns></returns>
        public IList<TblCLAMFUNCM> GetClamFuncsAll()
        {
            return base.QueryForListAll<TblCLAMFUNCM>("CLAM.getClamFuncsAll", null);
        }

        /// <summary>
        /// 取得系統已啟用的全部功能清單模組
        /// </summary>
        /// <returns></returns>
        public IList<ClamRoleFunc> getClamFuncModules()
        {
            return base.QueryForListAll<ClamRoleFunc>("CLAM.getClamFuncModules", null);
        }

        /// <summary>
        /// 更新登入密碼錯誤次數
        /// </summary>
        /// <param name="dburm"></param>
        public void UpdateUserErrCount(TblCLAMDBURM dburm)
        {
            TblCLAMDBURM where = new TblCLAMDBURM { USERNO = dburm.USERNO };
            TblCLAMDBURM upd = new TblCLAMDBURM { USERNO = dburm.USERNO, ERRCT = dburm.ERRCT };
            base.Update<TblCLAMDBURM>(upd, where, where);
        }

        /// <summary>
        /// 鎖定使用者帳號
        /// </summary>
        /// <param name="dburm"></param>
        public void UpdateUserAccountLock(TblCLAMDBURM dburm)
        {
            TblCLAMDBURM where = new TblCLAMDBURM { USERNO = dburm.USERNO };
            TblCLAMDBURM upd = new TblCLAMDBURM { USERNO = dburm.USERNO, AUTHSTATUS = "8" };
            base.Update<TblCLAMDBURM>(upd, where, where);
        }



        #region AM/C201M 群組維護作業
        /// <summary>
        /// 群組維護作業-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<C201MGridModel> QueryC201M(C201MFormModel parms)
        {
            return base.QueryForList<C201MGridModel>("CLAM.queryC201M", parms);
        }

        /// <summary>
        /// 以 PK 欄位(GRPID) 為條件, 
        /// 取得 群組維護作業(CLAMGRP)
        /// </summary>
        /// <param name="where">資料查詢條件</param>
        /// <returns></returns>
        public C201MDetailModel GetCLAMGRP(TblCLAMGRP where)
        {
            return (C201MDetailModel)base.QueryForObject("CLAM.getC201MDetail", where);
        }

        /// <summary>
        /// 傳回 群組維護作業(CLAMGRP)是否存在。若資料存在時傳回 true，否則傳回 false。
        /// </summary>
        /// <param name="where">資料查詢條件</param>
        /// <returns></returns>
        public bool ExistCLAMGRP(C201MDetailModel where)
        {
            var row = (TblCLAMGRP)base.QueryForObject("CLAM.isC201MFound", where);
            return (row != null);
        }
        /// <summary>
        /// 新增 群組維護作業(CLAMGRP)
        /// </summary>
        /// <param name="detail">群組維護作業 DetailModel</param>
        public void AppendCLAMGRP(C201MDetailModel detail)
        {
            base.Insert<TblCLAMGRP>(detail);
        }
        /// <summary>
        /// 更新 群組維護作業(CLAMGRP),
        /// 不允許變更 PK 欄位(GRPID)
        /// </summary>
        /// <param name="detail">群組維護作業 DetailModel</param>
        public void UpdateCLAMGRP(C201MDetailModel detail)
        {
            TblCLAMGRP where = new TblCLAMGRP
            {
                // PK 欄位
                GRPID = detail.GRPID
            };
            base.Update<TblCLAMGRP>(detail, where);
            
        }
        /// <summary>
        /// 刪除 群組維護作業(CLAMGRP),
        /// (根據PK 欄位: GRPID)
        /// </summary>
        /// <param name="detail">群組維護作業 DetailModel</param>
        public void DeleteCLAMGRP(C201MDetailModel detail)
        {
            TblCLAMGRP where = new TblCLAMGRP
            {
                GRPID = detail.GRPID
            };
            base.Delete<TblCLAMGRP>(where);

            ///刪除相關資料CLAMGMAPM資料表
            TblCLAMGMAPM apm_where = new TblCLAMGMAPM
            {
                GRPID = detail.GRPID
            };
            base.Delete<TblCLAMGMAPM>(apm_where);
        }
        #endregion

        #region AM/C201M1 群組程式設定
        /// <summary>
        /// 群組程式設定-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<C201M1GridModel> QueryC201M1(C201M1FormModel parms)
        {
            return base.QueryForList<C201M1GridModel>("CLAM.queryC201M1", parms);
        }

        /// <summary>群組程式設定-更新(TblCLAMGMAPM)</summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void UpdateORAppendC201M1(IList<C201M1GridModel> gridList)
        {
            base.BeginTransaction();
            try
            {
                foreach (TblCLAMGMAPM gridItem in gridList)
                {
                    TblCLAMGMAPM where = new TblCLAMGMAPM
                    {
                        GRPID = gridItem.GRPID,
                        SYS_ID = gridItem.SYS_ID,
                        MODULES = gridItem.MODULES,
                        SUBMODULES = gridItem.SUBMODULES,
                        PRGID = gridItem.PRGID
                    };
                    base.InsertOrUpdate<TblCLAMGMAPM>(gridItem, where);
                }

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("UpdateORAppend fail#C201M failed: " + ex.Message, ex);
            }
        }
        #endregion


        #region AM/N301M 報到場次維護 

        /// <summary>
        /// 系統代碼清單
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> QueryN301MSysidlist()
        {
            return base.QueryForListAll<KeyMapModel>("CLAM.getN301MSysidList", null);
        }

        /// <summary>
        /// 報到場次維護-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<N301MGridModel> QueryN301M(N301MFormModel parms)
        {
            return base.QueryForList<N301MGridModel>("CLAM.queryN301M", parms);
        }


        /// <summary>
        /// 取得指定 PROF_ID 的 NFCPROF 及 CHECKIN_ITEM
        /// </summary>
        /// <param name="profId"></param>
        /// <returns></returns>
        public N301MDetailModel GetN301MDetail(long profId)
        {
            N301MDetailModel model = null;
            NFCPROF where = new NFCPROF
            {
                PROF_ID = profId
            };

            NFCPROF nfcprof = base.GetRow<NFCPROF>(where);
            if(nfcprof != null)
            {
                model = new N301MDetailModel(nfcprof);

                // 取得 CHECKIN_ITEM
                IList<CHECKIN_ITEM> itemList = base
                    .GetRowList<CHECKIN_ITEM>(new CHECKIN_ITEM { PROF_ID = nfcprof.PROF_ID });
                if(itemList != null)
                {
                    foreach(var item in itemList.OrderBy(r=>r.SEQ) )
                    {
                        model.CheckInItems.Add(new N301MDetailItemModel(item));
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 報到場次維護-新增或修改儲存,依 Detail.IsNew 判斷執行新增或修改,
        /// 有異常時直接丟出 Exception.
        /// <para>新增時會回傳自動編號的PROF_ID值</para>
        /// <para>修改時會回傳受影響的筆數</para>
        /// </summary>
        /// <param name="Detail"></param>
        public int SaveN301M(N301MDetailModel Detail)
        {
            // 檢核 PROF_ID
            NFCPROF check = null;
            if (Detail.PROF_ID.HasValue && Detail.PROF_ID.Value > 0)
            {
                check = base.GetRow<NFCPROF>(new NFCPROF { PROF_ID = Detail.PROF_ID});
            }

            // 檢核 SYS_PROF_ID
            NFCPROF check2 = null;
            check2 = base.GetRow<NFCPROF>(new NFCPROF { SYS_PROF_ID = Detail.SYS_PROF_ID });

            base.BeginTransaction();
            try
            {
                int rtn = -1;
                if (Detail.IsNew)
                {
                    // 新增儲存

                    if (check != null)
                    {
                        throw new Exception("NFCPROF(PROF_ID = " + Detail.PROF_ID + ") 資料已存在, 不能新增!");
                    }
                    if(check2 != null)
                    {
                        throw new Exception("所屬系統場次代碼(" + Detail.SYS_PROF_ID + ") 資料已存在(重覆)!");
                    }

                    rtn = base.Insert<NFCPROF>(Detail);
                    Detail.PROF_ID = rtn;

                }
                else
                {
                    // 修改儲存
                    if (check == null)
                    {
                        throw new Exception("NFCPROF(PROF_ID = " + Detail.PROF_ID + ") 資料不存在!");
                    }
                    if (check2 != null && check2.PROF_ID != check.PROF_ID)
                    {
                        throw new Exception("所屬系統場次代碼(" + Detail.SYS_PROF_ID + ") 資料已存在(重覆)!");
                    }

                    rtn = base.Update<NFCPROF>(Detail, new NFCPROF { PROF_ID = Detail.PROF_ID });

                    // 先刪除舊的 CHECKIN_ITEM
                    // 異動 CHECKIN_ITEM 時, 可能會有已存在 CHECKIN 記錄
                    // 不能刪除舊資料, 不然 CHECKIN_ITEM_ID 會變成新的, 
                    // 會抓不到已存在的 CHECKIN 記錄
                    //base.Delete<CHECKIN_ITEM>(new CHECKIN_ITEM { PROF_ID = check.PROF_ID });

                }

                // 處理 CHECKIN_ITEM 新增
                foreach (var item in Detail.CheckInItems)
                {
                    CHECKIN_ITEM oldWhere = new CHECKIN_ITEM()
                    {
                        CHECKIN_ITEM_ID = item.CHECKIN_ITEM_ID
                    };
                    CHECKIN_ITEM old = null;
                    if (item.CHECKIN_ITEM_ID.HasValue)
                    {
                        old = base.GetRow<CHECKIN_ITEM>(oldWhere);
                    }

                    // 刪除項目
                    if (old != null && "Y".Equals(item.DELETE))
                    {
                        base.Delete<CHECKIN_ITEM>(oldWhere);
                        continue;
                    }

                    if (item.SEQ <= 0)
                        continue;


                    // 檢核 所屬系統報到項目代碼 是否重複存在
                    CHECKIN_ITEM chk = base.GetRow<CHECKIN_ITEM>(
                        new CHECKIN_ITEM()
                        {
                            PROF_ID = Detail.PROF_ID,
                            SYS_ITEM_ID = item.SYS_ITEM_ID
                        } );
                    if(chk != null)
                    {
                        if(old == null || chk.CHECKIN_ITEM_ID != old.CHECKIN_ITEM_ID )
                        {
                            throw new Exception("報到項目第" + item.SEQ + "項, 所屬系統報到項目代碼: " + item.SYS_ITEM_ID + " 在本場次中已存在(重覆)!");
                        }
                    }

                    // 更新資料
                    // 判斷採新增或修改模式 

                    item.PROF_ID = Detail.PROF_ID;
                    try
                    {
                        if (old != null)
                        {
                            // 修改
                            base.Update<CHECKIN_ITEM>(item, oldWhere);
                        }
                        else
                        {
                            // 新增
                            base.Insert<CHECKIN_ITEM>(item);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("報到項目第" + item.SEQ + "項, 寫入資料庫失敗: " + e.Message, e);
                    }
                    
                }

                base.CommitTransaction();
                return rtn;
            }
            catch(Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("SaveN301M: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 報到場次維護-刪除,有異常時直接丟出 Exception.
        /// <para>會回傳受影響的筆數</para>
        /// </summary>
        /// <param name="Detail"></param>
        /// <returns></returns>
        public int DelteN301M(N301MDetailModel Detail)
        {
            NFCPROF check = null;
            if (Detail.PROF_ID.HasValue && Detail.PROF_ID.Value > 0)
            {
                check = base.GetRow<NFCPROF>(new NFCPROF { PROF_ID = Detail.PROF_ID });
            }
            if (check == null)
            {
                throw new Exception("NFCPROF(PROF_ID = " + Detail.PROF_ID + ") 資料不存在!");
            }

            base.BeginTransaction();
            try
            {
                // 刪除舊的 NAMELIST
                base.Delete<NAMELIST>(new NAMELIST { PROF_ID = Detail.PROF_ID });

                // 先刪除舊的 CHECKIN_ITEM
                base.Delete<CHECKIN_ITEM>(new CHECKIN_ITEM { PROF_ID = Detail.PROF_ID });

                // 刪除 NFCPROF
                int rtn = base.Delete<NFCPROF>(new NFCPROF { PROF_ID = Detail.PROF_ID });

                base.CommitTransaction();
                return rtn;
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("DelteN301M: " + ex.Message, ex);
                throw ex;
            }
        }

        #endregion

        #region AM/N302M 報到名單維護

        /// <summary>
        /// 取得當前有效(未過期)的報到場次清單
        /// <para>
        /// 只抓尚未結束的場次(但有包含尚未開始的場次), 
        /// 供維護報到名單之用
        /// </para>
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> QueryN302MProfIdlist()
        {
            return base.QueryForListAll<KeyMapModel>("CLAM.getN302MProfIdList", null);
        }

        /// <summary>
        /// 報到名單維護-查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<N302MGridModel> QueryN302M(N302MFormModel parms)
        {
            return base.QueryForList<N302MGridModel>("CLAM.queryN302M", parms);
        }


        /// <summary>
        /// 取得指定 NAMELIST_ID 的 NAMELIST
        /// </summary>
        /// <param name="namelistId"></param>
        /// <returns></returns>
        public N302MDetailModel GetN302MDetail(long namelistId)
        {
            N302MDetailModel model = null;
            NAMELIST where = new NAMELIST
            {
                NAMELIST_ID = namelistId
            };

            NAMELIST namelist = base.GetRow<NAMELIST>(where);
            if (namelist != null)
            {
                model = new N302MDetailModel(namelist);
            }

            return model;
        }

        /// <summary>
        /// 報到名單維護-新增或修改儲存,依 Detail.IsNew 判斷執行新增或修改,
        /// 有異常時直接丟出 Exception.
        /// <para>新增時會回傳自動編號的NAMELIST_ID值</para>
        /// <para>修改時會回傳受影響的筆數</para>
        /// </summary>
        /// <param name="Detail"></param>
        public int SaveN302M(N302MDetailModel Detail)
        {
            NAMELIST check = null;
            if (Detail.NAMELIST_ID.HasValue && Detail.NAMELIST_ID.Value > 0)
            {
                check = base.GetRow<NAMELIST>(new NAMELIST { NAMELIST_ID = Detail.NAMELIST_ID });
            }

            base.BeginTransaction();
            try
            {
                int rtn = -1;
                if (Detail.IsNew)
                {
                    // 新增儲存

                    if (check != null)
                    {
                        throw new Exception("NAMELIST(NAMELIST_ID = " + Detail.NAMELIST_ID + ") 資料已存在, 不能新增!");
                    }
                    rtn = base.Insert<NAMELIST>(Detail);
                }
                else
                {
                    // 修改儲存
                    if (check == null)
                    {
                        throw new Exception("NAMELIST(NAMELIST_ID = " + Detail.NAMELIST_ID + ") 資料不存在!");
                    }
                    rtn = base.Update<NAMELIST>(Detail, new NAMELIST { NAMELIST_ID = Detail.NAMELIST_ID });
                }

                base.CommitTransaction();
                return rtn;
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("SaveN302M: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// N302M/MakeCard 儲存, 只會更新 NFC_ID 欄位
        /// </summary>
        /// <param name="Detail"></param>
        /// <returns></returns>
        public int SaveN302MMakeCard(N302MDetailModel Detail)
        {
            NAMELIST where = new NAMELIST { NAMELIST_ID = Detail.NAMELIST_ID };
            NAMELIST check = null;

            if (Detail.NAMELIST_ID.HasValue && Detail.NAMELIST_ID.Value > 0)
            {
                // 檢查 NAMELIST 資料是否存在
                check = base.GetRow<NAMELIST>(where);
                if (check == null)
                {
                    throw new Exception("NAMELIST(NAMELIST_ID = " + Detail.NAMELIST_ID + ") 資料不存在!");
                }

                // 檢查 NFC_ID 在相同報到場次中是否已制卡綁定過
                // ==> 存在任一筆 PROF_ID, NFC_ID 相同, 但 NAMELIST_ID 不同的資料
                NAMELIST check2 = base.GetRow<NAMELIST>(
                    new NAMELIST {
                        PROF_ID = check.PROF_ID,
                        NFC_ID = Detail.NFC_ID
                    });
                if(check2 != null && check2.NAMELIST_ID != Detail.NAMELIST_ID)
                {
                    throw new Exception(string.Format("NFC卡號: {0} 在本場次中已制卡過(已存在)!", Detail.NFC_ID));
                }
            }
            else
            {
                throw new ArgumentNullException("NAMELIST_ID 不能為 null");
            }

            base.BeginTransaction();
            try
            {
                // 修改儲存
                NAMELIST data = new NAMELIST
                {
                    NAMELIST_ID = Detail.NAMELIST_ID,
                    NFC_ID = Detail.NFC_ID
                };
                int rtn = base.Update<NAMELIST>(data, where);

                base.CommitTransaction();
                return rtn;
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("SaveN302M: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 報到名單維護-刪除,有異常時直接丟出 Exception.
        /// <para>會回傳受影響的筆數</para>
        /// </summary>
        /// <param name="Detail"></param>
        /// <returns></returns>
        public int DelteN302M(N302MDetailModel Detail)
        {
            NAMELIST check = null;
            if (Detail.NAMELIST_ID.HasValue && Detail.NAMELIST_ID.Value > 0)
            {
                check = base.GetRow<NAMELIST>(new NAMELIST { NAMELIST_ID = Detail.NAMELIST_ID });
            }
            if (check == null)
            {
                throw new Exception("NAMELIST(NAMELIST_ID = " + Detail.NAMELIST_ID + ") 資料不存在!");
            }

            base.BeginTransaction();
            try
            {
                // 刪除 NAMELIST
                int rtn = base.Delete<NAMELIST>(new NAMELIST { NAMELIST_ID = Detail.NAMELIST_ID });

                base.CommitTransaction();
                return rtn;
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                LOG.Error("DelteN302M: " + ex.Message, ex);
                throw ex;
            }
        }

        #endregion

        #region AM/N303M NFC報到作業

        /// <summary>
        /// 取得當前有效(在報到起迄日期內)的報到場次清單
        /// <para>
        /// 供 NFC 報到作業之用
        /// </para>
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> QueryN303MProfIdlist()
        {
            return base.QueryForListAll<KeyMapModel>("CLAM.getN303MProfIdList", null);
        }

        /// <summary>
        /// 報到項目 選項清單來源(依報到場次過濾)
        /// </summary>
        /// <param name="PROF_ID"></param>
        /// <returns></returns>
        public IList<KeyMapModel> QueryN303MCheckinItemIdlist(string PROF_ID)
        {
            Hashtable parms = new Hashtable();
            parms["PROF_ID"] = PROF_ID;
            return base.QueryForListAll<KeyMapModel>("CLAM.getN303MCheckinItemIdList", parms);
        }

        /// <summary>
        /// NFC報到作業 - 報到場次/項目 資料查詢
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public N303MProfModel QueryN303MProf(N303MFormModel parms)
        {
            N303MProfModel profModel = null;

            long profId, chekinItemId;
            if(!long.TryParse(parms.PROF_ID, out profId))
            {
                throw new ArgumentException("PROF_ID '" + parms.PROF_ID + "' 不是有效的數值");
            }
            if (!long.TryParse(parms.CHECKIN_ITEM_ID, out chekinItemId))
            {
                throw new ArgumentException("CHECKIN_ITEM_ID '" + parms.CHECKIN_ITEM_ID + "' 不是有效的數值");
            }

            NFCPROF prof = base.GetRow<NFCPROF>(
                new NFCPROF { PROF_ID = profId }
                );
            if (prof != null)
            {
                prof =
                profModel = new N303MProfModel(prof);

                profModel.CheckinItem = base.GetRow<CHECKIN_ITEM>(
                    new CHECKIN_ITEM { CHECKIN_ITEM_ID = chekinItemId }
                    );

                if(profModel.CheckinItem == null)
                {
                    throw new Exception("找不到報到項目資料: CHECKIN_ITEM_ID=" + chekinItemId);
                }
            }
            else
            {
                throw new Exception("找不到場次資料: PROF_ID=" + profId);
            }

            return profModel;
        }

        /// <summary>
        /// NFC報到作業 - 報到名單查詢
        /// </summary>
        /// <param name="parms">查詢條件 Form Model</param>
        /// <returns></returns>
        public IList<N303MGridModel> QueryN303M(N303MFormModel parms)
        {
            return base.QueryForListAll<N303MGridModel>("CLAM.queryN303M", parms);
        }


        /// <summary>
        /// 取得指定 NAMELIST_ID 的 NAMELIST
        /// </summary>
        /// <param name="namelistId"></param>
        /// <returns></returns>
        public N303MDetailModel GetN303MDetail(long namelistId)
        {
            N303MDetailModel model = null;
            NAMELIST where = new NAMELIST
            {
                NAMELIST_ID = namelistId
            };

            NAMELIST namelist = base.GetRow<NAMELIST>(where);
            if (namelist != null)
            {
                model = new N303MDetailModel(namelist);
            }

            return model;
        }

        /// <summary>
        /// N303M/Checkin NFC報到 儲存
        /// </summary>
        /// <param name="Detail"></param>
        /// <returns></returns>
        public int SaveN303MNfcCheckin(N303MDetailModel Detail)
        {
            if (!Detail.PROF_ID.HasValue || Detail.PROF_ID.Value == 0)
            {
                throw new ArgumentNullException("PROF_ID 不能為 null");
            }
            if (string.IsNullOrEmpty(Detail.NFC_ID))
            {
                throw new ArgumentNullException("NFC_ID 不能為 null");
            }
            if (!Detail.CHECKIN_ITEM_ID.HasValue || Detail.CHECKIN_ITEM_ID.Value == 0)
            {
                throw new ArgumentNullException("CHECKIN_ITEM_ID 不能為 null");
            }

            // 以 PROF_ID + NFC_ID 檢查 NAMELIST 資料是否存在
            NAMELIST where = new NAMELIST {
                PROF_ID = Detail.PROF_ID,
                NFC_ID = Detail.NFC_ID
            };
            
            NAMELIST check = base.GetRow<NAMELIST>(where);
            if (check == null)
            {
                throw new Exception(string.Format("這個NFC卡號【{0}】在本場次中找不到已制卡的名單資料!", Detail.NFC_ID));
            }
            /*將檢核的名單資料寫回Deatil, 前端 View 需要*/
            Detail.NAMELIST_ID = check.NAMELIST_ID;
            Detail.IDNO = check.IDNO;
            Detail.NAME = check.NAME;

            // 檢查 報到場次/報到項目 中是否已報到過
            // ==> CHECKIN 存在任一筆 NAMELIST_ID, CHECKIN_ITEM_ID 相同
            Models.Entities.CHECKIN check2 = base.GetRow<Models.Entities.CHECKIN>(
                new Models.Entities.CHECKIN
                {
                    CHECKIN_ITEM_ID = Detail.CHECKIN_ITEM_ID,
                    NAMELIST_ID = check.NAMELIST_ID
                });
            if (check2 != null)
            {
                throw new Exception(string.Format("【{0} {1}】 已報到過(重覆報到)!", check.IDNO , check.NAME));
            }

            if (string.IsNullOrEmpty(Detail.CHECKIN_TYPE))
            {
                // 預設報到類型: A.NFC報到
                Detail.CHECKIN_TYPE = "A";
            }

            // 新增一筆報到記錄
            Detail.DATETIME = HelperUtil.DateTimeToLongString(DateTime.Now);
            Models.Entities.CHECKIN checkin = new Models.Entities.CHECKIN
            {
                CHECKIN_TYPE = Detail.CHECKIN_TYPE,
                CHECKIN_ITEM_ID = Detail.CHECKIN_ITEM_ID,
                NAMELIST_ID = check.NAMELIST_ID,
                DATETIME = Detail.DATETIME
            };
            
            try
            {
                return base.Insert<Models.Entities.CHECKIN>(checkin);
            }
            catch (Exception ex)
            {
                LOG.Error("SaveN303MCheckin: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// N303M/ManualCheckin 人工報到 儲存
        /// </summary>
        /// <param name="Detail"></param>
        /// <returns></returns>
        public int SaveN303MManualCheckin(N303MDetailModel Detail)
        {
            if (!Detail.PROF_ID.HasValue || Detail.PROF_ID.Value == 0)
            {
                throw new ArgumentNullException("PROF_ID 不能為 null");
            }
            if (!Detail.NAMELIST_ID.HasValue || Detail.NAMELIST_ID.Value == 0)
            {
                throw new ArgumentNullException("NAMELIST_ID 不能為 null");
            }
            if (!Detail.CHECKIN_ITEM_ID.HasValue || Detail.CHECKIN_ITEM_ID.Value == 0)
            {
                throw new ArgumentNullException("CHECKIN_ITEM_ID 不能為 null");
            }

            // 以 PROF_ID + NAMELIST_ID 檢查 NAMELIST 資料是否存在
            NAMELIST where = new NAMELIST
            {
                PROF_ID = Detail.PROF_ID,
                NAMELIST_ID = Detail.NAMELIST_ID
            };

            NAMELIST check = base.GetRow<NAMELIST>(where);
            if (check == null)
            {
                throw new Exception(string.Format("在本場次中找不到名單資料【NAMELIST_ID={0}】!", Detail.NAMELIST_ID));
            }
            /*將檢核的名單資料寫回Deatil, 前端 View 需要*/
            Detail.IDNO = check.IDNO;
            Detail.NAME = check.NAME;

            // 檢查 報到場次/報到項目 中是否已報到過
            // ==> CHECKIN 存在任一筆 NAMELIST_ID, CHECKIN_ITEM_ID 相同
            Models.Entities.CHECKIN where2 = new Models.Entities.CHECKIN
            {
                CHECKIN_ITEM_ID = Detail.CHECKIN_ITEM_ID,
                NAMELIST_ID = Detail.NAMELIST_ID
            };
            Models.Entities.CHECKIN check2 = base.GetRow<Models.Entities.CHECKIN>(where2);

            if (check2 != null)
            {
                throw new Exception(string.Format("【{0} {1}】 已報到過(重覆報到)!", check.IDNO, check.NAME));
            }

            if (string.IsNullOrEmpty(Detail.CHECKIN_TYPE))
            {
                // 預設報到類型: B.人工報到
                Detail.CHECKIN_TYPE = "B";
            }

            // 新增一筆報到記錄
            Detail.DATETIME = HelperUtil.DateTimeToLongString(DateTime.Now);
            Models.Entities.CHECKIN checkin = new Models.Entities.CHECKIN
            {
                CHECKIN_TYPE = Detail.CHECKIN_TYPE,
                CHECKIN_ITEM_ID = Detail.CHECKIN_ITEM_ID,
                NAMELIST_ID = Detail.NAMELIST_ID,
                DATETIME = Detail.DATETIME
            };

            try
            {
                return base.Insert<Models.Entities.CHECKIN>(checkin);
            }
            catch (Exception ex)
            {
                LOG.Error("SaveN303MCheckin: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// N303M/CancelCheckin NFC取消報到
        /// 有異常時直接丟出 Exception.
        /// <para>會回傳受影響的筆數</para>
        /// </summary>
        /// <param name="Detail"></param>
        /// <returns></returns>
        public int SaveN303MCalcelCheckin(N303MDetailModel Detail)
        {
            if (!Detail.NAMELIST_ID.HasValue || Detail.NAMELIST_ID.Value == 0)
            {
                throw new ArgumentNullException("NAMELIST_ID 不能為 null");
            }
            if (!Detail.CHECKIN_ITEM_ID.HasValue || Detail.CHECKIN_ITEM_ID.Value == 0)
            {
                throw new ArgumentNullException("CHECKIN_ITEM_ID 不能為 null");
            }

            // 檢查 NAMELIST 是否存在
            NAMELIST namelist = base.GetRow<NAMELIST>(new NAMELIST { NAMELIST_ID = Detail.NAMELIST_ID});
            if (namelist == null)
            {
                throw new Exception(string.Format("在本場次中找不到名單資料【NAMELIST_ID={0}】!", Detail.NAMELIST_ID));
            }
            /*將檢核的名單資料寫回Deatil, 前端 View 需要*/
            Detail.IDNO = namelist.IDNO;
            Detail.NAME = namelist.NAME;

            // 檢查 報到場次/報到項目 中是否已報到過
            // ==> CHECKIN 存在任一筆 NAMELIST_ID, CHECKIN_ITEM_ID 相同
            Models.Entities.CHECKIN where = new Models.Entities.CHECKIN
            {
                CHECKIN_ITEM_ID = Detail.CHECKIN_ITEM_ID,
                NAMELIST_ID = Detail.NAMELIST_ID
            };
            Models.Entities.CHECKIN check = base.GetRow<Models.Entities.CHECKIN>(where);
            if (check == null)
            {
                throw new Exception(string.Format("找不到已報到的資料(NAMELIST_ID={0})!", Detail.NAMELIST_ID));
            }

            try
            {
                // 刪除 CHECKIN
                return base.Delete<Models.Entities.CHECKIN>(where);
            }
            catch (Exception ex)
            {
                LOG.Error("SaveN303MCalcelCheckin: " + ex.Message, ex);
                throw ex;
            }
        }

        #endregion

        #region AM/N304F NFC報到記錄查詢

        /// <summary>
        /// 取得指定系統別的報到場次清單
        /// <para>
        /// 供 NFC 報到記錄查詢之用
        /// </para>
        /// </summary>
        /// <returns></returns>
        public IList<KeyMapModel> QueryN304RProfIdlist(string SYS_ID)
        {
            Hashtable parms = new Hashtable();
            parms["SYS_ID"] = SYS_ID;
            return base.QueryForListAll<KeyMapModel>("CLAM.getN304RProfIdList", parms);
        }

        /// <summary>
        /// 報到項目 選項清單來源(依 所屬系統場次代碼 過濾)
        /// </summary>
        /// <param name="SYS_PROF_ID"></param>
        /// <returns></returns>
        public IList<KeyMapModel> QueryN304RCheckinItemIdlist(string SYS_PROF_ID)
        {
            Hashtable parms = new Hashtable();
            parms["SYS_PROF_ID"] = SYS_PROF_ID;
            return base.QueryForListAll<KeyMapModel>("CLAM.getN304RCheckinItemIdList", parms);
        }
        #endregion
    }
}