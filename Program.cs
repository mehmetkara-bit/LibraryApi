using System.Text;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection["Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<LibraryApi.Infrastructure.Auth.JwtTokenService>();

var app = builder.Build();

//geçici test kullanıcısı
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Users.AnyAsync())
    {
        var admin = new LibraryApi.Domain.Entities.User
        {
            Email = "admin@local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = LibraryApi.Domain.Enums.UserRole.Staff,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        db.Staff.Add(new LibraryApi.Domain.Entities.Staff
        {
            Id = admin.Id,
            FirstName = "Admin",
            LastName = "User",
            RoleType = LibraryApi.Domain.Enums.StaffRoleType.Admin
        });

        await db.SaveChangesAsync();
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();