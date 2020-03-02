using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public Coupon MyNewCoupon { get; set; }

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
           // MyNewCoupon = new Coupon();
        }
        public async Task<IActionResult> Index()
        {
            var couponList = await _db.Coupon.ToListAsync();
            return View(couponList);
        }

        //GET CREATE for Coupon
        public IActionResult Create()
        {
            
            return View();
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if (!ModelState.IsValid)
            {
                return View(MyNewCoupon);
            }
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                //to convert the image to stream of bytes to store in the db since Picture prop is of type byte[]
                byte[] p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                MyNewCoupon.Picture = p1;
            }
            _db.Coupon.Add(MyNewCoupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MyNewCoupon = await _db.Coupon.FindAsync(id);
            if(MyNewCoupon == null)
            {
                return NotFound();
            }
            return View(MyNewCoupon);
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if(!ModelState.IsValid)
            {
                return View(MyNewCoupon);
            }
            var couponFromDb = await _db.Coupon.FindAsync(id);
            if (couponFromDb == null)
                return NotFound();

            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                //to convert the image to stream of bytes to store in the db since Picture prop is of type byte[]
                byte[] p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                couponFromDb.Picture = p1;
            }
            couponFromDb.MinimumAmount = MyNewCoupon.MinimumAmount;
            couponFromDb.Name = MyNewCoupon.Name;
            couponFromDb.Discount = MyNewCoupon.Discount;
            couponFromDb.CouponType = MyNewCoupon.CouponType;
            couponFromDb.IsActive = MyNewCoupon.IsActive;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MyNewCoupon = await _db.Coupon.FindAsync(id);

            //incase if i want to use input field and to display just the name
            //var couponTypeName = Enum.GetName(typeof(Coupon.ECouponType), Int32.Parse(model.CouponType));
            if (MyNewCoupon == null)
            {
                return NotFound();
            }
            return View(MyNewCoupon);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MyNewCoupon = await _db.Coupon.FindAsync(id);
            if (MyNewCoupon == null)
            {
                return NotFound();
            }
            return View(MyNewCoupon);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var couponFromDb = await _db.Coupon.FindAsync(MyNewCoupon.Id);
            if (couponFromDb == null)
                return NotFound();
         
            _db.Coupon.Remove(couponFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}