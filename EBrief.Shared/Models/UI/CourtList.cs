using EBrief.Shared.Models.Data;
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
            foreach (var casefile in defendant.Casefiles)
            {
                casefile.GenerateInformationFromCharges();
            }
        }
    }

    public void AddCasefiles(List<Casefile> casefiles)
    {
        foreach (var casefile in casefiles)
        {
            var defendant = Defendants.FirstOrDefault(d => d.Id == casefile.Defendant.Id);
            if (defendant is null)
            {
                defendant = new Defendant
                {
                    Id = casefile.Defendant.Id,
                    LastName = casefile.Defendant.LastName,
                    FirstName = casefile.Defendant.FirstName
                };
                Defendants.Add(defendant);
            }

            defendant.Casefiles.Add(casefile);
            defendant.Casefiles.Sort((cf1, cf2) => cf1.Charges.First().Date.CompareTo(cf2.Charges.First().Date));
        }
    }

    public List<Casefile> GetCasefiles()
    {
        var casefiles = new List<Casefile>();
        foreach (var defendant in Defendants)
        {
            casefiles.AddRange(defendant.Casefiles);
        }

        return casefiles;
    }

    public List<Casefile> GetCasefiles(int amount)
    {
        var casefiles = new List<Casefile>();
        foreach (var defendant in Defendants)
        {
            casefiles.AddRange(defendant.Casefiles);
            if (casefiles.Count == amount)
            {
                break;
            }
        }

        return casefiles;
    }

    public void UpdateCasefiles(List<CasefileModel> updatedCasefiles)
    {
        var casefiles = GetCasefiles();
        casefiles.Sort((a, b) => a.CasefileNumber.CompareTo(b.CasefileNumber));
        updatedCasefiles.Sort((a, b) => a.CasefileNumber.CompareTo(b.CasefileNumber));
        for (int i = 0; i < casefiles.Count; i++)
        {
            var casefile = casefiles[i];
        }
    }
}
