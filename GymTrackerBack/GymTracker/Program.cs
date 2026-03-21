using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.Auth;
using GymTracker.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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
                                 policy.WithOrigins("http://localhost:4200")
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
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUser, UserRepository>();
builder.Services.AddScoped<IWorkout, WorkoutRepository>();
builder.Services.AddScoped<IExercise, ExerciseRepository>();
builder.Services.AddScoped<IPersonalRecord,PRsRepository>();
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

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
   public static bool IsTestRun { get; set; } = false;
}
