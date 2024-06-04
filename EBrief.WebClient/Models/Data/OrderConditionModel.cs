using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;

public class OrderConditionModel
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string Condition { get; set; } = string.Empty;

    public OrderCondition ToUIModel()
    {
        return new OrderCondition(Number, Condition);
    }
}