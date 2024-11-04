using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using Order.API.Entities;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order", async (CreateOrderVM model, OrderDbContext orderDbContext, ISendEndpointProvider sendEndpointProvider) =>
{
    Order.API.Entities.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid result) ? result : Guid.NewGuid(),
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Order.API.Entities.OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = Guid.TryParse(oi.ProductId, out Guid result) ? result : Guid.NewGuid(),
        }).ToList(),
    };

    await orderDbContext.Orders.AddAsync(order);
    await orderDbContext.SaveChangesAsync();

    Guid idempotentToken = Guid.NewGuid();
    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new Shared.Datas.OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId
        }).ToList(),
        IdempotentToken = idempotentToken,
    };
    //var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEvent}"));
    //await sendEndpoint.Send(orderCreatedEvent);

    OrderOutbox orderOutbox = new()
    {
        OccuredOn = DateTime.UtcNow,
        ProcessedDate = null,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = orderCreatedEvent.GetType().Name,
        IdempotentToken = idempotentToken
    };
    await orderDbContext.OrderOutboxes.AddAsync(orderOutbox);
    await orderDbContext.SaveChangesAsync();

});


app.Run();
