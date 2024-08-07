using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Models.Validation;
public sealed class CourtListBuilder
{
    private ICourtListValidationState _state;
    private DateTime? _courtDate;
    private CourtCode? _courtCode;
    private int? _courtRoom;
    public DateTime? CourtDate
    {
        get
        {
            return _courtDate;
        }
        set
        {
            _courtDate = value;
            _state = _state.Validate();
        }
    }

    public CourtCode? CourtCode
    {
        get
        {
            return _courtCode;
        }
        set
        {
            _courtCode = value;
            _state = _state.Validate();
        }
    }

    public int? CourtRoom
    {
        get
        {
            return _courtRoom;
        }
        set
        {
            _courtRoom = value;
            _state = _state.Validate();
        }
    }

    public CourtListBuilder()
    {
        _state = new UnvalidatedCourtListState(this);
    }

    public bool IsValid => _state.IsValid;

    public void SetCourtDate(DateTime? courtDate)
    {
        CourtDate = courtDate;
    }

    public void SetCourtCode(CourtCode? courtCode)
    {
        CourtCode = courtCode;
    }

    public void SetCourtRoom(int? courtRoom)
    {
        CourtRoom = courtRoom;
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

    public void Reset()
    {
        CourtCode = null;
        CourtDate = null;
        CourtRoom = null;
    }
}
