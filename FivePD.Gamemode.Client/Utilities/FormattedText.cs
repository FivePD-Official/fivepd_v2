// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Text;

#pragma warning disable CS1591
#pragma warning disable SA1602
#pragma warning disable SA1611
#pragma warning disable SA1615

namespace FivePD.Gamemode.Client.Utilities
{
    /// <summary>
    /// A string builder wrapper to easily create multicolored texts.
    /// </summary>
    public class FormattedText
    {
        private readonly StringBuilder _stringBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedText"/> class.
        /// </summary>
        public FormattedText()
        {
            this._stringBuilder = new StringBuilder();
        }

        /// <summary>
        /// Sets the text color to red.
        /// </summary>
        public FormattedText Red(string text = "")
        {
            return this.Append(Formats.Red, text);
        }

        /// <summary>
        /// Sets the text color to green.
        /// </summary>
        public FormattedText Green(string text = "")
        {
            return this.Append(Formats.Green, text);
        }

        /// <summary>
        /// Sets the text color to blue.
        /// </summary>
        public FormattedText Blue(string text = "")
        {
            return this.Append(Formats.Blue, text);
        }

        /// <summary>
        /// Sets the text color to yellow.
        /// </summary>
        public FormattedText Yellow(string text = "")
        {
            return this.Append(Formats.Yellow, text);
        }

        /// <summary>
        /// Sets the text color to grey.
        /// </summary>
        public FormattedText Grey(string text = "")
        {
            return this.Append(Formats.Grey, text);
        }

        /// <summary>
        /// Sets the text color to orange.
        /// </summary>
        public FormattedText Orange(string text = "")
        {
            return this.Append(Formats.Orange, text);
        }

        /// <summary>
        /// Sets the text color to purple.
        /// </summary>
        public FormattedText Purple(string text = "")
        {
            return this.Append(Formats.Purple, text);
        }

        /// <summary>
        /// Sets the text color to pink.
        /// </summary>
        public FormattedText Pink(string text = "")
        {
            return this.Append(Formats.Pink, text);
        }

        /// <summary>
        /// Sets the text color to black.
        /// </summary>
        public FormattedText Black(string text = "")
        {
            return this.Append(Formats.Black, text);
        }

        /// <summary>
        /// Sets the text color to dark blue.
        /// </summary>
        public FormattedText DarkBlue(string text = "")
        {
            return this.Append(Formats.DarkBlue, text);
        }

        /// <summary>
        /// Sets the text color to white.
        /// </summary>
        public FormattedText White(string text = "")
        {
            return this.Append(Formats.White, text);
        }

        /// <summary>
        /// Adds a new line to the text.
        /// </summary>
        public FormattedText NewLine()
        {
            this._stringBuilder.Append(Formats.NewLine);
            return this;
        }

        /// <summary>
        /// Removes all text formatting.
        /// </summary>
        public FormattedText Reset(string text = "")
        {
            return this.Append(Formats.Reset, text);
        }

        /// <summary>
        /// Returns the created text.
        /// </summary>
        public string Build()
        {
            return this._stringBuilder.ToString();
        }

        private FormattedText Append(string color, string text)
        {
            this._stringBuilder.Append(color);
            if (!string.IsNullOrEmpty(text))
            {
                this._stringBuilder.Append(text);
            }

            return this;
        }

        private abstract class Formats
        {
            public const string Red = "~r~";
            public const string Green = "~g~";
            public const string Blue = "~b~";
            public const string Yellow = "~y~";
            public const string Grey = "~c~";
            public const string Orange = "~o~";
            public const string Purple = "~p~";
            public const string Pink = "~q~";
            public const string Black = "~l~";
            public const string DarkBlue = "~d~";
            public const string White = "~w~";
            public const string Reset = "~s~";
            public const string NewLine = "~n~";
        }
    }
}