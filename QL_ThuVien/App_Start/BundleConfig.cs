using System.Web;
using System.Web.Optimization;

namespace QL_ThuVien
{   
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Content/bootstrap/js/bootstrap.bundle.min.js"));
            bundles.Add(new Bundle("~/bundles/tinymce").Include(
                     "~/Content/tinymce/js/tinymce/tinymce.min.js",
                     "~/Content/scripts/tinymce-config.js"));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/css/reset.css",
                      "~/Content/bootstrap/css/bootstrap.min.css",
                      "~/Content/css/general.css",
                      "~/Content/css/_variables.css",
                      "~/Content/icons/fontawesome/css/all.min.css"));
            // admin
            bundles.Add(new StyleBundle("~/Content/admin/css").Include(
                     "~/Content/css/reset.css",
                     "~/Content/bootstrap/css/bootstrap.min.css",
                     "~/Content/icons/fontawesome/css/all.min.css",
                     "~/Content/admin/css/admin.css"
                    ));
        }
    }
}
