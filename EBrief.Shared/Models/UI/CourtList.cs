using EBrief.Shared.Models.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.UI;
public class CourtList
{
    public Guid Id { get; set; }
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

    public void AddCaseFiles(List<Casefile> caseFiles)
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
            defendant.CaseFiles.Sort((cf1, cf2) => cf1.Charges.First().Date.CompareTo(cf2.Charges.First().Date));
        }
    }

    public List<Casefile> GetCaseFiles()
    {
        var caseFiles = new List<Casefile>();
        foreach (var defendant in Defendants)
        {
            caseFiles.AddRange(defendant.CaseFiles);
        }

        return caseFiles;
    }

    public List<Casefile> GetCaseFiles(int amount)
    {
        var caseFiles = new List<Casefile>();
        foreach (var defendant in Defendants)
        {
            caseFiles.AddRange(defendant.CaseFiles);
            if (caseFiles.Count == amount)
            {
                break;
            }
        }

        return caseFiles;
    }
}
