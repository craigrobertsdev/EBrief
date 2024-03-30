﻿using CourtSystem.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace CourtSystem.Models.Data;
public class CourtListModel {
    [Key]
    public int Id { get; set; }
    public List<CaseFileModel> CaseFiles { get; set; } = [];

    public CourtList ToUIModel() {
        var defendants = new List<Defendant>();
        foreach (var caseFileModel in CaseFiles) {
            var caseFile = caseFileModel.ToUIModel();
            caseFile.GenerateInformationFromCharges();
            if (!defendants.Any(d => d.Id == caseFile.Defendant.Id)) {
                defendants.Add(caseFile.Defendant);
                caseFile.Defendant.CaseFiles.Add(caseFile);
            }
            else {
                defendants.First(d => d.Id == caseFile.Defendant.Id).CaseFiles.Add(caseFile);
            }
        }

        return new CourtList {
            Defendants = defendants
        };
    }

    // To handle the situation where a defendant has multiple case files
    // Every object from the server has a different reference, so we need to combine them
    public void CombineDefendantCaseFiles() {
        var defendants = CaseFiles.Select(cf => cf.Defendant).DistinctBy(d => d.Id).ToList();
        foreach (var caseFile in CaseFiles) {
            caseFile.Defendant = defendants.First(d => d.Id == caseFile.Defendant.Id);
        }
    }
}
