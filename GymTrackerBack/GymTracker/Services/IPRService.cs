using GymTracker.DTOs.PersonalRecords;

namespace GymTracker.Services
{
    public interface IPRService
    {
        public float ORM(float weight, int reps);
        public Task<PersonalRecordDto> GetPersonalRecordInExerc(int userId, int exerciseId);
        public Task<IEnumerable<PersonalRecordDto>> GetPersonalRecordsForUser(int userId);
        public Task AddPersonalRecord(int userId, AddPersonalRecordRequest request);
        public Task UpdatePersonalRecord(int userId, UpdatePersonalRecordRequest request);
    }
}
