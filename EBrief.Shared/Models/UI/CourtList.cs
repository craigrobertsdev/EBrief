using EBrief.Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.UI;
public class CourtList
{
    public int Id { get; set; }
    public DateTime CourtDate { get; set; }
    public CourtCode CourtCode { get; set; }
    public int CourtRoom { get; set; }
    public List<Defendant> Defendants { get; set; } = [];

    public void GenerateInformations()
    {
        foreach (var defendant in Defendants)
        {
            foreach (var caseFile in defendant.CaseFiles)
            {
                caseFile.GenerateInformationFromCharges();
            }
        }
    }

    public void AddCaseFiles(List<CaseFile> caseFiles)
    {
        foreach (var caseFile in caseFiles)
        {
            var defendant = Defendants.FirstOrDefault(d => d.Id == caseFile.Defendant.Id);
            if (defendant is null)
            {
                defendant = new Defendant
                {
                    Id = caseFile.Defendant.Id,
                    LastName = caseFile.Defendant.LastName,
                    FirstName = caseFile.Defendant.FirstName
                };
                Defendants.Add(defendant);
            }

            defendant.CaseFiles.Add(caseFile);
        }
    }

    public List<CaseFile> GetCaseFiles()
    {
        var caseFiles = new List<CaseFile>();
        foreach (var defendant in Defendants)
        {
            caseFiles.AddRange(defendant.CaseFiles);
        }

        return caseFiles;
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
