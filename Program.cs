using GB.BLL.Interfaces;
using GB.BLL.Repository;
using GB.DAL.Data.Conetxts;
using GB.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace School.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<GBContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

            builder.Services.AddScoped(
                typeof(IGenericRepository<,>),
                typeof(GenericRepository<,>));

            builder.Services
                .AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<GBContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(conf =>
            {
                conf.LoginPath = "/Account/SignIn";
            });
            builder.Services.AddSession();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<AppUser>>();

                string[] roles = { "Admin", "Teacher", "Student" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(
                            new IdentityRole(role));
                    }
                }

                var adminEmail = "ahmedbanhawy538@gmail.com";

                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    admin = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Ahmed",
                        LastName = "Banhawy",
                        IsAgree = true
                    };

                    var result = await userManager.CreateAsync(
                        admin,
                        "Aaa@1234");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(
                            admin,
                            "Admin");
                    }
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSession();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=SignIn}/{id?}");

            await app.RunAsync();
        }
    }
}