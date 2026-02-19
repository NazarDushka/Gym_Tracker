using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Controllers
{
    [ApiController]
    [Route("GymTracker/[controller]")]
    public class WorkoutController : Controller
    {
        private readonly IUnitOfWork _unitOfWork; 

        public WorkoutController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("GetMyWorkouts")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetMyWorkouts(int userId)
        {
            if (userId == null) return Unauthorized("User not authenticated.");
            var workouts = await _unitOfWork.Workout.GetMyWorkouts(userId);
            if (workouts == null || !workouts.Any()) return NotFound("No workouts found for this user.");
            return Ok(workouts);
        }

        [HttpGet("GetAllWorkouts")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetAllWorkouts()
        {
           var workouts = await _unitOfWork.Workout.GetAll();
            return Ok(workouts); 
        }
        [HttpGet("GetWorkout{id}")]
        public async Task<ActionResult<Workout>> GetWorkout(int id) 
        {
            var workout = await _unitOfWork.Workout.Get(id);
            if (workout == null) { return NotFound($"Workout with ID {id} not found."); }
            return workout; }

        [HttpPost("AddWorkout")]
        public async Task<ActionResult<Workout>> AddWorkout([FromBody] Workout workout) 
        {
            await _unitOfWork.Workout.Add(workout); 
            await _unitOfWork.CompleteAsync();
            return Ok(workout);
        }

        [HttpPut("UpdateWorkout{id}")]
        public async Task<ActionResult> UpdateWorkout(int id, [FromBody] Workout updatedWorkout)
        {
            if (id != updatedWorkout.Id) // Ensure the ID in the route matches the ID in the body
            {
                return BadRequest("Workout ID in URL does not match ID in body.");
            }


            var existingWorkout = await _unitOfWork.Workout.Get(id);
            if (existingWorkout == null)
            {
                return NotFound($"Workout with ID {id} not found.");
            }

            existingWorkout.Sets=updatedWorkout.Sets;
            existingWorkout.Date = updatedWorkout.Date;
            existingWorkout.Notes = updatedWorkout.Notes;

            _unitOfWork.Workout.Update(existingWorkout);
            await _unitOfWork.CompleteAsync();

            return Ok("Workout with was updated");
        }
        [HttpDelete("DeleteWorkout{id}")]
        public async Task<ActionResult> DeleteWorkout(int id) {

            var workoutToDelete = await _unitOfWork.Workout.Get(id);
            if (workoutToDelete == null)
            {
                return NotFound($"Workout with ID {id} not found.");
            }
            _unitOfWork.Workout.Delete(workoutToDelete);
            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpGet("LastWorkout")]
        public async Task<ActionResult<Workout>> GetLastWorkout(int userId) {
       var LastWorkout= await _unitOfWork.Workout.GetMyLastWorkout(userId);
            if (LastWorkout == null) return NotFound();
            return Ok(LastWorkout);
        }

        [HttpGet("GetExercises")]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises()
        {
          var exercises = await _unitOfWork.Exercises.GetExercises();
            if(exercises==null) return NotFound();
            return Ok(exercises);
        }
    }
}
