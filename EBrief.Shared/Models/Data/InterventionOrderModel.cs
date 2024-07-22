using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;

public class InterventionOrderModel
{
    [Key]
    [JsonIgnore]
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
