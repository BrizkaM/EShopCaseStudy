//------------------------------------------------------------------------------------------
// File: Program.cs
//------------------------------------------------------------------------------------------
using EShopApi.WebApi.Handlers;
using EShopProject.Core.Interfaces;
using EShopProject.EShopDB;
using EShopProject.EShopDB.Data;
using EShopProject.EShopDB.Repositories;
using EShopProject.MessageQueue;
using EShopProject.MessageQueue.Interfaces;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Database configuration - SQLite
builder.Services.AddDbContext<EShopDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("EShopProject.EShopDB")));

// Repository
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IProductService, ProductService>();

// Inmemory Queue
builder.Services.AddSingleton<IStockUpdateQueue, InMemoryStockUpdateQueue>();
builder.Services.AddScoped<IStockUpdateHandler, StockUpdateHandler>();
builder.Services.AddHostedService<StockUpdateQueueProcessor>();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Shop API",
        Version = "v1",
        Description = "REST API for e-shop product management - Version 1",
        Contact = new OpenApiContact
        {
            Name = "E-Shop Support",
            Email = "support@eshop.com"
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "E-Shop API",
        Version = "v2",
        Description = "REST API for e-shop product management - Version 2",
        Contact = new OpenApiContact
        {
            Name = "E-Shop Support",
            Email = "support@eshop.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Configure Swagger to recognize API versions
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.RelativePath == null)
            return false;

        // Include endpoints based on version in route
        if (docName == "v1" && apiDesc.RelativePath.Contains("/v1/"))
            return true;

        if (docName == "v2" && apiDesc.RelativePath.Contains("/v2/"))
            return true;

        return false;
    });
});

// CORS (optional, for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EShopDbContext>();
        context.Database.Migrate();

        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Shop API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "E-Shop API v2");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "E-Shop API Documentation";
        options.DisplayRequestDuration();
    });
}

// Exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(exception, "Unhandled exception");

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "An error occurred while processing your request",
            Errors = new List<string> { exception?.Message ?? "Unknown error" }
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            // Add more exception types as needed
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    });
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Logger.LogInformation("E-Shop API is starting...");
app.Logger.LogInformation("Swagger UI available at: https://localhost:7001/swagger");

app.Run();
