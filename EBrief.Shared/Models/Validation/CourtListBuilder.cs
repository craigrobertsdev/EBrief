using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Models.Validation;
public sealed class CourtListBuilder
{
    private ICourtListValidationState _state;
    public DateTime? CourtDate { get; private set; }
    public CourtCode? CourtCode { get; private set; }
    public int? CourtRoom { get; private set; }


    public CourtListBuilder()
    {
        _state = new UnvalidatedCourtListState(this);
    }

    public bool IsValid => _state.IsValid;

    public void SetCourtDate(DateTime? courtDate)
    {
        CourtDate = courtDate;
        _state = _state.Validate();
    }

    public void SetCourtCode(CourtCode? courtCode)
    {
        CourtCode = courtCode;
        _state = _state.Validate();
    }

    public void SetCourtRoom(int? courtRoom)
    {
        CourtRoom = courtRoom;
        _state = _state.Validate();
    }

    public CourtListModel Build()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException("Cannot build a court list in an invalid state");
        }

        return new()
        {
            CourtDate = CourtDate!.Value,
            CourtCode = CourtCode!.Value,
            CourtRoom = CourtRoom!.Value
        };
    }
}
