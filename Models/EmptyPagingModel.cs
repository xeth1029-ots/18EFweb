using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 制作雛型頁面用的靜態分頁 Model
    /// </summary>
    public class EmptyPagingModel : Turbo.DataLayer.PagingResultsViewModel
    {
        /// <summary>
        /// 制作雛型頁面用的靜態分頁 Model, 實際程式要直引用各個 FormModel
        /// </summary>
        public static EmptyPagingModel Instance
        {
            get
            {
                EmptyPagingModel model = new EmptyPagingModel();
                model.PagingInfo.Total = 10;
                model.PagingInfo.PageIdx = 1;
                model.PagingInfo.TotalPages = 1;
                return model;
            }
        }

    }
}