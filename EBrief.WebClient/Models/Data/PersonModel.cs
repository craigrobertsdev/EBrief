using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;

public class PersonModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public Person ToUIModel()
    {
        return new Person
        {
            FirstName = FirstName,
            LastName = LastName,
            DateOfBirth = DateOfBirth
        };
    }
}
