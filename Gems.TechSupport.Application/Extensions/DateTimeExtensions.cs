namespace Gems.TechSupport.Application.Extensions;

public static class DateTimeExtensions
{
    public static readonly string DateTimeFormat;
    public static readonly TimeZoneInfo MoscowTimeZone;

    static DateTimeExtensions()
    {
        DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
        MoscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
    }

    public static string ToRussianStdDateTime(this DateTime dateTime)
    {
        var dateTimeInUtc = dateTime.ToUniversalTime();
        var stdDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeInUtc, MoscowTimeZone);

        return stdDateTime.ToString(DateTimeFormat);
    }
}
