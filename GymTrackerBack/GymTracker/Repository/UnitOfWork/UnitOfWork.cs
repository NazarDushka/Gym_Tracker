using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.Auth;
using System.Threading.Tasks;

namespace GymTracker.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly WorkoutDbContext _context;
        readonly JwtService _jwtService;
        public UnitOfWork(WorkoutDbContext context, JwtService jwtService) {
        _context=context;
        _jwtService = jwtService;
            User = new UserRepository(_context, _jwtService);
            Workout = new WorkoutRepository(_context);
            Exercises = new ExerciseRepository(_context);
            PersonalRecords = new PRsRepository(_context);
            // WorkoutSets = new WorkoutSetRepository(_context);
            BodyMeasurements = new BodyMeasurementsRepository(_context);
        }

        public IUser User { get; private set; }
        public IWorkout Workout { get; private set; }
        public IExercise Exercises { get; private set; }
        public IPersonalRecord PersonalRecords { get; private set; }
        //public IWorkoutSetRepository WorkoutSets { get; private set; }
        public IBodyMeasurementRep BodyMeasurements { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
