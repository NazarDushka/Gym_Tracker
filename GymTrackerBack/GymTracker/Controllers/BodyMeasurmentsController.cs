using GymTracker.DTOs.BodyMeasurements;
using GymTracker.Extensions;
using GymTracker.Services.BodyMeasurements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("GymTracker/[controller]")]
    [ApiController]
    [Authorize]
    public class MeasurementsController : ControllerBase
    {
        private readonly IBodyMeasurementsService _bodyMeasurementsService;
        public MeasurementsController(IBodyMeasurementsService bodyMeasurementsService)
        {
            _bodyMeasurementsService = bodyMeasurementsService;
        }

        // GET: api/measurements/types
        [AllowAnonymous]
        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<MeasurementTypeDto>>> GetMeasurementTypes()
        {
            var types = await _bodyMeasurementsService.GetMeasurementTypesAsync();
            return Ok(types);
        }

        // GET: api/measurements
        [HttpGet("last")]
        public async Task<ActionResult<IEnumerable<MeasurementLogDto>>> GetLastLogsForUser()
        {
            try
            {
                var logs = await _bodyMeasurementsService.GetLastUsersMeasurementLogsAsync(User.GetUserId());
                return Ok(logs);
            } catch (KeyNotFoundException ex) {
                return NotFound(new { message = ex.Message });
            } catch (Exception ex) {
                return StatusCode(500, new { message = "Error retrieving last measurement logs" });
            }
        }

        // GET: api/users/{userId}/measurements
        [HttpGet("UsersMeasurements")]
        public async Task<ActionResult<IEnumerable<MeasurementLogDto>>> GetUserLogs() 
        {  
            var logs = await _bodyMeasurementsService.GetMeasurementLogsAsync(User.GetUserId());
            return Ok(logs);
        }

        // POST: api/measurements
        [HttpPost]
        public async Task<ActionResult> AddMeasurementLog([FromBody] CreateMeasurementLogRequest request)
        {
            try
            {
                await _bodyMeasurementsService.CreateMeasurementLogAsync(User.GetUserId(), request);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating measurement log" });
            }
        }

        // DELETE: api/measurements/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMeasurementLog(Guid id) 
        {
            try
            {
               await _bodyMeasurementsService.DeleteMeasurementLogAsync(User.GetUserId(), id);

               return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
               return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
               return StatusCode(500, new { message = "Error deleting measurement log"});
            }
        }

        // GET: api/targets
        [HttpGet("~/targets")]
        public async Task<ActionResult<IEnumerable<MeasurementTargetDto>>> GetUserTargets() 
        {
            var targets = await _bodyMeasurementsService.GetActiveMeasurementTargetsAsync(User.GetUserId());
            return Ok(targets);
        }

        // POST: api/targets
        [HttpPost("targets")]
        public async Task<ActionResult> AddTarget([FromBody] CreateTargetRequest target)
        {
            try 
            {
                await _bodyMeasurementsService.CreateMeasurementTargetAsync(User.GetUserId(), target);
                return StatusCode(StatusCodes.Status201Created);
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
                await _bodyMeasurementsService.DeactivateTargetAsync(User.GetUserId(), id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deactivating target"});
            }
        }
    }
}