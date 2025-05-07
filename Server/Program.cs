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
using System.Text.Json.Serialization;
using ServerLibrary.Services;
using ServerLibrary.Hubs;
using ServerLibrary.MiddleWare;


var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

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

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Ping client mỗi 15s
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // Timeout sau 30s
    options.MaximumReceiveMessageSize = 32 * 1024; // Giới hạn message 32KB
    options.EnableDetailedErrors = true; // Debug chi tiết (tắt trong production)
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
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Autuna CRM API",
        Description = "List of APIs for Autuna CRM",
        Contact = new OpenApiContact
        {
            Name = "Trang chủ",
            Url = new Uri("https://autuna.com/contact-us")
        },
        License = new OpenApiLicense
        {
            Name = "Phần mềm giải pháp ERP - CRM",
            Url = new Uri("https://app.autuna.com")
        }
    });
    options.CustomSchemaIds(type => type.Name);
});

//** Mongodb database
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

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

Console.WriteLine($"Connection string: {connectionString}");
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
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IUserService, UserService>();
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
builder.Services.AddScoped<IProductInventoryService, ProductInventoryService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<ICustomerCareService, CustomerCareService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<ILicenseCenterService, LicenseCenterService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<S3Service>();

builder.Services.Configure<FrontendConfig>(
    builder.Configuration.GetSection("Frontend"));
var frontendSection = builder.Configuration.GetSection("Frontend");

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
var baseUrl = frontendSection["BaseUrl"];
var devUrl = frontendSection["DevUrl"];
var clientUrls = new List<string>();
if (!string.IsNullOrEmpty(baseUrl)) clientUrls.Add(baseUrl);
if (!string.IsNullOrEmpty(devUrl)) clientUrls.Add(devUrl);
// CORS configuration
Console.WriteLine($"CORS allowed origins: {string.Join(", ", clientUrls)}");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AUTUNA_CRM", policyBuilder =>
    {
        policyBuilder.WithOrigins(clientUrls.ToArray())
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
    });
});
var httpClient = new HttpClient();
var ip = await httpClient.GetStringAsync("https://api.ipify.org");
Console.WriteLine($"Outbound IP: {ip}");
Console.WriteLine($"CORS allowed origins: {string.Join(", ", clientUrls)}");

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DefaultModelsExpandDepth(-1);
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API nội bộ Autuna");
});
// }

// **  Enable Authentication & Authorization
app.UseCors("AUTUNA_CRM");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();
