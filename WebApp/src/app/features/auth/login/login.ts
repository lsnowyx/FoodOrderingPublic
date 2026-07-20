import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';

import { getApiErrorMessage } from '../../checkout/services/api-error-message';
import { AuthService } from '../services/auth.service';
import { AuthStore } from '../services/auth-store.service';

function getSafeReturnUrl(value: string | null): string {
  return value !== null && value.startsWith('/') && !value.startsWith('//')
    ? value
    : '/categories';
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly authStore = inject(AuthStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly submitted = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly loginError = signal('');
  protected readonly registrationComplete = this.route.snapshot.queryParamMap.get('registered') === 'true';
  private readonly returnUrl = getSafeReturnUrl(
    this.route.snapshot.queryParamMap.get('returnUrl')
  );

  protected readonly loginForm = this.formBuilder.nonNullable.group({
    username: [this.route.snapshot.queryParamMap.get('username') ?? '', [
      Validators.required,
      Validators.maxLength(256)
    ]],
    password: ['', [
      Validators.required,
      Validators.maxLength(256)
    ]]
  });

  protected showError(controlName: 'username' | 'password'): boolean {
    const control = this.loginForm.controls[controlName];
    return control.invalid && (control.touched || this.submitted());
  }

  protected submit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.submitted.set(true);
    this.loginError.set('');
    this.loginForm.markAllAsTouched();
    if (this.loginForm.invalid) {
      return;
    }

    const values = this.loginForm.getRawValue();
    this.isSubmitting.set(true);
    this.authService.login({
      username: values.username.trim(),
      password: values.password
    }).pipe(
      take(1),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: response => {
        if (!this.authStore.setSession(response)) {
          this.loginError.set('The server returned an invalid or expired session.');
          return;
        }

        void this.router.navigateByUrl(this.returnUrl);
      },
      error: error => this.loginError.set(getApiErrorMessage(
        error,
        'Login failed. Check your username and password and try again.'
      ))
    });
  }
}
