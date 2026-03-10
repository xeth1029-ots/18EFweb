using eYVTR_mng_n.Models.Entities;
using eYVTR_mng_n.Commons;
using Turbo.DataLayer;

namespace eYVTR_mng_n.Models
{
    /// <summary>
    /// 角色功能權限 Model
    /// </summary>
    public class ClamRoleFunc: TblCLAMFUNCM
    {
        /// <summary>
        /// 項次(已排序)
        /// </summary>
        public int RNUM { get; set; }

        /// <summary>
        /// 類別: 1.選單TreeNode(沒有 action path), 0.選單功能連結項目 (有 action path)
        /// </summary>
        [NotDBField]
        public string NODE { get; set; }

    }
}