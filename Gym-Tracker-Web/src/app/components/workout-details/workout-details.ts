import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Workout } from '../../Data/Services/Interfaces/workout.interface';
import { ActivatedRoute } from '@angular/router';
import { WorkoutService } from '../../Data/Services/workout.service';
import { finalize } from 'rxjs/internal/operators/finalize';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-workout-details',
  imports: [DatePipe],
  templateUrl: './workout-details.html',
  styleUrl: './workout-details.scss'
})
export class WorkoutDetails {
  route = inject(ActivatedRoute); 
   workoutService = inject(WorkoutService);
  workout?: Workout;
errorMessage: any;
set: any;

// Инжектируем ChangeDetectorRef
  private changeDetectorRef = inject(ChangeDetectorRef);
  constructor() {
     this.workoutService.getWorkoutDetails(this.route.snapshot.params['id']).pipe(finalize(() => {}))
        .subscribe(val => this.workoutService.getWorkoutDetails(this.route.snapshot.params['id'])
            .subscribe((val: Workout | undefined) => {
       console.log("Workout data received by component:", val);
       this.workout = val;
       // Явно запускаем обнаружение изменений
       this.changeDetectorRef.detectChanges();
     }));
  }
}

