using EBrief.Shared.Helpers;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace EBrief;
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Directory.CreateDirectory(Path.Combine(FileHelpers.AppDataPath));

        Current.DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) =>
        {
            MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }
}

