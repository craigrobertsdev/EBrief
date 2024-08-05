using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Models.Data;
public class LandscapeList
{
    public CourtCode CourtCode { get; set; }
    public DateTime CourtDate { get; set; }
    public List<CourtListModel> CourtLists { get; set; } = [];
}
