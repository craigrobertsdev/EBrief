using EBrief.Commands;
using EBrief.Services;
using System.Windows.Input;

namespace EBrief.ViewModels;
public class HomeViewModel : ViewModelBase
{
    public ICommand AddNewCourtListCommand { get; }
    public ICommand LoadPreviousCourtListCommand { get; }
    public ICommand OpenCourtListFromFileCommand { get; }

    public HomeViewModel(INavigationService modalNavigationService, INavigationService courtListNavigationService)
    {
        AddNewCourtListCommand = new AddNewCourtListCommand(modalNavigationService, courtListNavigationService);
    }
}
