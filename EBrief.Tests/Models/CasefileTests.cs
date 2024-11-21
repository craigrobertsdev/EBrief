using EBrief.Shared.Models.UI;

namespace EBrief.Tests.Models;
public class CasefileTests
{
    [Fact]
    public void Update_UpdatesCfel()
    {
        // Arrange
        var casefile = new Casefile()
        {
            Cfel = new List<CasefileEnquiryLogEntry>
            {
                new CasefileEnquiryLogEntry("Entry 1", "User 1"),
                new CasefileEnquiryLogEntry("Entry 2", "User 2")
            }
        };

        var updatedCasefile = new Casefile()
        {
            Cfel = new List<CasefileEnquiryLogEntry>
            {
                new CasefileEnquiryLogEntry("Entry 1", "User 1", DateTime.Now.AddDays(-1)),
                new CasefileEnquiryLogEntry("Entry 2", "User 2", DateTime.Now.AddDays(-2)),
                new CasefileEnquiryLogEntry("Entry 3", "User 3", DateTime.Now.AddDays(-3)),
            }
        };

        // Act 
        casefile.Update(updatedCasefile);

        // Assert
        Assert.Equal(3, casefile.Cfel.Count);
        Assert.Equal("Entry 3", casefile.Cfel[2].EntryText);
    }

    [Fact]
    public void Update_OrdersCfelEntriesChronologically()
    {
        var casefile = new Casefile()
        {
            Cfel = new List<CasefileEnquiryLogEntry>
            {
                new CasefileEnquiryLogEntry("Entry 1", "User 1", DateTime.Now.AddDays(-2)),
                new CasefileEnquiryLogEntry("Entry 2", "User 2", DateTime.Now.AddDays(-1)),
            }
        };

        var updatedCasefile = new Casefile()
        {
            Cfel = new List<CasefileEnquiryLogEntry>
            {
                new CasefileEnquiryLogEntry("Entry 1", "User 1", DateTime.Now.AddDays(-2)),
                new CasefileEnquiryLogEntry("Entry 2", "User 2", DateTime.Now.AddDays(-1)),
                new CasefileEnquiryLogEntry("Entry 3", "User 3", DateTime.Now.AddDays(-3)),
            }
        };

        // Act 
        casefile.Update(updatedCasefile);

        // Assert
        Assert.Equal(casefile.Cfel[0].EntryDate.Day, DateTime.Now.AddDays(-1).Day);
        Assert.Equal(casefile.Cfel[1].EntryDate.Day, DateTime.Now.AddDays(-2).Day);
        Assert.Equal(casefile.Cfel[2].EntryDate.Day, DateTime.Now.AddDays(-3).Day);
    }

    [Fact]
    public void Update_UpdatesCasefileProperties()
    {
        var casefile = new Casefile()
        {
            FactsOfCharge = "Facts",
            Information = new Information()
            { Charges  = [new InformationEntry(1, "Charge 1")] },
            Charges = new List<Charge>
            {
                new Charge() { ChargeWording = "Charge 1", Sequence = 1 },
            },
            CaseFileDocuments = [],
            OccurrenceDocuments = [],
            TimeInCustody = TimeSpan.FromDays(1)
        };
        var updatedCasefile = new Casefile()
        {
            FactsOfCharge = "Updated facts",
            Information = new Information()
            { Charges  = [new InformationEntry(1, "Charge 1"), new InformationEntry(2, "Charge 2")] },
            Charges = new List<Charge>
            {
                new Charge() { ChargeWording = "Charge 1", Sequence = 1 },
                new Charge() { ChargeWording = "Charge 2", Sequence = 2 },
            },
            CaseFileDocuments = new List<Document>
            {
                new Document()
            },
            OccurrenceDocuments = new List<Document>
            {
                new Document()
            },
            TimeInCustody = TimeSpan.FromDays(2)
        };
        // Act 
        casefile.Update(updatedCasefile);
        // Assert
        Assert.Equal("Updated facts", casefile.FactsOfCharge);
        Assert.Equal(2, casefile.Information.Charges.Count);
        Assert.Equal(2, casefile.Charges.Count);
        Assert.Single(casefile.CaseFileDocuments);
        Assert.Single(casefile.OccurrenceDocuments);
        Assert.Equal(TimeSpan.FromDays(2), casefile.TimeInCustody);
    }
}
