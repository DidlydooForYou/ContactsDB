using System.Web;
using System.Web.Optimization;

namespace ContactsDB
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/clientViewScript").Include(
                         "~/Scripts/validation.js",
                         "~/Scripts/jquery-maskedinput.js",
                         "~/Scripts/bootbox.js",
                         "~/Scripts/autoRefreshPanel.js",
                         "~/Scripts/image-control.js",

                            "~/Scripts/selection.js",
                            "~/Scripts/SiteScripts.js",
                            "~/Scripts/session.js",
                            "~/Scripts/SiteNotificationsHandler.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/_layout.css",
                     "~/Content/site.css",
                     "~/Content/menu.css",
                     "~/Content/contacts.css",

                        "~/Content/Accounts.css",
                        "~/Content/popup.css",
                        "~/Content/Selection.css",
                        "~/Content/media.css",
                        "~/Content/Icons.css",
                        "~/Content/image-control.css",
                        "~/Content/jqui-custom-datepicker.css"));
        }
    }
}
