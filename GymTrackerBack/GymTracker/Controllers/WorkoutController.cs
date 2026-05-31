using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace GymTracker.Controllers
{
    [ApiController]
    [Route("GymTracker/[controller]")]
    [Authorize]
    public class WorkoutController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkoutController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private int GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("UserId claim is missing or invalid.");
            }
            return userId;
        }

        [HttpGet("GetMyWorkouts")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetMyWorkouts()
        {
            int userId = GetUserId();
            var workouts = await _unitOfWork.Workout.GetMyWorkouts(userId);
            if (workouts == null || !workouts.Any()) return NotFound("No workouts found for this user.");
            workouts = workouts.OrderByDescending(w => w.Date).ToList();
            return Ok(workouts);
        }

        [HttpGet("GetAllWorkouts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetAllWorkouts()
        {
            var workouts = await _unitOfWork.Workout.GetAll();
            workouts = workouts.OrderByDescending(w => w.Date).ToList();
            return Ok(workouts);
        }

        [HttpGet("GetWorkout/{id}")]
        public async Task<ActionResult<Workout>> GetWorkout(int id)
        {
            int userId = GetUserId();
            var workout = await _unitOfWork.Workout.Get(id);
            if (workout == null) 
            { 
                return NotFound($"Workout with ID {id} not found."); 
            }
            if (userId != workout.UserId)
            {
                return Forbid("You are not authorized to view this workout.");
            }
            return Ok(workout);
        }

        [HttpPost("AddWorkout")]
        public async Task<ActionResult<Workout>> AddWorkout([FromBody] Workout workout)
        {
            int userId = GetUserId();
            if (userId != workout.UserId)
            {
                return Forbid("You are not authorized to add this workout.");
            }

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
            int userId = GetUserId();
            if (userId != updatedWorkout.UserId)
            {
                return Forbid("You are not authorized to update this workout.");
            }
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
            }

            await _unitOfWork.CompleteAsync();

            foreach (var set in existingWorkout.Sets)
            {
                await _unitOfWork.PersonalRecords.AddOrUpdatePersonalRecord(existingWorkout.UserId, set.ExerciseId, set);
            }

            await _unitOfWork.CompleteAsync();

            return Ok("Workout was updated");
        }

        [HttpDelete("DeleteWorkout/{id}")]
        public async Task<ActionResult> DeleteWorkout(int id)
        {
            int userId = GetUserId();
            var workoutToDelete = await _unitOfWork.Workout.Get(id);
            if (workoutToDelete == null)
            {
                return NotFound($"Workout with ID {id} not found.");
            }
            if (userId != workoutToDelete.UserId)
            {
                return Forbid("You are not authorized to delete this workout.");
            }
            
            _unitOfWork.Workout.Delete(workoutToDelete);
            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpGet("LastWorkout")]
        public async Task<ActionResult<Workout>> GetLastWorkout()
        {
            int userId = GetUserId();
            var lastWorkout = await _unitOfWork.Workout.GetMyLastWorkout(userId);
            if (lastWorkout == null) 
                return NotFound();
            return Ok(lastWorkout);
        }
    }
}
