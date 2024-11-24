
using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.UI;

public record OrderCondition(int Number, string Text)
{
    internal OrderConditionModel ToDataModel()
    {
        return new OrderConditionModel
        {
            Number = Number,
            Condition = Text
        };
    }
}
