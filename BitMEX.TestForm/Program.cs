using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Serilog;
using Serilog.Events;

namespace BitMEX.TestForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitLogging();

            Application.Run(new Form1());
        }

        private static void InitLogging()
        {
            var executingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var logPath = Path.Combine(executingDir, "logs", "verbose.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                //.WriteTo.Console(LogEventLevel.Information)
                .WriteTo.Debug(LogEventLevel.Debug)
                .CreateLogger();
        }
    }
}
