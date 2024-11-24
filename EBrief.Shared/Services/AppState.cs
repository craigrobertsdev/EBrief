using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Services;
public class AppState
{
    private List<CourtSitting> _courtSittings = [];
    public bool IsFirstLoad { get; private set; }
    public CourtList? CurrentCourtList { get; set; }
    public List<CourtSitting> CourtSittings
    {
        get => _courtSittings;
        set
        {
            _courtSittings = value;
            NotifyStateChanged();
        }
    }

    public string CurrentUser = "Craig Roberts";

    public AppState()
    {
        IsFirstLoad = true;
    }

    public void ApplicationLoaded()
    {
        IsFirstLoad = false;
    }

    public event Action? OnStateChanged;
    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
