using BinyanAv.PublicGateway.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Gateway")
    ?? throw new InvalidOperationException("ConnectionStrings:Gateway is not configured (MySQL).");

// יצירת wwwroot כדי ש-UseStaticFiles יוכל להגיש /uploads (תמונות ומסמכים).
var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(webRoot);
Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "photos"));

var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<GatewayDbContext>(o => o.UseMySql(cs, mysqlVersion));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yeshivabinyanav.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors();
app.MapControllers();
app.Run();
