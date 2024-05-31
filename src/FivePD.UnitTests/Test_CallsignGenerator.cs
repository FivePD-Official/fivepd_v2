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
        var callsignGenerator = new TestCallsignGenerator(new List<CallsignFormatItem>
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
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.That(callsign, Is.EqualTo($"{RandomLetterWithSeed0}8"));
    }
    
    [Test]
    public void Generate_Static_Callsign()
    {
        var callsignGenerator = new TestCallsignGenerator(new List<CallsignFormatItem>
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
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.That(callsign, Is.EqualTo("LN"));
    }
    
    [Test]
    public void Generate_Dynamic_And_Static_Callsign()
    {
        // var callsignGenerator = new CallsignGenerator("*LL*NN");
        var callsignGenerator = new TestCallsignGenerator(new List<CallsignFormatItem>
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
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.That(callsign, Is.EqualTo($"L{RandomLetterWithSeed0}N8"));
    }

    [Test]
    public void Generate_Dynamic_Callsign_With_W()
    {
        // var callsignGenerator = new CallsignGenerator("N-W-NN");
        var callsignGenerator = new TestCallsignGenerator(new List<CallsignFormatItem>
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
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.That(callsign, Is.EqualTo("7-V-75"));
    }
}