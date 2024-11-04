using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.Service.Consumers;
using Stock.Service.Contexts;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<StockDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);

        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEvent, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

var host = builder.Build();
host.Run();
