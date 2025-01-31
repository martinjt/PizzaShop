using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Shared;

namespace Courier;

/// <summary>
/// Enulates the delivery of an order. A timer is used to simulate the passage of time.
/// For simplicity, we will only deliver one order at a time.
/// </summary>
/// <param name="orderStatusTopic">The stream to produce updates to</param>
/// <param name="deliveryJobs">A channel for delivery jobs, we will trigger when one of these is received</param>
/// <param name="orderStatusProducer"></param>
internal class DeliveryService(string orderStatusTopic, Channel<OrderStatus> deliveryJobs, IProducer<int, string> orderStatusProducer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //should only service one order at a time
            var orderStatus = await deliveryJobs.Reader.ReadAsync(stoppingToken);
            using var activity = orderStatus.StartNewSpanFromRequest();

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (DateTimeOffset.UtcNow > orderStatus.ETA)
                {
                    orderStatus.Status = DeliveryStatus.Delivered;
                    await orderStatusProducer.ProduceAsync(orderStatusTopic, new Message<int, string>
                    {
                        Key = orderStatus.OrderId,
                        Value = JsonSerializer.Serialize(orderStatus)
                    }, stoppingToken);
                    break;
                }

                orderStatus.Status = DeliveryStatus.OnTheWay;
                await orderStatusProducer.ProduceAsync(orderStatusTopic, new Message<int, string>
                {
                    Key = orderStatus.OrderId,
                    Value = JsonSerializer.Serialize(orderStatus)
                }, stoppingToken);
            }
        }
    }
}