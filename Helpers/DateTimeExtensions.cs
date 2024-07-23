namespace CloudCtrl.Kubernetes.Agent.Helpers;

public static class DateTimeExtensions
{
    // Rounds a DateTime down to the nearest time interval.
    // E.g. if the interval is 30 mins and the current time is 12:43, the time will be rounded
    // down to 12:30. If the interval is 1h the time will be rounded down to 12:00.
    public static DateTime RoundToNearestInterval(this DateTime dateTime, TimeSpan interval)
    {
        var ticks = dateTime.Ticks / interval.Ticks;

        return new DateTime(ticks * interval.Ticks, dateTime.Kind);
    }
}