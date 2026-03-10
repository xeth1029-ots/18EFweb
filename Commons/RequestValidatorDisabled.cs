using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Util;
using log4net;

namespace WDAIIP.WEB.Commons
{
    class RequestValidatorDisabled : System.Web.Util.RequestValidator
    {
        private static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override bool IsValidRequestString(System.Web.HttpContext context, string value, System.Web.Util.RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
        {
            bool flag_ok = ValidRequestlog(context, value, requestValidationSource, collectionKey, out validationFailureIndex);
            validationFailureIndex = -1;
            return flag_ok;
        }

        bool ValidRequestlog(System.Web.HttpContext context, string value, System.Web.Util.RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
        {
            bool flag_ok = false;
            validationFailureIndex = -1;
            if (requestValidationSource == RequestValidationSource.Form)
            {
                //單一簽入功能 直接通過
                bool fg_minfo = (!string.IsNullOrEmpty(collectionKey) && collectionKey.Equals("Minfo")) ? true : false;
                bool fg_member = (!string.IsNullOrEmpty(value) && value.Contains("MEMBER_INFO")) ? true : false;
                if (fg_minfo && fg_member) { return true; }
            }

            try
            {
                switch (requestValidationSource)
                {
                    case RequestValidationSource.QueryString:
                        break;
                    case RequestValidationSource.Form:
                        //對查詢字串進行驗證
                        //檢查是否包含<,當然也可以檢查其他特殊符號,或者忽略某些特殊符號.
                        //直接轉到自定義的錯誤頁面.
                        //if (value.Contains("||"))
                        //{
                        //    context.Response.Redirect("~/Error.aspx", true);
                        //    return false;
                        //}
                        break;
                    case RequestValidationSource.Cookies:
                        break;
                    case RequestValidationSource.Files:
                        break;
                    case RequestValidationSource.RawUrl:
                        break;
                    case RequestValidationSource.Path:
                        break;
                    case RequestValidationSource.PathInfo:
                        break;
                    case RequestValidationSource.Headers:
                        break;
                    default:
                        break;
                }
                //return base.IsValidRequestString(context, value, requestValidationSource, collectionKey, out validationFailureIndex); //true;
                flag_ok = base.IsValidRequestString(context, value, requestValidationSource, collectionKey, out validationFailureIndex);
                if (!flag_ok)
                {
                    string s_msg1 = "";
                    s_msg1 = string.Format("##IsValidRequestString: HTTP要求資料類型={0}, collectionKey={1}, value={2}", requestValidationSource, collectionKey, value);
                    logger.Debug(s_msg1);
                    s_msg1 = string.Format("##IsValidRequestString: validationFailureIndex={0}", validationFailureIndex);
                    logger.Debug(s_msg1);
                }
            }
            catch (Exception ex)
            {
                string s_error1 = "";
                s_error1 = string.Format("##IsValidRequestString.Exception: 驗證失敗：{0}", ex.Message);
                logger.Error(s_error1, ex);
                //throw;
            }
            return flag_ok;
        }

    }
}