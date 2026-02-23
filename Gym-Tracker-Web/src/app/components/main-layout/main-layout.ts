import { Component } from '@angular/core';
import { SideBar } from "../../side-bar/side-bar";
import { RouterOutlet } from '@angular/router';
import { LastWorkout } from "../last-workout/last-workout";
import { WorkoutsHistory } from './workouts-history/workouts-history';

@Component({
  selector: 'app-main-layout',
  imports: [SideBar, RouterOutlet, LastWorkout,WorkoutsHistory],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {

}
