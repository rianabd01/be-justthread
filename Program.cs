var builder = WebApplication.CreateBuilder(args);

// bind Supabase options from configuration (env vars, appsettings, user-secrets)
builder.Services.Configure<be_justthread.Services.SupabaseOptions>(builder.Configuration.GetSection("Supabase"));
// register JwtValidator that uses IOptions<SupabaseOptions>
builder.Services.AddSingleton<be_justthread.Services.JwtValidator>();

// 🧩 Tambahkan ini supaya controller aktif:
builder.Services.AddControllers(); // <── penting
// NOTE: Use HTTPS redirection only outside Development so local HTTP tests work
if (!builder.Environment.IsDevelopment())
{
    // in production/staging enforce HTTPS
    // the app will still be able to bind HTTP if configured in launchSettings or env
}
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});


var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🧩 Tambahkan ini supaya route controller terdaftar:
app.MapControllers(); // <── penting

// (optional) contoh endpoint lama boleh kamu hapus atau biarkan
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
