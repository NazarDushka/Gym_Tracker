using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            workouts = workouts.OrderByDescending(w => w.Date).ToList();
            return Ok(workouts);
        }

        [HttpGet("GetAllWorkouts")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetAllWorkouts()
        {
           var workouts = await _unitOfWork.Workout.GetAll();
            workouts = workouts.OrderByDescending(w => w.Date).ToList();

            return Ok(workouts); 
        }

        [HttpGet("GetWorkout/{id}")]
        public async Task<ActionResult<Workout>> GetWorkout(int id) 
        {
            var workout = await _unitOfWork.Workout.Get(id);
            if (workout == null) { return NotFound($"Workout with ID {id} not found."); }
            return workout; }

        [HttpPost("AddWorkout")]
        public async Task<ActionResult<Workout>> AddWorkout([FromBody] Workout workout) 
        {
            await _unitOfWork.Workout.Add(workout);

            if (workout.Sets != null && workout.Sets.Any())
            {
                foreach (var set in workout.Sets)
                {
                    await _unitOfWork.PersonalRecords.AddOrUpdatePersonalRecord(workout.UserId, set.ExerciseId, set);
                }
            }
            await _unitOfWork.CompleteAsync();
            return Ok(workout);
        }

        [HttpPut("UpdateWorkout/{id}")]
        public async Task<ActionResult> UpdateWorkout(int id, [FromBody] Workout updatedWorkout)
        {
            if (id != updatedWorkout.Id)
            {
                return BadRequest("Workout ID in URL does not match ID in body.");
            }

            var existingWorkout = await _unitOfWork.Workout.Get(id);

            if (existingWorkout == null)
            {
                return NotFound($"Workout with ID {id} not found.");
            }

            existingWorkout.Date = updatedWorkout.Date;
            existingWorkout.Notes = updatedWorkout.Notes;

            var incomingSetIds = updatedWorkout.Sets.Select(s => s.Id).ToList();
            var setsToRemove = existingWorkout.Sets
                .Where(existingSet => !incomingSetIds.Contains(existingSet.Id))
                .ToList();

            foreach (var setToRemove in setsToRemove)
            {
                existingWorkout.Sets.Remove(setToRemove);
            }

            foreach (var incomingSet in updatedWorkout.Sets)
            {
                var existingSet = existingWorkout.Sets.FirstOrDefault(s => s.Id == incomingSet.Id);

                if (existingSet != null && existingSet.Id != 0)
                {
                    existingSet.Reps = incomingSet.Reps;
                    existingSet.Weight = incomingSet.Weight;
                    existingSet.ExerciseId = incomingSet.ExerciseId;
                }
                else
                {

                    existingWorkout.Sets.Add(new WorkoutSet
                    {
                        Reps = incomingSet.Reps,
                        Weight = incomingSet.Weight,
                        ExerciseId = incomingSet.ExerciseId
                    });
                }
                return Ok();

            }

            await _unitOfWork.CompleteAsync();

            foreach (var set in existingWorkout.Sets)
            {
                await _unitOfWork.PersonalRecords.AddOrUpdatePersonalRecord(existingWorkout.UserId, set.ExerciseId, set);
            }

            await _unitOfWork.CompleteAsync();

            return Ok("Workout was updated");
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

        
    }
}
