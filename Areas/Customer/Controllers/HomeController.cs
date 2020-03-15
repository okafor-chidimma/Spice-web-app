using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utilities;

namespace Spice.Controllers
{
    //if we dont specify this area name, during runtime, .netcore wll go to the general controllers folder to look for it and throws error if it does not find it
    //this way we are telling it to look for this in the customer area controller folder.
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var IndexModelVM = new IndexViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                CouponList = await _db.Coupon.Where(c => c.IsActive == true).ToListAsync(),
                MenuItemList = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync()
            };
            //incase if a user closes the tab and opens it again
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                var userOrderCount = await _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToListAsync();

                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, userOrderCount.Count);
            }
            
            return View(IndexModelVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();
            var shoppingCartObj = new ShoppingCart()
            {
                MenuItem = menuItemFromDb,
                MenuItemId = menuItemFromDb.Id
            };
            return View(shoppingCartObj);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Details(ShoppingCart modelFromDetails)
        {
            modelFromDetails.Id = 0;
            if (ModelState.IsValid)
            {
                //get the user details
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                //model from form details
                modelFromDetails.ApplicationUserId = claim.Value;
                //check if the user has placed the order for that item before
                ShoppingCart userOrdersFromDb = await _db.ShoppingCart.Where(s => s.ApplicationUserId == modelFromDetails.ApplicationUserId && s.MenuItemId == modelFromDetails.MenuItemId).FirstOrDefaultAsync();

                if (userOrdersFromDb == null)
                {
                    await _db.ShoppingCart.AddAsync(modelFromDetails);
                }
                else
                {
                    userOrdersFromDb.Count += modelFromDetails.Count;
                }
                await _db.SaveChangesAsync();
                //get the total count and put it in a session
                var userOrderCount =  _db.ShoppingCart.Where(s => s.ApplicationUserId == modelFromDetails.ApplicationUserId).ToList().Count();

                //set the session
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, userOrderCount);
                return RedirectToAction(nameof(Index));
            }
            var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == modelFromDetails.MenuItemId).FirstOrDefaultAsync();
            ShoppingCart shoppingCartObj = new ShoppingCart()
            {
                MenuItem = menuItemFromDb,
                MenuItemId = menuItemFromDb.Id
            };
            return View(shoppingCartObj);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
