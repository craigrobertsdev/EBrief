using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.UI;

public class BailAgreement
{
    public DateTime DateEnteredInto { get; set; }
    public List<OrderCondition> Conditions { get; set; } = [];

    public BailAgreementModel ToDataModel()
    {
        return new BailAgreementModel()
        {
            DateEnteredInto = DateEnteredInto,
            Conditions = Conditions.Select(c => c.ToDataModel()).ToList()
        };
    }
}
