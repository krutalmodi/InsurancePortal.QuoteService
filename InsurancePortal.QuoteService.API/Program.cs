using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.Services;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Infrastructure.Clients;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 1. REGISTER SWAGGER SERVICES
builder.Services.AddEndpointsApiExplorer(); // Required for endpoint discovery
builder.Services.AddSwaggerGen();

#region Database Configuration

// Add DbContext with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<QuoteDbContext>(options =>
    options.UseSqlServer(connectionString));
#endregion

#region Dependency Injection

// Register application services
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddHttpClient<ILocationClient, LocationClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["LocationService:BaseUrl"] ?? throw new InvalidOperationException("LocationService:BaseUrl is not configured")));

#endregion

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// 2. ENABLE SWAGGER MIDDLEWARE (Best practice: Restrict to Development environment)
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Serves the generated OpenAPI spec as a JSON endpoint
    app.UseSwaggerUI(); // Serves the interactive web UI
    app.MapOpenApi();
}

// 3. ENABLE CORS MIDDLEWARE (Must be before UseAuthorization)
app.UseCors("AllowFrontend");

#region Database Initialization

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<QuoteDbContext>().Database.Migrate();
}

#endregion

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

