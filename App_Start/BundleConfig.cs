using System.Web;
using System.Web.Optimization;

namespace WDAIIP.WEB
{
    public class BundleConfig
    {
        // 如需「搭配」的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // 將 EnableOptimizations 設為 false 以進行偵錯。如需詳細資訊，
            // 請造訪 http://go.microsoft.com/fwlink/?LinkId=301862
#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
            //"~/Scripts/modernizr-*"
            bundles.Add(new StyleBundle("~/Content/jquery").Include("~/Content/jquery-confirm.min.css"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js", "~/Scripts/jquery-confirm.min.js", "~/Scripts/jquery.blockUI.js"));
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));
            //"~/Scripts/bootstrap-treeview.js",
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));
            //Datepicker  "~/Scripts/ui.datepicker-zh-TW.js",
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include( "~/Scripts/jquery-ui-1.13.2.js" ));

            bundles.Add(new ScriptBundle("~/bundles/globaljs").Include("~/Scripts/global.js", "~/Scripts/print.js"));
            bundles.Add(new ScriptBundle("~/bundles/common").Include("~/Scripts/Controllers/commonController.js"));
            bundles.Add(new ScriptBundle("~/bundles/main").Include("~/js/main.js"));
            //"~/Content/bootstrap-treeview.css", //"~/Content/font-awesome-4.7.0.min.css") //"~/Content/font-awesome-5.0.13.css") //"~/Content/bootstrap.css"
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/font-awesome-5.1.0.min.css"));
            bundles.Add(new StyleBundle("~/css/base").Include("~/css/base.css", new CssRewriteUrlTransform()));
            //bundles.Add(new StyleBundle("~/css/bootstrap").Include("~/css/bootstrap.css", "~/css/flexslider.css")); //@Styles.Render("~/css/flexslider")
            bundles.Add(new StyleBundle("~/css/flexslider").Include("~/css/bootstrap.css", "~/css/flexslider.css"));
            //fontawesome
            bundles.Add(new StyleBundle("~/Content/fontawesome").Include("~/Content/fontawesome-free-5.0.13-all.css"));
            bundles.Add(new ScriptBundle("~/bundles/flexslider").Include("~/Scripts/jquery.flexslider.js"));
            bundles.Add(new ScriptBundle("~/bundles/popupDialog").Include("~/scripts/popupDialog.js"));
            //帳號維護 Scripts.Render("~/bundles/enterType") Scripts\Controllers\EnterTypeControllers\enterTypeControllers.js
            bundles.Add(new ScriptBundle("~/bundles/enterType").Include("~/Scripts/Controllers/EnterTypeControllers/enterTypeControllers.js"));
            bundles.Add(new ScriptBundle("~/bundles/enterTypeD").Include("~/Scripts/Controllers/EnterTypeControllers/enterTypeDControllers.js"));

        }
    }
}