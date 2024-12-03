using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.UI;
public class Defendant
{
    public Guid Id { get; set; }
    public int ListStart { get; set; }
    public int ListEnd { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<Casefile> Casefiles { get; set; } = [];
    public Casefile? ActiveCasefile { get; set; }
    public string AddCfelText { get; set; } = string.Empty;
    public string? OffenderHistory { get; set; }
    public List<BailAgreement> BailAgreements { get; set; } = [];
    public List<InterventionOrder> InterventionOrders { get; set; } = [];

    public DefendantModel ToDataModel() => 
        new DefendantModel
        {
            Id = Id,
            ListStart = ListStart,
            ListEnd = ListEnd,
            FirstName = FirstName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            Address = Address,
            Phone = Phone,
            Email = Email,
            OffenderHistory = OffenderHistory,
            BailAgreements = BailAgreements.Select(ba => ba.ToDataModel()).ToList(),
            InterventionOrders = InterventionOrders.Select(io => io.ToDataModel()).ToList()
        };
}

public static class DefendantExtensions
{
    public static List<DefendantModel> ToDataModels(this IEnumerable<Defendant> models) =>
        models.Select(d => d.ToDataModel()).ToList();
}
