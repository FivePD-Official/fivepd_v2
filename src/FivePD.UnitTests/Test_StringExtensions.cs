using System.Collections.Generic;
using FivePD.Common.Extensions;
using NUnit.Framework;

#pragma warning disable

namespace FivePD.UnitTests;

[TestFixture]
internal class Test_StringExtensions
{
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