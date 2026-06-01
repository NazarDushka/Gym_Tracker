import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Exercise } from './interface/exercise.interface';
import { environment } from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class ExerciseService {
http = inject(HttpClient)
baseUrl = environment.apiUrl + '/GymTracker/Exercise/';

getExercises() {
  return this.http.get<Exercise[]>(this.baseUrl+'GetExercises')
}

getExercise(id: number) {
  return this.http.get<Exercise>(this.baseUrl+'GetExercise'+id)
}

}
