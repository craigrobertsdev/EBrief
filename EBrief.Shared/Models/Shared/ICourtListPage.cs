﻿using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Models.Shared;
public interface ICourtListPage
{
    Defendant? ActiveDefendant { get; set; }
    event Func<Task> OnDefendantChanged;
    event Action<CourtSitting>? OnCourtSessionAdded;
    bool IsSelected(Defendant defendant);
    void ActivateDefendant(Defendant defendant);
}
