using EBrief.Models.UI;

namespace EBrief.Services;
public class AppState
{
    public bool IsFirstLoad { get; private set; }
    public CourtList? CurrentCourtList { get; set; }

    public AppState()
    {
        IsFirstLoad = true;
    }

    public void ApplicationLoaded()
    {
        IsFirstLoad = false;
    }
}
