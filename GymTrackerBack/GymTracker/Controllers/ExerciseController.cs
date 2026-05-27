using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [Route("GymTracker/[controller]")]
    [ApiController]
    
    public class ExerciseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ExerciseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetExercises")]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises()
        {
            var exercises = await _unitOfWork.Exercises.GetAllExercises();
            if (exercises == null) return NotFound();
            return Ok(exercises);
        }

        [HttpGet("GetExercise{id}")]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExerciseById(int id)
        {
            var exercise = await _unitOfWork.Exercises.GetExerciseById(id);
            if (exercise == null) return NotFound();
            return Ok(exercise);
        }

        [HttpPost("AddExercise")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddExercise([FromBody] Exercise exercise)
        {
            await _unitOfWork.Exercises.AddExercise(exercise);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }


        [HttpDelete("DeleteExercise{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteExercise(int id)
        {
            await _unitOfWork.Exercises.DeleteExercise(id);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpPut("UpdateExercise{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateExercise(int id, [FromBody] Exercise exercise)
        {
            var existingExercise = await _unitOfWork.Exercises.GetExerciseById(id);
            if (existingExercise == null) return NotFound();
             existingExercise.Name = exercise.Name;
             existingExercise.Description = exercise.Description;
             existingExercise.MuscleGroup = exercise.MuscleGroup;
            await _unitOfWork.CompleteAsync();
            return Ok();

        }
    }
}


