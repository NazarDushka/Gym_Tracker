using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("last")]
        public async Task<ActionResult<IEnumerable<MeasurementLog>>> GetLastLogsForUser()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");
            var logs = await _unitOfWork.Measurements.GetLastLogsForUserAsync(userId);
            if (logs == null || !logs.Any())
                return NotFound(new { message = "Logs not found for the specified user" });
            return Ok(logs);
        }

        // GET: api/users/{userId}/measurements
        [HttpGet("~/api/users/{userId}/measurements")]
        public async Task<ActionResult<IEnumerable<MeasurementLog>>> GetUserLogs(int userId) 
        {
            // Проверка существования пользователя
            var user = await _unitOfWork.User.GetUser(userId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var logs = await _unitOfWork.Measurements.GetLogsByUserIdAsync(userId);
            return Ok(logs);
        }

        // POST: api/measurements
        [HttpPost]
        public async Task<ActionResult> AddMeasurementLog([FromBody] MeasurementLog log)
        {
            // Валидация входных данных
            if (log == null)
                return BadRequest(new { message = "Данные логирования не могут быть пустыми" });
            
            if (log.UserId <= 0)
                return BadRequest(new { message = "UserId должен быть больше 0" });
            
            if (log.MeasurementTypeId <= 0)
                return BadRequest(new { message = "MeasurementTypeId должен быть больше 0" });
            
            if (log.Value < 0)
                return BadRequest(new { message = "Value не может быть отрицательным" });

            // Проверка существования пользователя
            var user = await _unitOfWork.User.GetUser(log.UserId);
            if (user == null)
                return BadRequest(new { message = "Пользователь не найден" });

            // Проверка существования типа измерения
            var types = await _unitOfWork.Measurements.GetAllTypesAsync();
            if (!types.Any(t => t.Id == log.MeasurementTypeId))
                return BadRequest(new { message = "Тип измерения не найден" });

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
                return StatusCode(500, new { message = "Ошибка при создании логирования", details = ex.Message });
            }
        }

        // DELETE: api/measurements/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMeasurementLog(int id) 
        {
            try
            {
                var log = await _unitOfWork.Measurements.GetLogByIdAsync(id);
                if (log == null)
                    return NotFound(new { message = "Логирование не найдено" });

                await _unitOfWork.Measurements.DeleteLogAsync(id);
                await _unitOfWork.CompleteAsync(); 

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при удалении логирования", details = ex.Message });
            }
        }

        // GET: api/users/{userId}/targets
        [HttpGet("~/api/users/{userId}/targets")]
        public async Task<ActionResult<IEnumerable<MeasurementTarget>>> GetUserTargets(int userId) 
        {
            // Проверка существования пользователя
            var user = await _unitOfWork.User.GetUser(userId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            var targets = await _unitOfWork.Measurements.GetActiveTargetsByUserIdAsync(userId);
            return Ok(targets);
        }

        // POST: api/targets
        [HttpPost("targets")]
        public async Task<ActionResult> AddTarget([FromBody] MeasurementTarget target)
        {
            // Валидация входных данных
            if (target == null)
                return BadRequest(new { message = "Целевое значение не может быть пустым" });
            
            if (target.UserId <= 0)
                return BadRequest(new { message = "UserId должен быть больше 0" });
            
            if (target.MeasurementTypeId <= 0)
                return BadRequest(new { message = "MeasurementTypeId должен быть больше 0" });
            
            if (target.TargetValue < 0)
                return BadRequest(new { message = "TargetValue не может быть отрицательным" });

            // Проверка существования пользователя
            var user = await _unitOfWork.User.GetUser(target.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

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
                return StatusCode(500, new { message = "Ошибка при создании целевого значения", details = ex.Message });
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
                return StatusCode(500, new { message = "Ошибка при деактивации целевого значения", details = ex.Message });
            }
        }
    }
}