namespace EBrief.Shared.Services;
public record CourtListDto(IEnumerable<string> CasefileNumbers, DateTime CourtDate);
