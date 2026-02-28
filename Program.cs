using System.Text;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LibraryApi.Domain.Entities;
using LibraryApi.Domain.Enums;
using LibraryApi.Infrastructure.Catalog;
using LibraryApi.Infrastructure.Circulation;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library API",
        Version = "v1"
    });

    // 1. JWT Güvenlik Tanımı
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header. Örnek: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // Küçük harf 'bearer' önemli
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    // 2. Güvenlik Gereksinimi (Global olarak tüm endpoint'lere ekler)
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    };

    options.AddSecurityRequirement(securityRequirement);
});



builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookCopyService, BookCopyService>();
builder.Services.AddScoped<ILoanService, LoanService>();


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
        //admin staff
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

        // Member staff
        var memberUser = new User
        {
            Email = "member@local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member123!"),
            Role = UserRole.Member,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        db.Users.Add(memberUser);
        await db.SaveChangesAsync();

        db.Members.Add(new Member
        {
            Id = memberUser.Id,
            FirstName = "Test",
            LastName = "Member",
            Phone = null,
            Address = null,
            RegistrationDate = DateTime.UtcNow
        });


        await db.SaveChangesAsync();
    }

    await DbSeeder.SeedAsync(db);
}

//hemen yukarıda await DbSeeder.SeedAsync(db); diyerek hallettim
/*using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}*/


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();