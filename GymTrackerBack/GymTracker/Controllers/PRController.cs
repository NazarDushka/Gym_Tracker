using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PRController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public PRController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetPersonalRecordInExercise")]
        public async Task<ActionResult<PersonalRecord>> GetPersonalRecordInExercise(int userId, int exerciseId)
        {
            var pr = await _unitOfWork.PersonalRecords.GetPersonalRecordInExerc(userId, exerciseId);
            if (pr == null) return NotFound($"No personal record found for user {userId} in exercise {exerciseId}.");
            return Ok(pr);
        }

        [HttpGet("GetPersonalRecordsForUser")]
        public async Task<ActionResult<List<PersonalRecord>>> GetPersonalRecordsForUser(int userId)
        {
            var prs = await _unitOfWork.PersonalRecords.GetPersonalRecordsForUser(userId);
            if (prs == null || !prs.Any()) return NotFound($"No personal records found for user {userId}.");
            return Ok(prs);
        }

        [HttpGet("OneRepMax")]
        public async Task<ActionResult<float>> GetOneRepMax(Workout workout)
        {
            float orm = 0;

            foreach (var set in workout.Sets)
            {
                float currentOrm = await _unitOfWork.PersonalRecords.GetORM(set);
                if (currentOrm > orm) orm = currentOrm;
            }

            return Ok(orm);
        }
    }
}
