using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MoneyTron.Presenter;
using Serilog;
using Serilog.Events;
using System.Configuration;

namespace MoneyTron
{
    static class Program
    {
        private static MTMainPresenter _presenter;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitLogging();

            var mainForm = new MTMainForm();
            _presenter = new MTMainPresenter(mainForm);

            Application.Run(mainForm);
        }

        private static void InitLogging()
        {
            var executingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var logPath = Path.Combine(executingDir, "logs", "verbose.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                //.WriteTo.Console(LogEventLevel.Information)
                .WriteTo.Debug(LogEventLevel.Debug)
                .CreateLogger();
        }
    }
}
