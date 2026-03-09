using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IPersonalRecord
    {
        Task<float> GetORM(WorkoutSet set);
        Task<PersonalRecord> GetPersonalRecordInExerc(int userId, int exerciseId);
        Task<List<PersonalRecord>> GetPersonalRecordsForUser(int userId);
        Task AddOrUpdatePersonalRecord(int userId, int exerciseId, WorkoutSet set);
    }
}
