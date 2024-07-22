using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services;
public class AppState
{
    public bool IsFirstLoad { get; private set; }
    public CourtList? CurrentCourtList { get; set; }
    public string CurrentUser = "Craig Roberts";

    public AppState()
    {
        IsFirstLoad = true;
    }

    public void ApplicationLoaded()
    {
        IsFirstLoad = false;
    }
}
