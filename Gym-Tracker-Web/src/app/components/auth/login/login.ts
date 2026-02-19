import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { Router } from '@angular/router';
import { finalize, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { LoginRequest } from '../../../Data/Services/Interfaces/login-request';
import { AuthService } from '../../../Data/Services/auth.service';
import { RouterLink } from '@angular/router';


@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule,RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  loginRequest: LoginRequest = {
    email: '', // Или username, в зависимости от вашей логики
    password: ''
  };
  isLoading: boolean = false;
  errorMessage: string | null = null;

  private authService = inject(AuthService);
  private router = inject(Router);

  constructor() { }

  onLoginSubmit(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.authService.login(this.loginRequest).pipe(
      finalize(() => this.isLoading = false),
      catchError(error => {
        this.errorMessage = error.message || 'Login failed.';
        return of(null); // Возвращаем Observable, чтобы поток завершился
      })
    ).subscribe(response => {
      if (response) {
        // Логин успешен, перенаправляем пользователя
        this.router.navigate(['/last-workout']); // Или на любую другую страницу
      }
    });
  }
}
