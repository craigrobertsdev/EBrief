namespace EBrief.Shared.Models.UI;

public class BailAgreement
{
    public DateTime DateEnteredInto { get; set; }
    public List<OrderCondition> Conditions { get; set; } = [];
}
