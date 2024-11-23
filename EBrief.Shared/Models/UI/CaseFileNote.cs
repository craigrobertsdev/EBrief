namespace EBrief.Shared.Models.UI;
public class CasefileNote
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
