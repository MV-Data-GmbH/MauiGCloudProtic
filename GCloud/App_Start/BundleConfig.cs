using System.Web;
using System.Web.Optimization;

namespace GCloud
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js", 
                        "~/Scripts/jquery.unobtrusive-ajax.min.js",
                        "~/Scripts/jquery.validate.unobtrusive.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/jquery.autocomplete").Include(
                "~/Content/jquery.auto-complete.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquery.autocomplete").Include(
                "~/Scripts/jquery.auto-complete.js"));

            bundles.Add(new StyleBundle("~/Content/croppie").Include(
                "~/Content/croppie.css"));

            bundles.Add(new ScriptBundle("~/bundles/croppie").Include(
                "~/Scripts/croppie.js"));

            bundles.Add(new StyleBundle("~/Content/CouponImage").Include(
                "~/Content/site/coupon/image.css"));

            bundles.Add(new ScriptBundle("~/bundles/CouponImage").Include(
                "~/Scripts/site/coupon/image.js"));
            
            bundles.Add(new StyleBundle("~/Content/select2").Include(
                "~/Content/css/select2.css"));

            bundles.Add(new ScriptBundle("~/bundles/select2").Include(
                "~/Scripts/select2.js"));

            bundles.Add(new StyleBundle("~/Content/daterangepicker").Include(
                "~/Content/daterangepicker.css"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                "~/Scripts/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/daterangepicker").Include(
                "~/Scripts/daterangepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/sweetalert.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/gridmvc").Include(
                    "~/Scripts/gridmvc.js",
                    "~/Scripts/gridmvc.lang.de.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-tagsinput").Include(
                    "~/Scripts/bootstrap-tagsinput.js"
                    ));
            bundles.Add(new StyleBundle("~/Content/bootstrap-tagsinput").Include(
                    "~/Content/bootstrap-tagsinput.css"
                ));

            bundles.Add(new StyleBundle("~/Content/visibility").Include(
                "~/Content/site/coupon/visibility.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/spinkit.css",
                      "~/Content/site.css",
                      "~/Content/Gridmvc.css",
                      "~/Content/jk-Layout.css"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/app.js"));
        }
    }
}
