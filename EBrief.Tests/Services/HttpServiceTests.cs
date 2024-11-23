using EBrief.Shared.Helpers;
using EBrief.Shared.Services;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EBrief.Tests.Services;
public class HttpServiceTests
{
    [Fact]
    public async Task UpdateCasefiles_ShouldSendUpdatedCasefilesToServer()
    {
        // Arrange
        var text = "update text";
        var casefiles = new List<string>()
        {
            "CO2300012345" ,
            "CO2300012346"
        };

        var cfelUpdate = new HttpService.CfelUpdate(casefiles, text);
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(x => x.PostAsJsonAsync(
            $"{AppConstants.ApiBaseUrl}/update-cfels",
            cfelUpdate,
            new JsonSerializerOptions(),
            CancellationToken.None))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpService = new HttpService(mockHttpClient.Object);

        var result = await httpService.UpdateCasefileLogs(casefiles, text);

        Assert.True(result);
    }
}
