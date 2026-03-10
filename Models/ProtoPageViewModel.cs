using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 雛型頁面用的 View Model
    /// </summary>
    public class ProtoPageViewModel
    {
        public ProtoPageViewModel()
        {
            this.Addr = new AddressModel();
        }

        /// <summary>
        /// 地址欄位
        /// </summary>
        public AddressModel Addr { get; set; }

    }
}