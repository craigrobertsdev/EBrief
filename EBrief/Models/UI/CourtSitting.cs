using EBrief.Helpers;

namespace EBrief.Models.UI;
public class CourtSitting
{
    public int Id { get; set; }
    public DateTime SittingTime { get; set; }
    public string Name { get; set; } = string.Empty; 
    public List<Defendant> Defendants { get; set; } = [];

    public CourtSitting(int id, DateTime sittingTime)
    {
        Id = id;
        SittingTime = sittingTime;
        Name = SittingTime.ToDisplayTime();
    }
}
