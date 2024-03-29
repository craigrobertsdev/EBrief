using CourtSystem.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace CourtSystem.Models.Data;
public class CourtListModel {
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<CaseFileModel> CaseFiles { get; set; } = [];

    public CourtList ToCourtList() {
        var defendants = new List<Defendant>();
        foreach (var caseFileModel in CaseFiles) {
            var caseFile = caseFileModel.ToUIModel();
            caseFile.GenerateInformationFromCharges();
            if (!defendants.Any(d => d.Id == caseFile.Defendant.Id)) {
                defendants.Add(caseFile.Defendant);
            }
            else {
                defendants.First(d => d.Id == caseFile.Defendant.Id).CaseFiles.Add(caseFile);
            }
        }

        return new CourtList {
            Date = Date,
            Defendants = defendants
        };
    }
}
