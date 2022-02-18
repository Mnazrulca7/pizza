using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pizza.Db
{
    public class OrdersDbContext:DbContext
    {
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        public OrdersDbContext(DbContextOptions options) : base(options)
        { 
        
        
        
        }
    }
}
