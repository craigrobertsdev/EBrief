using EBrief.Services;

namespace EBrief.Commands
{
    public class AddNewCourtListCommand : CommandBase
    {
        private readonly INavigationService _courtListNavigationService;

        public AddNewCourtListCommand(INavigationService courtListNavigationService)
        {
            _courtListNavigationService = courtListNavigationService;
        }

        public override void Execute(object? parameter)
        {
            _courtListNavigationService.Navigate();
        }
    }
}
