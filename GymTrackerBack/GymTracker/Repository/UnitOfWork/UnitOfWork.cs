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
        readonly JwtSerwice _jwtSerwice;
        public UnitOfWork(WorkoutDbContext context, JwtSerwice jwtSerwice) {
        _context=context;
        _jwtSerwice = jwtSerwice;
            User = new UserRepository(_context,_jwtSerwice );
            Workout = new WorkoutRepository(_context);
            Exercises = new ExerciseRepository(_context);
           // WorkoutSets = new WorkoutSetRepository(_context);
           // BodyMeasurements = new BodyMeasurementRepository(_context);
        }

        public IUser User { get; private set; }
        public IWorkout Workout { get; private set; }
        public IExercise Exercises { get; private set; }
        //public IWorkoutSetRepository WorkoutSets { get; private set; }
        //public IBodyMeasurementRepository BodyMeasurements { get; private set; }

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
