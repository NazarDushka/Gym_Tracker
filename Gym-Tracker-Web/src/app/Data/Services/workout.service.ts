import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Workout } from './Interfaces/workout.interface';
import { Exercise } from './Interfaces/exercise.interface';

@Injectable({
  providedIn: 'root'
})
export class WorkoutService {
  http = inject(HttpClient)
baseUrl='https://localhost:7079/GymTracker/Workout/'

  getWorkouts() {
    return this.http.get<Workout[]>(this.baseUrl+'GetAllWorkouts')
  }
  getLastWorkout(myId: number) {
    return this.http.get<Workout>(`${this.baseUrl}LastWorkout?userId=${myId}`);
  }

  getWorkoutDetails(id: number) {
    return this.http.get<Workout>(this.baseUrl+'GetWorkout'+id)
  }
  deleteWorkout(id: number) {
    return this.http.delete<Workout>(this.baseUrl+'DeleteWorkout'+id)
  }
  
  updateWorkout(workout: Workout) {
    return this.http.put<Workout>(this.baseUrl+'UpdateWorkout'+workout.id,workout)
  }

  addWorkout(workout: Workout) {
    return this.http.post<Workout>(this.baseUrl+'AddWorkout',workout)
  }

  getExercises() {
    return this.http.get<Exercise[]>(this.baseUrl+'GetExercises')
  }
}
