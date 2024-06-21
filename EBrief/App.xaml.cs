using EBrief.Helpers;
using System.IO;
using System.Windows;

namespace EBrief;
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Directory.CreateDirectory(Path.Combine(FileHelpers.AppDataPath));
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            MessageBox.Show(error.ExceptionObject.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };
    }
}

