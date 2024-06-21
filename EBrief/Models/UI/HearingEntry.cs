namespace EBrief.Models.UI;
public class HearingEntry
{
    public int Id { get; set; }
    public DateTime HearingDate { get; set; }
    public string AppearanceType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
