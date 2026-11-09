using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.WebApp.Auth.Services;
using ResourceBookingSystem.WebApp.Auth.ServiceInterfaces;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using ResourceBookingSystem.WebApp.UserDomain.ServiceInterfaces;
using ResourceBookingSystem.WebApp.UserDomain.Services;
using ResourceBookingSystem.WebApp.Resource.ServiceInterfaces;
using ResourceBookingSystem.WebApp.Resource.Services;
using ResourceBookingSystem.WebApp.Booking.ServiceInterfaces;
using ResourceBookingSystem.WebApp.Booking.Services;
using ResourceBookingSystem.WebApp.ResourceCategory.ServiceInterfaces;
using ResourceBookingSystem.WebApp.ResourceCategory.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IResourceCategoryService, ResourceCategoryService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd(
            "requestId",
            context.HttpContext.TraceIdentifier);

        var activity = context.HttpContext.Features
            .Get<IHttpActivityFeature>()?.Activity;

        context.ProblemDetails.Extensions.TryAdd(
            "traceId",
            activity?.Id);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Resource Booking API",
        Version = "v1",
        Description = "Resource Booking Backend API"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without 'Bearer ' prefix)"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Resource Booking API")
            .WithTheme(ScalarTheme.Moon)
            .WithOpenApiRoutePattern("/openapi/{documentName}.json");
    });
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
