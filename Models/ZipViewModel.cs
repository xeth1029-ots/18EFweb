using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;
//using log4net;

namespace WDAIIP.WEB.Models
{
    #region ZipViewModel
    public class ZipViewModel
    {
        //protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ZipViewModel()
        {
            this.Form = new ZipFormModel();
        }

        /// <summary> 查詢條件 </summary>
        public ZipFormModel Form { get; set; }

        /// <summary> 查詢結果清單 </summary>
        public IList<ZipGridModel> Grid { get; set; }

        /// <summary> 縣市 清單來源 </summary>
        public IList<SelectListItem> CTID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCityCodeList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary> POSTTYPE1_list </summary>
        public IList<SelectListItem> POSTTYPE1_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel> {
                                { new KeyMapModel {CODE="3",TEXT="3+3郵遞區號" } },
                                { new KeyMapModel {CODE="2",TEXT="3+2郵遞區號" } }
                            };
                //MyKeyMapDAO dao = new MyKeyMapDAO();
                //IList<KeyMapModel> list = dao.GetCodeMapList(WDAIIP.WEB.Commons.StaticCodeMap.CodeMap.POSTTYPE1);
                //LOG.Debug("##POSTTYPE1_list");
                //foreach (var km in list) { LOG.DebugFormat("km.CODE: {0},km.TEXT: {1}", km.CODE, km.TEXT); }
                //foreach (var km in list) { if (km.CODE.Equals("3")) { km.Selected = true; break; } }
                return MyCommonUtil.ConvertSelItems(list); // return MyCommonUtil.ConvertSelItems(list, false, "3");
            }
        }
        /// <summary> 鄉鎮市區 清單來源 </summary>
        /// <returns></returns>
        public IList<SelectListItem> ZIPCODE_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                if (!string.IsNullOrEmpty(this.Form.CTID)) list = dao.GetCountyZipList(this.Form.CTID);
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
    }
    #endregion

    #region ZipFormModel
    /// <summary> 查詢 </summary>
    public class ZipFormModel : PagingResultsViewModel
    {

        /// <summary> 郵遞區號種類 POSTTYPE1(for TextBox) </summary>
        public string POSTTYPE1 { get; set; }

        /// <summary> 郵遞區號(for TextBox) </summary>
        public string CTIDZIP { get; set; }

        /// <summary> 縣市代碼 </summary>
        public string CTID { get; set; }

        /// <summary> 鄉鎮市區代碼(for DropDownList) </summary>
        public string ZIPCODE { get; set; }

        /// <summary> 鄉鎮市區名稱(for DropDownList) </summary>
        public string ZIPNAME { get; set; }

        /// <summary>
        /// 街道名稱
        /// </summary>
        public string ROAD { get; set; }

        /// <summary>
        /// 選擇後 javascript call back funtion name
        /// </summary>
        public string onclickHandler { get; set; }
    }
    #endregion

    #region ZipGridModel
    /// <summary> 結果 </summary>
    public class ZipGridModel
    {
        /// <summary> 縣市代碼 </summary>
        public Int64? CTID { get; set; }

        /// <summary> 郵遞區號前三碼 </summary>
        public string ZIPCODE { get; set; }

        /// <summary> 郵遞區號後兩碼 </summary>
        public string ZIPCODE2 { get; set; }

        /// <summary> 郵遞區號6碼 </summary>
        public string ZIPCODE6 { get; set; }

        /// <summary> 縣市名稱 </summary>
        public string CTNAME { get; set; }

        /// <summary> 鄉鎮市區名稱 </summary>
        public string ZIPNAME { get; set; }

        /// <summary> 街道名稱 </summary>
        public string ROAD { get; set; }

        /// <summary> 說明 </summary>
        public string NOTE { get; set; }
    }
    #endregion
}