import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService, ToastService } from '@core/services';
import { InputComponent, ButtonComponent, IconComponent } from '@shared/components/ui';

// ============================================================================
// Login Page Component
// User authentication page
// ============================================================================

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    InputComponent,
    ButtonComponent,
    IconComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="login-page">
      <div class="login-header">
        <div class="mobile-logo">
          <div class="logo-icon">
            <app-icon name="package" [size]="24" />
          </div>
          <span class="logo-text">
            <span class="logo-primary">Techsys</span><span class="logo-secondary">Log</span>
          </span>
        </div>
        
        <h1 class="login-title">Bem-vindo de volta</h1>
        <p class="login-subtitle">Entre com suas credenciais para acessar sua conta</p>
      </div>
      
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="login-form">
        <app-input
          label="E-mail"
          type="email"
          placeholder="seu@email.com"
          formControlName="email"
          iconLeft="mail"
          autocomplete="email"
          [error]="getError('email')"
          [required]="true"
        />
        
        <app-input
          label="Senha"
          type="password"
          placeholder="••••••••"
          formControlName="password"
          iconLeft="lock"
          autocomplete="current-password"
          [error]="getError('password')"
          [required]="true"
        />
        
        <app-button
          type="submit"
          [fullWidth]="true"
          [loading]="authService.isLoading()"
          [disabled]="form.invalid"
          size="lg"
        >
          Entrar
        </app-button>
      </form>
      
      <div class="login-footer">
        <p>
          Não tem uma conta?
          <a routerLink="/auth/register">Criar conta</a>
        </p>
      </div>
      
      <!-- Demo credentials hint -->
      <div class="demo-hint">
        <app-icon name="info" [size]="16" />
        <span>Para testar, crie uma conta com qualquer e-mail válido</span>
      </div>
    </div>
  `,
  styles: [`
    .login-page {
      width: 100%;
      animation: fadeIn 0.4s ease-out;
    }
    
    .login-header {
      text-align: center;
      margin-bottom: var(--space-8);
    }
    
    .mobile-logo {
      display: none;
      align-items: center;
      justify-content: center;
      gap: var(--space-3);
      margin-bottom: var(--space-8);
    }
    
    .logo-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      background: linear-gradient(135deg, var(--color-primary-500) 0%, var(--color-primary-600) 100%);
      border-radius: var(--radius-lg);
      color: white;
    }
    
    .logo-text {
      font-family: var(--font-serif);
      font-size: var(--text-2xl);
      font-weight: var(--font-normal);
    }
    
    .logo-primary { color: var(--text-primary); }
    .logo-secondary { color: var(--color-primary-400); }
    
    .login-title {
      font-family: var(--font-serif);
      font-size: var(--text-3xl);
      font-weight: var(--font-normal);
      color: var(--text-primary);
      margin: 0 0 var(--space-2);
    }
    
    .login-subtitle {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0;
    }
    
    .login-form {
      display: flex;
      flex-direction: column;
      gap: var(--space-5);
    }
    
    .form-options {
      display: flex;
      align-items: center;
      justify-content: space-between;
      font-size: var(--text-sm);
    }
    
    .remember-me {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      color: var(--text-secondary);
      cursor: pointer;
      
      input {
        width: 16px;
        height: 16px;
        accent-color: var(--color-primary-500);
        cursor: pointer;
      }
      
      &:hover {
        color: var(--text-primary);
      }
    }
    
    .forgot-password {
      color: var(--color-primary-400);
      
      &:hover {
        text-decoration: underline;
      }
    }
    
    .login-footer {
      text-align: center;
      margin-top: var(--space-8);
      
      p {
        font-size: var(--text-sm);
        color: var(--text-secondary);
        margin: 0;
      }
      
      a {
        color: var(--color-primary-400);
        font-weight: var(--font-medium);
        
        &:hover {
          text-decoration: underline;
        }
      }
    }
    
    .demo-hint {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-2);
      margin-top: var(--space-6);
      padding: var(--space-3);
      background: rgba(99, 102, 241, 0.1);
      border-radius: var(--radius-lg);
      font-size: var(--text-sm);
      color: var(--color-primary-400);
    }
    
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    @media (max-width: 1024px) {
      .mobile-logo {
        display: flex;
      }
    }
  `],
})
export class LoginPageComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);

  readonly form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.value;

    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.toastService.success('Login realizado com sucesso!');
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        // Error is already handled by the service
      },
    });
  }

  getError(field: string): string {
    const control = this.form.get(field);
    if (!control?.touched || !control?.errors) return '';

    if (control.errors['required']) return 'Campo obrigatório';
    if (control.errors['email']) return 'E-mail inválido';
    if (control.errors['minlength']) return 'Mínimo de 6 caracteres';

    return '';
  }
}
