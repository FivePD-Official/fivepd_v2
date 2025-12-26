using System;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Utilities;

#pragma warning disable

/// <summary>
/// Provides utility methods for generating random dates and date-related information.
/// </summary>
public static class DateGenerator
{
    private static readonly IRandomService RandomService = ServiceLocator.GetService<IRandomService>();

    /// <summary>
    /// Generates a random date between the specified minimum and maximum years.
    /// The method ensures valid dates by considering month lengths and leap years.
    /// </summary>
    /// <param name="minYear">The minimum year for the generated date (inclusive).</param>
    /// <param name="maxYear">The maximum year for the generated date (exclusive).</param>
    /// <returns>A random DateTime between the specified years with valid day/month combinations.</returns>
    /// <remarks>
    /// This method handles month-specific day limits:
    /// - February: 29 days (accounts for leap years)
    /// - April, June, September, November: 30 days
    /// - All other months: 31 days
    /// </remarks>
    public static DateTime GetRandomDate(int minYear, int maxYear)
    {
        var monthsWith30Days = new[] { 4, 6, 9, 11 };
        var year = RandomService.Next(minYear, maxYear);
        var month = RandomService.Next(1, 13);
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

        var day = RandomService.Next(1, maxDay);

        return new DateTime(year, month, day);
    }

    /// <summary>
    /// Generates random issue and expiration dates for ped licenses and vehicle documents.
    /// The dates are generated based on whether the document should be expired or valid.
    /// </summary>
    /// <param name="isExpired">True to generate dates for an expired document, false for a valid document.</param>
    /// <returns>A tuple containing the issue date and expiration date.</returns>
    /// <remarks>
    /// When isExpired is true:
    /// - Issue date is generated from 10 years ago to 1 year ago
    /// - Expiration date is typically 1-10 years after issue date, but adjusted to be in the past
    /// 
    /// When isExpired is false:
    /// - Issue date is generated from 5 years ago today
    /// - Expiration date is 1-10 years after issue date, but adjusted to be in the future
    /// </remarks>
    public static (DateTime IssueDate, DateTime ExpirationDate) GeneratePedLicenseAndVehicleDocumentDates(bool isExpired)
    {
        var now = DateTime.Now;
        DateTime issueDate;
        DateTime expirationDate;

        if (isExpired)
        {
            issueDate = GetRandomDate(now.Year - 10, now.Year - 1);
            expirationDate = issueDate.AddYears(RandomService.Next(1, Math.Max(2, now.Year - issueDate.Year)));
            if (expirationDate >= now)
            {
                expirationDate = now.AddDays(-RandomService.Next(1, 365));
            }
        }
        else
        {
            issueDate = GetRandomDate(now.Year - 5, now.Year);
            expirationDate = issueDate.AddYears(RandomService.Next(1, 11));
            if (expirationDate <= now)
            {
                expirationDate = now.AddYears(RandomService.Next(1, 11));
            }
        }

        return (issueDate, expirationDate);
    }
}