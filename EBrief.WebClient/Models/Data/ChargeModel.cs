using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;
public class ChargeModel
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? VictimName { get; set; } = string.Empty;
    public string ChargeWording { get; set; } = string.Empty;

    public Charge ToUIModel()
    {
        return new Charge
        {
            Sequence = Sequence,
            Name = Name,
            Date = Date,
            VictimName = VictimName,
            ChargeWording = ChargeWording
        };
    }
}
