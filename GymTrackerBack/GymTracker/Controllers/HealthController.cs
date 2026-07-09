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
      public IActionResult WakeUp(WorkoutDbContext db)
      {
            db.Database.CanConnectAsync(); // Check if the database connection is available
            return Ok("GymTracker API is running.");
        }
    }
}
