// This is a quick-n-dirty modification of the OpenTelemetry.ConsoleExporter to support Activity objects.
// It uses the same basic code to write the activity in a single JSON line to the console, rather than the OTLP format.
// See https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/ConsoleExporterHelperExtensions.cs

using Microsoft.Extensions.Options;

using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

using Telemetry.Exporter;

namespace Telemetry.Trace;

public static class JsonConsoleExporterHelperExtensions
{
    /// <summary>
    /// Adds Console exporter to the TracerProvider.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddJsonConsoleExporter(this TracerProviderBuilder builder)
        => AddJsonConsoleExporter(builder, name: null, configure: null);

    /// <summary>
    /// Adds Console exporter to the TracerProvider.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
    /// <param name="configure">Callback action for configuring <see cref="ConsoleExporterOptions"/>.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddJsonConsoleExporter(this TracerProviderBuilder builder, Action<ConsoleExporterOptions> configure)
        => AddJsonConsoleExporter(builder, name: null, configure);

    /// <summary>
    /// Adds Console exporter to the TracerProvider.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> builder to use.</param>
    /// <param name="name">Name which is used when retrieving options.</param>
    /// <param name="configure">Callback action for configuring <see cref="ConsoleExporterOptions"/>.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddJsonConsoleExporter(
        this TracerProviderBuilder builder,
        string name,
        Action<ConsoleExporterOptions> configure)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        name ??= Options.DefaultName;

        if (configure != null)
        {
            builder.ConfigureServices(services => services.Configure(name, configure));
        }

        return builder.AddProcessor(sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<ConsoleExporterOptions>>().Get(name);

            return new SimpleActivityExportProcessor(new JsonConsoleActivityExporter(options));
        });
    }
}