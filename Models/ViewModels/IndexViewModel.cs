using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Coupon> CouponList { get; set; }
        public IEnumerable<MenuItem> MenuItemList { get; set; }

        public IEnumerable<Category> CategoryList { get; set; }
    }
}
