import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { RegisterRequest } from './Interfaces/register-request';
import { AuthResponse } from './Interfaces/auth-response';
import { LoginRequest } from './Interfaces/login-request';
import { jwtDecode } from 'jwt-decode'; 

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  http = inject(HttpClient)
  baseUrl = 'https://localhost:7079/Auth/'
  router = inject(Router)

private _isAuthenticated = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this._isAuthenticated.asObservable(); // Публичный Observable

  constructor() { }

  private hasToken(): boolean {
    return !!localStorage.getItem('authToken');
  }

  // Метод для регистрации пользователя
  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.baseUrl + 'register', request).pipe(
      tap(response => {
        // Опционально: если вы хотите автоматически логинить после регистрации
        this.saveAuthData(response.token);
        this._isAuthenticated.next(true);
      }),
      catchError(this.handleError)
    );
  }

  // Метод для входа пользователя
  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.baseUrl + 'login?email='+request.email+'&passwd='+request.password, request).pipe(
      tap(response => {
        this.saveAuthData(response.token);
        this._isAuthenticated.next(true);
      }),
      catchError(this.handleError)
    );
  }

  // Метод для выхода пользователя
  logout(): void {
    localStorage.removeItem('authToken');
    // Очистите другие данные пользователя из localStorage/сессии, если храните
    this._isAuthenticated.next(false);
    this.router.navigate(['/login']); // Перенаправляем на страницу входа
  }

  // Получить токен
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // Сохранить данные аутентификации (токен)
  private saveAuthData(token: string): void {
    localStorage.setItem('token', token);
  }

// Новый метод для извлечения ID пользователя
  getUserIdFromToken(): number | null {
    const token = this.getToken();
    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        // Предполагаем, что ID пользователя хранится в поле 'nameid' или 'sub'
        // Проверьте структуру вашего токена

        const userId = decodedToken.Id || decodedToken.sub; 
        return userId ? +userId : null; // Преобразуем в число
      } catch (error) {
        console.error('Error decoding token:', error);
        return null;
      }
    }
    return null;
  }

  // Обработка ошибок
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      // Ошибка на стороне клиента или сети
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Ошибка на стороне сервера
      if (error.status === 400 && error.error) {
        // Если API возвращает конкретные ошибки валидации
        errorMessage = Array.isArray(error.error) ? error.error.join(', ') : error.error.message || JSON.stringify(error.error);
      } else if (error.status === 401) {
        errorMessage = 'Unauthorized: Invalid credentials.';
      } else if (error.status === 403) {
        errorMessage = 'Forbidden: You do not have permission to access this resource.';
      } else if (error.status === 404) {
        errorMessage = 'Not Found: The requested resource could not be found.';
      }
      else {
        errorMessage = `Server returned code: ${error.status}, error message: ${error.message || JSON.stringify(error.error)}`;
      }
    }
    console.error('Auth Service Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}


