using AskProject.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
}

app.UseHttpsRedirection();

// Later in the pipeline
app.MapControllers();

app.Run();


