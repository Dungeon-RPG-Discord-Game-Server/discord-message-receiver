using OpenTelemetry.Trace;

namespace DiscordMessageReceiver.Utils;

// A simple factory-patternwrapper for OpenTelemetry logging
public class Logger
{
    private readonly Tracer _tracer;

    public Logger(string serviceName)
    {
        if (string.IsNullOrEmpty(serviceName)) { throw new ArgumentException("Service name cannot be null or empty"); }

        _tracer = TracerProvider.Default.GetTracer(serviceName);
    }

    // Used for internal methods or blocks to track. This will not add any HTTP context
    // and assumes that the method is not exposed to the outside world. 
    // Note this doesn't have to be a "method" per-se, any block of code you are interested in
    // logging parameters, results and time duration
    public MethodLogger StartMethod(string methodName)
    {
        if (string.IsNullOrEmpty(methodName)) { throw new ArgumentException("Service name cannot be null or empty"); }

        return new MethodLogger(_tracer, methodName);
    }

    // Used for public methods that are exposed to the outside world. This will add HTTP context
    public MethodLogger StartMethod(string methodName, HttpContext context)
    {
        if (string.IsNullOrEmpty(methodName)) { throw new ArgumentException("Service name cannot be null or empty"); }
        if (null == context) { throw new ArgumentException("HttpContext cannot be null"); }

        return new MethodLogger(_tracer, methodName, context);
    }   
}