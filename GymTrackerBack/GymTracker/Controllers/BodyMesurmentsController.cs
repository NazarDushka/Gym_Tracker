using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("GymTracker/[controller]")]
    [ApiController]
    public class BodyMesurmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public BodyMesurmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetBodyMeasurementsByUserId{id}")]
        public async Task<ActionResult> GetBodyMeasurementsByUserId(int id)
        {
            var bodyMeasurements = await _unitOfWork.BodyMeasurements.GetBodyMeasurementsByUserId(id);
            if (bodyMeasurements == null) return NotFound();
            return Ok(bodyMeasurements);
        }

        [HttpGet("GetBodyMeasurementById{id}")]
        public async Task<ActionResult> GetBodyMeasurementById(int id)
        {
            var bodyMeasurement = await _unitOfWork.BodyMeasurements.GetBodyMeasurementById(id);
            if (bodyMeasurement == null) return NotFound();
            return Ok(bodyMeasurement);
        }

        [HttpPost("AddBodyMeasurement")]
        public async Task<ActionResult> AddBodyMeasurement([FromBody] BodyMeasurements bodyMeasurement)
        {
            await _unitOfWork.BodyMeasurements.AddBodyMeasurement(bodyMeasurement);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpPut("UpdateBodyMeasurement{id}")]
        public async Task<ActionResult> UpdateBodyMeasurement(int id, [FromBody] BodyMeasurements bodyMeasurement)
        {
            if (id != bodyMeasurement.Id)
            {
                return BadRequest("Body Measurement ID in URL does not match ID in body.");
            }
            var existingBodyMeasurement = await _unitOfWork.BodyMeasurements.GetBodyMeasurementById(id);
            if (existingBodyMeasurement == null) return NotFound();
            existingBodyMeasurement.Weight = bodyMeasurement.Weight;
            existingBodyMeasurement.BodyFatPercentage = bodyMeasurement.BodyFatPercentage;
            existingBodyMeasurement.Chest = bodyMeasurement.Chest;
            existingBodyMeasurement.Waist = bodyMeasurement.Waist;
            existingBodyMeasurement.Hips = bodyMeasurement.Hips;
            existingBodyMeasurement.Biceps = bodyMeasurement.Biceps;
            existingBodyMeasurement.Legs = bodyMeasurement.Legs;
            existingBodyMeasurement.Calves = bodyMeasurement.Calves;
            existingBodyMeasurement.Forearms = bodyMeasurement.Forearms;
            existingBodyMeasurement.Height = bodyMeasurement.Height;
            existingBodyMeasurement.Date = bodyMeasurement.Date;

            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpDelete("DeleteBodyMeasurement{id}")]
        public async Task<ActionResult> DeleteBodyMeasurement(int id)
        {
            await _unitOfWork.BodyMeasurements.DeleteBodyMeasurement(id);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }
    }
}
