import { Routes } from '@angular/router';
import { LastWorkout } from './components/last-workout/last-workout';
import { RegisterComponent } from './components/auth/register/register';
import { LoginComponent } from './components/auth/login/login';
import { MainLayout } from './components/main-layout/main-layout';
import { WorkoutsHistory } from './components/main-layout/workouts-history/workouts-history';
import { WorkoutDetails } from './components/main-layout/workout-details/workout-details';
import { WorkoutFormComponent } from './components/main-layout/workout-form/workout-form';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: '', component: MainLayout, children: [
        { path: '', component: LastWorkout },
        { path: 'workouts-history', component: WorkoutsHistory },
        { path : 'workout/:id', component: WorkoutDetails },
        { path : 'edit-workout/:id', component: WorkoutFormComponent },
        { path : 'add-workout', component: WorkoutFormComponent },
    ] },
   
];
