using Microsoft.AspNetCore.Mvc;
using Spice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.ViewComponents
{
    //View compnents are like partials except they are more powerful in the sense that model binding is not used as it is in partial view
    //they have classes which serve as their own controller, the only diff between it and a regular controller, is that the methods can be used as endpoints but they support DI
    public class UserNameViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        public UserNameViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        //a compulsory action method listed below is needed in other to invoke this view component from a view or a controller
        //this method accepts parameters that are passed to it when the View component is invoked
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = await _db.ApplicationUser.FindAsync(claim.Value);
            //can return a view located in the view component folder in the shared folder
            return View(user);
        }
    }
}
