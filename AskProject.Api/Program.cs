using AskProject.Api.Services;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = false;
    options.StackBlockedRequests = false;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 60,
            Period = "1m"  // 60 requests per minute
        }
    };
});

builder.Services.AddHealthChecks();

// Register dependencies
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Add these service registrations
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ask Project API",
        Version = "v1",
        Description = "An API for the Ask Project"
    });
});

// Add controllers support
builder.Services.AddControllers();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercelApp", 
        builder =>
        {
            builder
                .WithOrigins(
                    "https://chat-app-two-phi-11.vercel.app",
                    "https://chat-app-two-phi-11.vercel.app:443"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Add in Program.cs before builder.Build()
builder.Services.AddHttpClient<HuggingFaceService>();
builder.Services.AddScoped<HuggingFaceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add this middleware in the pipeline (inside if(app.Environment.IsDevelopment()) block)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ask Project API v1");
    });
    // Add the CORS middleware - this position is important!
    app.UseCors("AllowAll");
}

// Add the CORS middleware - MOVE THIS OUTSIDE the development block
app.UseCors("AllowVercelApp");

// In middleware pipeline
app.MapHealthChecks("/health");

app.UseRouting();
app.UseHttpsRedirection();

// Later in the pipeline
app.MapControllers();

app.Run();


