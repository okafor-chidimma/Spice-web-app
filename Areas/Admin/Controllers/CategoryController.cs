using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        //cannot use this because i have a create method already, so if i create a new Create() with the same signatures, it will
        //throw error. this is a typical e.g of method overloading

        //[BindProperty]
        //public Category MyNewCategory { get; set; }
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        //GET Category Index page
        public async Task<IActionResult> Index()
        {
            var categoriesList = await _db.Category.ToListAsync();
            return View(categoriesList);
        }

        //GET Create category page
        public IActionResult Create()
        {
            return View();
        }

        //POST Create Category Page
        //this two lines are needed for the compiler to know it is a post action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category myNewCategory)
        {
            if (ModelState.IsValid)
            {
                await _db.Category.AddAsync(myNewCategory);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myNewCategory);
        }

        //GET Category Edit Page
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            var category = await _db.Category.FindAsync(id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        //Post Category Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category catToUpdate)
        {

            if (ModelState.IsValid)
            {
                //category.Name = catToUpdate.Name;
                //update() ==> uses the primary key to search for this row and updates all the fields available
                _db.Category.Update(catToUpdate);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                
            }
            return View(catToUpdate);
            
        }
        //GET Category Delete Page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        //Post Category Delete
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(Category catToUpdate)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        //category.Name = catToUpdate.Name;
        //        //update() ==> uses the primary key to search for this row and updates all the fields available
        //        _db.Category.Remove(catToUpdate);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));

        //    }
        //    return View(catToUpdate);

        //}

        //Or

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            var category = await _db.Category.FindAsync(id);
            if (category == null)
                return NotFound();
            _db.Category.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}