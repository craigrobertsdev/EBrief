namespace EBrief.Shared.Services;
public record CourtListDto(IEnumerable<string> CaseFileNumbers, DateTime CourtDate);
