using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// auth google

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google/callback";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Gunakan port dari command line args atau default ke 5237
var port = Environment.GetEnvironmentVariable("PORT") ?? "5237";

// Konfigurasi Kestrel untuk menggunakan port yang ditentukan
builder.WebHost.UseUrls($"http://localhost:{port}");


var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("development");
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    Console.WriteLine("production");
    app.UseHttpsRedirection();
}

// Tambahkan middleware autentikasi dan otorisasi
app.UseAuthentication();
app.UseAuthorization();

// 🧩 Tambahkan ini supaya route controller terdaftar:
app.MapControllers(); // <── penting

// (optional) contoh endpoint lama boleh kamu hapus atau biarkan
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};



// 🔹 Endpoint memulai login Google
app.MapGet("/auth/google/login", async (HttpContext ctx) =>
{
    var props = new AuthenticationProperties
    {
        RedirectUri = "/signin-google/callback"
    };
    await ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
});

// 🔹 Callback dari Google OAuth
app.MapGet("/signin-google/callback", async (HttpContext ctx) =>
{
    var result = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (!result.Succeeded || result.Principal == null)
        return Results.BadRequest("Authentication failed");

    var email = result.Principal.FindFirstValue(ClaimTypes.Email);
    var name = result.Principal.FindFirstValue(ClaimTypes.Name);

    // TODO: cek di database apakah user sudah ada
    // var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    // 🔹 Buat JWT token
    var jwtKey = builder.Configuration["Jwt:Key"];
    var issuer = builder.Configuration["Jwt:Issuer"];
    var audience = builder.Configuration["Jwt:Audience"];
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim("name", name),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    // Redirect ke frontend Next.js bawa token (contoh: query string)
    var redirectUrl = $"https://myapp.com/auth/callback?token={jwt}";
    ctx.Response.Redirect(redirectUrl);
    return Results.Ok();
});

// 🔹 Protected route
app.MapGet("/api/me", [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] (ClaimsPrincipal user) =>
{
    return new { email = user.FindFirstValue(ClaimTypes.Email), name = user.FindFirstValue("name") };
});

app.Run();

