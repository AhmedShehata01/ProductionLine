using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using StartUp.BLL.Middleware;
using StartUp.BLL.Services.AppSecurity;
using StartUp.DAL.Database;
using StartUp.DAL.Extend;

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


    #region AddScoped Services

    // Register SubscriptionService as Singleton
    builder.Services.AddSingleton<Subscription>();  // Register Subscription service
    #endregion


    #region Bind the TimeZoneSettings to appsettings.json
    builder.Services.Configure<TimeZoneSettings>(builder.Configuration.GetSection("TimeZoneSettings"));

    #endregion


    var app = builder.Build();

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

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();

}
catch (Exception ex)
{

    logger.Error(ex);
}
finally
{
    LogManager.Shutdown();
}