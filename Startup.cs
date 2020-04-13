using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spice.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Spice.Models;
using Spice.Utilities;
using Stripe;

namespace Spice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            //to register the role manager
            services.AddIdentity<IdentityUser,IdentityRole>(options =>
                {
                    //for the password settings, to customise the password a user will supply
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredUniqueChars = 6;
                    options.SignIn.RequireConfirmedAccount = true;
                }
                
            )
                //the token for sending email
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //to register the dbinitializer
            services.AddScoped<IDbIntializer, DbInitializer>();

            //to register the email service
            //I can use add transisent
            services.AddTransient<IEmailSender, EmailSender>();


            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();

            //to use memory caching
            services.AddMemoryCache();
            //services.AddTransient<ICarService, CarService>();

            //this is me configuring a class that I want to be adding to other files through Dependency Injection.
            //this method adds the said class to the IOptions<<ClassName>> <Variable> and makes it available to all the files in your project

            //to configure the stripe payment gateway
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
            //during the configuration in the line above, i am telling the system that in my configuration(which is one of the ways of accessing secret keys), it will find a section named Stripe, that when it does, it should link each property of the stripe section to each property of the StripeSettings class that matches by name.
            //this now makes the StripeSettings class available with the values set from above all through the project and can be accessed via dependency Injection

            //this also means that this is a way of registering this our service but since we do not have an interface to register the service with, we will use the system's default interface "IOptions"

            //this is me configuring a class that I want to be adding to other files through Dependency Injection.
            //this method adds it to the IOptions<<ClassName>> <Variable> and makes it available to all the files in your project
            //Notice i did not use Configuration.GetSection("<SectionName>"), this is because in my secrets .json file, i stored the send grid api key as a key:value pair and not as a section like stripe
            //note that the key name is SendGridKey which matches the only property in EmailOptions Class
            services.Configure<EmailOptions>(Configuration);



            //application cookie controls the path to pages and any customising we have to do on the identity pages will be done on it
            //to specify the path for our login,accessdenied and logout pages
            services.ConfigureApplicationCookie(options =>

            {

                options.LoginPath = $"/Identity/Account/Login";

                options.LogoutPath = $"/Identity/Account/Logout";

                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

            });

            //to be able to use session
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                //makes sure the session lasts for only 30mins
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
            });


            //for 3rd party authentication
            //different ways of accessing the secret keys stored in the secret.json file
            services.AddAuthentication()
                .AddFacebook(facebookOptions =>
                    {
                        //way 1
                        facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                        facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    })
                .AddGoogle(options =>
                     {
                         //way 2
                         //my secret keys are being accessed using the configuration interface
                         IConfigurationSection googleAuthNSection =
                             Configuration.GetSection("Authentication:Google");

                         options.ClientId = googleAuthNSection["ClientId"];
                         options.ClientSecret = googleAuthNSection["ClientSecret"];
                     });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbIntializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            //to set stripe to use api key in appsettings
            StripeConfiguration.SetApiKey(Configuration.GetSection("Stripe")["SecretKey"]);

            //seed the data
            dbInitializer.Initialize();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
