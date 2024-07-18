using EBrief.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Models.Data;

public class InformationModel
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public Guid CaseFileId { get; set; }
    public List<InformationEntryModel> Charges { get; set; } = [];

    public Information ToUIModel()
    {
        return new Information
        {
            Charges = Charges.ToUIModel()
        };
    }
}

[Owned]
public record InformationEntryModel(int Sequence, string Text)
{
    public InformationEntry ToUIModel()
    {
        return new InformationEntry(Sequence, Text);
    }
}

static class Extensions
{
    public static List<InformationEntry> ToUIModel(this List<InformationEntryModel> charges) =>
        charges.Select(e => e.ToUIModel()).ToList();
}