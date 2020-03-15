using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utilities;

namespace Spice.Areas.Admin.Controllers
{
   
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        //for posting our images to the server we need this by dependency injection
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel NewMenuItemViewModel { get; set; }

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            NewMenuItemViewModel = new MenuItemViewModel()
            {
                MyNewMenuItem = new Models.MenuItem(),
                CategoryList = _db.Category.ToList()
            };
        }
        public async  Task<IActionResult> Index()
        {
            var menuItemList = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            foreach(var menuItem in menuItemList)
            {
                menuItem.Price = (menuItem.Price) / 100;
            }
            return View(menuItemList);
        }
        public IActionResult Create()
        {
            return View(NewMenuItemViewModel);
        }

        
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CreatePost()
        {
            NewMenuItemViewModel.MyNewMenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            NewMenuItemViewModel.MyNewMenuItem.Price = NewMenuItemViewModel.MyNewMenuItem.Price * 100;
            if (!ModelState.IsValid)
            {
                return View(NewMenuItemViewModel);
            }
            await _db.MenuItem.AddAsync(NewMenuItemViewModel.MyNewMenuItem);
            await _db.SaveChangesAsync();

            //Image Upload to server
            //to get the web root path
            string webRootPath = _hostingEnvironment.WebRootPath;
            //                                   the value of the name attr for the iput type file
            var files = HttpContext.Request.Form.Files;
            //the id of the newly saved file so we can append it to the image name
            var menuItemFromDb = await _db.MenuItem.FindAsync(NewMenuItemViewModel.MyNewMenuItem.Id);
            if(files.Count > 0)
            {
                //file was uploaded
                var uploads = Path.Combine(webRootPath, @"images\UploadedImage\");
                //will get the extension from the filename
                var extension = Path.GetExtension(files[0].FileName);
                //uploads to the server
                using(var fileStream = new FileStream(Path.Combine(uploads, NewMenuItemViewModel.MyNewMenuItem.Id + extension), FileMode.Create))
                {
                    //copies the image to images/UploadedImage folder
                    files[0].CopyTo(fileStream);
                }
                //update the Image column in db
                menuItemFromDb.Image = @"\images\UploadedImage\" + NewMenuItemViewModel.MyNewMenuItem.Id + extension;
            }
            else
            {
                //use default, nothing was uploaded
                //get the path of the image in my syatem
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                //to make a copy of file from uploads into our folder
                System.IO.File.Copy(uploads, webRootPath + @"\images\UploadedImage\" + NewMenuItemViewModel.MyNewMenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\UploadedImage\" + NewMenuItemViewModel.MyNewMenuItem.Id + ".png";

            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            
            NewMenuItemViewModel.MyNewMenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            NewMenuItemViewModel.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == NewMenuItemViewModel.MyNewMenuItem.CategoryId).ToListAsync();
            NewMenuItemViewModel.MyNewMenuItem.Price = NewMenuItemViewModel.MyNewMenuItem.Price / 100;
            if (NewMenuItemViewModel.MyNewMenuItem == null)
            {
                return NotFound();
            }
            return View(NewMenuItemViewModel);
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditPost(int? id)
        {
            NewMenuItemViewModel.MyNewMenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            NewMenuItemViewModel.MyNewMenuItem.Price = NewMenuItemViewModel.MyNewMenuItem.Price * 100;
            if (!ModelState.IsValid)
            {
                NewMenuItemViewModel.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == NewMenuItemViewModel.MyNewMenuItem.CategoryId).ToListAsync();
                return View(NewMenuItemViewModel);
            }
          
            //Image Upload to server
            //to get the web root path
            string webRootPath = _hostingEnvironment.WebRootPath;
            //                                   the value of the name attr for the iput type file
            var files = HttpContext.Request.Form.Files;
            //the id of the newly saved file so we can append it to the image name
            var menuItemFromDb = await _db.MenuItem.FindAsync(NewMenuItemViewModel.MyNewMenuItem.Id);
            if (files.Count > 0)
            {
                //newfile was uploaded
                var uploads = Path.Combine(webRootPath, @"images\UploadedImage\");
                //will get the extension from the filename
                var extension = Path.GetExtension(files[0].FileName);

                //Delete the original file
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //upload new file to the server
                using (var fileStream = new FileStream(Path.Combine(uploads, NewMenuItemViewModel.MyNewMenuItem.Id + extension), FileMode.Create))
                {
                    //copies the image to images/UploadedImage folder
                    files[0].CopyTo(fileStream);
                }
                //update the Image column in db
                menuItemFromDb.Image = @"\images\UploadedImage\" + NewMenuItemViewModel.MyNewMenuItem.Id + extension;
                
            }
            menuItemFromDb.Name = NewMenuItemViewModel.MyNewMenuItem.Name;
            menuItemFromDb.Description = NewMenuItemViewModel.MyNewMenuItem.Description;
            menuItemFromDb.Price = NewMenuItemViewModel.MyNewMenuItem.Price;
            menuItemFromDb.Spicyness = NewMenuItemViewModel.MyNewMenuItem.Spicyness;
            menuItemFromDb.CategoryId = NewMenuItemViewModel.MyNewMenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = NewMenuItemViewModel.MyNewMenuItem.SubCategoryId;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            NewMenuItemViewModel.MyNewMenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
 
            NewMenuItemViewModel.MyNewMenuItem.Price = NewMenuItemViewModel.MyNewMenuItem.Price / 100;
            if (NewMenuItemViewModel.MyNewMenuItem == null)
            {
                return NotFound();
            }
            return View(NewMenuItemViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            NewMenuItemViewModel.MyNewMenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            NewMenuItemViewModel.MyNewMenuItem.Price = NewMenuItemViewModel.MyNewMenuItem.Price / 100;
            if (NewMenuItemViewModel.MyNewMenuItem == null)
            {
                return NotFound();
            }
            return View(NewMenuItemViewModel);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var menuItemFromDb = await _db.MenuItem.FindAsync(NewMenuItemViewModel.MyNewMenuItem.Id);
            if (menuItemFromDb == null)
                return NotFound();
            // delete picture from system and server
            //from system
            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _db.MenuItem.Remove(menuItemFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
