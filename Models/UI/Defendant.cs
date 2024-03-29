namespace CourtSystem.Models.UI;
public class Defendant {
    public int Id { get; set; }
    public int ListNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<CaseFile> CaseFiles { get; set; } = [];
    public CaseFile? ActiveCaseFile { get; set; }
}
