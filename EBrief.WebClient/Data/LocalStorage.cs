using EBrief.Shared.Data;
using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.WebClient.Data;

public class LocalStorage(IServiceProvider serviceProvider) : IDataAccess
{
    private readonly IJSRuntime _jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
    private static readonly JsonSerializerOptions _options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public static string BuildKey(CourtListModel courtList)
    {
        var entry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
        return JsonSerializer.Serialize(entry);
    }

    public static string BuildKey(CourtList courtList)
    {
        var entry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
        return JsonSerializer.Serialize(entry);
    }

    public static string BuildKey(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var entry = new CourtListEntry(courtCode, courtDate, courtRoom);
        return JsonSerializer.Serialize(entry);
    }

    public static string BuildKey(CourtListEntry entry)
    {
        return JsonSerializer.Serialize(entry);
    }

    public async Task CreateCourtList(CourtListModel courtList)
    {
        var key = BuildKey(courtList);
        await _jsRuntime.InvokeVoidAsync("saveCourtList", [key, JsonSerializer.Serialize(courtList, _options)]);
    }

    public async Task<List<CourtListEntry>> GetSavedCourtLists()
    {
        var json = await _jsRuntime.InvokeAsync<string>("getPreviousCourtLists");
        return json is null ? new List<CourtListEntry>() : JsonSerializer.Deserialize<CourtListEntry[]>(json)!.ToList();
    }

    public async Task UpdateCourtList(CourtList courtList)
    {
        var key = BuildKey(courtList);
        var courtListModel = await GetCourtList(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);

        foreach (var caseFile in courtList.GetCaseFiles())
        {
            courtListModel!.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber).Notes = caseFile.Notes.Text;
        }

        await _jsRuntime.InvokeVoidAsync("saveCourtList", [key, JsonSerializer.Serialize(courtListModel, _options)]);
    }

    public async Task AddCaseFiles(List<CaseFileModel> newCaseFiles, CourtList courtList)
    {
        throw new NotImplementedException();
    }

    public async Task<CourtListModel?> GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var key = BuildKey(courtCode, courtDate, courtRoom);
        var json = await _jsRuntime.InvokeAsync<string>("getCourtList", key);
        if (json is null) return null;
        var courtList = JsonSerializer.Deserialize<CourtListModel>(json, _options)!;
        return courtList;
    }

    public async Task<bool> CheckCourtListExists(CourtListEntry courtListEntry)
    {
        var key = BuildKey(courtListEntry);
        return await _jsRuntime.InvokeAsync<bool>("checkPreviousListExists", key);
    }

    public async Task DeleteCourtList(CourtListEntry courtList)
    {
        var key = BuildKey(new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom));
        await _jsRuntime.InvokeVoidAsync("removeCourtList", key);
    }

    public Task UpdateCaseFiles(IEnumerable<string> caseFileNumbers, string updateText)
    {
        throw new NotImplementedException();
    }

    public Task UpdateDocumentName(string fileName, string newFileName)
    {
        throw new NotImplementedException();
    }
}
