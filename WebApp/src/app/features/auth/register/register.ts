import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';

import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import { AuthService } from '../services/auth.service';

const USERNAME_PATTERN = /^[A-Za-z.']+$/;
const PASSWORD_DIGIT_PATTERN = /\d/;

const passwordsMatch: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password: unknown = control.get('password')?.value;
  const confirmation: unknown = control.get('confirmPassword')?.value;
  return password === confirmation ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitted = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly registerError = signal('');

  protected readonly registerForm = this.formBuilder.nonNullable.group({
    userName: ['', [
      Validators.required,
      Validators.maxLength(256),
      Validators.pattern(USERNAME_PATTERN)
    ]],
    password: ['', [
      Validators.required,
      Validators.minLength(6),
      Validators.maxLength(256),
      Validators.pattern(PASSWORD_DIGIT_PATTERN)
    ]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordsMatch });

  protected showError(controlName: 'userName' | 'password' | 'confirmPassword'): boolean {
    const control = this.registerForm.controls[controlName];
    return control.invalid && (control.touched || this.submitted());
  }

  protected showPasswordMismatch(): boolean {
    return this.registerForm.hasError('passwordMismatch')
      && (this.registerForm.controls.confirmPassword.touched || this.submitted());
  }

  protected submit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.submitted.set(true);
    this.registerError.set('');
    this.registerForm.markAllAsTouched();
    if (this.registerForm.invalid) {
      return;
    }

    const values = this.registerForm.getRawValue();
    const userName = values.userName.trim();
    this.isSubmitting.set(true);
    this.authService.register({
      userName,
      password: values.password
    }).pipe(
      take(1),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: () => void this.router.navigate(['/login'], {
        queryParams: { registered: true, username: userName }
      }),
      error: error => this.registerError.set(getApiErrorMessage(
        error,
        'Registration failed. Review your details and try again.'
      ))
    });
  }
}
