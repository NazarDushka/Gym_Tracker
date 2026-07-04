namespace GymTracker.Interfaces
{
    public interface IUnitOfWork
    {
        IUser User { get; }
        IWorkout Workout { get; }
        IExercise Exercises { get; }
        IPersonalRecord PersonalRecords { get; }
        IMeasurementRepository Measurements { get; }

        Task<int> CompleteAsync();
    }
}
