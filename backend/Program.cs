using AutoServ.Core.Interfaces;
using AutoServ.Infrastructure.Data;
using AutoServ.Infrastructure.Services;
using AutoServ.Infrastructure.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// реєстрація стратегій та базових сервісів
builder.Services.AddScoped<IMasterAllocationStrategy, LeastLoadStrategy>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IMasterAllocationService, MasterAllocationService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IAdminServiceService, AdminServiceService>();


builder.Services.AddScoped<IPaymentService, LiqPayService>(); // реєстрація liqpay

// реєстрація файлового сховища (LocalStorageService)
// builder.Services.AddSingleton<IFileStorageService, LocalStorageService>();

// реєстрація AWS3
//builder.Services.AddSingleton<IFileStorageService, S3StorageService>();
// реєстрація Cloudinary
builder.Services.AddSingleton<IFileStorageService, CloudinaryStorageService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
if (string.IsNullOrEmpty(secretKey)) throw new Exception("JWT SecretKey не знайдено!");

builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; })
.AddJwtBearer(options => { options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = jwtSettings["Issuer"], ValidAudience = jwtSettings["Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) }; });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoServ API", Version = "v1" }); c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.Http, Scheme = "bearer" }); c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } } }); });

builder.Services.AddCors(options => { options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:3000", "https://localhost:3000").AllowAnyHeader().AllowAnyMethod()); });

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});
// ====================================================================================

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();