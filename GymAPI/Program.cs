using GymAPI.Data;
using GymAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Missing connection string.");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton(sp => new EmailService(
    Environment.GetEnvironmentVariable("SMTP_HOST")!,
    int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!),
    true,
    Environment.GetEnvironmentVariable("SMTP_USER")!,
    Environment.GetEnvironmentVariable("SMTP_PASS")!,
    Environment.GetEnvironmentVariable("FROM_NAME")!,
    Environment.GetEnvironmentVariable("FROM_EMAIL")!
));

builder.Services.AddScoped<ChipNumberGenerator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
