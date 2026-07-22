using AspNetAuthSystem.Data;
using AspNetAuthSystem.Models;
using AspNetAuthSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "AspNetAuthCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ReturnUrlParameter = "returnUrl";
    options.SlidingExpiration = true;
});

// Register Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

var app = builder.Build();

// Apply Migrations and Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await SeedDatabase(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Seed Database Function
async Task SeedDatabase(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

    // Create Admin User
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "admin",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Create Teacher User
    var teacherUser = await userManager.FindByNameAsync("teacher");
    if (teacherUser == null)
    {
        teacherUser = new User
        {
            UserName = "teacher",
            Email = "teacher@example.com",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(teacherUser, "Teacher@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(teacherUser, "Teacher");
        }
    }

    // Create Student Users
    var studentUser = await userManager.FindByNameAsync("student");
    if (studentUser == null)
    {
        studentUser = new User
        {
            UserName = "student",
            Email = "student@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(studentUser, "Student@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(studentUser, "Student");
        }
    }

    // Seed Sample Courses
    if (!context.Courses.Any())
    {
        var courses = new List<Course>
        {
            new Course
            {
                Title = "C# Fundamentals",
                Description = "Learn the basics of C# programming language",
                Content = "This course covers variables, loops, functions, and object-oriented programming concepts.",
                TeacherId = teacherUser!.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                Title = "ASP.NET Core Web Development",
                Description = "Build modern web applications with ASP.NET Core",
                Content = "Learn MVC architecture, Entity Framework, authentication, and deployment.",
                TeacherId = teacherUser!.Id,
                CreatedAt = DateTime.UtcNow
            },
            new Course
            {
                Title = "Database Design with SQL Server",
                Description = "Master SQL Server and database design principles",
                Content = "Cover normalization, indexes, stored procedures, and query optimization.",
                TeacherId = teacherUser!.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();
    }
}
