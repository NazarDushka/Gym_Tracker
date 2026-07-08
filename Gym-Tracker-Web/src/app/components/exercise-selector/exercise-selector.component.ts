import { Component, Inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MAT_BOTTOM_SHEET_DATA, MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { map, startWith, debounceTime } from 'rxjs/operators';
import { Exercise } from '../../data/service/interface/exercise.interface';

@Component({
  selector: 'app-exercise-selector',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ScrollingModule],
  templateUrl: './exercise-selector.component.html',
  styleUrls: ['./exercise-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExerciseSelectorComponent implements OnInit {
  searchControl = new FormControl('');
  
  // Available filters based on muscle groups
  filters = ['All', 'Chest', 'Back', 'Legs', 'Shoulders', 'Arms', 'Core'];
  selectedFilter$ = new BehaviorSubject<string>('All');
  
  filteredExercises$!: Observable<Exercise[]>;

  constructor(
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: { exercises: Exercise[] },
    private bottomSheetRef: MatBottomSheetRef<ExerciseSelectorComponent>
  ) {}

  ngOnInit(): void {
    const search$ = this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      map(value => (value || '').toLowerCase())
    );

    this.filteredExercises$ = combineLatest([
      search$,
      this.selectedFilter$
    ]).pipe(
      map(([searchTerm, filter]) => {
        let results = this.data.exercises;

        /*
          // Smart Sorting Placeholder:
          // Here we would sort the exercises based on user's history
          // so that frequently used exercises appear at the top.
          results = sortExercisesByFrequency(results, userHistory);
        */

        if (filter !== 'All') {
          results = results.filter(e => 
            e.muscleGroup?.toLowerCase().includes(filter.toLowerCase())
          );
        }

        if (searchTerm) {
          results = results.filter(e => 
            e.name.toLowerCase().includes(searchTerm) || 
            e.muscleGroup?.toLowerCase().includes(searchTerm)
          );
        }

        return results;
      })
    );
  }

  setFilter(filter: string): void {
    this.selectedFilter$.next(filter);
  }

  selectExercise(exercise: Exercise): void {
    this.bottomSheetRef.dismiss(exercise);
  }
}
