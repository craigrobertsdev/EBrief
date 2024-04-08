namespace EBrief.Models.UI;
public class Defendant {
    public int Id { get; set; }
    public int ListNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<CaseFile> CaseFiles { get; set; } = [];
    public CaseFile? ActiveCaseFile { get; set; }
    public string? OffenderHistory { get; set; }
    public List<BailAgreement> BailAgreements { get; set; } = [];
    public List<InterventionOrder> InterventionOrders { get; set; } = [];
}
