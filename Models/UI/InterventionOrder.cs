namespace CourtSystem.Models.UI;

public class InterventionOrder {
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime DateIssued { get; set; }
    public Person ProtectedPerson { get; set; } = default!;
    public List<OrderCondition> Conditions { get; set; } = [];
}
