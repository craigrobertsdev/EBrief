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
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public List<CaseFileModel> CaseFiles { get; set; } = [];
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

    public void CombineCaseFiles(List<CaseFileModel> caseFiles)
    {
        CaseFiles.Sort((c1, c2) => c1.CaseFileNumber.CompareTo(c2.CaseFileNumber));
        caseFiles.Sort((c1, c2) => c1.CaseFileNumber.CompareTo(c2.CaseFileNumber));

        for (int i = 0; i < CaseFiles.Count; i++)
        {
            var c1 = CaseFiles[i];
            var c2 = caseFiles[i];
            if (c1.CaseFileNumber != c2.CaseFileNumber)
            {
                throw new InvalidDataException("Casefile numbers should be equal");
            }

            c1.CombineWith(c2);
        }
    }

    public void CombineDefendantsWithServerResponse(List<CaseFileModel> caseFiles)
    {
        CaseFiles.Sort((c1, c2) => c1.CaseFileNumber.CompareTo(c2.CaseFileNumber));
        caseFiles.Sort((c1, c2) => c1.CaseFileNumber.CompareTo(c2.CaseFileNumber));

        for (int i = 0; i < CaseFiles.Count; i++)
        {
            var d1 = CaseFiles[i].Defendant;
            var d2 = caseFiles[i].Defendant;

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

    public void CombineAndSortDefendantCaseFiles()
    {
        var defendants = CaseFiles.Select(cf => cf.Defendant).DistinctBy(d => d.Id).ToList();
        defendants.ForEach(d =>
        {
            d.ListStart = int.MaxValue;
            d.ListEnd = 0;
        });
        CaseFiles.Sort((a, b) => a.Charges.First().Date.CompareTo(b.Charges.First().Date));

        foreach (var caseFile in CaseFiles)
        {
            caseFile.Defendant = defendants.First(d => d.Id == caseFile.Defendant.Id);
            if (caseFile.Defendant.ListStart > caseFile.ListNumber)
            {
                caseFile.Defendant.ListStart = caseFile.ListNumber;
            }
            if (caseFile.Defendant.ListEnd < caseFile.ListNumber)
            {
                caseFile.Defendant.ListEnd = caseFile.ListNumber;
            }
        }

    }

    public void OrderAndAssignListingNumbers()
    {
        CaseFiles.Sort((a, b) => a.Schedule.Last().HearingDate.CompareTo(b.Schedule.Last().HearingDate));
        int listNo = 1;
        foreach (var caseFile in CaseFiles)
        {
            caseFile.ListNumber = listNo++;
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
