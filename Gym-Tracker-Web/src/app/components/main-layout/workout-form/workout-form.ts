import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, DatePipe, NgIf, NgFor } from '@angular/common'; // Для директив
import { FormsModule, NgForm } from '@angular/forms'; // Для форм на основе шаблонов
import { MatBottomSheetModule, MatBottomSheet } from '@angular/material/bottom-sheet';
import { WorkoutService } from '../../../data/service/workout.service';
import { Workout} from '../../../data/service/interface/workout.interface';
import { finalize, switchMap, of, catchError, Observable } from 'rxjs'; // Добавляем необходимые операторы
import { Exercise } from '../../../data/service/interface/exercise.interface';
import { Sets } from '../../../data/service/interface/sets.interface';
import { Token } from '@angular/compiler';
import { AuthService } from '../../../data/service/auth.service';
import { ExerciseService } from '../../../data/service/exercise.service';
import { ExerciseSelectorComponent } from '../../exercise-selector/exercise-selector.component';

@Component({
  selector: 'app-workout-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatBottomSheetModule
  ],
  templateUrl: './workout-form.html',
  styleUrl: './workout-form.scss'
})
export class WorkoutFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private workoutService = inject(WorkoutService);
  private exerciseService = inject(ExerciseService);
  private bottomSheet = inject(MatBottomSheet);

  private authService = inject(AuthService);

  MyId: number = this.authService.getUserINFOFromToken().userId ?? 0;

  workout: Workout = this.initializeNewWorkout(); 

  isEditMode: boolean = false; 
  isLoading: boolean = false; 
  errorMessage: string | null = null;
  successMessage: string | null = null;
  availableExercises: Exercise[] = [];

  ngOnInit(): void {
    // Загрузка списка доступных упражнений
    this.exerciseService.getExercises().subscribe({ // Предполагается, что у вас есть метод getExercises()
      next: (exercises: Exercise[]) => {
        this.availableExercises = exercises;
      },
      error: (err: any) => {
        console.error('Error loading exercises:', err);
        this.errorMessage = 'Failed to load exercises for selection.';
      }
    });

    // Проверяем параметры маршрута для определения режима (редактирование или добавление)
    this.route.paramMap.pipe(
      switchMap(params => {
        const workoutId = params.get('id');
        if (workoutId) {
          this.isEditMode = true;
          this.isLoading = true;
          return this.workoutService.getWorkoutDetails(Number(workoutId)).pipe(
            finalize(() => this.isLoading = false),
            catchError(err => {
              this.errorMessage = 'Failed to load workout for editing.';
              console.error('Error loading workout:', err);
              this.router.navigate(['/workouts-history']); // Перенаправляем на историю, если ошибка
              return of(null);
            })
          );
        } else {
          this.isEditMode = false;
          this.workout = this.initializeNewWorkout(); // Создаем пустую тренировку
          return of(null); // Возвращаем пустой Observable, так как загружать нечего
        }
      })
    ).subscribe({
      next: (workoutData: Workout | null) => {
        if (workoutData) {
          
          this.workout = {
            ...workoutData,
            date: workoutData.date ? new Date(workoutData.date).toISOString().split('T')[0] : new Date().toISOString().split('T')[0]
          };
         
          if (!this.workout.sets) {
            this.workout.sets = [];
          }
        }
      }
    });
  }

  // Инициализация новой тренировки с текущей датой
  initializeNewWorkout(): Workout {
    return {
      id: 0, // 0 для новых, сервер присвоит реальный ID
      date: new Date().toISOString().split('T')[0], // Формат YYYY-MM-DD для input type="date"
      notes: '',
      sets: [],
      userId: this.MyId,
    };
  }

  // Добавление нового пустого подхода
  addSet(): void {
    if (!this.workout.sets) {
      this.workout.sets = [];
    }
    this.workout.sets.push({
      id: 0, 
      exerciseId: null, 
      reps: null,
      weight: null,
      workoutId: 0,
      workout: null,
      exercise: null
    } as any); 
  }

  // Удаление подхода
  removeSet(setToRemove: Sets): void {
    if (this.workout.sets) {
      this.workout.sets = this.workout.sets.filter(set => set !== setToRemove);
    }
  }

  // Открытие Bottom Sheet для выбора упражнения
  openExerciseSelector(setIndex: number): void {
    const bottomSheetRef = this.bottomSheet.open(ExerciseSelectorComponent, {
      data: { exercises: this.availableExercises }
    });

    bottomSheetRef.afterDismissed().subscribe((selectedExercise: Exercise | undefined) => {
      if (selectedExercise && this.workout.sets) {
        this.workout.sets[setIndex].exerciseId = selectedExercise.id;
        // Optionally update local object for immediate display if needed, but we rely on getExerciseName
      }
    });
  }

  // Получение имени упражнения по ID
  getExerciseName(exerciseId: number | null): string {
    if (!exerciseId) return 'Select Exercise';
    const exercise = this.availableExercises.find(e => e.id === exerciseId);
    return exercise ? exercise.name : 'Unknown Exercise';
  }

  // Степперы: Уменьшить значение
  decrementValue(set: any, field: string, min: number, step: number = 1): void {
    if (set[field] == null || set[field] === '') {
      set[field] = min;
    } else {
      const current = parseFloat(set[field]);
      if (current > min) {
        set[field] = parseFloat((current - step).toFixed(1));
      }
    }
  }

  // Степперы: Увеличить значение
  incrementValue(set: any, field: string, step: number = 1): void {
    if (set[field] == null || set[field] === '') {
      set[field] = step;
    } else {
      const current = parseFloat(set[field]);
      set[field] = parseFloat((current + step).toFixed(1));
    }
  }

  // Обработка отправки формы
  onSubmit(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    let operation$: Observable<any>;

    if (this.isEditMode) {
      operation$ = this.workoutService.updateWorkout(this.workout); // Предполагается метод updateWorkout
    } else {
      operation$ = this.workoutService.addWorkout(this.workout); // Предполагается метод addWorkout
    }

    operation$.pipe(
      finalize(() => this.isLoading = false),
      catchError(err => {
        this.errorMessage = `Failed to ${this.isEditMode ? 'save changes' : 'add workout'}.`;
        console.error('Error saving workout:', err);
        return of(null);
      })
    ).subscribe({
      next: (response) => {
        if (response) { // Убедитесь, что ваш API возвращает что-то при успехе
          this.successMessage = `Workout ${this.isEditMode ? 'updated' : 'added'} successfully!`;
          // Опционально: перенаправить пользователя после успешного сохранения
          setTimeout(() => {
            this.router.navigate(['/workouts-history']); // Например, на страницу истории
          }, 1500);
        } else {
            // Если API ничего не возвращает, но запрос успешен, можно считать успехом
             this.successMessage = `Workout ${this.isEditMode ? 'updated' : 'added'} successfully!`;
             setTimeout(() => {
                this.router.navigate(['/workouts-history']);
            }, 1500);
        }
      }
    });
  }

  // Обработка отмены
  onCancel(): void {
    this.router.navigate(['/workouts-history']); // Возвращаемся на страницу истории
  }
}

