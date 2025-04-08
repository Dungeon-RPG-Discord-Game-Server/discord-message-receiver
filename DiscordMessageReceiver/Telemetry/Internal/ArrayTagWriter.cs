// This is a quick-n-dirty modification of the OpenTelemetry.ConsoleExporter to support Activity objects.
// It uses the same basic code to write the activity in a single JSON line to the console, rather than the OTLP format.
// See https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/Shared/TagWriter/ArrayTagWriter.cs

namespace Telemetry.Internal;

internal abstract class ArrayTagWriter<TArrayState>
    where TArrayState : notnull
{
    public abstract TArrayState BeginWriteArray();

    public abstract void WriteNullValue(ref TArrayState state);

    public abstract void WriteIntegralValue(ref TArrayState state, long value);

    public abstract void WriteFloatingPointValue(ref TArrayState state, double value);

    public abstract void WriteBooleanValue(ref TArrayState state, bool value);

    public abstract void WriteStringValue(ref TArrayState state, string value);

    public abstract void EndWriteArray(ref TArrayState state);
}