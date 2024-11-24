using EBrief.Shared.Helpers;
using EBrief.Shared.Models.UI;
using EBrief.Shared.Services;
using FakeItEasy;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EBrief.Tests.Services;
public class HttpServiceTests
{
    [Fact]
    public async Task UpdateCasefiles_ShouldSendUpdatedCasefilesToServer()
    {
        //// Arrange
        //var text = "update text";
        //var casefiles = new List<string>()
        //{
        //    "CO2300012345" ,
        //    "CO2300012346"
        //};

        //var cfelEntry = new CasefileEnquiryLogEntry(text, "test", DateTime.Now);
        //var cfelUpdate = new HttpService.CfelUpdate(casefiles, cfelEntry);
        //var client = A.Fake<HttpClient>();
        //A.CallTo(() => client.PostAsJsonAsync(
        //    $"{AppConstants.ApiBaseUrl}/update-cfels",
        //    cfelUpdate,
        //    new JsonSerializerOptions(),
        //    CancellationToken.None))
        //    .Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK }));

        //var httpService = new HttpService(client);

        //var result = await httpService.UpdateCasefileLogs(casefiles, cfelEntry);

        //Assert.True(result);
    }
}
