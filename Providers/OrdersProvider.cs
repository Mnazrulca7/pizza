using AutoMapper;
using pizza.Db;
using pizza.Interfaces;
using pizza.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pizza.Interfaces;
using pizza.Db;
using Microsoft.AspNetCore.Mvc;

namespace pizza.Prodivers
    {
    public class OrdersProvider : IOrderProvider
    {
        private readonly OrdersDbContext DbContext;
        private readonly ILogger<OrdersProvider> ILogger;
        private readonly IMapper mapper;

        public OrdersProvider(OrdersDbContext dbContext, ILogger<OrdersProvider> Logger, IMapper Mapper)
        {
            this.DbContext = dbContext;
            this.ILogger = Logger;
            this.mapper = Mapper;
            SeedData();


        }

        private void SeedData()
        {
            if (!DbContext.Order.Any())
            {
                DbContext.Order.Add(new Db.Order()
                {
                    Id = 1,
                    CustomerId = 1,
                    OrderDate = DateTime.Now,
                    Items = new List<Db.OrderItem>()
                    {
                        new Db.OrderItem() { OrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 1, ProductId = 2, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 1, ProductId = 3, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 2, ProductId = 2, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 3, ProductId = 3, Quantity = 1, UnitPrice = 100 }
                    },
                    Total = 100
                });
                DbContext.Order.Add(new Db.Order()
                {
                    Id = 2,
                    CustomerId = 1,
                    OrderDate = DateTime.Now.AddDays(-1),
                    Items = new List<Db.OrderItem>()
                    {
                        new Db.OrderItem() { OrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 1, ProductId = 2, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 1, ProductId = 3, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 2, ProductId = 2, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 3, ProductId = 3, Quantity = 1, UnitPrice = 100 }
                    },
                    Total = 100
                });
                DbContext.Order.Add(new Db.Order()
                {
                    Id = 3,
                    CustomerId = 2,
                    OrderDate = DateTime.Now,
                    Items = new List<Db.OrderItem>()
                    {
                        new Db.OrderItem() { OrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 2, ProductId = 2, Quantity = 10, UnitPrice = 10 },
                        new Db.OrderItem() { OrderId = 3, ProductId = 3, Quantity = 1, UnitPrice = 100 }
                    },
                    Total = 100
                });
                DbContext.SaveChanges();
            }
        }



       

        public async Task<(bool IsSuccess, IEnumerable<Models.Order> Orders, string ErrorMessage)> GetOrdersByCustomerIdAsync(int customerId)
        {
            try
            {
                var orders = await DbContext.Order
                    .Where(o => o.CustomerId == customerId)
                    .Include(o => o.Items)
                    .ToListAsync();
                if (orders != null && orders.Any())
                {
                    var result = mapper.Map<IEnumerable<Db.Order>,
                        IEnumerable<Models.Order>>(orders);
                    return (true, result, null);
                }
                return (false, null, "Not Found");
            }
            catch (Exception ex)
            {
                ILogger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool IsSuccess,  string ErrorMessage)> DeleteAsync(int Id)
        {
            var order = await DbContext.Order.FindAsync(Id);
            if (order == null)
            {
                return (false,  "Not Found");
            }

            DbContext.Order.Remove(order);
         
            var result = await DbContext.SaveChangesAsync();

            return (result>0,  "Success");
        }

        public async Task<(bool IsSuccess, int Id, string ErrorMessage)> UpdateAsync(int Id, Db.Order Order)
        {
           

            var orders = await DbContext.Order
                  .Where(o => o.Id == Id)
                  .Include(o => o.Items)
                  .ToListAsync();
            var OldOrder = new List<Db.Order>();
            if (orders.Count>0)
            {
                OldOrder.Add(orders[0]);




            }
           
            if (OldOrder==null)
            {
                return (false, 0, "Null");

            }
                 
            
            List<Db.OrderItem> copyOldOrderItems = new List<Db.OrderItem>();
            
           if (OldOrder.Count>0 && OldOrder[0].Items != null)
            {
                foreach (var item in OldOrder[0].Items) copyOldOrderItems.Add(item);

            }
            List<Db.OrderItem> itemsTobeAdded = new List<Db.OrderItem>();
            List<Db.OrderItem> newOrderItems = new List<Db.OrderItem>();
            if (Order.Items != null)
            {
                foreach (var item in Order.Items) newOrderItems.Add(item);

            }
            List<(int, int)> indexTobeUpdated = new List<(int, int)>();
            for (int i = 0; i <newOrderItems.Count; i++)
            {
                bool found = false;
                for(int j=0;j< copyOldOrderItems.Count; j++)
                {
                    if (newOrderItems[i].ProductId == copyOldOrderItems[j].ProductId)
                    {
                        indexTobeUpdated.Add((i, j));
                        found = true;
                    }
                    
                }
                if (!found) itemsTobeAdded.Add(Order.Items[i]);
            }
            foreach (var pair in indexTobeUpdated)
            {
                copyOldOrderItems[pair.Item2] = newOrderItems[pair.Item1];

            }
            foreach (var newOrderItem in itemsTobeAdded)
            {
                copyOldOrderItems.Add(newOrderItem);

            }
            
            OldOrder[0].Items = copyOldOrderItems;


            OldOrder[0].Total = CalculateTotalPrice(OldOrder[0].Items);
            DbContext.Order.Update(OldOrder[0]);
            var result = await DbContext.SaveChangesAsync();


            return (result > 0, Id, null);
           

        }

        public async Task<(bool IsSuccess,int Id,string ErrorMessage)> InsertAsync( Db.Order order)
        {
            order.Id = await DbContext.Order.CountAsync() + 1;

            order.Total = CalculateTotalPrice(order.Items);
            DbContext.Order.Add(order);
            var result = await DbContext.SaveChangesAsync();


            return (result>0, order.Id, null);
        }


        public async Task<(bool IsSuccess, IEnumerable<Models.Order> Orders, string ErrorMessage)> GetAllOrdersAsync()
        {
            try
            {
                var orders = await DbContext.Order
                    .Include(o => o.Items)
                    .ToListAsync();
                if (orders != null && orders.Any())
                {
                    var result = mapper.Map<IEnumerable<Db.Order>,
                        IEnumerable<Models.Order>>(orders);
                    return (true, result, null);
                }
                return (false, null, "Not Found");
            }
            catch (Exception ex)
            {
                ILogger?.LogError(ex.ToString());
                return (false, null, ex.Message);
            }
        }

        public decimal CalculateTotalPrice(List<Db.OrderItem> Items)
        {
            decimal Total= 0;
            if (Items == null)
            {
                return Total;

            }
            foreach (var product in Items)
            {
                Total += product.UnitPrice * product.Quantity;

            }

           
            return Total;
        }
    }
}
