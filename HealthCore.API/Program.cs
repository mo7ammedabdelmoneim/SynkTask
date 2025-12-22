using SynkTask.API.Configurations.Models;
using SynkTask.DataAccess.Data;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SynkTask.API.Configurations;

namespace SynkTask.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Add DB Service
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JWT"));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register the Identity Service
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {   // with Reset password Rules
                // Password settings (strong but user-friendly)
                options.Password.RequireDigit = true;                    // at least one number
                options.Password.RequireLowercase = true;                // at least one lowercase letter
                options.Password.RequireUppercase = true;                // at least one uppercase letter
                options.Password.RequireNonAlphanumeric = true;          // at least one special character
                options.Password.RequiredLength = 8;                     // minimum length
                options.Password.RequiredUniqueChars = 1;                // at least one unique character

                // User settings
                options.User.RequireUniqueEmail = true;                  // emails must be unique

                // Lockout settings (optional, security feature)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            // Register Global Exception Handling 
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails(); 


            // Register the Cors Service
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });

            // ******************* Check Token *******************
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //[Authrize]
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // return UnAuthorized not "Not Found" 
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>  // check veryfied Key
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:VaildIssuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:VaildAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecurityKey"])),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            await SeedRolesAsync(app);

          //  app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles()  ;

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }


        static async Task SeedRolesAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "admin", "teamLead", "teamMember" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
