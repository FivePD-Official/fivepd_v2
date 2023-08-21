using System;
using FivePD.Server.Interfaces;
using FivePD.Server.Tests.Helpers;
using FivePD.Server.Utilities;
using NSubstitute;
using NUnit.Framework;

namespace FivePD.Server.Tests;

#pragma warning disable

[TestFixture]
internal class CallsignGeneratorTests
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
        var callsignGenerator = new CallsignGenerator("LN");
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.AreEqual($"{RandomLetterWithSeed0}8", callsign);
    }
    
    [Test]
    public void Generate_Static_Callsign()
    {
        var callsignGenerator = new CallsignGenerator("*L*N");
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.AreEqual("LN", callsign);
    }
    
    [Test]
    public void Generate_Dynamic_And_Static_Callsign()
    {
        var callsignGenerator = new CallsignGenerator("*LL*NN");
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.AreEqual($"L{RandomLetterWithSeed0}N8", callsign);
    }

    [Test]
    public void Generate_Dynamic_Callsign_With_W()
    {
        var callsignGenerator = new CallsignGenerator("N-W-NN");
        var callsign = callsignGenerator.GenerateCallsign();
        Assert.AreEqual($"7-V-75", callsign);
    }
}