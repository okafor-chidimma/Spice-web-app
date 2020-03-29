using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utilities;
using Stripe;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public OrderDetailsCart ListCartVM { get; set; }
        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            //initialize the view model and order header objects so that the total can be 0
            ListCartVM = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };
            ListCartVM.OrderHeader.OrderTotal = 0;
            //get the users id after logging in
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //retrieve all the items this user added to cart
            var userItemsInCartFromDb = await _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToListAsync();


            //loop through if it is not empty and add all the prices together
            if (userItemsInCartFromDb != null)
            {
                foreach (var userItem in userItemsInCartFromDb)
                {
                    userItem.MenuItem = await _db.MenuItem.Where(m => m.Id == userItem.MenuItemId).FirstOrDefaultAsync();
                    
                    ListCartVM.OrderHeader.OrderTotal += ((userItem.MenuItem.Price * userItem.Count) / 100);
                    userItem.MenuItem.Description = SD.ConvertToRawHtml(userItem.MenuItem.Description);
                    if (userItem.MenuItem.Description.Length > 100)
                    {
                        userItem.MenuItem.Description = userItem.MenuItem.Description.Substring(0, 100) + "...";
                    }
                    
                }
            }
            ListCartVM.ListCart = userItemsInCartFromDb;
            ListCartVM.OrderHeader.OrderTotalOriginal = ListCartVM.OrderHeader.OrderTotal;
            //check to see if the coupon code session is set
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                //retrieve what ever is stored in that session
                ListCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == ListCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                ListCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, ListCartVM.OrderHeader.OrderTotalOriginal);
            }
            return View(ListCartVM);
        }
        public IActionResult AddCoupon()
        {
            if (ListCartVM.OrderHeader.CouponCode == null)
            {
                ListCartVM.OrderHeader.CouponCode = "";
            }
            //sets the session
            HttpContext.Session.SetString(SD.ssCouponCode, ListCartVM.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveCoupon()
        {

            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Count++;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            else
            {
                cart.Count--;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);


            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {

            ListCartVM = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            ListCartVM.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await _db.ApplicationUser.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();
            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                ListCartVM.ListCart = cart.ToList();
            }

            foreach (var list in ListCartVM.ListCart)
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
                ListCartVM.OrderHeader.OrderTotal += ((list.MenuItem.Price * list.Count) / 100);

            }
            ListCartVM.OrderHeader.OrderTotalOriginal = ListCartVM.OrderHeader.OrderTotal;
            ListCartVM.OrderHeader.PickupName = applicationUser.Name;
            ListCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            ListCartVM.OrderHeader.PickUpTime = DateTime.Now;


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                ListCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == ListCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                ListCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, ListCartVM.OrderHeader.OrderTotalOriginal);
            }


            return View(ListCartVM);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //since the menu order was placed in a list item which is not an input, so the ListCart will be null
            ListCartVM.ListCart = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            //setting the details of the order header
            ListCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ListCartVM.OrderHeader.OrderDate = DateTime.Now;
            ListCartVM.OrderHeader.UserId = claim.Value;
            ListCartVM.OrderHeader.Status = SD.PaymentStatusPending;
            //ListCartVM.OrderHeader.PickUpTime = Convert.ToDateTime(ListCartVM.OrderHeader.PickUpDate.ToShortDateString() + ListCartVM.OrderHeader.PickUpTime.ToShortTimeString());

            //initializing the list of order details objects
            List<OrderDetails> orderDetailsList = new List<OrderDetails>();

            //saving the order header
            _db.OrderHeader.Add(ListCartVM.OrderHeader);
            await _db.SaveChangesAsync();


            ListCartVM.OrderHeader.OrderTotalOriginal = 0;
            foreach (var item in ListCartVM.ListCart)
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                //create new orderdetails object for each menuitem 
                //another way of initializing an object is without () because the constructor does not require parameters
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = ListCartVM.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                ListCartVM.OrderHeader.OrderTotalOriginal += orderDetails.Count * (orderDetails.Price/100);
                _db.OrderDetails.Add(orderDetails);

               
            }

            //check if coupon was applied and discount the ordertotal appropriately
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                ListCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == ListCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                ListCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, ListCartVM.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                ListCartVM.OrderHeader.OrderTotal = ListCartVM.OrderHeader.OrderTotalOriginal;
            }

            ListCartVM.OrderHeader.CouponCodeDiscount = ListCartVM.OrderHeader.OrderTotalOriginal - ListCartVM.OrderHeader.OrderTotal;

            //remove all the menuitems from the shopping cart table
            _db.ShoppingCart.RemoveRange(ListCartVM.ListCart);
            //set the chart count to 0
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await _db.SaveChangesAsync();

            //for the stripe payement gateway
            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(ListCartVM.OrderHeader.OrderTotal * 100),
                Currency = "ngn",
                Description = "Order ID : " + ListCartVM.OrderHeader.Id,
                SourceId = stripeToken

            };
            var service = new ChargeService();
            Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                ListCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                ListCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if (charge.Status.ToLower() == "succeeded")
            {
                ListCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                ListCartVM.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                ListCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
            //action name, controller, what i want to pass to the action method
            //return RedirectToAction("Confirm", "Order", new { id = ListCartVM.OrderHeader.Id });
        }

    }
}