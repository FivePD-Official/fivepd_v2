using System.Collections.Generic;
using System.Linq;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Utilities;

#pragma warning disable

public class CallsignGenerator
{
    private readonly IRandomService _randomService = ServiceLocator.GetService<IRandomService>();
    private readonly List<CallsignFormatItem> _format;

    public CallsignGenerator(List<CallsignFormatItem> format)
    {
        this._format = format;
    }

    public List<CallsignFormatItem> GeneratedItems { get; private set; }

    public CallsignGenerator GenerateCallsign()
    {
        this.GeneratedItems = this._format.Select(item => new CallsignFormatItem
            {
                Type = item.Type,
                Value = item.Type == CallsignFormatItemType.Word
                    ? "PLACEHOLDER"
                    : item.Type switch
                    {
                        CallsignFormatItemType.Dash => "-",
                        CallsignFormatItemType.FixedCharacter => item.Value[0].ToString(),
                        CallsignFormatItemType.RandomLetter => this.GetRandomLetter().ToString(),
                        CallsignFormatItemType.RandomNumber => this.GetRandomNumber().ToString(),
                        _ => item.Value[0].ToString(),
                    }
            })
            .ToList();

        return this;
    }

    public List<string> GetGeneratedItemValues()
    {
        return this.GeneratedItems.Select(x => x.Value).ToList();
    }

    public string BuildCallsign()
    {
        return string.Join(string.Empty, this.GetGeneratedItemValues());
    }

    private char GetRandomLetter()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return letters[this._randomService.Random.Next(letters.Length)];
    }

    private char GetRandomNumber()
    {
        const string numbers = "0123456789";
        return numbers[this._randomService.Random.Next(numbers.Length)];
    }
}