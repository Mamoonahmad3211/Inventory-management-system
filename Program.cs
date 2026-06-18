using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Inventory_Management_System
{
    public class Program
    {
        public static async Task Main(string[] args) // 👈 async karna zaroori
        {
            

            var builder = WebApplication.CreateBuilder(args);

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // Services
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<StripePaymentService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // If user tries to access protected page without login
            // tells Identity where to redirect unauthenticated users
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            builder.Services.AddTransient<IEmailSender, EmailSender>();
           
            builder.Services.AddRazorPages();
            builder.Services.AddSession();

            var app = builder.Build();

// ==============================================================================================================
            // ROLE SEEDING + DEFAULT ADMIN CREATION
            // App startup pe:
            // 1) Roles create hongi
            // 2) Default admin create hoga (sirf agar koi admin exist nahi karta)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager =
                    services.GetRequiredService<RoleManager<IdentityRole>>();

                var userManager =
                    services.GetRequiredService<UserManager<ApplicationUser>>();


                // -----------------------------
                // Create Roles
                // Authorization ke liye roles
                // -----------------------------
                string[] roles = { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(
                            new IdentityRole(role));
                    }
                }
        

                // -----------------------------------------
                // Default Admin Create (Bootstrap Admin)
                // Sirf tab create hoga agar koi Admin role
                // user already exist nahi karta
                // -----------------------------------------
                var existingAdmins =
                    await userManager.GetUsersInRoleAsync("Admin");

                if (!existingAdmins.Any())
                {
                    var adminUser = new ApplicationUser
                    {
                        FullName = "Admin",
                        UserName = "admin@gmail.com",
                        //Password = "Admin@123",
                        Email = "admin@gmail.com",
                        EmailConfirmed = true,
                        Address = "N/A"
                        
                    };

                    var result = await userManager.CreateAsync(
                        adminUser,
                        "Admin@123"
                    );

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(
                            adminUser,
                            "Admin"
                        );
                    }
                }
            }


          
            // Environment check krta hy dev mode/ production mode

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

         
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapRazorPages(); // 🔥
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run(); // 👈 LAST line
        }
    }
}