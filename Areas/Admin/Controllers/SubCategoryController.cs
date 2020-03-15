using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utilities;

namespace Spice.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        //GET SubCategory Index page
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subCategories);
        }

        //GET SubCategory Create Page
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel subCatCreateModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync()
            };
            return View(subCatCreateModel);
        }

        //POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesItExist = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesItExist.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category Exists Under " + doesItExist.First().Category.Name + " category. Please find a new name";
                }
                else
                {
                    await _db.SubCategory.AddAsync(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel subCatCreateModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(subCatCreateModel);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            IEnumerable<SubCategory> subCategoriesList = new List<SubCategory>();
            subCategoriesList = await _db.SubCategory.Where(s => s.CategoryId == id).ToListAsync();

            //this line converts the subcat list into a list of options tag
            //List<SelectListItem> subme = new List<SelectListItem>();
            //foreach(var subhe in subCategoriesList)
            //{
            //    subme.Add(new SelectListItem { Value = subhe.Id.ToString(), Text = subhe.Name,Selected = true });
            //}
            //return Json(subme);


            return Json(new SelectList(subCategoriesList, "Id", "Name"));

            //does same thing as the commented line above, only difference is that we have to pass in a value for this one's selected value option, so that the compiler just assigns true to the selected option and not a condition 
            // new SelectList() is a collection of selectListItem Objects 
            //now this select list item objects are created once you initialize your selectList as shown below
            //new SelectList(subCategoriesList, "Id", "Name", <SelectedOptionValue>) where 
            //subCategoriesList is my list of subcat model object that i want to convert to option object
            //id is the property i want to use as the value for the option object
            //Name is the property i want to use as the option text
            //SelectedOptionValue which is the selected option value, the actual value and not T or F or a condition


        }

        //GET Edit page
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel subCatEditModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync()
            };
            return View(subCatEditModel);
        }

        //POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                //using eager loading to add the Category Table
                var doesItExist = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesItExist.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category Exists Under " + doesItExist.First().Category.Name + " category. Please find a new name";
                }
                else
                {
                    var subCatToUpdate = await _db.SubCategory.FindAsync(id);
                    subCatToUpdate.Name = model.SubCategory.Name;
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel subCatCreateModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(subCatCreateModel);
        }
        //GET Details Page
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel subCatCreateModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
            };
            return View(subCatCreateModel);
        }

        //GET Delete Page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel subCatCreateModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
            };
            return View(subCatCreateModel);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
                return NotFound();
            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}