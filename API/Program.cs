using API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDbContext<SqliteDataContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
});
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedAirlines(context);
    await Seed.SeedAirports(context);
    await Seed.SeedTailNumbers(context);
    await Seed.SeedTraining(context);
    await Seed.SeedUsers(context);
    await Seed.SeedUserPredictions(context);
}
catch (Exception ex)
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });
    ILogger logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred while calling the API.");
}

app.Run();
