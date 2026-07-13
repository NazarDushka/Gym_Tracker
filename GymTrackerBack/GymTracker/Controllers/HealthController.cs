using GymTracker.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("GymTracker/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
      [HttpGet("WakeUp")]
      public async Task<IActionResult> WakeUp(WorkoutDbContext db)
      {
            var canConnect = await db.Database.CanConnectAsync(); // Check if the database connection is available
            if (canConnect)
            {
                return Ok("GymTracker API is running.");
            }
            else
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Database connection is unavailable.");
            }
        }
    }
}
