namespace EBrief.Services;
public class AppState
{
    public bool IsFirstLoad { get; private set; }

    public AppState()
    {
        IsFirstLoad = true;
    }

    public void ApplicationLoaded()
    {
        IsFirstLoad = false;
    }
}
