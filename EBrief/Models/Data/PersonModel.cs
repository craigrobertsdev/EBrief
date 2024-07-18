using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Models.Data;

public class PersonModel
{
    [Key]
    [JsonIgnore]
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
