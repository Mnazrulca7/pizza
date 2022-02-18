using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
namespace pizza.Profiles
{
    public class OrderProfiles : AutoMapper.Profile //AutoMapper.Profile
    {
        public OrderProfiles() {

            CreateMap<Db.Order, Models.Order>();
            CreateMap<Db.OrderItem, Models.OrderItem>();

        }
        
    }
}
