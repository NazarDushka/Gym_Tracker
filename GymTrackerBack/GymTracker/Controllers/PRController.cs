using GymTracker.DTOs.PersonalRecords;
using GymTracker.Extensions;
using GymTracker.Services.PersonalRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PRController : ControllerBase
    {
        private readonly IPRService _prService;
        public PRController(IPRService prService)
        {
            _prService = prService;
        }

        [HttpGet("GetPersonalRecordInExercise")]
        public async Task<ActionResult<PersonalRecordDto>> GetPersonalRecordInExercise(int exerciseId)
        {
            var pr = await _prService.GetPersonalRecordInExerc(User.GetUserId(), exerciseId);
            if (pr == null) return NotFound($"No personal record found in exercise.");
            return Ok(pr);
        }

        [HttpGet("GetPersonalRecordsForUser")]
        public async Task<ActionResult<List<PersonalRecordDto>>> GetPersonalRecordsForUser()
        {
            var prs = await _prService.GetPersonalRecordsForUser(User.GetUserId());
            if (prs == null || !prs.Any()) return NotFound($"No personal records found for user {User.GetUserId()}.");
            return Ok(prs);
        }

    }
}
