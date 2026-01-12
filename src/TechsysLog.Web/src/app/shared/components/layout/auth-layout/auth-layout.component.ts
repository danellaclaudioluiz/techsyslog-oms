import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainerComponent, IconComponent } from '../../ui';

// ============================================================================
// Auth Layout Component
// Layout wrapper for authentication pages (login/register)
// ============================================================================

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, ToastContainerComponent, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-layout">
      <!-- Left Panel - Branding -->
      <div class="auth-branding">
        <div class="branding-content">
          <div class="brand-logo">
            <div class="logo-icon">
              <app-icon name="package" [size]="32" />
            </div>
            <h1 class="logo-text">
              <span class="logo-primary">Techsys</span><span class="logo-secondary">Log</span>
            </h1>
          </div>
          
          <div class="brand-tagline">
            <h2>Sistema de Controle de Pedidos e Entregas</h2>
            <p>Gerencie seus pedidos e entregas com eficiência e receba notificações em tempo real.</p>
          </div>
          
          <div class="brand-features">
            <div class="feature-item">
              <div class="feature-icon">
                <app-icon name="package" [size]="24" />
              </div>
              <div class="feature-text">
                <h4>Gestão de Pedidos</h4>
                <p>Controle completo do ciclo de vida dos pedidos</p>
              </div>
            </div>
            
            <div class="feature-item">
              <div class="feature-icon">
                <app-icon name="truck" [size]="24" />
              </div>
              <div class="feature-text">
                <h4>Rastreamento de Entregas</h4>
                <p>Acompanhe suas entregas em tempo real</p>
              </div>
            </div>
            
            <div class="feature-item">
              <div class="feature-icon">
                <app-icon name="bell-ring" [size]="24" />
              </div>
              <div class="feature-text">
                <h4>Notificações em Tempo Real</h4>
                <p>Receba atualizações instantâneas via SignalR</p>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Decorative Elements -->
        <div class="decoration decoration-1"></div>
        <div class="decoration decoration-2"></div>
        <div class="decoration decoration-3"></div>
      </div>
      
      <!-- Right Panel - Form -->
      <div class="auth-form-panel">
        <div class="auth-form-wrapper">
          <router-outlet />
        </div>
        
        <footer class="auth-footer">
          <p>&copy; {{ currentYear }} TechsysLog. Desenvolvido por Luiz Pugliele.</p>
        </footer>
      </div>
      
      <app-toast-container />
    </div>
  `,
  styles: [`
    .auth-layout {
      display: flex;
      min-height: 100vh;
    }
    
    .auth-branding {
      flex: 1;
      position: relative;
      background: linear-gradient(135deg, var(--color-neutral-900) 0%, var(--color-neutral-950) 100%);
      padding: var(--space-12);
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
    }
    
    .branding-content {
      position: relative;
      z-index: 1;
      max-width: 480px;
    }
    
    .brand-logo {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      margin-bottom: var(--space-10);
    }
    
    .logo-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      background: linear-gradient(135deg, var(--color-primary-500) 0%, var(--color-primary-600) 100%);
      border-radius: var(--radius-xl);
      color: white;
      box-shadow: 0 8px 32px rgba(99, 102, 241, 0.4);
    }
    
    .logo-text {
      font-family: var(--font-serif);
      font-size: var(--text-4xl);
      font-weight: var(--font-normal);
      letter-spacing: var(--tracking-tight);
      margin: 0;
    }
    
    .logo-primary {
      color: white;
    }
    
    .logo-secondary {
      color: var(--color-primary-400);
    }
    
    .brand-tagline {
      margin-bottom: var(--space-12);
      
      h2 {
        font-family: var(--font-serif);
        font-size: var(--text-2xl);
        font-weight: var(--font-normal);
        color: white;
        margin: 0 0 var(--space-4);
        line-height: var(--leading-snug);
      }
      
      p {
        font-size: var(--text-lg);
        color: var(--color-neutral-400);
        margin: 0;
        line-height: var(--leading-relaxed);
      }
    }
    
    .brand-features {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    .feature-item {
      display: flex;
      gap: var(--space-4);
      animation: fadeInUp 0.6s ease-out backwards;
      
      &:nth-child(1) { animation-delay: 0.1s; }
      &:nth-child(2) { animation-delay: 0.2s; }
      &:nth-child(3) { animation-delay: 0.3s; }
    }
    
    .feature-icon {
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 48px;
      height: 48px;
      background: rgba(99, 102, 241, 0.15);
      border-radius: var(--radius-lg);
      color: var(--color-primary-400);
    }
    
    .feature-text {
      h4 {
        font-size: var(--text-base);
        font-weight: var(--font-semibold);
        color: white;
        margin: 0 0 var(--space-1);
      }
      
      p {
        font-size: var(--text-sm);
        color: var(--color-neutral-400);
        margin: 0;
      }
    }
    
    /* Decorations */
    .decoration {
      position: absolute;
      border-radius: 50%;
      opacity: 0.05;
      background: linear-gradient(135deg, var(--color-primary-400) 0%, var(--color-accent-400) 100%);
    }
    
    .decoration-1 {
      width: 600px;
      height: 600px;
      top: -200px;
      right: -200px;
    }
    
    .decoration-2 {
      width: 400px;
      height: 400px;
      bottom: -100px;
      left: -100px;
    }
    
    .decoration-3 {
      width: 200px;
      height: 200px;
      top: 50%;
      right: 20%;
    }
    
    /* Form Panel */
    .auth-form-panel {
      flex: 1;
      display: flex;
      flex-direction: column;
      background: var(--bg-primary);
      padding: var(--space-8);
      overflow-y: auto;
    }
    
    .auth-form-wrapper {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      max-width: 440px;
      margin: 0 auto;
    }
    
    .auth-footer {
      text-align: center;
      padding-top: var(--space-8);
      
      p {
        font-size: var(--text-sm);
        color: var(--text-muted);
        margin: 0;
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
    
    @media (max-width: 1024px) {
      .auth-branding {
        display: none;
      }
      
      .auth-form-panel {
        padding: var(--space-6);
      }
    }
  `],
})
export class AuthLayoutComponent {
  readonly currentYear = new Date().getFullYear();
}
