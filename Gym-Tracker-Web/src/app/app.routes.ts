import { Routes } from '@angular/router';
import { LastWorkout } from './components/last-workout/last-workout';
import { RegisterComponent } from './components/auth/register/register';
import { LoginComponent } from './components/auth/login/login';

export const routes: Routes = [

    { path: '', component: LoginComponent },
    { path: 'login', component: LoginComponent },
    { path: 'last-workout', component: LastWorkout },
    { path: 'register', component: RegisterComponent },
];
