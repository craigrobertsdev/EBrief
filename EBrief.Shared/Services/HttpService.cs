using EBrief.Shared.Helpers;
using EBrief.Shared.Models.Data;
using System.Net.Http.Json;

namespace EBrief.Shared.Services;
public class HttpService
{
    private readonly HttpClient _client;

    public async Task<List<CaseFileModel>> GetCaseFiles(List<string> caseFileNumbers, DateTime courtDate)
    {
        var body = new CourtListDto(caseFileNumbers, courtDate);
        var response = await _client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/add-custody", body);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get case files.");
        }

        return await response.Content.ReadFromJsonAsync<List<CaseFileModel>>() ?? [];
    }

    public HttpService(HttpClient client)
    {
        _client = client;
    }
}
