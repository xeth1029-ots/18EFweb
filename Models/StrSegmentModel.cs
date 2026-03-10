using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.Services;

namespace WDAIIP.WEB.Models
{
    public class StrSegmentModel
    {
        /// <summary>status: 1.成功, 0.失敗</summary>
        public string message { get; set; }
        /// <summary>message: 失敗時的額外訊息</summary>
        public int status { get; set; }
        /// <summary>keywords 為斷詞結果陣列，第1個元素一定是傳入字串str本身，之後是不定個數的分詞結果</summary>
        public string[] keywords { get; set; }
    }
}