using EBrief.Commands;
using EBrief.Services;
using System.Windows.Input;

namespace EBrief.ViewModels;
public class HomeViewModel : ViewModelBase
{
    public ICommand NavigateAddNewCourtListCommand { get; }
    public ICommand LoadPreviousCourtListCommand { get; }
    public ICommand OpenCourtListFromFileCommand { get; }

    public HomeViewModel(INavigationService addCourtListNavigationService)
    {
        NavigateAddNewCourtListCommand = new NavigateCommand(addCourtListNavigationService);
    }
}
