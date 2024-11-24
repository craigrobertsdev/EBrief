﻿using EBrief.Shared.Helpers;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;
using System.Net.Http.Json;
using System.Text.Json;

namespace EBrief.Shared.Services;
public class HttpService
{
    private readonly HttpClient _client;

    public async Task<List<CasefileModel>> GetCasefiles(List<string> casefileNumbers, DateTime courtDate)
    {
        var body = new CourtListDto(casefileNumbers, courtDate);
        var response = await _client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/add-custody", body);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get case files.");
        }

        return await response.Content.ReadFromJsonAsync<List<CasefileModel>>() ?? [];
    }

    public async Task<bool> UpdateCasefileLogs(IEnumerable<string> casefiles, CasefileEnquiryLogEntry entry)
    {
        var result = await _client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/update-cfels",
            new CfelUpdate(casefiles, entry));

        return result.IsSuccessStatusCode;
    }

    public async Task<List<CasefileModel>> RefreshData(IEnumerable<string> casefileNumbers)
    {
        var content = new CasefileUpdateContent(casefileNumbers);
        var result = await _client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/refresh", content);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception("Failed to refresh data.");
        }

        var updatedCasefiles = await result.Content.ReadFromJsonAsync<List<CasefileModel>>();
        if (updatedCasefiles == null)
        {
            throw new Exception("Failed to refresh data.");
        }

        return updatedCasefiles;
    }

    public HttpService(HttpClient client)
    {
        _client = client;
    }

    internal struct CfelUpdate
    {
        public IEnumerable<string> CasefileNumbers { get; }
        public CasefileEnquiryLogEntry Entry { get; }

        public CfelUpdate(IEnumerable<string> casefileNumbers, CasefileEnquiryLogEntry entry)
        {
            CasefileNumbers = casefileNumbers;
            Entry = entry;
        }
    }
    internal record CasefileUpdateContent(IEnumerable<string> CasefileNumbers);
}
