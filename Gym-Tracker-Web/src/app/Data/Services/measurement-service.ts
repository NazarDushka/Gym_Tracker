import { inject, Injectable } from '@angular/core';
import { MeasurementLog } from './Interfaces/body-measurements';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MeasurementService {
  http = inject(HttpClient)
 
  getLatestMeasurementLogs(userId?: number): Observable<MeasurementLog[]> {
  return this.http.get<MeasurementLog[]>(`https://localhost:7079/api/users/${userId}/measurements`)
    .pipe(
      map((logs: MeasurementLog[]) => {
        if (!logs || logs.length === 0) return [];

        // 1. Группируем логи по MeasurementTypeId, оставляя только самые новые
        const latestLogsMap = new Map<number, MeasurementLog>();

        logs.forEach(log => {
          const currentLogTime = new Date(log.Date).getTime();
          const existingLog = latestLogsMap.get(log.MeasurementTypeId);

          if (!existingLog) {
            latestLogsMap.set(log.MeasurementTypeId, log);
          } else {
            const existingLogTime = new Date(existingLog.Date).getTime();
            if (currentLogTime > existingLogTime) {
              latestLogsMap.set(log.MeasurementTypeId, log);
            }
          }
        });

        const sortedUniqueLogs = Array.from(latestLogsMap.values())
          .sort((a, b) => a.MeasurementTypeId - b.MeasurementTypeId);

        return sortedUniqueLogs;
      })
    );
}
}
