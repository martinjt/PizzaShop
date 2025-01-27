using System.Diagnostics;

namespace KafkaGateway;

public static class DiagnosticSettings
{
    public static ActivitySource Source = new("KafkaGateway");
}
