using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Implementations;
using ServerLibrary.Services.Interfaces;
using System.Text;
using Microsoft.OpenApi.Models;
using Data.Interceptor;
using Data.MongoModels;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Text.Json.Serialization;
using ServerLibrary.Services;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);

Env.Load("../.env");

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "Jwt",
        Scheme = "Bearer"
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
            new string[]{}
        }
    });
    options.CustomSchemaIds(type => type.Name);
});

//** Mongodb database
// builder.Services.Configure<MongoDBSettings>(
//     builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.Configure<MongoDBSettings>(options =>
{
    options.ConnectionString = builder.Configuration.GetValue<string>("MongoDBSettings:ConnectionString")
     ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");


    options.DatabaseName = builder.Configuration.GetValue<string>("MongoDBSettings:DatabaseName")
    ?? Environment.GetEnvironmentVariable("MONGODB_DATABASE");

});
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    if (string.IsNullOrEmpty(settings.ConnectionString))
    {
        throw new ArgumentNullException(nameof(settings.ConnectionString), "MongoDB connection string cannot be null or empty.");
    }

    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var mongoSettings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return new MongoDbContext(mongoClient, mongoSettings.DatabaseName);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
//     $"Server={Environment.GetEnvironmentVariable("DB_HOST")};" +
//     $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
//     $"User={Environment.GetEnvironmentVariable("DB_USER")};" +
//     $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
//     $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
//     "SslMode=Required;" ?? connectionString_1;


Console.WriteLine($"Connection: {connectionString}");
//** Mysql database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString,
        new MySqlServerVersion(new Version(8, 0, 30)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
    );
});

builder.Services.AddSingleton<TimestampInterceptor>();

// services
builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrdersService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IJobGroupService, JobGroupService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<S3Service>();

// authentication
var jwtKey = builder.Configuration["JwtSection:Key"];
var jwtIssuer = builder.Configuration["JwtSection:Issuer"];
var jwtAudience = builder.Configuration["JwtSection:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});
var clientUrls = new List<string>
{
    "http://localhost:3000"
};
var prodClientUrl = Environment.GetEnvironmentVariable("NEXT_PUBLIC_CLIENT_URL");

if (!string.IsNullOrEmpty(prodClientUrl))
{
    clientUrls.Add(prodClientUrl);
}

// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("OVIE_CLIENT", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000", "https://app.ovie.vn");
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowCredentials();
    });
});
Console.WriteLine($"CORS allowed origins: {string.Join(", ", clientUrls)}");

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

// **  Enable Authentication & Authorization
app.UseCors("OVIE_CLIENT");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
