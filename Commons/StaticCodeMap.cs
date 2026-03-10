using System.Collections.Generic;
using Turbo.Commons;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 系統代碼及表格名稱列舉
    /// </summary>
    public partial class StaticCodeMap
    {
        /// <summary>
        /// 代碼表類別列舉清單, 叫用 KeyMapDAO.GetCodeMapList() 所需的參數
        /// </summary>
        public class CodeMap : CodeMapType
        {
            #region 私有(隱藏) CodeMap 建構式

            private CodeMap(string codeName) : base(codeName)
            { }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="codeName"></param>
            /// <param name="sqlStatementId"></param>
            private CodeMap(string codeName, string sqlStatementId) : base(codeName, sqlStatementId)
            { }

            #endregion

            /// <summary>
            /// 計畫別
            /// </summary>
            public static CodeMapType PLAN = new CodeMapType("PLAN", "KeyMap.getPlan");

            /// <summary>
            /// 其他政府單位課程查詢-訓練單位代碼
            /// </summary>
            public static CodeMapType GOVORGID = new CodeMapType("GOVORGID", "KeyMap.queryGovernIDOrgList");

            /// <summary>
            /// 訊息類別
            /// </summary>
            public static CodeMapType NEWSTYPE = new CodeMapType("NEWSTYPE", "KeyMap.getNewsType");

            /// <summary>
            /// 訊息類別(全)
            /// </summary>
            public static CodeMapType FAQTYPE = new CodeMapType("FAQTYPE", "KeyMap.getFaqType");

            /// <summary>
            /// 訊息類別(使用中)
            /// </summary>
            public static CodeMapType FAQTYPE_USED = new CodeMapType("FAQTYPE_USED", "KeyMap.getFaqTypeUsed");

            /// <summary>
            /// 是否（'Y': 是，'N': 否）
            /// </summary>
            public static CodeMapType YESNO = new CodeMapType("YESNO", "KeyMap.getYesNoList");

            /// <summary>
            /// 性別（'M': 男，'F': 女）
            /// </summary>
            public static CodeMapType SEX = new CodeMapType("SEX", "KeyMap.getSexList");

            /// <summary>
            /// 功能類別
            /// </summary>
            public static CodeMapType FUNCCLASS = new CodeMapType("FUNCCLASS", "KeyMap.getFuncClass");

            /// <summary>
            /// 是否啟用-查詢用 ('':不拘， 'Y':啟用，'N': 停用)
            /// </summary>
            public static readonly Dictionary<string, string> USED_TYPE_QRY = new Dictionary<string, string>
            {
                {"", "不拘"}, {"Y", "啟用"}, {"N", "停用"}
            };

            /// <summary>
            /// 是否啟用-編輯用 ('Y':啟用，'N': 停用)
            /// </summary>
            public static readonly Dictionary<string, string> USED_TYPE = new Dictionary<string, string>
            {
                {"Y", "啟用"}, {"N", "停用"}
            };

            // <summary> 功能類型 </summary> public static readonly Dictionary<string, string> FUNC_TYPE = new Dictionary<string, string>
            //{ {"0", "一般"}, {"1", "會員"} };
            // <summary>轄區</summary>            //public static CodeMapType DIST = new CodeMapType("DIST", "KeyMap.getDIST");
            // <summary>群組</summary>            //public static CodeMapType GROUP = new CodeMapType("GROUP", "KeyMap.getGROUP");
            // <summary> [網站會員] 帳號類型-查詢用 </summary> public static readonly Dictionary<string, string> MEM_TYPE_QRY = new Dictionary<string, string>
            //{ {"1", "學員"}, {"2", "事業單位"}, {"3", "學校"} };

            /// <summary> 郵遞區號種類 POSTTYPE1 </summary>
            public static CodeMapType POSTTYPE1 = new CodeMapType("POSTTYPE1",
                new Dictionary<string, string> {
                    { "3", "3+3郵遞區號" },
                    { "2", "3+2郵遞區號" }
                });


        }
    }
}