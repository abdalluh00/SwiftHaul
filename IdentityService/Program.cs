using IdentityService.Common.Helpers;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SharedKernel.Auth;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// Jwt setup using the shared extension method for cleaner code and better separation of concerns
builder.Services.AddSharedJwt(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 1. Safely initialize Components if null
        document.Components ??= new();

        // 2. Initialize SecuritySchemes using the matching Interface types
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // 3. Define the JWT Bearer security scheme safely
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token in the format: Bearer {your_token}"
        });

        // 4. Safely initialize the global security list
        document.Security ??= new List<OpenApiSecurityRequirement>();

        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddControllers();

builder.Services.AddScoped<JwtHelper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 1. This generates and serves the native JSON document at /openapi/v1.json
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        // Point directly to the native JSON file generated above
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();