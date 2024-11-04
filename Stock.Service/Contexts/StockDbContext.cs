using Microsoft.EntityFrameworkCore;
using Stock.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Service.Contexts
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderInbox> OrderInboxes { get; set; }
    }
}
