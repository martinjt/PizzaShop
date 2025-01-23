using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Shared;

public abstract class TraceableRequest
{
    public TraceableRequest()
    {
        this.AddCurrentTraceContext();
    }
    public Dictionary<string, string[]> RequestHeaders { get; set; } = [];
}

public static class TraceableRequestExtensions
{

    public static ActivitySource Source = new("Shared.RequestTracing");
    public static void AddCurrentTraceContext(this TraceableRequest request)
    {
        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(Activity.Current?.Context ?? new ActivityContext(), Baggage.Current), 
            request.RequestHeaders, (headers, key, value) => headers[key] = [value]);
    }

    public static Activity? SetCurrentTraceContext(this TraceableRequest request)
    {
        var context = Propagators.DefaultTextMapPropagator.Extract(
            new PropagationContext(Activity.Current?.Context ?? new ActivityContext(), Baggage.Current), 
            request.RequestHeaders, (headers, key) => headers.TryGetValue(key, out var value) ? value : null);
        
        Baggage.Current = context.Baggage;

        return Source.StartActivity($"Process {request.GetType().Name}", 
            ActivityKind.Internal, 
            context.ActivityContext);
    }
}