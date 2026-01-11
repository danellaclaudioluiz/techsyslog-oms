import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { OrderService, AddressService, ToastService } from '@core/services';
import { 
  CardComponent, 
  IconComponent, 
  ButtonComponent,
  InputComponent,
} from '@shared/components/ui';

@Component({
  selector: 'app-order-form-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CardComponent,
    IconComponent,
    ButtonComponent,
    InputComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="order-form-page">
      <!-- Header -->
      <header class="page-header">
        <a routerLink="/orders" class="back-link">
          <app-icon name="arrow-left" [size]="20" />
          <span>Voltar para pedidos</span>
        </a>
        <div class="header-content">
          <h1 class="page-title">Novo Pedido</h1>
          <p class="page-subtitle">Preencha os dados abaixo para criar um novo pedido</p>
        </div>
      </header>
      
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="order-form">
        <div class="form-layout">
          <!-- Left Column - Main Info -->
          <div class="form-main">
            <app-card title="Informações do Pedido" class="form-card">
              <div class="card-icon" card-actions>
                <app-icon name="package" [size]="20" />
              </div>
              
              <div class="form-fields">
                <div class="form-field">
                  <app-input
                    label="Descrição do Pedido"
                    type="text"
                    placeholder="Ex: Notebook Dell XPS 15, iPhone 15 Pro..."
                    formControlName="description"
                    iconLeft="file-text"
                    [error]="getError('description')"
                    [required]="true"
                    hint="Descreva o produto ou serviço do pedido"
                  />
                </div>
                
                <div class="form-row">
                  <div class="form-field">
                    <app-input
                      label="Valor (R$)"
                      type="number"
                      placeholder="0,00"
                      formControlName="value"
                      iconLeft="dollar-sign"
                      [error]="getError('value')"
                      [required]="true"
                    />
                  </div>
                  
                  <div class="form-field value-preview">
                    <label class="preview-label">Valor formatado</label>
                    <div class="preview-value">
                      {{ formatCurrency(form.get('value')?.value || 0) }}
                    </div>
                  </div>
                </div>
              </div>
            </app-card>
            
            <app-card title="Endereço de Entrega" class="form-card">
              <div class="card-icon" card-actions>
                <app-icon name="map-pin" [size]="20" />
              </div>
              
              <div class="form-fields">
                <!-- CEP Search -->
                <div class="cep-section">
                  <label class="field-label">
                    CEP <span class="required">*</span>
                  </label>
                  <div class="cep-input-group">
                    <div class="cep-input-wrapper">
                      <app-icon name="map-pin" [size]="18" class="cep-icon" />
                      <input
                        type="text"
                        placeholder="00000-000"
                        formControlName="cep"
                        class="cep-input"
                        maxlength="9"
                        (input)="onCepInput($event)"
                        (keydown.enter)="lookupCep(); $event.preventDefault()"
                      />
                    </div>
                    <app-button
                      type="button"
                      variant="secondary"
                      [loading]="addressService.isLoading()"
                      [disabled]="!isValidCep()"
                      (clicked)="lookupCep()"
                      iconLeft="search"
                    >
                      Buscar CEP
                    </app-button>
                  </div>
                  @if (getError('cep')) {
                    <span class="field-error">{{ getError('cep') }}</span>
                  }
                  <span class="field-hint">Digite o CEP e clique em buscar para preencher o endereço</span>
                </div>
                
                <!-- Address Preview -->
                @if (addressService.currentAddress(); as address) {
                  <div class="address-found">
                    <div class="address-header">
                      <div class="address-icon">
                        <app-icon name="check-circle" [size]="20" />
                      </div>
                      <span class="address-title">Endereço encontrado</span>
                      <button type="button" class="clear-btn" (click)="clearAddress()" title="Limpar endereço">
                        <app-icon name="x" [size]="16" />
                      </button>
                    </div>
                    <div class="address-details">
                      <div class="address-line">
                        <app-icon name="home" [size]="16" />
                        <span>{{ address.street }}</span>
                      </div>
                      <div class="address-line">
                        <app-icon name="building" [size]="16" />
                        <span>{{ address.neighborhood }}</span>
                      </div>
                      <div class="address-line">
                        <app-icon name="map-pin" [size]="16" />
                        <span>{{ address.city }} - {{ address.state }}</span>
                      </div>
                    </div>
                  </div>
                }
                
                <!-- Number and Complement -->
                <div class="form-row">
                  <div class="form-field">
                    <app-input
                      label="Número"
                      type="text"
                      placeholder="123"
                      formControlName="number"
                      iconLeft="hash"
                      [error]="getError('number')"
                      [required]="true"
                    />
                  </div>
                  
                  <div class="form-field">
                    <app-input
                      label="Complemento"
                      type="text"
                      placeholder="Apto 101, Bloco B, Sala 5..."
                      formControlName="complement"
                      iconLeft="building"
                      hint="Opcional"
                    />
                  </div>
                </div>
              </div>
            </app-card>
          </div>
          
          <!-- Right Column - Summary -->
          <div class="form-sidebar">
            <div class="summary-card">
              <h3 class="summary-title">Resumo do Pedido</h3>
              
              <div class="summary-items">
                <div class="summary-item">
                  <span class="summary-label">Descrição</span>
                  <span class="summary-value">
                    {{ form.get('description')?.value || 'Não informado' }}
                  </span>
                </div>
                
                <div class="summary-item">
                  <span class="summary-label">Valor</span>
                  <span class="summary-value highlight">
                    {{ formatCurrency(form.get('value')?.value || 0) }}
                  </span>
                </div>
                
                <div class="summary-divider"></div>
                
                <div class="summary-item">
                  <span class="summary-label">Endereço</span>
                  <span class="summary-value">
                    @if (addressService.currentAddress(); as addr) {
                      {{ addr.street }}, {{ form.get('number')?.value || '...' }}
                      @if (form.get('complement')?.value) {
                        - {{ form.get('complement')?.value }}
                      }
                    } @else {
                      Busque o CEP
                    }
                  </span>
                </div>
                
                <div class="summary-item">
                  <span class="summary-label">Cidade</span>
                  <span class="summary-value">
                    @if (addressService.currentAddress(); as addr) {
                      {{ addr.city }} - {{ addr.state }}
                    } @else {
                      -
                    }
                  </span>
                </div>
              </div>
              
              <div class="summary-status">
                <div class="status-item" [class.complete]="form.get('description')?.valid">
                  <app-icon [name]="form.get('description')?.valid ? 'check-circle' : 'clock'" [size]="16" />
                  <span>Descrição</span>
                </div>
                <div class="status-item" [class.complete]="form.get('value')?.valid">
                  <app-icon [name]="form.get('value')?.valid ? 'check-circle' : 'clock'" [size]="16" />
                  <span>Valor</span>
                </div>
                <div class="status-item" [class.complete]="addressService.currentAddress()">
                  <app-icon [name]="addressService.currentAddress() ? 'check-circle' : 'clock'" [size]="16" />
                  <span>Endereço</span>
                </div>
                <div class="status-item" [class.complete]="form.get('number')?.valid">
                  <app-icon [name]="form.get('number')?.valid ? 'check-circle' : 'clock'" [size]="16" />
                  <span>Número</span>
                </div>
              </div>
              
              <div class="summary-actions">
                <app-button
                  type="submit"
                  [fullWidth]="true"
                  [loading]="orderService.isLoading()"
                  [disabled]="form.invalid || !addressService.currentAddress()"
                  iconLeft="check"
                  size="lg"
                >
                  Criar Pedido
                </app-button>
                
                <app-button
                  type="button"
                  variant="ghost"
                  [fullWidth]="true"
                  routerLink="/orders"
                >
                  Cancelar
                </app-button>
              </div>
            </div>
          </div>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .order-form-page {
      max-width: 1200px;
      margin: 0 auto;
    }
    
    /* Header */
    .page-header {
      margin-bottom: var(--space-8);
      animation: fadeInDown 0.4s ease-out;
    }
    
    .back-link {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-sm);
      color: var(--text-secondary);
      margin-bottom: var(--space-4);
      padding: var(--space-2) var(--space-3);
      border-radius: var(--radius-lg);
      transition: all var(--transition-fast);
      
      &:hover {
        color: var(--text-primary);
        background: var(--bg-secondary);
      }
    }
    
    .page-title {
      font-size: var(--text-3xl);
      font-weight: var(--font-bold);
      color: var(--text-primary);
      margin: 0 0 var(--space-2);
    }
    
    .page-subtitle {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0;
    }
    
    /* Form Layout */
    .form-layout {
      display: grid;
      grid-template-columns: 1fr 380px;
      gap: var(--space-6);
      align-items: start;
    }
    
    .form-main {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    .form-card {
      animation: fadeInUp 0.4s ease-out backwards;
      
      &:nth-child(2) {
        animation-delay: 0.1s;
      }
    }
    
    .card-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 36px;
      height: 36px;
      background: rgba(99, 102, 241, 0.15);
      border-radius: var(--radius-lg);
      color: var(--color-primary-400);
    }
    
    .form-fields {
      display: flex;
      flex-direction: column;
      gap: var(--space-5);
    }
    
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
    }
    
    /* Value Preview */
    .value-preview {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .preview-label {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
    }
    
    .preview-value {
      height: 44px;
      display: flex;
      align-items: center;
      padding: 0 var(--space-4);
      background: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      font-size: var(--text-lg);
      font-weight: var(--font-semibold);
      color: var(--color-success);
    }
    
    /* CEP Section */
    .cep-section {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .field-label {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
      
      .required {
        color: var(--color-error);
      }
    }
    
    .cep-input-group {
      display: flex;
      gap: var(--space-3);
    }
    
    .cep-input-wrapper {
      position: relative;
      flex: 1;
      max-width: 200px;
    }
    
    .cep-icon {
      position: absolute;
      left: var(--space-3);
      top: 50%;
      transform: translateY(-50%);
      color: var(--text-muted);
    }
    
    .cep-input {
      width: 100%;
      height: 44px;
      padding: 0 var(--space-4) 0 var(--space-10);
      background: var(--input-bg);
      border: 1px solid var(--input-border);
      border-radius: var(--radius-lg);
      color: var(--text-primary);
      font-size: var(--text-base);
      font-family: var(--font-mono, monospace);
      letter-spacing: 0.05em;
      transition: all var(--transition-fast);
      
      &::placeholder {
        color: var(--text-muted);
      }
      
      &:focus {
        outline: none;
        border-color: var(--color-primary-500);
        box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
      }
    }
    
    .field-error {
      font-size: var(--text-sm);
      color: var(--color-error);
    }
    
    .field-hint {
      font-size: var(--text-sm);
      color: var(--text-muted);
    }
    
    /* Address Found */
    .address-found {
      background: linear-gradient(135deg, rgba(16, 185, 129, 0.1) 0%, rgba(16, 185, 129, 0.05) 100%);
      border: 1px solid rgba(16, 185, 129, 0.3);
      border-radius: var(--radius-xl);
      padding: var(--space-4);
      animation: fadeIn 0.3s ease-out;
    }
    
    .address-header {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      margin-bottom: var(--space-3);
      padding-bottom: var(--space-3);
      border-bottom: 1px solid rgba(16, 185, 129, 0.2);
    }
    
    .address-icon {
      color: var(--color-success);
    }
    
    .address-title {
      flex: 1;
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
      color: var(--color-success);
    }
    
    .clear-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: var(--radius-md);
      background: transparent;
      color: var(--text-muted);
      transition: all var(--transition-fast);
      
      &:hover {
        background: rgba(239, 68, 68, 0.1);
        color: var(--color-error);
      }
    }
    
    .address-details {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .address-line {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-sm);
      color: var(--text-secondary);
      
      app-icon {
        color: var(--text-muted);
      }
    }
    
    /* Sidebar Summary */
    .form-sidebar {
      position: sticky;
      top: calc(var(--header-height) + var(--space-6));
    }
    
    .summary-card {
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
      padding: var(--space-6);
      animation: fadeInRight 0.4s ease-out 0.2s backwards;
    }
    
    .summary-title {
      font-size: var(--text-lg);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
      margin: 0 0 var(--space-5);
      padding-bottom: var(--space-4);
      border-bottom: 1px solid var(--border-primary);
    }
    
    .summary-items {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
      margin-bottom: var(--space-5);
    }
    
    .summary-item {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    
    .summary-label {
      font-size: var(--text-xs);
      font-weight: var(--font-medium);
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    
    .summary-value {
      font-size: var(--text-sm);
      color: var(--text-primary);
      word-break: break-word;
      
      &.highlight {
        font-size: var(--text-lg);
        font-weight: var(--font-bold);
        color: var(--color-success);
      }
    }
    
    .summary-divider {
      height: 1px;
      background: var(--border-primary);
    }
    
    /* Status Checklist */
    .summary-status {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      padding: var(--space-4);
      background: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      margin-bottom: var(--space-5);
    }
    
    .status-item {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-sm);
      color: var(--text-muted);
      
      app-icon {
        color: var(--text-muted);
      }
      
      &.complete {
        color: var(--color-success);
        
        app-icon {
          color: var(--color-success);
        }
      }
    }
    
    .summary-actions {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    
    /* Animations */
    @keyframes fadeInDown {
      from {
        opacity: 0;
        transform: translateY(-20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    @keyframes fadeInUp {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    @keyframes fadeInRight {
      from {
        opacity: 0;
        transform: translateX(20px);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }
    
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
    
    /* Responsive */
    @media (max-width: 1024px) {
      .form-layout {
        grid-template-columns: 1fr;
      }
      
      .form-sidebar {
        position: static;
        order: -1;
      }
      
      .summary-card {
        animation: fadeInDown 0.4s ease-out;
      }
    }
    
    @media (max-width: 640px) {
      .form-row {
        grid-template-columns: 1fr;
      }
      
      .cep-input-group {
        flex-direction: column;
      }
      
      .cep-input-wrapper {
        max-width: none;
      }
    }
  `],
})
export class OrderFormPageComponent implements OnInit {
  readonly orderService = inject(OrderService);
  readonly addressService = inject(AddressService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);

  readonly form: FormGroup = this.fb.group({
    description: ['', [Validators.required, Validators.minLength(3)]],
    value: ['', [Validators.required, Validators.min(0.01)]],
    cep: ['', [Validators.required, Validators.pattern(/^\d{5}-?\d{3}$/)]],
    number: ['', [Validators.required]],
    complement: [''],
  });

  ngOnInit(): void {
    this.addressService.clearAddress();
  }

  onCepInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, '');
    if (value.length > 5) {
      value = value.slice(0, 5) + '-' + value.slice(5, 8);
    }
    input.value = value;
    this.form.get('cep')?.setValue(value, { emitEvent: false });
  }

  isValidCep(): boolean {
    const cep = this.form.get('cep')?.value || '';
    return cep.replace(/\D/g, '').length === 8;
  }

  lookupCep(): void {
    const cep = this.form.get('cep')?.value;
    if (!cep) return;
    
    this.addressService.lookupCep(cep).subscribe({
      next: () => this.toastService.success('Endereço encontrado!'),
      error: () => {},
    });
  }

  clearAddress(): void {
    this.addressService.clearAddress();
    this.form.get('cep')?.setValue('');
  }

  onSubmit(): void {
    if (this.form.invalid || !this.addressService.currentAddress()) {
      this.form.markAllAsTouched();
      return;
    }

    const { description, value, cep, number, complement } = this.form.value;

    this.orderService.createOrder({
      description,
      value: Number(value),
      cep: cep.replace(/\D/g, ''),
      number,
      complement,
    }).subscribe({
      next: (response) => {
        this.toastService.success('Pedido criado com sucesso!');
        this.router.navigate(['/orders', response.data?.id]);
      },
    });
  }

  getError(field: string): string {
    const control = this.form.get(field);
    if (!control?.touched || !control?.errors) return '';

    if (control.errors['required']) return 'Campo obrigatório';
    if (control.errors['minlength']) return `Mínimo de ${control.errors['minlength'].requiredLength} caracteres`;
    if (control.errors['min']) return 'Valor deve ser maior que zero';
    if (control.errors['pattern']) return 'CEP inválido';

    return '';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value || 0);
  }
}
