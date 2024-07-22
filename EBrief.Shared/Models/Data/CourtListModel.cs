using EBrief.Shared.Models;
using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class CourtListModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public List<CaseFileModel> CaseFiles { get; set; } = new List<CaseFileModel>();
    public DateTime CourtDate { get; set; }
    public CourtCode CourtCode { get; set; }
    public int CourtRoom { get; set; }

    public CourtList ToUIModel()
    {
        var defendants = new List<Defendant>();
        foreach (var caseFileModel in CaseFiles)
        {
            var caseFile = caseFileModel.ToUIModel();
            caseFile.GenerateInformationFromCharges();
            if (!defendants.Any(d => d.Id == caseFile.Defendant.Id))
            {
                defendants.Add(caseFile.Defendant);
                caseFile.Defendant.CaseFiles.Add(caseFile);
            }
            else
            {
                defendants.First(d => d.Id == caseFile.Defendant.Id).CaseFiles.Add(caseFile);
            }
        }

        return new CourtList
        {
            Id = Id,
            Defendants = defendants,
            CourtCode = CourtCode,
            CourtDate = CourtDate,
            CourtRoom = CourtRoom

        };
    }

    // To handle the situation where a defendant has multiple case files
    // Every object from the server has a different reference, so we need to combine them
    public void CombineDefendantCaseFiles()
    {
        var defendants = CaseFiles.Select(cf => cf.Defendant).DistinctBy(d => d.Id).ToList();
        CaseFiles.Sort((a, b) => a.Charges.First().Date.CompareTo(b.Charges.First().Date));

        foreach (var caseFile in CaseFiles)
        {
            caseFile.Defendant = defendants.First(d => d.Id == caseFile.Defendant.Id);
        }
    }

    public string SerialiseToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
    }
}
