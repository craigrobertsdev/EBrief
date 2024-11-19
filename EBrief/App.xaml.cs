using EBrief.Services;
using EBrief.Shared.Helpers;
using EBrief.Stores;
using EBrief.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace EBrief;
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    public App()
    {
        var services = new ServiceCollection();

        services.AddSingleton<NavigationStore>();
        services.AddSingleton<ModalNavigationStore>();

        services.AddSingleton<INavigationService>(CreateHomeNavigationService);
        services.AddSingleton<CloseModalNavigationService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>(s => new MainWindow()
        {
            DataContext = s.GetRequiredService<MainViewModel>()
        });

        services.AddTransient<HomeViewModel>(s => new HomeViewModel(CreateAddNewCourtListNavigationService(s)));
        services.AddTransient<AddNewCourtListViewModel>(s => new AddNewCourtListViewModel());

        _serviceProvider = services.BuildServiceProvider();


    }

    private INavigationService CreateAddNewCourtListNavigationService(IServiceProvider serviceProvider)
    {
        var modalNavigationStore = serviceProvider.GetRequiredService<ModalNavigationStore>();

        return new ModalNavigationService<AddNewCourtListViewModel>(
            modalNavigationStore,
            () => serviceProvider.GetRequiredService<AddNewCourtListViewModel>());
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var initialNavigationService = _serviceProvider.GetRequiredService<INavigationService>();
        initialNavigationService.Navigate();
        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();

        base.OnStartup(e);
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Directory.CreateDirectory(Path.Combine(FileHelpers.AppDataPath));

        Current.DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) =>
        {
            MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }

    private INavigationService CreateHomeNavigationService(IServiceProvider serviceProvider)
    {
        return new NavigationService<HomeViewModel>(
            serviceProvider.GetRequiredService<NavigationStore>(),
            () => serviceProvider.GetRequiredService<HomeViewModel>());
    }

}

