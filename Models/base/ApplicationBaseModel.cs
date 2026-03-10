using System;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 這個抽象類提供一組 GetApplicationVar/SetApplicationVar Methods,
    /// 用來儲存全域(跨不同 Session)並且經常會用到的共用資料, 
    /// 並可以設定共用資料的 expired 時間.
    /// </summary>
    public abstract class ApplicationBaseModel
    {
        #region HttpApplicationState Get/Set Helper method (private scope)

        private const string PREFIX_EXPIRE = "_EXPIRE_"; 

        /// <summary>
        /// 以指定的 key 到 HttpApplicationState 中取回儲存的 Object value,
        /// 若指定的 key 不存在或 value 已過期則回傳 null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected Object GetApplicationVar(string key)
        {
            HttpApplicationState application = HttpContext.Current.Application;
            if (key == null)
            {
                throw new ArgumentNullException("GetApplicationVar: key is null");
            }
            else
            {
                object value = application[key];
                object expire = application[PREFIX_EXPIRE + key];
                if(expire != null && expire is DateTime)
                {
                    // 有設定 expire 時間
                    DateTime now = DateTime.Now;
                    if(now.CompareTo((DateTime)expire) >= 0)
                    {
                        // ApplicationVar 已過期, 直接丟棄
                        value = null;
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// 將 Object value 以指定的 key 儲存到 HttpApplicationState 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void SetApplicationVar(string key, Object value, DateTime? expire = null)
        {
            HttpApplicationState application = HttpContext.Current.Application;
            if (key == null)
            {
                throw new ArgumentNullException("SetApplicationVar: key is null");
            }
            else
            {
                application.Lock();
                application[key] = value;
                if(expire != null)
                {
                    application[PREFIX_EXPIRE + key] = expire.Value;
                }
                application.UnLock();
            }
        }
        #endregion


        /*
        private static readonly string LEFT_FUNCTION_RELATION = "SYS.LEFT_FUNCTION_RELATION";

        /// <summary>
        /// 取得 左選單功能顯示對應資料, 若未曾載入過, 則自動透過 DAO 載入.
        /// </summary>
        /// <returns></returns>
        public static IList<LeftFunctionRelation> GetLeftFunctionRelation()
        {
            IList<LeftFunctionRelation> list = null;
            Object value = GetApplicationVar(LEFT_FUNCTION_RELATION);
            if (value == null)
            {
                list = (new AccountDAO()).GetAllFunctionRelationList();
                SetApplicationVar(LEFT_FUNCTION_RELATION, list);
            }
            else
            {
                list = (IList<LeftFunctionRelation>)value;
            }

            return list;
        }

        /// <summary>
        /// 清除暫存的 左選單功能顯示對應資料, 下次讀取時再由資料庫從新載入
        /// </summary>
        public static void ClearLeftFunctionRelation()
        {
            SetApplicationVar(LEFT_FUNCTION_RELATION, null);
        }
        */
    }
}