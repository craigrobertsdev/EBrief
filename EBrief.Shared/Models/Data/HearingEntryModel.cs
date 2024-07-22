using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class HearingEntryModel
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public DateTime HearingDate { get; set; }
    public string AppearanceType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public HearingEntry ToUIModel()
    {
        return new HearingEntry
        {
            Id = Id,
            HearingDate = HearingDate,
            AppearanceType = AppearanceType,
            Notes = Notes
        };
    }
}
