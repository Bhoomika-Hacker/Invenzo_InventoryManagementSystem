using InventoryManagementSystem.AutoMapper;
using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Data;

using InventoryManagementSystem.Repository.CategoryRepo;
using InventoryManagementSystem.Repository.DashboardRepo;
using InventoryManagementSystem.Repository.ProductRepo;
using InventoryManagementSystem.Repository.PurchaseItemRepo;
using InventoryManagementSystem.Repository.PurchaseRepo;
using InventoryManagementSystem.Repository.ReportRepo;
using InventoryManagementSystem.Repository.SaleItemRepo;
using InventoryManagementSystem.Repository.SaleRepo;
using InventoryManagementSystem.Repository.StockMovementRepo;
using InventoryManagementSystem.Repository.SupplierRepo;

using InventoryManagementSystem.Services.CategoryService;
using InventoryManagementSystem.Services.DashboardService;
using InventoryManagementSystem.Services.ProductService;
using InventoryManagementSystem.Services.PurchaseItemService;
using InventoryManagementSystem.Services.PurchaseService;
using InventoryManagementSystem.Services.ReportService;
using InventoryManagementSystem.Services.SaleItemService;
using InventoryManagementSystem.Services.SaleService;
using InventoryManagementSystem.Services.StockMovementService;
using InventoryManagementSystem.Services.SupplierService;
using InventoryManagementSystem.Services.ZenoAIService;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InventoryManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================
            // CONNECTION STRING
            // =========================================================

            var connectionString = builder.Configuration
                .GetConnectionString("InventoryDbContext")
                ?? throw new InvalidOperationException(
                    "Connection string 'InventoryDbContext' not found.");

            // =========================================================
            // REGISTER DB CONTEXT
            // =========================================================

            builder.Services.AddDbContext<InventoryDbContext>(
                options =>
                {
                    options.UseSqlServer(connectionString);
                    options.ConfigureWarnings(w =>
                        w.Ignore(RelationalEventId.PendingModelChangesWarning));
                });

            // =========================================================
            // REGISTER IDENTITY
            // =========================================================

            builder.Services.AddDefaultIdentity<ApplicationUser>(
                options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<InventoryDbContext>();

            builder.Services.AddHttpClient<ZenoAIService>();

            // =========================================================
            // REGISTER REPOSITORIES
            // =========================================================

            builder.Services.AddScoped<
                IProductRepository,
                ProductRepository>();

            builder.Services.AddScoped<
                ICategoryRepository,
                CategoryRepository>();

            builder.Services.AddScoped<
                ISupplierRepository,
                SupplierRepository>();

            builder.Services.AddScoped<
                IPurchaseRepository,
                PurchaseRepository>();

            builder.Services.AddScoped<
                IPurchaseItemRepository,
                PurchaseItemRepository>();

            builder.Services.AddScoped<
                ISaleRepository,
                SaleRepository>();

            builder.Services.AddScoped<
                ISaleItemRepository,
                SaleItemRepository>();

            builder.Services.AddScoped<
                IStockMovementRepository,
                StockMovementRepository>();

            builder.Services.AddScoped<
                IDashboardRepository,
                DashboardRepository>();

            builder.Services.AddScoped<
                IReportRepository,
                ReportRepository>();

            // =========================================================
            // REGISTER SERVICES
            // =========================================================

            builder.Services.AddScoped<
                IProductService,
                ProductService>();

            builder.Services.AddScoped<
                ICategoryService,
                CategoryService>();

            builder.Services.AddScoped<
                ISupplierService,
                SupplierService>();

            builder.Services.AddScoped<
                IPurchaseService,
                PurchaseService>();

            builder.Services.AddScoped<
                IPurchaseItemService,
                PurchaseItemService>();

            builder.Services.AddScoped<
                ISaleService,
                SaleService>();

            builder.Services.AddScoped<
                ISaleItemService,
                SaleItemService>();

            builder.Services.AddScoped<
                IStockMovementService,
                StockMovementService>();

            builder.Services.AddScoped<
                IDashboardService,
                DashboardService>();

            builder.Services.AddScoped<
                IReportService,
                ReportService>();

            // =========================================================
            // REGISTER AUTOMAPPER
            // =========================================================

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ProductMapper>();
                cfg.AddProfile<CategoryMapper>();
                cfg.AddProfile<SupplierMapper>();
                cfg.AddProfile<PurchaseMapper>();
                cfg.AddProfile<PurchaseItemMapper>();
                cfg.AddProfile<SaleMapper>();
                cfg.AddProfile<SaleItemMapper>();
                cfg.AddProfile<StockMovementMapper>();
            });

            // =========================================================
            // MVC
            // =========================================================

            builder.Services.AddControllersWithViews();

            // =========================================================
            // ASP.NET CORE IDENTITY UI
            // =========================================================

            builder.Services.AddRazorPages();

            // =========================================================
            // BUILD APPLICATION
            // =========================================================

            var app = builder.Build();

            // =========================================================
            // SEED ROLES AND ADMIN USER
            // =========================================================

            using (var scope = app.Services.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider
                        .GetRequiredService<
                            RoleManager<IdentityRole>>();

                var userManager =
                    scope.ServiceProvider
                        .GetRequiredService<
                            UserManager<ApplicationUser>>();

                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                await db.Database.MigrateAsync();

                await SeedData.SeedRolesAsync(roleManager);

                await SeedData.SeedAdminAsync(userManager);
            }

            // =========================================================
            // HTTP REQUEST PIPELINE
            // =========================================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRouting();

            // Authentication
            app.UseAuthentication();

            // Authorization
            app.UseAuthorization();

            // =========================================================
            // STATIC FILES
            // =========================================================

            app.MapStaticAssets();

            // =========================================================
            // ASP.NET CORE IDENTITY PAGES
            // =========================================================

            app.MapRazorPages();

            // =========================================================
            // DEFAULT MVC ROUTE
            // =========================================================

            app.MapControllerRoute(
                name: "default",
                pattern:
                    "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // =========================================================
            // RUN APPLICATION
            // =========================================================

            app.Run();
        }
    }
}