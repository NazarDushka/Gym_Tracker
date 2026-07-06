using GymTracker.DTOs.PersonalRecords;
using GymTracker.Interfaces;
using GymTracker.Models;

namespace GymTracker.Services.PersonalRecords
{
    public class PRService : IPRService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PRService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public float ORM(float weight, int reps)
        {
            if (reps <= 0)
                throw new ArgumentException("Reps must be greater than zero.");
            return weight * (1 + reps / 30.0f);
        }

        public async Task<PersonalRecordDto> GetPersonalRecordInExerc(int userId, int exerciseId)
        {
            var pr = await _unitOfWork.PersonalRecords.GetPersonalRecordInExerc(userId, exerciseId);
            if (pr == null)
                throw new KeyNotFoundException("Personal record not found.");
            return MapToDto(pr);
        }

       

        public async Task<IEnumerable<PersonalRecordDto>> GetPersonalRecordsForUser(int userId)
        {
            var prs = await _unitOfWork.PersonalRecords.GetPersonalRecordsForUser(userId);
            return prs.OrderBy(pr => pr.AchievedDate).Select(MapToDto);
        }

        public async Task AddPersonalRecord(int userId, AddPersonalRecordRequest request)
        {
            await _unitOfWork.PersonalRecords.Add(new PersonalRecord
            {
                WorkoutSetId = request.WorkoutSetId,
                UserId = userId,
                ExerciseId = request.ExerciseId,
                Weight = request.Weight,
                Reps = request.Reps,
                CalculatedMaxLift = ORM(request.Weight, request.Reps),
                AchievedDate = DateTime.UtcNow
            });
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdatePersonalRecord(int userId, UpdatePersonalRecordRequest request)
        {
            await _unitOfWork.PersonalRecords.Update(userId, request.ExerciseId, new WorkoutSet
            {
                Weight = request.Weight,
                Reps = request.Reps
            });
            await _unitOfWork.CompleteAsync();
        }

        private PersonalRecordDto MapToDto(PersonalRecord pr)
        {
            return new PersonalRecordDto
            {

                ExerciseId = pr.ExerciseId,
                ExerciseName = pr.ExerciseName,
                CalculatedMaxLift = pr.CalculatedMaxLift,
                Weight = pr.Weight,
                Reps = pr.Reps,
                AchievedDate = pr.AchievedDate
            };
        }
    }
}
