namespace GymTracker.Interfaces
{
    public interface IUnitOfWork
    {
        IUser User { get; }
        IWorkout Workout { get; }
        IExercise Exercises { get; }
        //IWorkoutSetRepository WorkoutSets { get; } //In process...
        IBodyMeasurementRep BodyMeasurements { get; }

        Task<int> CompleteAsync();
    }
}
