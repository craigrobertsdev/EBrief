namespace EBrief.Models.UI;
public class Defendant
{
    public int Id { get; set; }
    public int ListNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<CaseFile> CaseFiles { get; set; } = [];
    public CaseFile? ActiveCaseFile { get; set; }
    public string? OffenderHistory { get; set; }
    public string CfelText { get; set; } = string.Empty;
    public List<BailAgreement> BailAgreements { get; set; } = [];
    public List<InterventionOrder> InterventionOrders { get; set; } = [];
}
