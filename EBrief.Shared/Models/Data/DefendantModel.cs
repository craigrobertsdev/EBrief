﻿using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBrief.Shared.Models.Data;
public class DefendantModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int ListStart { get; set; }
    public int ListEnd { get; set; }
    public List<BailAgreementModel> BailAgreements { get; set; } = [];
    public List<InterventionOrderModel> InterventionOrders { get; set; } = [];
    public string? OffenderHistory { get; set; }
    public Defendant ToUiModel()
    {
        return new Defendant
        {
            Id = Id,
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            Address = Address,
            Phone = Phone,
            Email = Email,
            Casefiles = [],
            ListStart = ListStart,
            ListEnd = ListEnd,
            OffenderHistory = OffenderHistory,
            BailAgreements = BailAgreements.Select(ba => ba.ToUIModel()).ToList(),
            InterventionOrders = InterventionOrders.Select(io => io.ToUIModel()).ToList()
        };
    }
}

public static class DefendantModelExtensions
{
    public static List<Defendant> ToUiModels(this IEnumerable<DefendantModel> models)
    {
        return models.Select(model => model.ToUiModel()).ToList();
    }
}
