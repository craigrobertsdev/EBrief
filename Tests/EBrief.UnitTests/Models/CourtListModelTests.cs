using EBrief.Models.Data;

namespace EBrief.UnitTests.Models;
public class CourtListModelTests
{
    [Fact]
    public void CombineDefendantCaseFiles_WhenCalled_CorrectlyOrdersCaseFiles()
    {
        var charges = new List<ChargeModel>
        {
            new ChargeModel { Date = new DateTime(2024, 1, 1) },
            new ChargeModel { Date = new DateTime(2023, 1, 1) },
            new ChargeModel { Date = new DateTime(2022, 1, 1) },
            new ChargeModel { Date = new DateTime(2021, 1, 1) }
        };

        // TODO: Finish this later.
    }

}
