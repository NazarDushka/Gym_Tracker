import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Observable, combineLatest, map } from 'rxjs';
import { MeasurementService } from '../../../data/services/measurement-service';
import { MeasurementLog, MeasurementType } from '../../../data/services/interfaces/body-measurements';

// Интерфейс для удобной отрисовки строки в таблице
interface DashboardItem {
  typeId: string;
  typeName: string;
  unit: string;
  latestValue: number | null;
  date: Date | string | null;
  hasData: boolean;
}

@Component({
  selector: 'app-my-measuraments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-measuraments.html',
  styleUrl: './my-measuraments.scss'
})
export class MyMeasuraments {
  router = inject(Router);
  measurementService = inject(MeasurementService);

  // Получаем оба потока (убедитесь, что метод getMeasurementTypes существует в сервисе)
  private types$: Observable<MeasurementType[]> = this.measurementService.getMeasurementTypes();
  private latestLogs$: Observable<MeasurementLog[]> = this.measurementService.getLatestMeasurementLogs();

  // Склеиваем данные
  dashboardItems$: Observable<DashboardItem[]> = combineLatest({
    types: this.types$,
    logs: this.latestLogs$
  }).pipe(
    map(({ types, logs }) => {
      return types.map(type => {
        const userLog = logs.find(log => log.measurementTypeId === type.id);
        
        
        return {
          typeId: type.id,        
          typeName: type.name,    
          unit: type.unit,       
          latestValue: userLog ? userLog.value : null, 
          date: userLog ? userLog.date : null,         
          hasData: !!userLog
        };
      });
    })
  );

  AddMeasurement() {
    this.router.navigate(['/add-measurement']);
  }

  // Кнопка добавления для конкретного типа (из строки таблицы)
  AddMeasurementForType(typeId: string) {
    // В идеале можно передать typeId в queryParams, чтобы на странице добавления 
    // этот тип выбрался в select автоматически.
    this.router.navigate(['/add-measurement'], { queryParams: { typeId: typeId } });
  }

  navigateToMeasurementDetails(typeId: string) {
    console.log('Navigating to history of type ID:', typeId);
    // this.router.navigate(['/measurement-details', typeId]);
  }
}