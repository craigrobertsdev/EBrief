using CourtSystem.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace CourtSystem.Models.Data;

public class OrderConditionModel {
    [Key]
    public int Id { get; set; }
    public int Number { get; set; }
    public string Condition { get; set; } = string.Empty;

    public OrderCondition ToUIModel() {
        return new OrderCondition(Number, Condition);
    }
}