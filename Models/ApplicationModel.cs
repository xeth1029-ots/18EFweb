using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.DataLayers;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 提供跨Session共用資料的存取類
    /// </summary>
    public class ApplicationModel: ApplicationBaseModel
    {
        #region Private Methods
        private static object _lock = new object();
        private static ApplicationModel _instance = null;

        private ApplicationModel()
        {

        }


        private static ApplicationModel GetInstance()
        {
            lock (_lock)
            {
                if(_instance == null)
                {
                    _instance = new ApplicationModel();
                }
                return _instance;
            }
        }
        #endregion
        

        /// <summary>
        /// 取得系統已啟用的全部功能清單模組
        /// </summary>
        /// <returns></returns>
        //public static IList<eYVTRmngRoleFunc> getFuncModules()
        //{
        //    const string _KEY = "ClamFuncModules";
        //    ApplicationModel model = GetInstance();
        //    object value = model.GetApplicationVar(_KEY);

        //    if (value != null && value is IList<eYVTRmngRoleFunc>)
        //    {
        //        // 已存在, 直接返回
        //        return (IList<eYVTRmngRoleFunc>)value;
        //    }
        //    else
        //    {
        //        // 不存在或過期, 從DB中載入
        //        eYVTRmngDAO dao = new eYVTRmngDAO();
        //        IList<eYVTRmngRoleFunc> list = dao.geteYVTRmngFuncModules();

        //        // 將 list 儲存至 ApplictionModel 中
        //        // 並設定有效時間至系統時間當天的 23:59:59 
        //        // (也就是隔天的 00:00:00 失效)
        //        DateTime now = DateTime.Now;
        //        now = now.AddDays(1);
        //        DateTime expire = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        //        model.SetApplicationVar(_KEY, list, expire);
        //        return list;
        //    }
        //}

        /// <summary>
        /// 取得系統已啟用的全部功能清單(已排序)
        /// </summary>
        /// <returns></returns>
        //public static IList<TblE_FUN> GeteYVTRmngFuncsAll()
        //{
        //    const string _KEY = "TblE_FUN";
        //    ApplicationModel model = GetInstance();
        //    object value = model.GetApplicationVar(_KEY);

        //    if (value != null && value is IList<TblE_FUN>)
        //    {
        //        // 已存在, 直接返回
        //        return (IList<TblE_FUN>)value;
        //    }
        //    else
        //    {
        //        // 不存在或過期, 從DB中載入
        //        eYVTRmngDAO dao = new eYVTRmngDAO();
        //        IList<TblE_FUN> list = dao.GeteYVTRmngFuncsAll();

        //        // 將 list 儲存至 ApplictionModel 中
        //        // 並設定有效時間至系統時間當天的 23:59:59 
        //        // (也就是隔天的 00:00:00 失效)
        //        DateTime now = DateTime.Now;
        //        now = now.AddDays(1);
        //        DateTime expire = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        //        model.SetApplicationVar(_KEY, list, expire);
        //        return list;
        //    }
        //}

    }
}