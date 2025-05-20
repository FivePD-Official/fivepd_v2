using System;
using System.IO;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace FivePD.Server;

/// <summary>
/// Custom Serilog sink that writes log events to CitizenFX's debug output.
/// Provides color-coded output based on log level.
/// </summary>
[Obfuscate]
internal class CfxDebugSink : ILogEventSink
{
    private readonly MessageTemplateTextFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="CfxDebugSink"/> class.
    /// </summary>
    /// <param name="outputTemplate">A message template describing the format used to write to the sink.</param>
    /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
    public CfxDebugSink(string outputTemplate, IFormatProvider formatProvider)
    {
        this._formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
    }

    /// <inheritdoc/>
    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        this._formatter.Format(logEvent, writer);
        var message = writer.ToString();

        message = logEvent.Level switch
        {
            LogEventLevel.Verbose => $"^5{message}^0",
            LogEventLevel.Debug => $"^5{message}^0",
            LogEventLevel.Information => $"^4{message}^0",
            LogEventLevel.Warning => $"^3{message}^0",
            LogEventLevel.Error => $"^1{message}^0",
            LogEventLevel.Fatal => $"^1{message}^0",
            _ => message,
        };

        Debug.Write(message);
    }
}