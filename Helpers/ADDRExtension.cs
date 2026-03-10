using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Turbo.Helpers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Services;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML ADDR 地址輸入框產生輔助方法類別
    /// </summary>
    public static class ADDRExtension
    {
        /// <summary>HTML 地址單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expressionADDR">ADDR(地址代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionADDR_TEXT">ADDR_TEXT(地址名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ADDRForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, AddressModel>> expressionADDR,
            object htmlAttributes = null, bool enabled = true)
        { 
            //ADDR HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expressionADDR);
            var ADDRname = name.ToSplit('.')[1];
            var metadata = ModelMetadata.FromLambdaExpression(expressionADDR, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();            
           
            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append(htmlHelper.EditorFor(expressionADDR).ToHtmlString());          

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");

            //縣市下拉選單連動鄉鎮市區下拉選單
            sb_script.Append("$('#"+ propertyId + "_City').on('change', function () {");
            sb_script.Append("GetZip('"+ propertyId + "_City', '" + propertyId + "_Town');");
            sb_script.Append("$('#" + propertyId+"_Zip').val('');");
            sb_script.Append("});");
            //鄉鎮市區下拉選單連動ZIP對話框
            sb_script.Append("$('#" + propertyId + "_Town').on('change', function () {");
            sb_script.Append("$('#" + propertyId + "_Zip').val($('#"+ propertyId + "_Town').val());");
            sb_script.Append("});");
            //ZIP對話框連動縣市、鄉鎮市區下拉選單
            sb_script.Append("$('#" + propertyId + "_Zip').on('blur', do" + ADDRname + "NameGet);");
            sb_script.Append("do" + ADDRname + "NameGet();");

            sb_script.Append("});");
            // Ajax 取得指定縣市的鄉鎮區Zip選項
            sb_script.Append("function GetZip(source, target, onfinish) {");
            sb_script.Append(" ajaxLoadMore(");
            sb_script.Append("'"+url.Content("~/Address/GetZip")+"',");
            sb_script.Append("{");
            sb_script.Append("CountryCode: $('#' + source).val(),");
            sb_script.Append("},");
            sb_script.Append("function (resp) {");
            sb_script.Append("var opts = \" <option value = ''> 請選擇 </option> \";");
            sb_script.Append("$('#' + target).html(opts + resp).show();");
            sb_script.Append(" if (onfinish) {");
            sb_script.Append("onfinish();");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("};");
            ////取得地址代碼對應的地址名稱。
            sb_script.Append("function do" + ADDRname + "NameGet() {");
            sb_script.Append("var url = '" + url.Action("GetCityCode", "Address", new { area = "" }) + "';");
            sb_script.Append("var parms = {");
            sb_script.Append("ZIP: $('#" + propertyId + "_Zip').val()");
            sb_script.Append("};");
            sb_script.Append("if ($('#" + propertyId + "_Zip').val() == '') {");
            sb_script.Append("$('#" + propertyId + "City').val('');");
            sb_script.Append(" GetZip('" + propertyId + "_City', '" + propertyId + "_Town');");
            sb_script.Append("} else {");
            sb_script.Append("ajaxLoadMore(url, parms, function (resp) {");
            sb_script.Append("if (resp != undefined) {");
            sb_script.Append("if (resp.data == '') {");
            sb_script.Append("blockAlert('查無該鄉鎮市區資訊!!');");
            sb_script.Append(" $('#" + propertyId + "_City').val('');");
            sb_script.Append("GetZip('" + propertyId + "_City', '" + propertyId + "_Town');");
            sb_script.Append("} else {");
            sb_script.Append("$('#" + propertyId + "_City').val(resp.data);");
            sb_script.Append("GetZip('" + propertyId + "_City', '" + propertyId + "_Town', function () {");
            sb_script.Append("$('#" + propertyId + "_Town').val($('#" + propertyId + "_Zip').val());");
            sb_script.Append(" });");
            sb_script.Append("}");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}