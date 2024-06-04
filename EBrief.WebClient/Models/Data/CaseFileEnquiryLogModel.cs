﻿using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;
public class CaseFileEnquiryLogModel
{
    public int Id { get; set; }
    public string EntryText { get; set; } = string.Empty;
    public string EnteredBy { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }

    public CaseFileEnquiryLog ToUIModel()
    {
        return new CaseFileEnquiryLog
        {
            Id = Id,
            EntryText = EntryText,
            EnteredBy = EnteredBy,
            EntryDate = EntryDate
        };
    }
}
