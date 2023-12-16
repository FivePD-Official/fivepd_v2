using System.Collections.Generic;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Utilities;

using System.Text;

#pragma warning disable

public class CallsignGenerator
{
    private readonly IRandomService _randomService = ServiceLocator.GetService<IRandomService>();
    private readonly List<CallsignFormatItem> _format;

    public CallsignGenerator(List<CallsignFormatItem> format)
    {
        this._format = format;
    }

    public string GenerateCallsign()
    {
        var callsignBuilder = new StringBuilder();
        foreach (var item in this._format)
        {
            var generatedChar = item.Type switch
            {
                CallsignFormatItemType.Dash => '-',
                CallsignFormatItemType.FixedCharacter => item.Value[0],
                CallsignFormatItemType.RandomLetter => this.GetRandomLetter(),
                CallsignFormatItemType.RandomNumber => this.GetRandomNumber(),
                _ => item.Value[0],
            };

            callsignBuilder.Append(generatedChar);
        }

        return callsignBuilder.ToString();
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