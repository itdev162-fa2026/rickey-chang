using Persistence;

var builder = WebApplication.CreateBuilder(args);
// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
    policy =>
    {
        policy.WithOrigins("http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic 400 response so we can return 422 for validation errors
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddDbContext<DataContext>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Use CORS
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();
app.Run();
