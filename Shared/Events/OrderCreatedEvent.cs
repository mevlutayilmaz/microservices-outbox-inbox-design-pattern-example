﻿using Shared.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Guid IdempotentToken { get; set; }
    }
}
