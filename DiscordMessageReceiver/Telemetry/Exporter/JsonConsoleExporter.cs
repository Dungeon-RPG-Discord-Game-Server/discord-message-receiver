// This is a quick-n-dirty modification of the OpenTelemetry.ConsoleExporter to support Activity objects.
// It uses the same basic code to write the activity in a single JSON line to the console, rather than the OTLP format.
// See https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/ConsoleExporter.cs

using OpenTelemetry;
using OpenTelemetry.Exporter;

namespace Telemetry.Exporter;

public abstract class JsonConsoleExporter<T> : BaseExporter<T>
    where T : class
{
    private readonly ConsoleExporterOptions options;

    protected JsonConsoleExporter(ConsoleExporterOptions options)
    {
        this.options = options ?? new ConsoleExporterOptions();

        this.TagWriter = new ConsoleTagWriter(this.OnUnsupportedTagDropped);
    }

    internal ConsoleTagWriter TagWriter { get; }

    protected void WriteLine(string message)
    {
        if (this.options.Targets.HasFlag(ConsoleExporterOutputTargets.Console))
        {
            Console.WriteLine(message);
        }

        if (this.options.Targets.HasFlag(ConsoleExporterOutputTargets.Debug))
        {
            System.Diagnostics.Trace.WriteLine(message);
        }
    }

    private void OnUnsupportedTagDropped(
        string tagKey,
        string tagValueTypeFullName)
    {
        this.WriteLine($"Unsupported attribute value type '{tagValueTypeFullName}' for '{tagKey}'.");
    }
}