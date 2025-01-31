using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared;

public static class TelemetryExtensions
{
    public const bool EnableTelemetry = false;
    public const bool EnableExperimentalServiceBus = false;
    public const bool EnableCustomTelemtry = false;
    public static IOpenTelemetryBuilder AddPizzaShopTelemetry(this IServiceCollection services, string serviceName, string[]? additionalSources = null, Dictionary<string, string>? additionalResourceAttributes = null)
    {
        var openTelemetryBuilder = services.AddOpenTelemetry()
            .UseOtlpExporter()
            .ConfigureResource(builder => builder.AddService(serviceName))
            .WithTracing(builder => {
                builder
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options => {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                });

                if (additionalSources != null && EnableCustomTelemtry)
                {
                    foreach (var source in additionalSources)
                    {
                        builder.AddSource(source);
                    }
                }

                if (EnableCustomTelemtry)
                {
                    builder
                        .AddSource(TraceableRequestExtensions.Source.Name)
                        .AddSource(AsbGateway.DiagnosticSettings.Source.Name)
                        .AddSource(KafkaGateway.DiagnosticSettings.Source.Name);
                }
            })
            .WithMetrics()
            .WithLogging();

        if (EnableExperimentalServiceBus)
        {
            AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);
            openTelemetryBuilder.WithTracing(builder => builder.AddSource("Azure.Messaging.ServiceBus.*"));
        }


        return openTelemetryBuilder;
    }

}
