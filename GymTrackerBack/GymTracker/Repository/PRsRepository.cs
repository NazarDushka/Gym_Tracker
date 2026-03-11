using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Repository
{
    public class PRsRepository: IPersonalRecord
    {
        WorkoutDbContext _db;
        public PRsRepository(WorkoutDbContext dbContext) 
        {
        _db = dbContext;
        }

        public async Task AddOrUpdatePersonalRecord(int userId, int exerciseId, WorkoutSet set)
        {
            var existingPR = await _db.PersonalRecords.FirstOrDefaultAsync(pr=>pr.UserId==userId && pr.ExerciseId==exerciseId);
            if (existingPR == null) { 
                var newPR = new PersonalRecord
                {
                    WorkoutSetId=set.Id,
                    UserId = userId,
                    ExerciseId = exerciseId,
                    CalculatedMax = PersonalRecord.ORM(set.Weight, set.Reps),
                    AchievedDate = DateTime.Now
                };
                _db.PersonalRecords.Add(newPR);
            }
            else if (PersonalRecord.ORM(set.Weight, set.Reps) > existingPR.CalculatedMax)
            {
                existingPR.CalculatedMax = PersonalRecord.ORM(set.Weight, set.Reps);
                _db.PersonalRecords.Update(existingPR);
            }
        }

        public Task<float> GetORM(WorkoutSet set)
        {
            float maxLift = PersonalRecord.ORM(set.Weight, set.Reps);
            return Task.FromResult(maxLift);
        }

        public async Task<PersonalRecord> GetPersonalRecordInExerc(int userId, int exerciseId)
        {
           var pr = await _db.PersonalRecords.FindAsync(userId, exerciseId);
            if (pr == null) return null;
            return pr;
        }

        public async Task<List<PersonalRecord>> GetPersonalRecordsForUser(int userId)
        {
            var prs = await _db.PersonalRecords.Where(pr => pr.UserId == userId).ToListAsync();
            if (prs == null) return null;
            return prs;
        }
    }
}
