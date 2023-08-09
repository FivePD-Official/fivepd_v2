using System.Collections.Generic;
using FivePD.Common.Extensions;
using NUnit.Framework;

namespace FivePD.Server.Tests;

#pragma warning disable

[TestFixture]
internal class StringExtensions
{
    [Test]
    public void ReplaceParams_Without_Parameters()
    {
        var actual = "lorem ipsum {{toReplace}} sit amet".ReplaceParams(new Dictionary<string, string>());
        Assert.AreEqual("lorem ipsum {{toReplace}} sit amet", actual);
    }

    [Test]
    public void ReplaceParams_With_Parameters()
    {
        var actual = "lorem ipsum {{toReplace}} sit amet".ReplaceParams(new Dictionary<string, string>
        {
            ["toReplace"] = "dolor"
        });
        Assert.AreEqual("lorem ipsum dolor sit amet", actual);
    }
    
    [Test]
    public void ReplaceParams_With_Parameters_Key()
    {
        var actual = "lorem ipsum {{keyToReplace(dolor)}} sit amet".ReplaceParams(new Dictionary<string, string>
        {
            ["keyToReplace"] = "~b~",
        });
        Assert.AreEqual("lorem ipsum ~b~dolor sit amet", actual);
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
        Assert.AreEqual("lorem ipsum ~b~dolor sit ~r~amet", actual);
    }

    [Test]
    public void ToAcronym_Single_Word()
    {
        Assert.AreEqual("A", "ABC".ToAcronym());
    }

    [Test]
    public void ToAcronym_Multiple_Words()
    {
        Assert.AreEqual("AD", "Abc Def".ToAcronym());
    }

    [Test]
    public void ToAcronym_Case_Conversion()
    {
        Assert.AreEqual("L", "lowercase".ToAcronym());
    }
}