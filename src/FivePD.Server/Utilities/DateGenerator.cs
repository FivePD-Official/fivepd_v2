using System;

namespace FivePD.Server.Utilities;

#pragma warning disable

public static class DateGenerator
{
    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

    public static DateTime GetRandomDate(int minYear, int maxYear)
    {
        var monthsWith30Days = new[] { 4, 6, 9, 11 };
        var year = Random.Next(minYear, maxYear);
        var month = Random.Next(1, 13);
        int maxDay;

        if (month == 2)
        {
            maxDay = 29;
        }
        else if (Array.Exists(monthsWith30Days, x => x == month))
        {
            maxDay = 31;
        }
        else
        {
            maxDay = 32;
        }

        var day = Random.Next(1, maxDay);

        return new DateTime(year, month, day);
    }
}