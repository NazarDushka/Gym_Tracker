namespace GymTracker.Interfaces
{
    public interface IUnitOfWork
    {
        IUser User { get; }
        IWorkout Workout { get; }
        IExercise Exercises { get; }
        //IWorkoutSetRepository WorkoutSets { get; } //In process...
        //IBodyMeasurementRepository BodyMeasurements { get; } //In process...

        Task<int> CompleteAsync();
    }
}
