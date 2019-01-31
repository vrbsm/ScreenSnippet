using Paragon.Plugins.ScreenCapture;
using System;
using System.Linq;
using System.Windows;

namespace ScreenSnippet
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] availableLocale = { "en-US", "ja-JP", "fr-FR" };

            string filename = "";
            string locale = "en-US";

            if (filename != null)
            {
                if (locale != null && availableLocale.Contains(locale))
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(locale);
                }
                SnippingWindow win = new SnippingWindow(filename);
                win.Show();
            }
            else
            {
                throw new Exception("Missing filename command line argument.");
            }
        }
    }
}
