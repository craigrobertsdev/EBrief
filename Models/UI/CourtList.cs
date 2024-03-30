namespace CourtSystem.Models.UI;
public class CourtList {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<Defendant> Defendants { get; set; } = [];

    public void GenerateInformations() {
        foreach (var defendant in Defendants) {
            foreach (var caseFile in defendant.CaseFiles) {
                caseFile.GenerateInformationFromCharges();
            }
        }
    }
    
    public List<CaseFile> GetCaseFiles() {
        var caseFiles = new List<CaseFile>();
        foreach (var defendant in Defendants) {
            caseFiles.AddRange(defendant.CaseFiles);
        }

        return caseFiles;
    }
}
