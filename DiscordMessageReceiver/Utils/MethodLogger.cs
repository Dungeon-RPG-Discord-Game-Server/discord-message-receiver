using System.Net;
using OpenTelemetry.Trace;

namespace DiscordMessageReceiver.Utils;

// A simple wrapper class to help with logging method entry and exit
// Note that you can use this recursively, so you can have a method logging
// and then create another method logger for a subclass. The span will be
// correctly managed.
public class MethodLogger : IDisposable
{
    private readonly Tracer _tracer;
    private readonly TelemetrySpan _currentSpan;
    private readonly HttpContext _context;

    public MethodLogger(Tracer tracer, string methodName, HttpContext context)
    {
        if (null == tracer) { throw new ArgumentException("Tracer cannot be null"); }
        if (string.IsNullOrEmpty(methodName)) { throw new ArgumentException("Service name cannot be null or empty"); }

        _tracer = tracer;
        _context = context;
        _currentSpan = _tracer.StartActiveSpan(methodName);
        
        if (_context != null)
        {        
            _currentSpan.SetAttribute("http.method", context.Request.Method);
            _currentSpan.SetAttribute("http.url", context.Request.Path);

            // Let's add the current TraceId to the response headers
            // Remember, good behavior is to include "x-" in front of custom headers
            _context.Response.Headers.Append("x-trace-id", _currentSpan.Context.TraceId.ToString());
        }
    }

    public MethodLogger(Tracer tracer, string methodName)
    {
        if (null == tracer) { throw new ArgumentException("Tracer cannot be null"); }
        if (string.IsNullOrEmpty(methodName)) { throw new ArgumentException("Service name cannot be null or empty"); }

        _tracer = tracer;
        _currentSpan = _tracer.StartActiveSpan(methodName);
    }

    public void SetAttribute(string key, object value)
    {
        if (string.IsNullOrEmpty(key)) { throw new ArgumentException("Key cannot be null or empty"); }
        if (null == value) { throw new ArgumentException("Value cannot be null"); }

        _currentSpan.SetAttribute(key, value.ToString());
    }

    public void LogUserError(string errorMessage)
    {
        SetAttribute("error", true);
        SetAttribute("error.message", errorMessage);

        if (_context != null)
        {
            // HTTP 400 indicates a bad request, which is what you return if the
            // user sends an invalid request, such as missing a required parameter
            _context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    public void HandleException(Exception e)
    {
        if (null == e) { throw new ArgumentException("Exception cannot be null"); }

        // If any error happend via an exception, we can include that information
        // in the log. Including the stack trace can be either useful or very
        // noisy, so you often control that with log levels.
        _currentSpan.SetAttribute("error", true);
        _currentSpan.SetAttribute("error.message", e.Message);
        _currentSpan.SetAttribute("error.stacktrace", e.StackTrace);
        
        if (_context != null)
        {
            // 500 is the typical status code for an internal server error
            // We got an unhandled exception, so we don't know what went wrong
            // Hence we log the information and return a 500 status code
            _context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }

    public void Dispose()
    {
        _currentSpan.End();
     }
}