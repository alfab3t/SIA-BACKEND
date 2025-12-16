using astratech_apps_backend.Helpers;
using astratech_apps_backend.Repositories.Implementations;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Services.Implementations;
using astratech_apps_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace astratech_apps_backend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            var corsOrigin = configuration.GetSection("Key:corsAllowFrom").Get<string[]>();
            if (corsOrigin == null || corsOrigin.Length == 0)
                throw new InvalidOperationException("Konfigurasi CORS 'Key:corsAllowFrom' tidak ditemukan atau kosong.");

            var jwtKey = Environment.GetEnvironmentVariable("DECRYPT_KEY_JWT");
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("Environment variable 'DECRYPT_KEY_JWT' tidak ditemukan atau kosong.");

            var issuer = configuration.GetSection("Key:jwtIssuer").Get<string[]>();
            if (issuer == null || issuer.Length == 0)
                throw new InvalidOperationException("Konfigurasi JWT 'Key:jwtIssuer' tidak ditemukan atau kosong.");

            var audience = configuration["Key:jwtAudience"];
            if (string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("Konfigurasi JWT 'Key:jwtAudience' tidak ditemukan atau kosong.");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "ASTRATECH API", 
                    Version = "v1",
                    Description = "API untuk sistem informasi akademik ASTRATECH"
                });
                
                // Include XML comments for better documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Masukkan token JWT:",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ILdapService, LdapService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthorizationHandler, HasPermissionHandler>();

            builder.Services.AddScoped<IInstitusiRepository, InstitusiRepository>();

            builder.Services.AddScoped<ICutiAkademikRepository, CutiAkademikRepository>();
            builder.Services.AddScoped<ICutiAkademikService, CutiAkademikService>();

            builder.Services.AddScoped<IMeninggalDuniaRepository, MeninggalDuniaRepository>();
            builder.Services.AddScoped<IMeninggalDuniaService, MeninggalDuniaService>();

            builder.Services.AddScoped<IPengunduranDiriRepository, PengunduranDiriRepository>();
            builder.Services.AddScoped<IPengunduranDiriService, PengunduranDiriService>();

            builder.Services.AddScoped<IDropOutRepository, DropOutRepository>();
            builder.Services.AddScoped<IDropOutService, DropOutService>();





            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("HasPermission", policy =>
                {
                    policy.AddRequirements(new HasPermissionRequirement(""));
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builderCors =>
                    {
                        builderCors.WithOrigins(corsOrigin!)
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuers = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                };
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Where(ms => ms.Value?.Errors.Count > 0).Select(ms => ms.Value?.Errors.Select(e => e.ErrorMessage));
                    var result = new BadRequestObjectResult(new
                    {
                        message = "Bentuk payload yang dikirimkan tidak valid.",
                        errors
                    });
                    return result;
                };
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
