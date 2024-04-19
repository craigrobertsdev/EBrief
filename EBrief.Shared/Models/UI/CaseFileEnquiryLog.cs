﻿using System.ComponentModel.DataAnnotations;

namespace EBrief.Shared.Models.UI;
public class CaseFileEnquiryLog
{
    [Key]
    public int Id { get; set; }
    public string EntryText { get; set; } = string.Empty;
    public string EnteredBy { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }
}