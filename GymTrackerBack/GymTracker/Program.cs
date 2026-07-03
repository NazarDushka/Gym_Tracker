using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.Auth;
using GymTracker.Repository.UnitOfWork;
using GymTracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; 
using System.Text; 
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                             policy =>
                             {
                                 policy.WithOrigins("https://mypersonalgymtrackerproject.netlify.app","http://localhost:4200")
                                         .AllowAnyHeader()
                                         .AllowAnyMethod();
                             });

    options.AddPolicy(name: "AllowAll",
                             policy =>
                             {
                                 policy.AllowAnyOrigin() 
                                         .AllowAnyHeader()
                                         .AllowAnyMethod();
                             });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (Program.IsTestRun)
{
    builder.Services.AddDbContext<WorkoutDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDbForTesting"));
}
else
{
    builder.Services.AddDbContext<WorkoutDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));

var secretKey = builder.Configuration["AuthSettings:SecretKey"];
var issuer = builder.Configuration["AuthSettings:Issuer"];
var audience = builder.Configuration["AuthSettings:Audience"];

if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("AuthSettings:SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(context.Exception, "Authentication failed.");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated successfully for user: {User}", context.Principal.Identity.Name);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUser, UserRepository>();
builder.Services.AddScoped<IWorkout, WorkoutRepository>();
builder.Services.AddScoped<IExercise, ExerciseRepository>();
builder.Services.AddScoped<IPersonalRecord, PRsRepository>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

if (Program.IsTestRun)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors(MyAllowSpecificOrigins);
}
app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
    public static bool IsTestRun { get; set; } = false;
}
