using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.UI;

public class InterventionOrder
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime DateIssued { get; set; }
    public Person ProtectedPerson { get; set; } = default!;
    public List<OrderCondition> Conditions { get; set; } = [];

    public InterventionOrderModel ToDataModel()
    {
        return new InterventionOrderModel
        {
            OrderNumber = OrderNumber,
            DateIssued = DateIssued,
            ProtectedPerson = ProtectedPerson.ToDataModel(),
            Conditions = Conditions.Select(c => c.ToDataModel()).ToList()
        };
    }
}
