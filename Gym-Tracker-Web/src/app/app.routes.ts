import { Routes } from '@angular/router';
import { LastWorkout } from './components/last-workout/last-workout';
import { RegisterComponent } from './components/auth/register/register';
import { LoginComponent } from './components/auth/login/login';
import { MainLayout } from './components/main-layout/main-layout';
import { WorkoutsHistory } from './components/main-layout/workouts-history/workouts-history';
import { WorkoutDetails } from './components/main-layout/workout-details/workout-details';
import { WorkoutFormComponent } from './components/main-layout/workout-form/workout-form';
import { ExercisesList } from './components/exercises/exercises';
import { ExerciseDetails } from './components/exercise-details/exercise-details';
import { MyProfile } from './components/myprofile/my-profile/my-profile';
import { MyMeasuraments } from './components/myprofile/my-measuraments/my-measuraments';
import { AddMeasurement } from './components/myprofile/my-measuraments/add-measurement/add-measurement';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: '', component: MainLayout, children: [
        { path: '', component: LastWorkout },
        { path: 'workouts-history', component: WorkoutsHistory },
        { path : 'workout/:id', component: WorkoutDetails },
        { path : 'edit-workout/:id', component: WorkoutFormComponent },
        { path : 'add-workout', component: WorkoutFormComponent },
        { path : 'exercises', component: ExercisesList },
        { path : 'exerciseDetails/:id', component: ExerciseDetails },
        { path : 'my-profile', component: MyProfile},

        { path: 'measurements', component: MyMeasuraments},
        { path : 'add-measurement', component: AddMeasurement },
        ]
    },
   
];
