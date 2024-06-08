﻿using EBrief.WebClient.Data;
using EBrief.WebClient.Models;
using EBrief.WebClient.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace EBrief.WebClient.Pages;

public partial class Home
{
    [Inject]
    public LocalStorage LocalStorage { get; set; } = default!;
    public ElementReference? NewCourtListDialog { get; set; }
    public ElementReference? PreviousCourtListDialog { get; set; }
    public ElementReference? FeedbackDialog { get; set; }
    public string CaseFileNumbers { get; set; } = string.Empty;
    public DateTime? CourtDate { get; set; }
    public CourtCode? CourtCode { get; set; }
    public int? CourtRoom { get; set; }
    public string? _error;
    private List<CourtListEntry>? PreviousCourtLists { get; set; }
    private CourtListEntry? SelectedCourtList { get; set; }
    private string Feedback { get; set; } = string.Empty;

    private async Task OpenNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", NewCourtListDialog);
        }
    }
    private async Task OpenPreviousCourtListDialog()
    {
        PreviousCourtLists = (await LocalStorage.GetPreviousCourtLists())
            .Select(e => new CourtListEntry(e.CourtCode, e.CourtDate, e.CourtRoom))
            .ToList();
        if (PreviousCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", PreviousCourtListDialog);
        }
    }

    private async Task OpenFeedbackDialog()
    {
        if (FeedbackDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", FeedbackDialog);
        }
    }
    private void LoadPreviousCourtList()
    {
        if (SelectedCourtList is null)
        {
            _error = "Please select a court list.";
            return;
        }

        NavManager.NavigateTo($"/EBrief/court-list?courtCode={SelectedCourtList.CourtCode}&courtDate={SelectedCourtList.CourtDate}&courtRoom={SelectedCourtList.CourtRoom}");
    }

    private async Task DeletePreviousCourtList()
    {
        if (SelectedCourtList is null || PreviousCourtLists is null)
        {
            return;
        }

        try
        {
            await LocalStorage.DeleteCourtList(SelectedCourtList);
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
            return;
        }

        PreviousCourtLists.Remove(SelectedCourtList);
    }

    private async Task FetchCourtList()
    {
        _error = null;
        if (CourtDate is null)
        {
            _error = "Please select a court date.";
            return;
        }

        if (CourtCode is null)
        {
            _error = "Please select a court code.";
            return;
        }

        if (CourtRoom is null)
        {
            _error = "Please select a court room.";
            return;
        }

        var previousCourtList = await LocalStorage.GetCourtList(LocalStorage.BuildKey(new CourtListEntry(CourtCode.Value, CourtDate.Value, CourtRoom!.Value)));
        if (previousCourtList is not null)
        {
           _error = "Court list already exists.";
            return;
        }

        var caseFileNumbers = CaseFileNumbers.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        try
        {
            var caseFiles = DummyData.GenerateCaseFiles(caseFileNumbers);
            if (caseFiles is null)
            {
                _error = "Failed to fetch court list.";
                return;
            }

            var courtList = new CourtListModel
            {
                CaseFiles = caseFiles,
                CourtCode = CourtCode.Value,
                CourtDate = CourtDate.Value,
                CourtRoom = CourtRoom!.Value
            };

            courtList.CombineDefendantCaseFiles();

            try
            {
                await LocalStorage.SaveCourtList(courtList.ToUIModel());
            }
            catch (Exception e)
            {
                _error = e.InnerException?.Message ?? e.Message;
                return;
            }

            NavManager.NavigateTo($"/EBrief/court-list/?newList=true&courtCode={CourtCode}&courtRoom={CourtRoom}&courtDate={CourtDate}");
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
        }

    }

    private async Task SendFeedback()
    {

       if (string.IsNullOrWhiteSpace(Feedback))
        {
            _error = "Please enter feedback.";
            return;
        }

        try
        {
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://")
            Feedback = string.Empty;
            await CloseFeedbackDialog();
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
        }
    }

    private async Task CloseLoadNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", NewCourtListDialog);
        }
    }

    private async Task ClosePreviousCourtListDialog()
    {
        if (PreviousCourtListDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", PreviousCourtListDialog);
        }
    }

    private void HandleSelectCourtListEntry(CourtListEntry courtListEntry)
    {
        if (SelectedCourtList == courtListEntry)
        {
            SelectedCourtList = null;
            return;
        }

        SelectedCourtList = courtListEntry;
    }

    private async Task CloseFeedbackDialog()
    {
        if (FeedbackDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", FeedbackDialog);
        }
    }

    private List<int> CourtRooms = [2, 3, 12, 15, 17, 18, 19, 20, 22, 23, 24];
}
