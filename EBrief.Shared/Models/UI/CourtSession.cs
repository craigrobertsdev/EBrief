using EBrief.Shared.Helpers;

namespace EBrief.Shared.Models.UI;
public class CourtSession
{
    public int Id { get; set; }
    public DateTime SittingTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Defendant> Defendants { get; set; } = [];

    public CourtSession(int id, DateTime sittingTime)
    {
        Id = id;
        SittingTime = sittingTime;
        Name = SittingTime.ToDisplayTime();
    }
}
