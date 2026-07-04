using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Repository
{
    public class PRsRepository : IPersonalRecord
    {
        WorkoutDbContext _db;
        public PRsRepository(WorkoutDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task Add(PersonalRecord pr)
        {
            await _db.PersonalRecords.AddAsync(pr);
        }

        public async Task Update(int userId, int exerciseId, WorkoutSet set)
        {
            await _db.PersonalRecords.Where(pr => pr.UserId == userId && pr.ExerciseId == exerciseId)
                .ExecuteUpdateAsync(pr => pr.SetProperty(p => p.Reps, set.Reps)
                .SetProperty(p => p.Weight, set.Weight));
        }
        public async Task<PersonalRecord> GetPersonalRecordInExerc(int userId, int exerciseId)
        {
            var pr = await _db.PersonalRecords.FirstOrDefaultAsync(pr => pr.UserId == userId && pr.ExerciseId == exerciseId);
            if (pr == null) return null;
            return pr;
        }

        public async Task<IEnumerable<PersonalRecord>> GetPersonalRecordsForUser(int userId)
        {
            var prs = await _db.PersonalRecords.Where(pr => pr.UserId == userId).ToListAsync();
            if (prs == null) return null;
            return prs;
        }
    }
}
