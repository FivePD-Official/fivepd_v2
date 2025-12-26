using System.Collections.Generic;
using FivePD.Server.Interfaces;
using FivePD.Server.Services;
using NSubstitute;
using NUnit.Framework;

#pragma warning disable

namespace FivePD.UnitTests;

[TestFixture]
internal class Test_ProbabilityService
{
    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Return_First_Outcome_When_Random_Value_Is_Zero()
    {
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(0);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, string Outcome)> 
        {
            (50, "First"),
            (50, "Second")
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo("First"));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Return_Second_Outcome_When_Random_Value_Is_In_Second_Range()
    {
        // Arrange
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(50);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, string Outcome)> 
        {
            (30, "First"),
            (70, "Second")
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo("Second"));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Return_Default_When_No_Outcome_Matches()
    {
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(1);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, string Outcome)> 
        {
            (0, "First"),
            (0, "Second")
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo(null));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Handle_Single_Outcome()
    {
        // Arrange
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(50);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, string Outcome)> 
        {
            (100, "Only")
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo("Only"));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Handle_Empty_List()
    {
        // Arrange
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(50);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, string Outcome)>();

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo(null));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Handle_Integer_Outcomes()
    {
        // Arrange
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(80);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, int Outcome)>
        {
            (25, 1),
            (25, 2),
            (25, 3),
            (25, 4)
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Handle_Complex_Objects()
    {
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(40);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var obj1 = new TestObject { Id = 1, Name = "First" };
        var obj2 = new TestObject { Id = 2, Name = "Second" };
        var obj3 = new TestObject { Id = 3, Name = "Third" };

        var probabilities = new List<(int Probability, TestObject Outcome)>
        {
            (10, obj1),
            (20, obj2),
            (70, obj3)
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo(obj3));
        Assert.That(result?.Id, Is.EqualTo(3));
        Assert.That(result?.Name, Is.EqualTo("Third"));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Handle_Probabilities_That_Dont_Sum_To_100()
    {
        var randomServiceMock = Substitute.For<IRandomService>();
        randomServiceMock.GetRandomChance.Returns(60);
        
        var probabilityService = new ProbabilityService(randomServiceMock);
        
        var probabilities = new List<(int Probability, string Outcome)> 
        {
            (20, "First"),
            (30, "Second")
        };

        var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);

        Assert.That(result, Is.EqualTo(null));
    }

    [Test]
    public void SelectOutcomeBasedOnCumulativeChance_Should_Work_With_Real_ProbabilityService()
    {
        var randomService = new RandomService();
        var probabilityService = new ProbabilityService(randomService);
        
        var probabilities = new List<(int Probability, string Outcome)> 
        {
            (50, "First"),
            (50, "Second")
        };

        var results = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            var result = probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);
            if (result != null)
            {
                results.Add(result);
            }
        }

        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results, Has.Some.EqualTo("First").Or.EqualTo("Second"));
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}