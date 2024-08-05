using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.Validation;
public interface ICourtListValidationState
{
    ICourtListValidationState Validate();
    bool IsValid { get; }
}

public sealed class UnvalidatedCourtListState : ICourtListValidationState
{
    private readonly CourtListBuilder _context;
    public bool IsValid => false;

    public UnvalidatedCourtListState(CourtListBuilder context) => _context = context;

    public ICourtListValidationState Validate()
    {
        if (_context.CourtDate.HasValue && _context.CourtCode.HasValue && _context.CourtRoom.HasValue)
        {
            return new ValidCourtListState(_context);
        }
        else
        {
            return new InvalidCourtListState(_context);
        }
    }
}
public sealed class ValidCourtListState : ICourtListValidationState
{
    private readonly CourtListBuilder _context;
    public bool IsValid => true;
    public ValidCourtListState(CourtListBuilder context) => _context = context;

    public ICourtListValidationState Validate()
    {
        if (_context.CourtDate.HasValue && _context.CourtCode.HasValue && _context.CourtRoom.HasValue)
        {
            return this;
        }
        else
        {
            return new InvalidCourtListState(_context);
        }
    }
}

public sealed class InvalidCourtListState : ICourtListValidationState
{
    private readonly CourtListBuilder _context;
    public bool IsValid { get; }
    public InvalidCourtListState(CourtListBuilder context) => _context = context;

    public ICourtListValidationState Validate()
    {
        if (_context.CourtDate.HasValue && _context.CourtCode.HasValue && _context.CourtRoom.HasValue)
        {
            return new ValidCourtListState(_context);
        }
        else
        {
            return this;
        }
    }
}
