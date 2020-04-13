using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spice.Models;
using Spice.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Data
{
    public class DbInitializer : IDbIntializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async void Initialize()
        {
            try
            {
                //check if there is any pending migrations and if yes, execute them
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Migration could not be applied because {ex}");

            }

            //check if there is a manager role in the roles table, this will show that the roles have been created before
            if (_db.Roles.Any(r => r.Name == SD.ManagerUser)) return;

            //else it means that the roles have not being created, so create them again
            _roleManager.CreateAsync(new IdentityRole(SD.ManagerUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.FrontDeskUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.KitchenUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.CustomerEndUser)).GetAwaiter().GetResult();

            //user seed data
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "okaforchidimma.c@gmail.com",
                Email = "okaforchidimma.c@gmail.com",
                Name = "Okafor Chidimma",
                EmailConfirmed = true,
                PhoneNumber = "1112223333"
            }, "Jennylove19").GetAwaiter().GetResult();


            //retrieve the just created user and assign manager role to it
            IdentityUser user = await _db.Users.FirstOrDefaultAsync(u => u.Email == "okaforchidimma.c@gmail.com");

            await _userManager.AddToRoleAsync(user, SD.ManagerUser);
        }
    }
}
