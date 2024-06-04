using EBrief.WebClient.Models;
using EBrief.WebClient.Models.Data;
using EBrief.WebClient.Models.UI;
using EBrief.WebClient.Pages;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBrief.WebClient.Data;

public class LocalStorage
{
    private readonly IJSRuntime _jsRuntime = default!;
    public LocalStorage(IServiceProvider serviceProvider)
    {
        _jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>(); 
    }

    public async Task<CourtList?> GetCourtList(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
        return json is null ? null : JsonSerializer.Deserialize<CourtList>(json, options)!;
    }

    public string BuildKey(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        return courtCode.ToString() + courtDate.ToString() + courtRoom;
    }

    public async Task SaveCourtList(CourtList courtList)
    {
        var key = BuildKey(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(courtList, options));
    }

    public async Task DeleteCourtList(CourtListEntry entry)
    {
        var key = BuildKey(entry.CourtCode, entry.CourtDate, entry.CourtRoom);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task<List<CourtListEntry>> GetPreviousCourtLists()
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "previousCourtLists");
        return json is null ? new List<CourtListEntry>() : JsonSerializer.Deserialize<List<CourtListEntry>>(json)!; }
}
