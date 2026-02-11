using System.Configuration;
using System.Data;
using System.Windows;

namespace KeyboardRuntime;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show($"UNHANDLED DISPATCHER EXCEPTION:\n\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                            "Runtime Crash", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
            e.Handled = true; // Try to prevent crash if possible
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            System.Windows.MessageBox.Show($"CRITICAL DOMAIN EXCEPTION:\n\n{ex?.Message ?? "Unknown Error"}\n\nStack Trace:\n{ex?.StackTrace}", 
                            "Runtime Fatal Error", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
        }
    }

