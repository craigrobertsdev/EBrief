using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBrief.Shared.Models.Data;
public class DefendantModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid DbKey { get; set; }
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
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
            CaseFiles = [],
            BailAgreements = BailAgreements.Select(ba => ba.ToUIModel()).ToList(),
            InterventionOrders = InterventionOrders.Select(io => io.ToUIModel()).ToList()
        };
    }
}
