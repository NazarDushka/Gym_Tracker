import { Component, inject ,ChangeDetectorRef } from '@angular/core';
import { WorkoutService } from '../../Data/Services/workout.service';
import { Workout } from '../../Data/Services/Interfaces/workout.interface';
import { CommonModule,DatePipe} from '@angular/common';
import { AuthService } from '../../Data/Services/auth.service';



@Component({
  selector: 'app-last-workout',
  imports: [DatePipe,CommonModule],
  templateUrl: './last-workout.html',
  styleUrl: './last-workout.scss'
})
export class LastWorkout {
  WorkoutService = inject(WorkoutService);
   authService = inject(AuthService);
  MyId = this.authService.getUserIdFromToken();
  workout?: Workout; 
errorMessage: any;
set: any;

// Инжектируем ChangeDetectorRef
  private changeDetectorRef = inject(ChangeDetectorRef);
  constructor() {
     this.WorkoutService.getLastWorkout(this.MyId!).subscribe(val => {
       console.log("Workout data received by component:", val);
       this.workout = val;
       // Явно запускаем обнаружение изменений
       this.changeDetectorRef.detectChanges();
     });
  }
}
