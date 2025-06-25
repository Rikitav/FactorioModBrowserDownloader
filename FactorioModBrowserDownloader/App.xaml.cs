using System.Diagnostics;
using System.Windows;

namespace FactorioNexus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            Console.Clear();
            Trace.Listeners.Add(new ConsoleTraceListener());
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
        }

        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!args.IsTerminating)
                return;

            if (args.ExceptionObject is not Exception exc)
                return;

            string msg = string.Format("\"{0}\" Application's execution was faulted by unhandled exception :\n\n{1}", AppDomain.CurrentDomain.FriendlyName, exc.ToString());
            MessageBox.Show(msg, AppDomain.CurrentDomain.FriendlyName, MessageBoxButton.OK);
        }
    }
}
