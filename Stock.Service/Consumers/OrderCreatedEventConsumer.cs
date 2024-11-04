using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.Service.Contexts;
using Stock.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Stock.Service.Consumers
{
    public class OrderCreatedEventConsumer(StockDbContext _context) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            bool result = await _context.OrderInboxes.AnyAsync(i => i.IdempotentToken == context.Message.IdempotentToken);

            if(!result)
            {
                await _context.OrderInboxes.AddAsync(new()
                {
                    Processed = false,
                    Payload = JsonSerializer.Serialize(context.Message),
                    IdempotentToken = context.Message.IdempotentToken
                });
                await _context.SaveChangesAsync();
            }

            List<OrderInbox> orderInboxes = await _context.OrderInboxes
                .Where(i => i.Processed == false)
                .ToListAsync();

            foreach (var orderInbox in orderInboxes)
            {
                OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload);

                Console.WriteLine($"{orderCreatedEvent.OrderId} order id değerine sahip siparişin stok işlemlerini tamamlanmıştır.");

                orderInbox.Processed = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
