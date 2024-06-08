using EBrief.WebClient.Models.UI;
using EBrief.WebClient.Pages;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.WebClient.Data;

public class LocalStorage(IServiceProvider serviceProvider)
{
    private readonly IJSRuntime _jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
    private static readonly JsonSerializerOptions _options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public async Task<CourtList?> GetCourtList(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("getCourtList", key);
        return json is null ? null : JsonSerializer.Deserialize<CourtList>(json, _options)!;
    }

    public async Task<bool> CheckCourtListExists(string key)
    {
        return await _jsRuntime.InvokeAsync<bool>("checkPreviousListExists", key);
    }

    public static string BuildKey(CourtListEntry entry)
    {
        return JsonSerializer.Serialize(entry); 
    }

    public async Task SaveCourtList(CourtList courtList)
    {
        var key = BuildKey(new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom));
        await _jsRuntime.InvokeVoidAsync("saveCourtList", [key, JsonSerializer.Serialize(courtList, _options)]);
    }

    public async Task DeleteCourtList(CourtListEntry entry)
    {
        var key = BuildKey(new CourtListEntry(entry.CourtCode, entry.CourtDate, entry.CourtRoom));
        await _jsRuntime.InvokeVoidAsync("removeCourtList", key);
    }

    public async Task<IEnumerable<CourtListEntry>> GetPreviousCourtLists()
    {
        var json = await _jsRuntime.InvokeAsync<string>("getPreviousCourtLists");
        return json is null ? new List<CourtListEntry>() : JsonSerializer.Deserialize<CourtListEntry[]>(json)!; }
}
