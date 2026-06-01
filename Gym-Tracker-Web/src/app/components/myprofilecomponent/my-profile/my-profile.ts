//Временная заглушка

import { Component, inject } from '@angular/core';
import { MeasurementService } from '../../../data/service/measurement-service';
import { UpperCasePipe, AsyncPipe, SlicePipe } from '@angular/common'; 
import { AuthService } from '../../../data/service/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [UpperCasePipe, AsyncPipe, SlicePipe],
  templateUrl: './my-profile.html',
  styleUrls: ['./my-profile.scss'] 
})
export class MyProfile {
 authService = inject(AuthService);
 UserInfo = this.authService.getUserINFOFromToken()|| { id: 0, name: "Unknown User", joinDate: "0000-00-00" };

 measurementService = inject(MeasurementService);
 measurements$ = this.measurementService.getLatestMeasurementLogs(); 

 router = inject(Router);

 
  recentRecords = []; // Здесь будет массив с последними рекордами пользователя (можно расширить модель данных для этого)

  // 4. Решение ошибки TS2339: Обработчики кликов
  viewAllMeasurements(): void {
    this.router.navigate(['/measurements']);
  }

  viewAllRecords(): void {
    console.log('Клик по кнопке: Показать все рекорды');
  }
}