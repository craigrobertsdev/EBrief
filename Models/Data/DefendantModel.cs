using CourtSystem.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace CourtSystem.Models.Data;
public class DefendantModel {
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Defendant ToUIModel() {
        return new Defendant {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            CaseFiles = []
        };
    }
}
