using Microsoft.AspNetCore.Mvc;
using pizza.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pizza.Interfaces
{
 
    public interface IOrderProvider
    {

        Task<(bool IsSuccess, IEnumerable<Order> Orders, String ErrorMessage)> GetOrdersByCustomerIdAsync(int customerId);
        Task<(bool IsSuccess,  String ErrorMessage)> DeleteAsync(int Id);
        Task<(bool IsSuccess, int Id, String ErrorMessage)> UpdateAsync(int Id, Db.Order Order);
        Task<(bool IsSuccess, int Id, String ErrorMessage)> InsertAsync(Db.Order Order);
        Task<(bool IsSuccess, IEnumerable<Order> Orders, String ErrorMessage)> GetAllOrdersAsync();
    }
}
