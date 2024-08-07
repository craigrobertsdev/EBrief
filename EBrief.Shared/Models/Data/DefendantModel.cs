using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class DefendantModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public Guid DbKey { get; set; }

    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int ListStart { get; set; }
    public int ListEnd { get; set; }
    public List<BailAgreementModel> BailAgreements { get; set; } = [];
    public List<InterventionOrderModel> InterventionOrders { get; set; } = [];
    public string? OffenderHistory { get; set; }
    public Defendant ToUIModel()
    {
        return new Defendant
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            Address = Address,
            Phone = Phone,
            Email = Email,
            CaseFiles = [],
            ListStart = ListStart,
            ListEnd = ListEnd,
            BailAgreements = BailAgreements.Select(ba => ba.ToUIModel()).ToList(),
            InterventionOrders = InterventionOrders.Select(io => io.ToUIModel()).ToList()
        };
    }
}
