using SystemZarzadzaniaSchroniskiem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaSchroniskiem.Areas.Identity.Data;


public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration.GetConnectionString("SystemZarzadzaniaSchroniskiemDbContextConnection") ?? throw new InvalidOperationException("Connection string 'SystemZarzadzaniaSchroniskiemDbContextConnection' not found.");

        services.AddDbContext<SystemZarzadzaniaSchroniskiem.Areas.Identity.Data.SchroniskoDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });
        });
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddTransient<IEmailSender, EmailSender>();

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<SchroniskoDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager<SignInManager<IdentityUser>>();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        });


        services.AddAuthentication();
        services.AddAuthorization();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/UserProfiles/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });

        services.AddMvc();

        services.AddControllersWithViews();

        services.AddRazorPages();

        services.AddScoped<BugReportService>(provider =>
        {
            var context = provider.GetRequiredService<SystemZarzadzaniaSchroniskiem.Areas.Identity.Data.SchroniskoDbContext>();
            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            var emailSender = provider.GetRequiredService<IEmailSender>();
            return new BugReportService(context, emailSender, userManager);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            // var userManager = context.RequestServices.GetService<UserManager<IdentityUser>>();
            // var user = await userManager.FindByEmailAsync("admin@admin.com");

            // if (user != null)
            // {
            // var token = await userManager.GeneratePasswordResetTokenAsync(user);
            // var resetLink = $"{context.Request.Scheme}://{context.Request.Host}/Identity/Account/ResetPassword?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(token)}";
            // logger.LogInformation($"Reset password link: {resetLink}");
            // }
            //

            logger.LogInformation("{} {}", context.Response.StatusCode, context.Request.Path);

            await next();

            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                // Loguj nieudane próby logowania
                logger.LogWarning($"404 error. Path: {context.Request.Path}");
            }
        });



        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated ?? false)
            {

                var dbContext = context.RequestServices.GetRequiredService<SystemZarzadzaniaSchroniskiem.Areas.Identity.Data.SchroniskoDbContext>();
                var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();

                if (await userManager.GetUserAsync(context.User) is IdentityUser user)
                {
                    var userId = await userManager.GetUserIdAsync(user);
                    var profile = await dbContext.UserProfiles.Where(p => p.UserId == userId).SingleOrDefaultAsync();
                    context.Items["UserProfile"] = profile;
                }
            }

            await next(context);
        });

        app.UseAuthorization();

        app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapControllerRoute(
                name: "redirect-account",
                pattern: "/Account/{**catchAll}",
                defaults: new { controller = "Redirect", action = "ToIdentityAccount" }
            );
            endpoints.MapFallbackToController("NotFoundPage", "Home");
            endpoints.MapRazorPages();
        });

    }

}
