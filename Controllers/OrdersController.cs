using pizza.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using pizza.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace pizza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        // GET: api/<OrdersController>
        private IOrderProvider Dbcontext;
        private readonly OrdersDbContext context;

        public OrdersController(IOrderProvider dbcontext, OrdersDbContext context)
        {

            this.Dbcontext = dbcontext;
            this.context = context;


        }
        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var result = await Dbcontext.GetAllOrdersAsync();
           
            return Ok(result.Orders);
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetOrdersAsync(int customerId)
        {
            var result = await Dbcontext.GetOrdersByCustomerIdAsync(customerId);
            if (result.IsSuccess)
            {

                return Ok(result.Orders);

            }
            return NotFound();
        }



        // POST api/<OrdersController>
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            Db.Order orderToStore = new Db.Order();
            orderToStore.OrderDate = DateTime.Now;
            orderToStore.Items = order.Items;
            orderToStore.CustomerId = order.CustomerId;

            var response = await Dbcontext.InsertAsync(orderToStore);
            if (response.IsSuccess)
            {
                return Ok( response.Id );
            }

            return NotFound();


        }


        private async Task<bool> OrderExistsAsync(int id)
        {
            var order = await context.Order.FindAsync(id);
             if (order == null)
            {
                return false;
            }
            return true;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            
            Db.Order orderToStore = new Db.Order();
            orderToStore.OrderDate = DateTime.Now;
            orderToStore.Items = order.Items;
            orderToStore.CustomerId = order.CustomerId;
            orderToStore.Id = id;

            var response = await Dbcontext.UpdateAsync(id, orderToStore);
            if (response.IsSuccess)
            {
                return Ok(response.Id);
            }

            return NotFound();
        }



        // DELETE api/<OrdersController>/5
        // DELETE: api/Orders1/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var response = await Dbcontext.DeleteAsync(id);
            if (response.IsSuccess)
            {
                return Ok("Delete successfully");
            }
            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return context.Order.Any(e => e.Id == id);
        }
    }
}
