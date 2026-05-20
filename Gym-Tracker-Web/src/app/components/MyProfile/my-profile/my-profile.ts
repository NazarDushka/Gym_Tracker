//Временная заглушка

import { Component, inject } from '@angular/core';
import { MeasurementService } from '../../../Data/Services/measurement-service';
import { UpperCasePipe, DatePipe, AsyncPipe, SlicePipe } from '@angular/common'; 
import { AuthService } from '../../../Data/Services/auth.service';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [UpperCasePipe, DatePipe, AsyncPipe, SlicePipe],
  templateUrl: './my-profile.html',
  styleUrls: ['./my-profile.scss'] 
})
export class MyProfile {
 authService = inject(AuthService);
 UserInfo = this.authService.getUserINFOFromToken()|| { id: 0, name: "Unknown User", joinDate: "0000-00-00" };

 measurementService = inject(MeasurementService);
 measurements$ = this.measurementService.getLatestMeasurementLogs(this.UserInfo.id || 0); 

 
  // 3. Решение ошибки TS2339: Массив для @for
  recentRecords = [
    { id: 1, exerciseName: 'Жим лежа', weight: 100, reps: 5, date: '2026-03-08' },
    { id: 2, exerciseName: 'Присед', weight: 140, reps: 5, date: '2026-03-09' }
  ];

  // 4. Решение ошибки TS2339: Обработчики кликов
  viewAllMeasurements(): void {
    console.log('Клик по кнопке: Показать все замеры');
    // Здесь позже будет логика навигации, например: this.router.navigate(['/measurements']);
  }

  viewAllRecords(): void {
    console.log('Клик по кнопке: Показать все рекорды');
  }
}