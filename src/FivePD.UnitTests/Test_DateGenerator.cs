using System;
using FivePD.Server;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using FivePD.UnitTests.Helpers;
using NSubstitute;
using NUnit.Framework;

#pragma warning disable

namespace FivePD.UnitTests;

[TestFixture]
internal class Test_DateGenerator
{
    [SetUp]
    public void Init()
    {
        var randomServiceMock = Substitute.For<IRandomService>();
        var random = new Random(0);
        randomServiceMock.Random.Returns(random);
        randomServiceMock.Next(Arg.Any<int>(), Arg.Any<int>()).Returns(x => random.Next((int)x[0], (int)x[1]));

        var s = new MockServiceProvider();
        s.AddService(randomServiceMock);

        ServiceLocator.SetServiceProvider(s);
    }

    [Test]
    public void GetRandomDate_Should_Generate_Date_Within_Specified_Range()
    {
        const int minYear = 2000;
        const int maxYear = 2025;

        var date = DateGenerator.GetRandomDate(minYear, maxYear);

        Assert.That(date.Year, Is.GreaterThanOrEqualTo(minYear));
        Assert.That(date.Year, Is.LessThan(maxYear));
        Assert.That(date.Month, Is.InRange(1, 12));
        Assert.That(date.Day, Is.InRange(1, 31));
    }

    [Test]
    public void GetRandomDate_Should_Handle_February_Leap_Year()
    {
        const int minYear = 2024;
        const int maxYear = 2025;

        DateTime? februaryDate = null;
        for (var i = 0; i < 100; i++)
        {
            var date = DateGenerator.GetRandomDate(minYear, maxYear);
            if (date.Month == 2)
            {
                februaryDate = date;
                break;
            }
        }

        Assert.That(februaryDate, Is.Not.Null, "Should have generated at least one February date");
        Assert.That(februaryDate?.Day, Is.InRange(1, 29), "February in leap year should have 1-29 days");
    }

    [Test]
    public void GetRandomDate_Should_Handle_30_Day_Months()
    {
        const int minYear = 2020;
        const int maxYear = 2021;

        var found30DayMonth = false;
        var monthsWith30Days = new[] { 4, 6, 9, 11 };
        for (var i = 0; i < 200; i++)
        {
            var date = DateGenerator.GetRandomDate(minYear, maxYear);
            if (Array.Exists(monthsWith30Days, x => x == date.Month))
            {
                Assert.That(date.Day, Is.InRange(1, 30), "30-day months should have 1-30 days");
                found30DayMonth = true;
            }
        }

        Assert.That(found30DayMonth, Is.True, "Should have generated at least one 30-day month date");
    }

    [Test]
    public void GetRandomDate_Should_Handle_31_Day_Months()
    {
        const int minYear = 2020;
        const int maxYear = 2021;

        var found31DayMonth = false;
        var monthsWith31Days = new[] { 1, 3, 5, 7, 8, 10, 12 };
        for (var i = 0; i < 200; i++)
        {
            var date = DateGenerator.GetRandomDate(minYear, maxYear);
            if (Array.Exists(monthsWith31Days, x => x == date.Month))
            {
                Assert.That(date.Day, Is.InRange(1, 31), "31-day months should have 1-31 days");
                found31DayMonth = true;
            }
        }

        Assert.That(found31DayMonth, Is.True, "Should have generated at least one 31-day month date");
    }

    [Test]
    public void GeneratePedLicenseAndVehicleDocumentDates_Expired_Should_Return_Past_Dates()
    {
        const bool isExpired = true;
        var now = DateTime.Now;

        var (issueDate, expirationDate) = DateGenerator.GeneratePedLicenseAndVehicleDocumentDates(isExpired);

        Assert.That(issueDate, Is.LessThanOrEqualTo(now), "Issue date should be in the past for expired documents");
        Assert.That(expirationDate, Is.LessThan(now), "Expiration date should be in the past for expired documents");
        Assert.That(expirationDate, Is.GreaterThanOrEqualTo(issueDate), "Expiration date should be after issue date");
    }

    [Test]
    public void GeneratePedLicenseAndVehicleDocumentDates_Valid_Should_Return_Future_Expiration()
    {
        const bool isExpired = false;
        var now = DateTime.Now;

        var (issueDate, expirationDate) = DateGenerator.GeneratePedLicenseAndVehicleDocumentDates(isExpired);

        Assert.That(expirationDate, Is.GreaterThan(now), "Expiration date should be in the future for valid documents");
        Assert.That(expirationDate, Is.GreaterThanOrEqualTo(issueDate), "Expiration date should be after issue date");
    }

    [Test]
    public void GeneratePedLicenseAndVehicleDocumentDates_Expired_Issue_Date_Should_Be_1_To_10_Years_Ago()
    {
        const bool isExpired = true;
        var now = DateTime.Now;
        var currentYear = now.Year;

        var (issueDate, _) = DateGenerator.GeneratePedLicenseAndVehicleDocumentDates(isExpired);

        Assert.That(issueDate.Year, Is.InRange(currentYear - 10, currentYear - 1),
            "Issue date for expired documents should be 1-10 years ago");
    }

    [Test]
    public void GeneratePedLicenseAndVehicleDocumentDates_Valid_Issue_Date_Should_Be_Within_5_Years()
    {
        const bool isExpired = false;
        var now = DateTime.Now;
        var currentYear = now.Year;

        var (issueDate, _) = DateGenerator.GeneratePedLicenseAndVehicleDocumentDates(isExpired);

        Assert.That(issueDate.Year, Is.InRange(currentYear - 5, currentYear),
            "Issue date for valid documents should be within the last 5 years");
    }
}