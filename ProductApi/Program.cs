using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

// Load environment variables dari file .env
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// === Register Services ===

// 1. Tambahkan Controllers
builder.Services.AddControllers();

// 2. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Register DbContext dengan PostgreSQL
// Ambil dari file .env terlebih dahulu, jika kosong baru ambil dari appsettings.json
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
                       ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// === Configure Middleware Pipeline ===

// Swagger UI (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Dinonaktifkan sementara untuk development tanpa HTTPS agar tidak memunculkan pesan "warning" di terminal
app.UseAuthorization();

// Map controller routes
app.MapControllers();

app.Run();
