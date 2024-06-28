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

    public static string ToDisplayString(this TimeSpan timeSpan)
    {
        var days = timeSpan.Days > 1 ? $"{timeSpan.Days} days" 
            : timeSpan.Days == 1 ? "1 day" 
            : null;
        var hours = timeSpan.Hours > 0 ? $"{timeSpan.Hours} hours" : null;
        var minutes = timeSpan.Minutes > 0 ? $"{timeSpan.Minutes} minutes" : null;

        return $"{days} {hours} {minutes}";
    }
}