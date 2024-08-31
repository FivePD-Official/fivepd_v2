using System;
using System.Collections.Generic;
using FivePD.Server;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Utilities;
using FivePD.UnitTests.Helpers;
using NSubstitute;
using NUnit.Framework;

#pragma warning disable

namespace FivePD.UnitTests;

[TestFixture]
internal class Test_CallsignGenerator
{
    private const string RandomLetterWithSeed0 = "S";

    [SetUp]
    public void Init()
    {
        var randomServiceMock = Substitute.For<IRandomService>();

        var random = new Random(0);
        randomServiceMock.Random.Returns(random);

        var s = new MockServiceProvider();
        s.AddService(randomServiceMock);

        ServiceLocator.SetServiceProvider(s);
    }

    [Test]
    public void Generate_Dynamic_Callsign()
    {
        var callsignGenerator = new CallsignGenerator(new List<CallsignFormatItem>
        {
            new()
            {
                Type = CallsignFormatItemType.RandomLetter,
            },
            new()
            {
                Type = CallsignFormatItemType.RandomNumber,
            },
        });
        var callsign = callsignGenerator.GenerateCallsign().GetGeneratedItemValues();
        Assert.That(callsign, Is.EqualTo(new List<string> { RandomLetterWithSeed0, "8" }));
    }

    [Test]
    public void Generate_Static_Callsign()
    {
        var callsignGenerator = new CallsignGenerator(new List<CallsignFormatItem>
        {
            new()
            {
                Type = CallsignFormatItemType.FixedCharacter,
                Value = "L",
            },
            new()
            {
                Type = CallsignFormatItemType.FixedCharacter,
                Value = "N",
            },
        });
        var callsign = callsignGenerator.GenerateCallsign().GetGeneratedItemValues();
        Assert.That(callsign, Is.EqualTo(new List<string> { "L", "N" }));
    }

    [Test]
    public void Generate_Dynamic_And_Static_Callsign()
    {
        var callsignGenerator = new CallsignGenerator(new List<CallsignFormatItem>
        {
            new()
            {
                Type = CallsignFormatItemType.FixedCharacter,
                Value = "L",
            },
            new()
            {
                Type = CallsignFormatItemType.RandomLetter,
            },
            new()
            {
                Type = CallsignFormatItemType.FixedCharacter,
                Value = "N",
            },
            new()
            {
                Type = CallsignFormatItemType.RandomNumber,
            },
        });
        var callsign = callsignGenerator.GenerateCallsign().GetGeneratedItemValues();
        Assert.That(callsign, Is.EqualTo(new List<string> { "L", RandomLetterWithSeed0, "N", "8" }));
    }

    [Test]
    public void Generate_Dynamic_Callsign_With_W()
    {
        var callsignGenerator = new CallsignGenerator(new List<CallsignFormatItem>
        {
            new()
            {
                Type = CallsignFormatItemType.RandomNumber,
            },
            new()
            {
                Type = CallsignFormatItemType.Dash,
            },
            new()
            {
                Type = CallsignFormatItemType.RandomLetter,
            },
            new()
            {
                Type = CallsignFormatItemType.Dash,
            },
            new()
            {
                Type = CallsignFormatItemType.RandomNumber,
            },
            new()
            {
                Type = CallsignFormatItemType.RandomNumber,
            },
        });
        var callsign = callsignGenerator.GenerateCallsign().GetGeneratedItemValues();
        Assert.That(callsign, Is.EqualTo(new List<string> { "7", "-", "V", "-", "7", "5" }));
    }

    [Test]
    public void Word_Type_Should_Return_The_Word_PLACEHOLDER()
    {
        var callsignGenerator = new CallsignGenerator(new List<CallsignFormatItem>
        {
            new()
            {
                Type = CallsignFormatItemType.Word,
            },
        });
        var callsign = callsignGenerator.GenerateCallsign().GetGeneratedItemValues();
        Assert.That(callsign, Is.EqualTo(new List<string> { "PLACEHOLDER" }));
    }
    
    [Test]
    public void BuildCallsign_Should_Return_The_String_Representation_Of_The_Callsign_Values()
    {
        var callsignGenerator = new CallsignGenerator(new List<CallsignFormatItem>
        {
            new()
            {
                Type = CallsignFormatItemType.RandomLetter,
            },
            new()
            {
                Type = CallsignFormatItemType.RandomNumber,
            },
        });
        var callsign = callsignGenerator.GenerateCallsign().BuildCallsign();
        Assert.That(callsign, Is.EqualTo($"{RandomLetterWithSeed0}8"));
    }
}