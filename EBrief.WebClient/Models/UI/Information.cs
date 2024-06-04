namespace EBrief.WebClient.Models.UI;

public class Information
{
    public int Id { get; set; }
    public List<InformationEntry> Charges { get; set; } = [];
}

public record InformationEntry(int Sequence, string Text);