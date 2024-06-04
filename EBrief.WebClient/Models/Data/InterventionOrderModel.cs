using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;

public class InterventionOrderModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public PersonModel ProtectedPerson { get; set; } = default!;
    public DateTime DateIssued { get; set; }
    public List<OrderConditionModel> Conditions { get; set; } = [];

    public InterventionOrder ToUIModel()
    {
        return new InterventionOrder
        {
            OrderNumber = OrderNumber,
            DateIssued = DateIssued,
            ProtectedPerson = ProtectedPerson.ToUIModel(),
            Conditions = Conditions.Select(c => c.ToUIModel()).ToList()
        };
    }
}
