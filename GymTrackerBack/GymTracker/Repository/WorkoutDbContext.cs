using GymTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Data.Common;

namespace GymTracker.Repository
{
    public class WorkoutDbContext : DbContext
    {
        public WorkoutDbContext(DbContextOptions<WorkoutDbContext> options)
         : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutSet> WorkoutSets { get; set; }
        public DbSet<PersonalRecord> PersonalRecords { get; set; }
        public DbSet<MeasurementType> MeasurementTypes { get; set; }
        public DbSet<MeasurementTarget> MeasurementTargets { get; set; }
        public DbSet<MeasurementLog> MeasurementLogs { get; set; }
    }
}
