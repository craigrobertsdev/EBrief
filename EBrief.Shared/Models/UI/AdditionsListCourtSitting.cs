
using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Models.UI;
public class AdditionsListCourtSitting : CourtSitting
{
    public AdditionsListCourtSitting(int id, DateTime sittingTime, CourtCode courtCode, int courtRoom) 
        : base(id, sittingTime, courtCode, courtRoom, true)
    {
    }
}
