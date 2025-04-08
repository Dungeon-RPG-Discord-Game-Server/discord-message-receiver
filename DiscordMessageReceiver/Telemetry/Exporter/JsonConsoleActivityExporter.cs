// This is a quick-n-dirty modification of the OpenTelemetry.ConsoleExporter to support Activity objects.
// It uses the same basic code to write the activity in a single JSON line to the console, rather than the OTLP format.
// See https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/ConsoleActivityExporter.cs

using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using Newtonsoft.Json;

namespace Telemetry.Exporter;

internal static class SpanAttributeConstants
{
    public const string StatusCodeKey = "otel.status_code";
    public const string StatusDescriptionKey = "otel.status_description";
}

public class JsonConsoleActivityExporter : JsonConsoleExporter<Activity>
{
    public JsonConsoleActivityExporter(ConsoleExporterOptions options)
        : base(options)
    {
    }

    public override ExportResult Export(in Batch<Activity> batch)
    {
        // Create a dictionary to store the parsed name-value pairs
        Dictionary<string, object> keyValuePairs = [];

        foreach (var activity in batch)
        {
            keyValuePairs["traceid"] = activity.TraceId.ToString();
            keyValuePairs["spanid"] = activity.SpanId.ToString();
            keyValuePairs["traceflags"] = activity.ActivityTraceFlags.ToString();

            if (!string.IsNullOrEmpty(activity.TraceStateString))
            {
                keyValuePairs["tracestate"] = activity.TraceStateString;
            }

            if (activity.ParentSpanId != default)
            {
                keyValuePairs["parentspanid"] = activity.ParentSpanId.ToString();
            }

            keyValuePairs["source.name"] = activity.Source.Name;
            if (!string.IsNullOrEmpty(activity.Source.Version))
            {
                keyValuePairs["source.version"] = activity.Source.Version;
            }

            keyValuePairs["displayname"] = activity.DisplayName;
            keyValuePairs["kind"] = activity.Kind.ToString();
            keyValuePairs["starttime"] = activity.StartTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            keyValuePairs["duration"] = activity.Duration.ToString();
            
            var statusCode = string.Empty;
            var statusDesc = string.Empty;

            if (activity.TagObjects.Any())
            {
                // Create a dictionary to store the parsed name-value tags
                Dictionary<string, object> tagKeyValuePairs = [];

                foreach (ref readonly var tag in activity.EnumerateTagObjects())
                {
                    if (tag.Key == SpanAttributeConstants.StatusCodeKey)
                    {
                        statusCode = tag.Value as string;
                        continue;
                    }

                    if (tag.Key == SpanAttributeConstants.StatusDescriptionKey)
                    {
                        statusDesc = tag.Value as string;
                        continue;
                    }

                    if (this.TagWriter.TryTransformTag(tag, out var result))
                    {
                        tagKeyValuePairs[result.Key] = result.Value;
                    }
                }

                keyValuePairs["tags"] = tagKeyValuePairs;                
            }

            if (activity.Status != ActivityStatusCode.Unset)
            {
                keyValuePairs["statuscode"] = activity.Status.ToString();
                if (!string.IsNullOrEmpty(activity.StatusDescription))
                {
                    keyValuePairs["statusdescription"] = activity.StatusDescription;    
                }
            }
            else if (!string.IsNullOrEmpty(statusCode))
            {
                keyValuePairs["statuscode"] = statusCode;
                if (!string.IsNullOrEmpty(statusDesc))
                {
                    keyValuePairs["statusdescription"] = statusDesc;
                }
            }

            if (activity.Events.Any())
            {
                // Create a dictionary to store the parsed name-value events
                Dictionary<string, object> eventKeyValuePairs = [];

                foreach (ref readonly var activityEvent in activity.EnumerateEvents())
                {
                    // Create a dictionary to store the parsed name-value attributes
                    Dictionary<string, object> attributeKeyValuePairs = [];

                    attributeKeyValuePairs["time"] = activityEvent.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
                    foreach (ref readonly var attribute in activityEvent.EnumerateTagObjects())
                    {
                        if (this.TagWriter.TryTransformTag(attribute, out var result))
                        {
                            attributeKeyValuePairs[result.Key] = result.Value;
                        }
                    }
                    eventKeyValuePairs[activityEvent.Name] = attributeKeyValuePairs;
                }

                keyValuePairs["events"] = eventKeyValuePairs;   
            }

            if (activity.Links.Any())
            {
                // Create a dictionary to store the parsed name-value links
                Dictionary<string, object> linkKeyValuePairs = [];

                foreach (ref readonly var activityLink in activity.EnumerateLinks())
                {
                    // Create a dictionary to store the parsed name-value attributes
                    Dictionary<string, object> attributeKeyValuePairs = [];

                    attributeKeyValuePairs["traceid"] = activityLink.Context.TraceId.ToString();                    
                    foreach (ref readonly var attribute in activityLink.EnumerateTagObjects())
                    {
                        if (this.TagWriter.TryTransformTag(attribute, out var result))
                        {
                            attributeKeyValuePairs[result.Key] = result.Value;
                        }
                    }
                }

                keyValuePairs["links"] = linkKeyValuePairs;
            }

            var resource = this.ParentProvider.GetResource();
            if (resource != Resource.Empty)
            {
                // Create a dictionary to store the parsed name-value resources
                Dictionary<string, object> resourceKeyValuePairs = [];

                foreach (var resourceAttribute in resource.Attributes)
                {
                    if (this.TagWriter.TryTransformTag(resourceAttribute, out var result))
                    {
                        resourceKeyValuePairs[result.Key] = result.Value;
                    }
                }

                keyValuePairs["resource"] = resourceKeyValuePairs;
            }

            string json = JsonConvert.SerializeObject(keyValuePairs, Formatting.None);

            this.WriteLine(json);
        }

        return ExportResult.Success;
    }
}