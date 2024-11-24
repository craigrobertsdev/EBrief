using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.UI;

public record Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public PersonModel ToDataModel()
    {
        return new PersonModel()
        {
            FirstName = FirstName,
            LastName = LastName,
            DateOfBirth = DateOfBirth
        };
    }
}
