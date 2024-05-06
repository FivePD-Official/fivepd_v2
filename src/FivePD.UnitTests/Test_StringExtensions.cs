using System.Collections.Generic;
using FivePD.Common.Extensions;
using NUnit.Framework;

namespace FivePD.Server.Tests;

#pragma warning disable

[TestFixture]
internal class Test_StringExtensions
{
    [Test]
    public void ReplaceParams_Without_Parameters()
    {
        var actual = "lorem ipsum {{toReplace}} sit amet".ReplaceParams(new Dictionary<string, string>());
        Assert.That(actual, Is.EqualTo("lorem ipsum {{toReplace}} sit amet"));
    }

    [Test]
    public void ReplaceParams_With_Parameters()
    {
        var actual = "lorem ipsum {{toReplace}} sit amet".ReplaceParams(new Dictionary<string, string>
        {
            ["toReplace"] = "dolor"
        });
        Assert.That(actual, Is.EqualTo("lorem ipsum dolor sit amet"));
    }
    
    [Test]
    public void ReplaceParams_With_Parameters_Key()
    {
        var actual = "lorem ipsum {{keyToReplace(dolor)}} sit amet".ReplaceParams(new Dictionary<string, string>
        {
            ["keyToReplace"] = "~b~",
        });
        Assert.That(actual, Is.EqualTo("lorem ipsum ~b~dolor sit amet"));
    }
    
    [Test]
    public void ReplaceParams_With_Multiple_Parameters()
    {
        var actual = "{{loremIpsum}} {{keyToReplace(dolor)}} {{replaceToSit}} {{colorRed(amet)}}".ReplaceParams(new Dictionary<string, string>
        {
            ["loremIpsum"] = "lorem ipsum",
            ["keyToReplace"] = "~b~",
            ["replaceToSit"] = "sit",
            ["colorRed"] = "~r~",
        });
        Assert.That(actual, Is.EqualTo("lorem ipsum ~b~dolor sit ~r~amet"));
    }

    [Test]
    public void ToAcronym_Single_Word()
    {
        Assert.That("A", Is.EqualTo("ABC".ToAcronym()));
    }

    [Test]
    public void ToAcronym_Multiple_Words()
    {
        Assert.That("AD", Is.EqualTo("Abc Def".ToAcronym()));
    }

    [Test]
    public void ToAcronym_Case_Conversion()
    {
        Assert.That("L", Is.EqualTo("lowercase".ToAcronym()));
    }
}