namespace EBrief.Services;
public record CourtListDto(IEnumerable<string> CaseFileNumbers, DateTime CourtDate);
