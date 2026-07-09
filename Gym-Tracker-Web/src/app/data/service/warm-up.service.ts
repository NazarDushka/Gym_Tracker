import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WarmUpService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/GymTracker/Health/WakeUp';
  private _hasWarmedUp = false;

  /**
   * Sends a lightweight GET request to wake up the backend server and database.
   * Called on landing page load so the DB is ready by the time the user logs in.
   * Silently ignores errors — this is a best-effort optimization.
   */
  warmUp(): void {
    if (this._hasWarmedUp) return; // Only warm up once per session
    this._hasWarmedUp = true;

    this.http.get(this.apiUrl, { responseType: 'text' }).pipe(
      catchError(() => of(null))
    ).subscribe();
  }
}
