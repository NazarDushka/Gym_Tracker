import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { Router, RouterLink } from '@angular/router';
import { finalize, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from '../../../data/service/auth.service';
import { RegisterRequest } from '../../../data/service/interface/register-request';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule,RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  registerRequest: RegisterRequest  = {
    name: '',
    email: '',
    password: '',
    confirmPassword: ''
  };
  isLoading: boolean = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  private authService = inject(AuthService);
  private router = inject(Router);

  constructor() { }

  onRegisterSubmit(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    // Простая валидация пароля на стороне клиента
    if (this.registerRequest.password !== this.registerRequest.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      this.isLoading = false;
      return;
    }

    this.authService.register(this.registerRequest).pipe(
      finalize(() => this.isLoading = false),
      catchError(error => {
        this.errorMessage = error.message || 'Registration failed.';
        return of(null); 
      })
    ).subscribe(response => {
      if (response) {
        this.successMessage = 'Registration successful! You are now logged in.';
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 1500);
      }
    });
  }
}
