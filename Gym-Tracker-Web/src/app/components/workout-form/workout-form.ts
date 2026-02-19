import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, DatePipe, NgIf, NgFor } from '@angular/common'; // Для директив
import { FormsModule, NgForm } from '@angular/forms'; // Для форм на основе шаблонов
import { WorkoutService } from '../../Data/Services/workout.service';
import { Workout} from '../../Data/Services/Interfaces/workout.interface';
import { finalize, switchMap, of, catchError, Observable } from 'rxjs'; // Добавляем необходимые операторы
import { Exercise } from '../../Data/Services/Interfaces/exercise.interface';
import { Sets } from '../../Data/Services/Interfaces/sets.interface';


@Component({
  selector: 'app-workout-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './workout-form.html',
  styleUrl: './workout-form.scss'
})
export class WorkoutFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private workoutService = inject(WorkoutService);

  workout: Workout = this.initializeNewWorkout(); // Объект тренировки для формы
  isEditMode: boolean = false; // Флаг: режим редактирования или добавления
  isLoading: boolean = false; // Флаг для индикации загрузки/сохранения
  errorMessage: string | null = null;
  successMessage: string | null = null;

  availableExercises: Exercise[] = []; // Список доступных упражнений для <select>

  ngOnInit(): void {
    // Загрузка списка доступных упражнений
    this.workoutService.getExercises().subscribe({ // Предполагается, что у вас есть метод getExercises()
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
              this.router.navigate(['/history']); // Перенаправляем на историю, если ошибка
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
          // Важно: если дата приходит как строка, преобразуйте ее в 'YYYY-MM-DD' для type="date"
          this.workout = {
            ...workoutData,
            date: workoutData.date ? new Date(workoutData.date).toISOString().split('T')[0] : new Date().toISOString().split('T')[0]
          };
          // Убедитесь, что sets инициализированы, если их нет
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
       // Установите реальный userId, если это требуется вашим API
    };
  }

  // Добавление нового пустого подхода
  addSet(): void {
    if (!this.workout.sets) {
      this.workout.sets = [];
    }
    this.workout.sets.push({
      id: 0, // Временно 0 для новых подходов
      exerciseId: null, // Должен быть выбран пользователем
      reps: null,
      weight: null,
      workoutId: 0,
      workout: null,
      exercise: null
    } as any); // Используем as any временно, если типы не идеально совпадают с null
  }

  // Удаление подхода
  removeSet(setToRemove: Sets): void {
    if (this.workout.sets) {
      this.workout.sets = this.workout.sets.filter(set => set !== setToRemove);
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
            this.router.navigate(['/history']); // Например, на страницу истории
          }, 1500);
        } else {
            // Если API ничего не возвращает, но запрос успешен, можно считать успехом
             this.successMessage = `Workout ${this.isEditMode ? 'updated' : 'added'} successfully!`;
             setTimeout(() => {
                this.router.navigate(['/history']);
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