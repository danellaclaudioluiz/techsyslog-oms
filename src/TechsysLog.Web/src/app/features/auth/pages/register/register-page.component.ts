import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService, ToastService } from '@core/services';
import { UserRole } from '@core/models';
import { InputComponent, ButtonComponent, IconComponent } from '@shared/components/ui';

// ============================================================================
// Register Page Component
// User registration page
// ============================================================================

@Component({
  selector: 'app-register-page',
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
    <div class="register-page">
      <div class="register-header">
        <div class="mobile-logo">
          <div class="logo-icon">
            <app-icon name="package" [size]="24" />
          </div>
          <span class="logo-text">
            <span class="logo-primary">Techsys</span><span class="logo-secondary">Log</span>
          </span>
        </div>
        
        <h1 class="register-title">Criar conta</h1>
        <p class="register-subtitle">Preencha os dados abaixo para criar sua conta</p>
      </div>
      
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="register-form">
        <app-input
          label="Nome completo"
          type="text"
          placeholder="Seu nome"
          formControlName="name"
          iconLeft="user"
          autocomplete="name"
          [error]="getError('name')"
          [required]="true"
        />
        
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
        
        <div class="form-row">
          <app-input
            label="Senha"
            type="password"
            placeholder="••••••••"
            formControlName="password"
            iconLeft="lock"
            autocomplete="new-password"
            [error]="getError('password')"
            [required]="true"
          />
          
          <app-input
            label="Confirmar senha"
            type="password"
            placeholder="••••••••"
            formControlName="confirmPassword"
            iconLeft="lock"
            autocomplete="new-password"
            [error]="getError('confirmPassword')"
            [required]="true"
          />
        </div>
        
        <div class="form-group">
          <label class="form-label">
            Tipo de conta <span class="required">*</span>
          </label>
          <div class="role-options">
            @for (role of roles; track role.value) {
              <label 
                class="role-option" 
                [class.selected]="form.get('role')?.value === role.value"
              >
                <input 
                  type="radio" 
                  formControlName="role" 
                  [value]="role.value"
                />
                <div class="role-content">
                  <app-icon [name]="role.icon" [size]="20" />
                  <div class="role-info">
                    <span class="role-label">{{ role.label }}</span>
                    <span class="role-description">{{ role.description }}</span>
                  </div>
                </div>
              </label>
            }
          </div>
        </div>
        
        <div class="password-requirements">
          <p class="requirements-title">A senha deve conter:</p>
          <ul class="requirements-list">
            <li [class.valid]="hasMinLength">
              <app-icon [name]="hasMinLength ? 'check' : 'x'" [size]="14" />
              Mínimo de 8 caracteres
            </li>
            <li [class.valid]="hasUppercase">
              <app-icon [name]="hasUppercase ? 'check' : 'x'" [size]="14" />
              Uma letra maiúscula
            </li>
            <li [class.valid]="hasLowercase">
              <app-icon [name]="hasLowercase ? 'check' : 'x'" [size]="14" />
              Uma letra minúscula
            </li>
            <li [class.valid]="hasNumber">
              <app-icon [name]="hasNumber ? 'check' : 'x'" [size]="14" />
              Um número
            </li>
            <li [class.valid]="hasSpecial">
              <app-icon [name]="hasSpecial ? 'check' : 'x'" [size]="14" />
              Um caractere especial
            </li>
          </ul>
        </div>
        
        <app-button
          type="submit"
          [fullWidth]="true"
          [loading]="authService.isLoading()"
          [disabled]="form.invalid"
          size="lg"
        >
          Criar conta
        </app-button>
      </form>
      
      <div class="register-footer">
        <p>
          Já tem uma conta?
          <a routerLink="/auth/login">Entrar</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .register-page {
      width: 100%;
      animation: fadeIn 0.4s ease-out;
    }
    
    .register-header {
      text-align: center;
      margin-bottom: var(--space-6);
    }
    
    .mobile-logo {
      display: none;
      align-items: center;
      justify-content: center;
      gap: var(--space-3);
      margin-bottom: var(--space-6);
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
    }
    
    .logo-primary { color: var(--text-primary); }
    .logo-secondary { color: var(--color-primary-400); }
    
    .register-title {
      font-family: var(--font-serif);
      font-size: var(--text-3xl);
      font-weight: var(--font-normal);
      color: var(--text-primary);
      margin: 0 0 var(--space-2);
    }
    
    .register-subtitle {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0;
    }
    
    .register-form {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
    }
    
    .form-group {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .form-label {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
      
      .required {
        color: var(--color-error);
      }
    }
    
    .role-options {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .role-option {
      display: block;
      cursor: pointer;
      
      input {
        position: absolute;
        opacity: 0;
        pointer-events: none;
      }
    }
    
    .role-content {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-4);
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-lg);
      transition: all var(--transition-fast);
      color: var(--text-muted);
      
      &:hover {
        border-color: var(--color-neutral-500);
      }
    }
    
    .role-option.selected .role-content {
      border-color: var(--color-primary-500);
      background: rgba(99, 102, 241, 0.1);
      color: var(--color-primary-400);
    }
    
    .role-info {
      display: flex;
      flex-direction: column;
    }
    
    .role-label {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
    }
    
    .role-description {
      font-size: var(--text-xs);
      color: var(--text-muted);
    }
    
    .password-requirements {
      padding: var(--space-3);
      background: var(--bg-secondary);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border-primary);
    }
    
    .requirements-title {
      font-size: var(--text-xs);
      font-weight: var(--font-medium);
      color: var(--text-secondary);
      margin: 0 0 var(--space-2);
    }
    
    .requirements-list {
      list-style: none;
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-1);
      margin: 0;
      padding: 0;
      
      li {
        display: flex;
        align-items: center;
        gap: var(--space-1);
        font-size: var(--text-xs);
        color: var(--text-muted);
        
        &.valid {
          color: var(--color-success);
        }
      }
    }
    
    .register-footer {
      text-align: center;
      margin-top: var(--space-6);
      
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
    
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    @media (max-width: 1024px) {
      .mobile-logo {
        display: flex;
      }
    }
    
    @media (max-width: 480px) {
      .form-row {
        grid-template-columns: 1fr;
      }
      
      .requirements-list {
        grid-template-columns: 1fr;
      }
    }
  `],
})
export class RegisterPageComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);

  readonly roles = [
    { value: 'Admin' as UserRole, label: 'Administrador', description: 'Acesso total ao sistema', icon: 'settings' as const },
    { value: 'Operator' as UserRole, label: 'Operador', description: 'Gerencia pedidos e entregas', icon: 'truck' as const },
    { value: 'Customer' as UserRole, label: 'Cliente', description: 'Visualiza seus pedidos', icon: 'user' as const },
  ];

  readonly form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, this.passwordValidator]],
    confirmPassword: ['', [Validators.required]],
    role: ['Operator' as UserRole, Validators.required],
  }, { validators: this.passwordMatchValidator });

  // Password validation helpers
  get password(): string {
    return this.form.get('password')?.value || '';
  }

  get hasMinLength(): boolean {
    return this.password.length >= 8;
  }

  get hasUppercase(): boolean {
    return /[A-Z]/.test(this.password);
  }

  get hasLowercase(): boolean {
    return /[a-z]/.test(this.password);
  }

  get hasNumber(): boolean {
    return /[0-9]/.test(this.password);
  }

  get hasSpecial(): boolean {
    return /[!@#$%^&*(),.?":{}|<>]/.test(this.password);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const userData = this.form.value;

    this.authService.register(userData).subscribe({
      next: () => {
        this.toastService.success('Conta criada com sucesso!');
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.toastService.error(error.message || 'Erro ao criar conta');
      },
    });
  }

  getError(field: string): string {
    const control = this.form.get(field);
    if (!control?.touched || !control?.errors) {
      if (field === 'confirmPassword' && control?.value && this.form.errors?.['passwordMismatch']) {
        return 'Senhas não coincidem';
      }
      return '';
    }

    if (control.errors['required']) return 'Campo obrigatório';
    if (control.errors['email']) return 'E-mail inválido';
    if (control.errors['minlength']) return `Mínimo de ${control.errors['minlength'].requiredLength} caracteres`;
    if (control.errors['passwordStrength']) return 'Senha não atende aos requisitos';
    if (field === 'confirmPassword' && this.form.errors?.['passwordMismatch']) {
      return 'Senhas não coincidem';
    }

    return '';
  }

  private passwordValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    if (!password) return null;

    const hasMinLength = password.length >= 8;
    const hasUppercase = /[A-Z]/.test(password);
    const hasLowercase = /[a-z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);

    const valid = hasMinLength && hasUppercase && hasLowercase && hasNumber && hasSpecial;

    return valid ? null : { passwordStrength: true };
  }

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }
}
