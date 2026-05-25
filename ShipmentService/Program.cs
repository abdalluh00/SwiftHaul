using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SharedKernel.Auth;
using ShipmentService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

builder.Services.AddSharedJwt(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 1. Safely initialize Components if null
        document.Components ??= new();

        // 2. Initialize SecuritySchemes using the matching Interface types
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // 3. Define the JWT Bearer security scheme safely
        var jwtScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer", // Must be lowercase for RFC standards
            BearerFormat = "JWT",
            Description = "Enter your JWT token in the format: Bearer {your_token}"
        };

        // Use TryAdd to avoid duplicate key exceptions on hot-reloads
        document.Components.SecuritySchemes.TryAdd("Bearer", jwtScheme);

        // 4. Safely initialize the global security list
        document.Security ??= new List<OpenApiSecurityRequirement>();

        // 5. CORRECT SYNTAX: Pass the 'document' here, NOT the scheme object
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});


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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.Run();













