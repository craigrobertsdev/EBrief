using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class CourtListModel
{
    [Key]
    public Guid Id { get; set; }
    public List<CasefileModel> Casefiles { get; set; } = [];
    public DateTime CourtDate { get; set; }
    public CourtCode CourtCode { get; set; }
    public int CourtRoom { get; set; }

    public CourtList ToUIModel()
    {
        var defendants = new List<Defendant>();
        foreach (var casefileModel in Casefiles)
        {
            var casefile = casefileModel.ToUIModel();
            casefile.GenerateInformationFromCharges();
            if (!defendants.Any(d => d.Id == casefile.Defendant.Id))
            {
                defendants.Add(casefile.Defendant);
                casefile.Defendant.Casefiles.Add(casefile);
            }
            else
            {
                defendants.First(d => d.Id == casefile.Defendant.Id).Casefiles.Add(casefile);
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

    public void CombineCasefiles(List<CasefileModel> casefiles)
    {
        Casefiles.Sort((c1, c2) => c1.CasefileNumber.CompareTo(c2.CasefileNumber));
        casefiles.Sort((c1, c2) => c1.CasefileNumber.CompareTo(c2.CasefileNumber));

        for (int i = 0; i < Casefiles.Count; i++)
        {
            var c1 = Casefiles[i];
            var c2 = casefiles[i];
            if (c1.CasefileNumber != c2.CasefileNumber)
            {
                throw new InvalidDataException("Casefile numbers should be equal");
            }

            c1.CombineWith(c2);
        }
    }

    public void CombineDefendantsWithServerResponse(List<CasefileModel> casefiles)
    {
        Casefiles.Sort((c1, c2) => c1.CasefileNumber.CompareTo(c2.CasefileNumber));
        casefiles.Sort((c1, c2) => c1.CasefileNumber.CompareTo(c2.CasefileNumber));

        for (int i = 0; i < Casefiles.Count; i++)
        {
            var d1 = Casefiles[i].Defendant;
            var d2 = casefiles[i].Defendant;

            d1.DateOfBirth = d2.DateOfBirth;
            d1.Phone = d2.Phone;
            d1.Email = d2.Email;
            d1.Address = d2.Address;
            d1.OffenderHistory = d2.OffenderHistory;
            d1.BailAgreements = d2.BailAgreements;
            d1.InterventionOrders = d2.InterventionOrders;
        }
    }

    // This handles the situation where a defendant has multiple case files
    // Every object from the server has a different reference, so we need to combine them
    public void CombineAndSortDefendantCasefiles()
    {
        var defendants = Casefiles.Select(cf => cf.Defendant).DistinctBy(d => d.Id).ToList();
        defendants.ForEach(d =>
        {
            d.ListStart = int.MaxValue;
            d.ListEnd = 0;
        });
        Casefiles.Sort((a, b) => a.Charges.First().Date.CompareTo(b.Charges.First().Date));

        foreach (var casefile in Casefiles)
        {
            casefile.Defendant = defendants.First(d => d.Id == casefile.Defendant.Id);
            if (casefile.Defendant.ListStart > casefile.ListNumber)
            {
                casefile.Defendant.ListStart = casefile.ListNumber;
            }
            if (casefile.Defendant.ListEnd < casefile.ListNumber)
            {
                casefile.Defendant.ListEnd = casefile.ListNumber;
            }
        }

    }

    public void OrderAndAssignListingNumbers()
    {
        Casefiles.Sort((a, b) => a.Schedule.Last().HearingDate.CompareTo(b.Schedule.Last().HearingDate));
        int listNo = 1;
        foreach (var casefile in Casefiles)
        {
            casefile.ListNumber = listNo++;
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
