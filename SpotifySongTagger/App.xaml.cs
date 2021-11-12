using Serilog;
using System.Windows;
using Util;

namespace SpotifySongTagger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var logConfig = new LoggerConfiguration()
                .WriteTo.File(formatter: new LogFormatter("UI"), @"log_frontend.log");
#if DEBUG
            logConfig = logConfig.WriteTo.Trace(formatter: new LogFormatter("UI"));
#endif
            Log.Logger = logConfig.CreateLogger();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}
