using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 系統使用者角色清單定義, 對應於 TB_KEY_MGR(KEY_GROUP='ACCOUNTLEVEL')
    /// </summary>
    public class SystemRoles
    {
        /// <summary>
        /// 系統管理員
        /// </summary>
        public static string ROLE_01_系統管理員 { get { return "01"; } }

        /// <summary>
        /// 勞動力發展署督導
        /// </summary>
        public static string ROLE_02_勞動力發展署督導 { get { return "02"; } }

        /// <summary>
        /// 專業督導
        /// </summary>
        public static string ROLE_03_專業督導 { get { return "03"; } }

        /// <summary>
        /// 縣市職業重建個管員
        /// </summary>
        public static string ROLE_11_縣市職業重建個管員 { get { return "11"; } }

        /// <summary>
        /// 分署督導
        /// </summary>
        public static string ROLE_12_分署督導 { get { return "12"; } }

        /// <summary>
        /// 支持機構督導
        /// </summary>
        public static string ROLE_13_支持機構督導 { get { return "13"; } }

        /// <summary>
        /// 職評機構督導
        /// </summary>
        public static string ROLE_14_職評機構督導 { get { return "14"; } }

        /// <summary>
        /// 居家機構督導
        /// </summary>
        public static string ROLE_15_居家機構督導 { get { return "15"; } }

        /// <summary>
        /// 庇護機構督導
        /// </summary>
        public static string ROLE_16_庇護機構督導 { get { return "16"; } }

        /// <summary>
        /// 縣市督導
        /// </summary>
        public static string ROLE_17_縣市督導 { get { return "17"; } }

        /// <summary>
        /// 就業中心督導
        /// </summary>
        public static string ROLE_18_就業中心督導 { get { return "18"; } }

        /// <summary>
        /// 職評資源網人員
        /// </summary>
        public static string ROLE_19_職評資源網人員 { get { return "19"; } }

        /// <summary>
        /// 專業督導
        /// </summary>
        public static string ROLE_20_專業督導 { get { return "20"; } }

        /// <summary>
        /// 縣市就服站就服員
        /// </summary>
        public static string ROLE_21_縣市就服站就服員 { get { return "21"; } }

        /// <summary>
        /// 就業中心就服員
        /// </summary>
        public static string ROLE_22_就業中心就服員 { get { return "22"; } }

        /// <summary>
        /// 支持機構就服員
        /// </summary>
        public static string ROLE_23_支持機構就服員 { get { return "23"; } }

        /// <summary>
        /// 職評機構人員
        /// </summary>
        public static string ROLE_24_職評機構人員 { get { return "24"; } }

        /// <summary>
        /// 居家機構就促員
        /// </summary>
        public static string ROLE_25_居家機構就促員 { get { return "25"; } }

        /// <summary>
        /// 庇護機構就服員
        /// </summary>
        public static string ROLE_26_庇護機構就服員 { get { return "26"; } }

        /// <summary>
        /// 業務承辦人
        /// </summary>
        public static string ROLE_27_業務承辦人 { get { return "27"; } }

        /// <summary>
        /// 庇護見習督導
        /// </summary>
        public static string ROLE_28_庇護見習督導 { get { return "28"; } }

        /// <summary>
        /// 庇護見習就服員
        /// </summary>
        public static string ROLE_29_庇護見習就服員 { get { return "29"; } }

        /// <summary>
        /// 穩定就業承辦人
        /// </summary>
        public static string ROLE_30_穩定就業承辦人 { get { return "30"; } }

        /// <summary>
        /// 職訓機構督導
        /// </summary>
        public static string ROLE_31_職訓機構督導 { get { return "31"; } }

        /// <summary>
        /// 職訓機構輔導員
        /// </summary>
        public static string ROLE_32_職訓機構輔導員 { get { return "32"; } }

        /// <summary>
        /// 職訓機構承辦人
        /// </summary>
        public static string ROLE_33_職訓機構承辦人 { get { return "33"; } }

        /// <summary>
        /// 職場紮根督導
        /// </summary>
        public static string ROLE_34_職場紮根督導 { get { return "34"; } }

        /// <summary>
        /// 職場紮根就服員
        /// </summary>
        public static string ROLE_35_職場紮根就服員 { get { return "35"; } }

        /// <summary>
        /// 職訓人員
        /// </summary>
        public static string ROLE_36_職訓人員 { get { return "36"; } }

        /// <summary>
        /// 視障按摩輔導員
        /// </summary>
        public static string ROLE_37_視障按摩輔導員 { get { return "37"; } }


#region 角色群組判斷用 function

        /// <summary>
        /// 判斷角色是否為系統管理人員, 包括: 01_系統管理員, 02_勞動力發展署督導
        /// </summary>
        /// <param name="accountLevel"></param>
        /// <returns></returns>
        public static bool IsSystemAdminRole(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_01_系統管理員
                || accountLevel == SystemRoles.ROLE_02_勞動力發展署督導);
        }

        /// <summary>
        /// 判斷角色是否為支持性機構人員, 包括: 13_支持機構督導, 23_支持機構就服員
        /// </summary>
        /// <param name="accountLevel"></param>
        /// <returns></returns>
        public static bool IsSupportOrgRole(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_13_支持機構督導
                    || accountLevel == SystemRoles.ROLE_23_支持機構就服員);
        }

        /// <summary>
        /// 判斷角色是否為職訓機構人員, 包括: 31_職訓機構督導, 32_職訓機構輔導員, 33_職訓機構承辦人
        /// </summary>
        /// <param name="accountLevel"></param>
        /// <returns></returns>
        public static bool Is職訓機構人員(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_31_職訓機構督導
                    || accountLevel == SystemRoles.ROLE_32_職訓機構輔導員
                    || accountLevel == SystemRoles.ROLE_33_職訓機構承辦人);
        }

        /// <summary>
        /// 判斷角色是否為職場紮根相關人員, 包括: 34_職場紮根督導, 35_職場紮根就服員
        /// </summary>
        /// <param name="accountLevel"></param>
        /// <returns></returns>
        public static bool Is職場紮根相關人員(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_34_職場紮根督導
                    || accountLevel == SystemRoles.ROLE_35_職場紮根就服員);
        }

        /// <summary>
        /// 判斷角色是否為居家就服機構人員, 包括: 15_居家機構督導, 25_居家機構就促員
        /// </summary>
        public static bool IsTelecommutingServRole(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_15_居家機構督導
                    || accountLevel == SystemRoles.ROLE_25_居家機構就促員);
        }

        /// <summary>
        /// 判斷角色是否為專業督導人員, 包括: 03_專業督導, 20_專業督導
        /// </summary>
        public static bool IsSupervisorRole(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_03_專業督導
                || accountLevel == SystemRoles.ROLE_20_專業督導);
        }

        /// <summary>
        /// 判斷角色是否為專業督導人員, 包括:
        /// 03_專業督導, 12_分署督導, 13_支持機構督導, 15_居家機構督導
        /// 16_庇護機構督導, 17_縣市督導, 18_就業中心督導, 20_專業督導
        /// </summary>
        public static bool Is督導(string accountLevel)
        {
            return (accountLevel == SystemRoles.ROLE_03_專業督導
                || accountLevel == SystemRoles.ROLE_12_分署督導
                || accountLevel == SystemRoles.ROLE_13_支持機構督導
                || accountLevel == SystemRoles.ROLE_15_居家機構督導
                || accountLevel == SystemRoles.ROLE_16_庇護機構督導
                || accountLevel == SystemRoles.ROLE_17_縣市督導
                || accountLevel == SystemRoles.ROLE_18_就業中心督導
                || accountLevel == SystemRoles.ROLE_20_專業督導);
        }

#endregion

    }
}