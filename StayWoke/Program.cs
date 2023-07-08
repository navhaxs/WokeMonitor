using Serilog;
using Serilog.Templates;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StayWoke
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ExpressionTemplate OUTPUT_TEMPLATE = new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3}]{#if SourceContext is not null} [{SourceContext:l}]{#end} {@m}\n{@x}");
            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .Enrich.FromLogContext()
                   .WriteTo.File(path: $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}_log.txt", formatter: OUTPUT_TEMPLATE)
                   .CreateLogger();

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainApp());
        }

        private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Fatal(e.ToString(), "TaskScheduler_UnobservedTaskException. Please report this error.");
        }

    }
}
