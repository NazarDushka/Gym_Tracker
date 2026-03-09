import { Component } from '@angular/core';
import { SideBar } from "../../side-bar/side-bar";
import { RouterOutlet } from '@angular/router';
import { LastWorkout } from "../last-workout/last-workout";
import { WorkoutsHistory } from './workouts-history/workouts-history';
import {ExercisesList} from "../exercises/exercises";

@Component({
  selector: 'app-main-layout',
  imports: [SideBar, RouterOutlet, LastWorkout,WorkoutsHistory, ExercisesList],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {

}
