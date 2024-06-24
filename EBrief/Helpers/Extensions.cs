namespace EBrief.Helpers;
public static class Extensions
{
    public static string ToDisplayTime(this DateTime dateTime)
    {
        var ordinal = dateTime.Hour >= 12 ? "pm" : "am";
        var hour = dateTime.Hour > 12 ? dateTime.Hour - 12 : dateTime.Hour;
        var minute = dateTime.Minute < 10 ? $"0{dateTime.Minute}" : dateTime.Minute.ToString();
        return $"{hour}:{minute}{ordinal}";
    }
}