import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Workout } from './interface/workout.interface';
import { Exercise } from './interface/exercise.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WorkoutService {
  http = inject(HttpClient)
baseUrl = environment.apiUrl + '/GymTracker/Workout/'

  getWorkouts() {
    return this.http.get<Workout[]>(this.baseUrl+'GetAllWorkouts')
  }
  getUsersWorkouts(myId: number) {
    return this.http.get<Workout[]>(`${this.baseUrl}GetMyWorkouts`);
  }
  getLastWorkout(myId: number) {
    return this.http.get<Workout>(`${this.baseUrl}LastWorkout`);
  }

  getWorkoutDetails(id: number) {
    return this.http.get<Workout>(this.baseUrl+'GetWorkout/'+id)
  }
  deleteWorkout(id: number) {
    return this.http.delete<Workout>(this.baseUrl+'DeleteWorkout/'+id)
  }
  
  updateWorkout(workout: Workout) {
    return this.http.put<Workout>(this.baseUrl+'UpdateWorkout/'+workout.id,workout)
  }

  addWorkout(workout: Workout) {
    return this.http.post<Workout>(this.baseUrl+'AddWorkout',workout)
  }

}
