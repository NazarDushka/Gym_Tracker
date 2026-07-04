using GymTracker.Extensions;
using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("GymTracker/[controller]")]
    [ApiController]
    [Authorize]
    public class MeasurementsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public MeasurementsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/measurements/types
        [AllowAnonymous]
        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<MeasurementType>>> GetMeasurementTypes()
        {
            var types = await _unitOfWork.Measurements.GetAllTypesAsync();
            return Ok(types);
        }

        // GET: api/measurements
        [HttpGet("last")]
        public async Task<ActionResult<IEnumerable<MeasurementLog>>> GetLastLogsForUser()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");
            var logs = await _unitOfWork.Measurements.GetLastLogsForUserAsync(userId);
      
            return Ok(logs);
        }

        // GET: api/users/{userId}/measurements
        [HttpGet("~/measurements")]
        public async Task<ActionResult<IEnumerable<MeasurementLog>>> GetUserLogs() 
        {
            // Get the user ID from the authentication context
            int userId = User.GetUserId();

            // Check if the user exists
            var user = await _unitOfWork.User.GetUser(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var logs = await _unitOfWork.Measurements.GetLogsByUserIdAsync(userId);
            return Ok(logs);
        }

        // POST: api/measurements
        [HttpPost]
        public async Task<ActionResult> AddMeasurementLog([FromBody] MeasurementLog log)
        {
            // Validation of input data
            if (log == null)
                return BadRequest(new { message = "Measurement log data cannot be null" });
            
            if (log.UserId <= 0)
                return BadRequest(new { message = "UserId must be greater than 0" });
            
            if (log.MeasurementTypeId == Guid.Empty)
                return BadRequest(new { message = "MeasurementTypeId must be specified" });
            
            if (log.Value < 0)
                return BadRequest(new { message = "Value cannot be negative" });

            // Check if the user exists
            int userId = User.GetUserId();
            var user = await _unitOfWork.User.GetUser(userId);
            if (user == null)
                return BadRequest(new { message = "User not found" });
            // Check if the measurement type exists
            var types = await _unitOfWork.Measurements.GetAllTypesAsync();
            if (!types.Any(t => t.Id == log.MeasurementTypeId))
                return BadRequest(new { message = "Measurement type not found" });

            try
            {
                if (log.Date == default)
                {
                    log.Date = DateTime.UtcNow;
                }

                log.MeasurementType = null;

                await _unitOfWork.Measurements.AddLogAsync(log);
                await _unitOfWork.CompleteAsync(); 

                return StatusCode(StatusCodes.Status201Created, log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating measurement log"});
            }
        }

        // DELETE: api/measurements/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMeasurementLog(int id) 
        {
            try
            {
                int userId = User.GetUserId();
                var log = await _unitOfWork.Measurements.GetLogByIdAsync(id);
                if (log == null)
                    return NotFound(new { message = "Measurement log not found" });
                if (log.UserId != userId)
                    return BadRequest(new { message = "You do not have permission to delete this measurement log" });
                await _unitOfWork.Measurements.DeleteLogAsync(id);
                await _unitOfWork.CompleteAsync(); 

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting measurement log"});
            }
        }

        // GET: api/targets
        [HttpGet("~/targets")]
        public async Task<ActionResult<IEnumerable<MeasurementTarget>>> GetUserTargets() 
        {
            int userId = User.GetUserId();

            var targets = await _unitOfWork.Measurements.GetActiveTargetsByUserIdAsync(userId);
            return Ok(targets);
        }

        // POST: api/targets
        [HttpPost("targets")]
        public async Task<ActionResult> AddTarget([FromBody] MeasurementTarget target)
        {
            // Validate input data
            if (target == null)
                return BadRequest(new { message = "Target cannot be null" });
            
            if (target.UserId <= 0)
                return BadRequest(new { message = "UserId must be greater than 0" });
            
            if (target.MeasurementTypeId == Guid.Empty)
                return BadRequest(new { message = "MeasurementTypeId must be provided" });
            
            if (target.TargetValue < 0)
                return BadRequest(new { message = "TargetValue cannot be negative" });

            // Check if the user exists
            int userId = User.GetUserId();
            var user = await _unitOfWork.User.GetUser(userId);
            if (user == null)
                return NotFound(new { message = "User not found " });

            try
            {
                var activeTargets = await _unitOfWork.Measurements.GetActiveTargetsByUserIdAsync(target.UserId);
                var oldTarget = activeTargets.FirstOrDefault(t => t.MeasurementTypeId == target.MeasurementTypeId);

                if (oldTarget != null)
                {
                    await _unitOfWork.Measurements.DeactivateTargetAsync(oldTarget.Id);
                }

                target.CreatedAt = DateTime.UtcNow;
                target.IsActive = true;

                await _unitOfWork.Measurements.AddTargetAsync(target);
                await _unitOfWork.CompleteAsync();

                return StatusCode(StatusCodes.Status201Created, target);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating target"});
            }
        }

        // DELETE (Deactivate): api/targets/{id}
        [HttpDelete("targets/{id}")]
        public async Task<ActionResult> DeactivateTarget(int id) 
        {
            try
            {
                await _unitOfWork.Measurements.DeactivateTargetAsync(id);
                await _unitOfWork.CompleteAsync(); 

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deactivating target"});
            }
        }
    }
}