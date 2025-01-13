using Azure.Messaging.ServiceBus;

namespace AsbGateway;

/// <summary>
/// When we read from a ServiceBusClient, holds the translated content type, and the message that was received
/// </summary>
/// <param name="Content">The content, deserialized from the body</param>
/// <param name="ReceivedMessage">The message, received from the ASB queue; needed to allow us to ack the message when handled</param>
/// <typeparam name="T">The type of the content</typeparam>
public record Request<T>(T Content, ServiceBusReceivedMessage ReceivedMessage);