using Cinema.Data;
using Cinema.Repositories;
using Cinema.Services;
using Microsoft.EntityFrameworkCore;

namespace Cinema
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

          
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICinemaService, CinemaService>();
            builder.Services.AddScoped<IActorService, ActorService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

           
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
            );

            
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}"
            );
            app.MapGet("/", () => Results.Redirect("/Admin/Dashboard"));
            app.Run();
        }
    }
}