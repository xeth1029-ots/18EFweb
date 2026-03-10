using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    public class ClassSearchTranDetail2Model : TblPLAN_TRAINDESC
    {
        /// <summary>授課師資</summary>
        public string TEACHCNAME { get; set; }

        /// <summary>助教</summary>
        public string TEACHCNAME2 { get; set; }
    }
}