﻿namespace EBrief.Shared.Models.UI;
public class Defendant
{
    public int Id { get; set; }
    public int ListStart { get; set; }
    public int ListEnd { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<CaseFile> CaseFiles { get; set; } = [];
    public CaseFile? ActiveCaseFile { get; set; }
    public string AddCfelText { get; set; } = string.Empty;
    public string? OffenderHistory { get; set; }
    public List<BailAgreement> BailAgreements { get; set; } = [];
    public List<InterventionOrder> InterventionOrders { get; set; } = [];
}
