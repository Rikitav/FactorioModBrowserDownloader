using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
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

#if !DEBUG
            if (!NativeMethods.IsInternetConnectionAvailable())
                throw new ApplicationException("No Internet connection");
#endif
        }

        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!args.IsTerminating)
                return;

            if (args.ExceptionObject is not Exception exc)
                return;

            string msg = string.Format("\"{0}\" Application's execution was faulted by unhandled exception in {1} :\n\n{2}", AppDomain.CurrentDomain.FriendlyName, sender.ToString(), exc.ToString());
            MessageBox.Show(msg, AppDomain.CurrentDomain.FriendlyName, MessageBoxButton.OK);
        }

        private static partial class NativeMethods
        {
            public enum InternetConnectionState
            {
                CONFIGURED = 0x40,
                LAN = 0x02,
                MODEM = 0x01,
                MODEM_BUSY = 0x08,
                OFFLINE = 0x20,
                PROXY = 0x04
            }

            private const string _CheckUriString = @"https://mods.factorio.com";
            public const int ERROR_NOT_CONNECTED = 0x8CA;

            [LibraryImport("wininet.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool InternetGetConnectedState(out InternetConnectionState lpdwFlags, int dwReserved);

            [LibraryImport("wininet.dll", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool InternetCheckConnectionA(string lpszUrl, int dwFlags, int dwReserved);

            public static bool IsInternetConnectionAvailable()
            {
                // Checking for any Internet devices is active
                if (!InternetGetConnectedState(out InternetConnectionState state, 0))
                {
                    Debug.WriteLine("No internet devices online, state : {0}", [state]);
                    return false;
                }

                // Checking for server availability
                if (!InternetCheckConnectionA(_CheckUriString, 0x00000001, 0))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    Debug.WriteLine(lastError == ERROR_NOT_CONNECTED ? "No Internet connection" : "Server unavailable");
                    return false;
                }

                return true;
            }
        }
    }
}
