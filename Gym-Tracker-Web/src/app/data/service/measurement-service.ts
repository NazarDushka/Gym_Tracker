import { inject, Injectable } from '@angular/core';
import { MeasurementLog, MeasurementType } from './interface/body-measurements';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Target } from '@angular/compiler';

@Injectable({
  providedIn: 'root'
})
export class MeasurementService {
  http = inject(HttpClient)
  baseUrl = 'https://localhost:7079/GymTracker/measurements';

  getMeasurementTypes(): Observable<MeasurementType[]> {
    return this.http.get<MeasurementType[]>(`${this.baseUrl}/types`);
  }
 getLatestMeasurementLogs(): Observable<MeasurementLog[]> {
    return this.http.get<MeasurementLog[]>(`${this.baseUrl}/last`);
  }

 getAllMeasurementLogs(): Observable<MeasurementLog[]> {
    return this.http.get<MeasurementLog[]>(`${this.baseUrl}`);
  }
  
  addMeasurementLog(log: MeasurementLog): Observable<MeasurementLog> {
    return this.http.post<MeasurementLog>(`${this.baseUrl}`, log);
  }

  deleteMeasurementLog(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getUserTargets(): Observable<Target[]> {
    return this.http.get<Target[]>(`${this.baseUrl}/targets`);
  }

  addTarget(target: Target): Observable<Target> {
    return this.http.post<Target>(`${this.baseUrl}/targets`, target);
  }
  
  deleteTarget(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/targets/${id}`);
  }
}

