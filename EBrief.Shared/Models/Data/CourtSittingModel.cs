using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class CourtSittingModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public Guid DbKey { get; set; }
    public int Id { get; set; }
    public CourtCode CourtCode { get; set; }
    public int CourtRoom { get; set; }
    public DateTime SittingTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<DefendantModel> Defendants { get; set; } = [];
    public bool IsAdditionsList { get; set; }

    public CourtSitting ToUIModel() =>
        new CourtSitting(Id, SittingTime, CourtCode, CourtRoom)
        {
            Name = Name,
            Defendants = Defendants.ToUIModels(),
            IsAdditionsList = IsAdditionsList
        };
}

public static class CourtSittingModelExtensions
{
    public static List<CourtSitting> ToUIModels(this IEnumerable<CourtSittingModel> models)
    {
        return models.Select(model => model.ToUIModel()).ToList();
    }
}
