import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { ExerciseService } from '../../data/services/exercise.service';
import { ActivatedRoute } from '@angular/router';
import { Exercise } from '../../data/services/interfaces/exercise.interface';

@Component({
  selector: 'app-exercise-details',
  imports: [],
  templateUrl: './exercise-details.html',
  styleUrl: './exercise-details.scss'
})
export class ExerciseDetails {
  route = inject(ActivatedRoute)
  ExerciseService = inject(ExerciseService)
  exercise?: Exercise
  errorMessage: any
  private changeDetectorRef = inject(ChangeDetectorRef)
  constructor() {
    this.ExerciseService.getExercise(this.route.snapshot.params['id']).subscribe(val => {
      console.log("Exercises data received by component:", val);
      this.exercise = val;
      this.changeDetectorRef.detectChanges();
    });
  }
}
