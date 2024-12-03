using EBrief.Shared.Helpers;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Models.UI;
public class CourtSitting
{
    public int Id { get; set; }
    public CourtCode CourtCode { get; set; }
    public int CourtRoom { get; set; }
    public DateTime SittingTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Defendant> Defendants { get; set; } = [];
    public bool IsAdditionsList { get; set; }

    public CourtSitting(int id, DateTime sittingTime, CourtCode courtCode, int courtRoom)
    {
        Id = id;
        SittingTime = sittingTime;
        CourtCode = courtCode;
        CourtRoom = courtRoom;
        Name = SittingTime.ToDisplayTime();
    }

    protected CourtSitting(int id, DateTime sittingTime, CourtCode courtCode, int courtRoom, bool isAdditionsList)
    {
        IsAdditionsList = isAdditionsList;
        SittingTime = sittingTime;
        Id = id;
        CourtCode = courtCode;
        CourtRoom = courtRoom;
        Name = "Additions";
    }

    public CourtSittingModel ToDataModel() =>
        new CourtSittingModel()
        {
            Id = Id,
            CourtCode = CourtCode,
            CourtRoom = CourtRoom,
            SittingTime = SittingTime,
            Name = Name,
            //Defendants = Defendants.ToDataModels(),
            IsAdditionsList = IsAdditionsList
        };
}
