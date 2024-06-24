using EBrief.Helpers;
using EBrief.Models.Data;
using System.Net.Http;
using System.Net.Http.Json;

namespace EBrief.Services;
public class HttpService
{
    public async Task<List<CaseFileModel>> GetCaseFiles(List<string> caseFileNumbers, DateTime courtDate)
    {
        var body = new CourtListDto(caseFileNumbers, courtDate);
        var response = await new HttpClient().PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/add-custody", body);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get case files.");
        }

        var json = await response.Content.ReadAsStringAsync();

        return await response.Content.ReadFromJsonAsync<List<CaseFileModel>>() ?? [];
    }
}
