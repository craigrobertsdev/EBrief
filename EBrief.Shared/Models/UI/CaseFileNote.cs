namespace EBrief.Shared.Models.UI;
public class CaseFileNote
{
    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set
        {
            HasChanged = true;
            _text = value;
        }
    }
    public bool HasChanged { get; set; }
}
