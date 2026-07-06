using GymTracker.DTOs.PersonalRecords;
using GymTracker.DTOs.Workout;
using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IPersonalRecord
    {
        Task<PersonalRecord> GetPersonalRecordInExerc(int userId, int exerciseId);
        Task<IEnumerable<PersonalRecord>> GetPersonalRecordsForUser(int userId);
        Task Add(PersonalRecord pr);
        Task Update (int userId, int exerciseId, WorkoutSet set);
    }
}
