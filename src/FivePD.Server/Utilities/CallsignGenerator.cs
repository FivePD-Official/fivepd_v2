using FivePD.Server.Interfaces;

namespace FivePD.Server.Utilities;

using System.Text;

#pragma warning disable

public class CallsignGenerator
{
    private readonly IRandomService _randomService = ServiceLocator.GetService<IRandomService>();
    private readonly string _format;

    public CallsignGenerator(string format)
    {
        this._format = format;
    }

    public string GenerateCallsign()
    {
        var callsignBuilder = new StringBuilder();
        var nextCharStatic = false;
        foreach (var placeholder in this._format)
        {
            if (placeholder == '*')
            {
                nextCharStatic = true;
                continue;
            }

            char generatedChar;
            if (nextCharStatic)
            {
                generatedChar = placeholder;
                nextCharStatic = false;
            }
            else
            {
                generatedChar = placeholder switch
                {
                    'L' => this.GetRandomLetter(),
                    'W' => this.GetRandomLetter(),
                    'N' => this.GetRandomNumber(),
                    _ => placeholder
                };
            }

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