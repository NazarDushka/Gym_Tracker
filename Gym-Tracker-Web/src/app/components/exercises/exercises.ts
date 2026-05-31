import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { ExerciseService } from '../../data/services/exercise.service';
import { Exercise } from '../../data/services/interfaces/exercise.interface';
import { Router } from '@angular/router';

@Component({
  selector: 'app-exercises-list',
  imports: [],
  templateUrl: './exercises.html',
  styleUrl: './exercises.scss'
})
export class ExercisesList {
  router = inject(Router)
  exerciseService = inject(ExerciseService)
  exercises?: Exercise[]
  errorMessage: any;
  


private changeDetectorRef = inject(ChangeDetectorRef);


constructor() {
  this.exerciseService.getExercises().subscribe(val => {
    console.log("Exercises data received by component:", val);
    this.exercises = val;
    this.changeDetectorRef.detectChanges();
  });
}

navigateToExerciseDetails(exerciseId: number): void {
  this.router.navigate(['/exerciseDetails', exerciseId]);
  }

}