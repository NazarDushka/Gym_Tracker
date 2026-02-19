import { ChangeDetectorRef, Component, inject } from '@angular/core'; 
import { Workout } from '../../Data/Services/Interfaces/workout.interface';
import { WorkoutService } from '../../Data/Services/workout.service';
import { CommonModule, DatePipe} from '@angular/common';
import {Router } from '@angular/router';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-workouts-history',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './workouts-history.html',
  styleUrl: './workouts-history.scss'
})
export class WorkoutsHistory  {
  router = inject(Router);
    WorkoutService = inject(WorkoutService);
  workouts?: Workout[]; 
errorMessage: any;
set: any;

// Инжектируем ChangeDetectorRef
  private changeDetectorRef = inject(ChangeDetectorRef);
  constructor() {
     this.WorkoutService.getWorkouts().subscribe(val => {
       console.log("Workout data received by component:", val);
       this.workouts = val;
       // Явно запускаем обнаружение изменений
       this.changeDetectorRef.detectChanges();
     });
  
 }

 navigateToWorkoutDetails(workoutId: number): void {
    // Навигация к маршруту /workouts/:id
    this.router.navigate(['/workout', workoutId]);
  }

  DeleteWorkout(workoutId: number): void {
  this.WorkoutService.deleteWorkout(workoutId).pipe(
    finalize(() => this.changeDetectorRef.detectChanges())
  ).subscribe(() => {
    // Удаление успешно — обновим список
    this.WorkoutService.getWorkouts().subscribe(val => {
      this.workouts = val;
      this.changeDetectorRef.detectChanges();
    });
  }, error => {
    this.errorMessage = 'Failed to delete workout';
  });
}

AddWorkout(): void {
    this.router.navigate(['/add-workout']); 
  }
EditWorkout(workoutId: number): void {
    this.router.navigate(['/edit-workout', workoutId]); 
  }

}