using Microsoft.OpenApi.Models;
using TaskHive.Application.UseCases.Projects;
using TaskHive.Application.UseCases.Users;
using TaskHive.Infrastructure.Configuration;
using TaskHive.API.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     });

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add use cases
builder.Services.AddScoped<SignUpUseCase>();
builder.Services.AddScoped<SignInUseCase>();
builder.Services.AddScoped<CreateProjectUseCase>();
builder.Services.AddScoped<ListProjectsUseCase>();
builder.Services.AddScoped<SendVerificationEmailUseCase>();
builder.Services.AddScoped<VerifyEmailUseCase>();

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskHive API",
        Version = "v1",
        Description = "API for TaskHive project management system",
        Contact = new OpenApiContact
        {
            Name = "TaskHive Team",
            Email = "support@taskhive.com"
        }
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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
            Array.Empty<string>()
        }
    });

    // Include XML Comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskHive API V1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "TaskHive API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide models section by default
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    await next();
});

app.UseHttpsRedirection();

// Add error handling middleware
app.UseErrorHandling();

// Add CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
