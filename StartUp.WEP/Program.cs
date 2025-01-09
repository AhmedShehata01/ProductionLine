using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using StartUp.BLL.Mapper;
using StartUp.BLL.Middleware;
using StartUp.BLL.Repository;
using StartUp.BLL.Services;
using StartUp.BLL.Services.AppSecurity;
using StartUp.DAL.Database;
using StartUp.DAL.Extend;
using StartUp.DAL.StaticData;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings()
.GetCurrentClassLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);


    #region Logger
    // NLog : setup NLog for dependency Injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    #endregion


    // Add services to the container.
    builder.Services.AddControllersWithViews();


    #region Auto Mapper Service

    builder.Services.AddAutoMapper(x => x.AddProfile(new DomainProfile()));

    #endregion


    #region Connection String Decryption
    // Retrieve the encrypted connection string from appsettings.json
    var encryptedConnectionString = builder.Configuration.GetConnectionString("ApplicationConnection");

    // Define the new secret key for decryption
    var secretKey = "012345678901234567890123456789Aa"; // Ensure this matches your encryption key

    // Decrypt the connection string using the custom decryption logic
    var decryptor = new Decrypt(secretKey);
    var decryptedConnectionString = decryptor.DecryptConnectionString(encryptedConnectionString);

    // Log the decrypted connection string for debugging (remove in production)
    Console.WriteLine("Decrypted Connection String: " + decryptedConnectionString);

    string cleanedConnectionString = decryptedConnectionString.Replace(@"\\", @"\");

    // Use the decrypted connection string for the DbContext
    builder.Services.AddDbContext<ApplicationContext>(options =>
        options.UseSqlServer(cleanedConnectionString));
    #endregion


    #region Microsoft Identity Configuration

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.LoginPath = new PathString("/Account/Login");
                            options.AccessDeniedPath = new PathString("/Account/Login");
                        });

    builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddRoles<ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationContext>()
                    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -._@+";

        // Default Password settings.

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 0;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    }).AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

    // reduce lifeTime of All Tokens Type (by default 1 day)
    builder.Services.Configure<DataProtectionTokenProviderOptions>(c =>
                                    c.TokenLifespan = TimeSpan.FromHours(1));

    #endregion

    #region Claim Policy 
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ViewRolePolicy",
            policy => policy.RequireAssertion(context =>
                context.User.HasClaim(claim => claim.Type == "View Role" && claim.Value == "true") ||
                context.User.IsInRole("Super Admin")
            ));

        options.AddPolicy("CreateRolePolicy",
            policy => policy.RequireAssertion(context =>
                context.User.HasClaim(claim => claim.Type == "Create Role" && claim.Value == "true") ||
                context.User.IsInRole("Super Admin")
            ));

        options.AddPolicy("EditRolePolicy",
            policy => policy.RequireAssertion(context =>
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                context.User.IsInRole("Super Admin")
                ));

        options.AddPolicy("DeleteRolePolicy",
            policy => policy.RequireAssertion(context =>
                context.User.HasClaim(claim => claim.Type == "Delete Role" && claim.Value == "true") ||
                context.User.IsInRole("Super Admin")
                ));
    });

    #endregion



    #region AddScoped Services

    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();

    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ISmsService, SmsService>();

    builder.Services.AddScoped<IFileStorageService, FileStorageService>();



    // Register SubscriptionService as Singleton
    builder.Services.AddSingleton<Subscription>();  // Register Subscription service
    #endregion


    #region Bind the TimeZoneSettings to appsettings.json
    builder.Services.Configure<TimeZoneSettings>(builder.Configuration.GetSection("TimeZoneSettings"));

    #endregion


    var app = builder.Build();

    // Ensure roles and admin user are seeded
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        await SeedData.SeedRolesAndAdminUser(services, userManager, roleManager);
    }


    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }


    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Add the SubscriptionMiddleware to the pipeline
    app.UseMiddleware<SubscriptionMiddleware>();

    app.UseRouting();

    app.UseAuthentication(); // Enable Authentication
    app.UseAuthorization();  // Enable Authorization

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();

}
catch (SqlException sqlEx)
{
    logger.Error($"Database connection failed: {sqlEx.Message}");
}
catch (Exception ex)
{
    logger.Error(ex, "An error occurred while starting the application.");
}
finally
{
    LogManager.Shutdown();
}