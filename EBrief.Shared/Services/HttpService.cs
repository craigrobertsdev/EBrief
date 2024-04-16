using EBrief.Shared.Helpers;
using EBrief.Shared.Models.Data;
using System.Net.Http.Json;

namespace EBrief.Shared.Services;
public class HttpService
{
    public async Task<List<CaseFileModel>> GetCaseFiles(List<string> caseFileNumbers)
    {
        var response = await new HttpClient().PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/generate-case-files", caseFileNumbers);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get case files.");
        }

        return await response.Content.ReadFromJsonAsync<List<CaseFileModel>>() ?? [];


    }
}
