﻿using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;
public class DefendantModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<BailAgreementModel> BailAgreements { get; set; } = [];
    public List<InterventionOrderModel> InterventionOrders { get; set; } = [];
    public string? OffenderHistory { get; set; }
    public Defendant ToUIModel()
    {
        return new Defendant
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            CaseFiles = [],
            BailAgreements = BailAgreements.Select(ba => ba.ToUIModel()).ToList(),
            InterventionOrders = InterventionOrders.Select(io => io.ToUIModel()).ToList()
        };
    }
}