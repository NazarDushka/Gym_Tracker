using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net.Http.Headers;
using GymTracker.Extensions;
using GymTracker.Services;
using GymTracker.DTOs.Workout;

namespace GymTracker.Controllers
{
    [ApiController]
    [Route("GymTracker/[controller]")]
    [Authorize]
    public class WorkoutController : ControllerBase
    {
        public readonly IWorkoutService _workoutService;

        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        [HttpGet("GetMyWorkouts")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetMyWorkouts()
        {
            var workouts = await _workoutService.GetUsersWorkoutsAsync(User.GetUserId());
            return Ok(workouts);
        }

        [HttpGet("GetAllWorkouts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetAllWorkouts()
        {
            var workouts = await _workoutService.GetUsersWorkoutsAsync(User.GetUserId());
            if (workouts == null || !workouts.Any())
            {
                return NotFound("No workouts found.");
            }
            return Ok(workouts);
        }

        [HttpGet("GetWorkout/{id}")]
        public async Task<ActionResult<Workout>> GetWorkout(int id)
        {
           var workout = await _workoutService.GetWorkoutByIdAsync(User.GetUserId(), id);
           if (workout == null)
           {
               return NotFound($"Workout with ID {id} not found.");
            }
            return Ok(workout);
        }

        [HttpPost("AddWorkout")]
        public async Task<ActionResult<Workout>> AddWorkout([FromBody]CreateWorkoutRequest request)
        {
            var createdWorkout = await _workoutService.CreateWorkoutAsync(User.GetUserId(), request);
            return CreatedAtAction(nameof(GetWorkout), new { id = createdWorkout.Id }, createdWorkout);
        }

        [HttpPut("UpdateWorkout/{id}")]
        public async Task<ActionResult> UpdateWorkout(int id, [FromBody] UpdateWorkoutRequest request)
        {
            try
            { 
                await _workoutService.UpdateWorkoutAsync(User.GetUserId(), id, request);
                return Ok("Workout was updated");
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Workout with ID {id} not found.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You are not authorized to update this workout.");
            }
        }

        [HttpDelete("DeleteWorkout/{id}")]
        public async Task<ActionResult> DeleteWorkout(int id)
        {
            try
            { 
                await _workoutService.DeleteWorkoutAsync(User.GetUserId(), id);
                return Ok("Workout was deleted");
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Workout with ID {id} not found.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You are not authorized to delete this workout.");
            }
        }

        [HttpGet("LastWorkout")]
        public async Task<ActionResult<Workout>> GetLastWorkout()
        {
            var workout = await _workoutService.GetLastUsersWorkoutAsync(User.GetUserId());
            if (workout == null)
                return NotFound();
            return Ok(workout);
        }
    }
}
