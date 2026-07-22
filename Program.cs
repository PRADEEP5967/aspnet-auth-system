using AspNetAuthSystem.Data;
using AspNetAuthSystem.Models;
using AspNetAuthSystem.Repository;
using AspNetAuthSystem.Services.Email;
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
    // Password settings
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    // Lockout settings
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
    
    // Email settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Configure Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "AspNetAuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ReturnUrlParameter = "returnUrl";
    options.SlidingExpiration = true;
});

// Register Email Services
if (builder.Environment.IsProduction())
{
    // Use SMTP email sender in production
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}
else
{
    // Use mock email sender in development (logs to console instead of sending)
    builder.Services.AddScoped<IEmailSender, MockEmailSender>();
}

// Register additional email services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Register Repository Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

// Add CORS policy if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply Migrations and Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        
        logger.LogInformation("Seeding database...");
        await SeedDatabase(services);
        
        logger.LogInformation("Database setup completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while setting up the database.");
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
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    try
    {
        // Ensure roles exist
        var roles = new[] { "Admin", "Teacher", "Student" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new Role 
                { 
                    Name = roleName, 
                    NormalizedName = roleName.ToUpper(),
                    Description = $"{roleName} role" 
                });
                
                if (result.Succeeded)
                {
                    logger.LogInformation("Role {RoleName} created successfully.", roleName);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Create Admin User from configuration
        var adminConfig = configuration.GetSection("AdminAccount");
        var adminUsername = adminConfig["Username"];
        var adminEmail = adminConfig["Email"];
        var adminPassword = adminConfig["Password"];
        var adminFirstName = adminConfig["FirstName"] ?? "Admin";
        var adminLastName = adminConfig["LastName"] ?? "User";

        if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Admin user '{AdminUsername}' created successfully.", adminUsername);
                }
                else
                {
                    logger.LogError("Failed to create admin user '{AdminUsername}': {Errors}", 
                        adminUsername,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user '{AdminUsername}' already exists.", adminUsername);
            }
        }
        else
        {
            logger.LogWarning("Admin account configuration is incomplete in appsettings.json. Skipping admin user creation.");
        }

        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}
