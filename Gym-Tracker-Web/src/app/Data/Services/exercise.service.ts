import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Exercise } from './Interfaces/exercise.interface';

@Injectable({
  providedIn: 'root'
})
export class ExerciseService {
http = inject(HttpClient)
baseUrl = 'https://localhost:7079/';

getExercises() {
  return this.http.get<Exercise[]>(this.baseUrl+'GetExercises')
}

getExercise(id: number) {
  return this.http.get<Exercise>(this.baseUrl+'GetExercise'+id)
}

}
