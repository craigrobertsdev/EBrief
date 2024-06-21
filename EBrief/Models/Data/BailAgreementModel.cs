using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace EBrief.Models.Data;

public class BailAgreementModel
{
    [Key]
    public int Id { get; set; }
    public DateTime DateEnteredInto { get; set; }
    public List<OrderConditionModel> Conditions { get; set; } = [];

    public BailAgreement ToUIModel()
    {
        return new BailAgreement
        {
            DateEnteredInto = DateEnteredInto,
            Conditions = Conditions.Select(c => c.ToUIModel()).ToList()
        };
    }
}
