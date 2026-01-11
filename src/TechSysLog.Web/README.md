# TechsysLog Web - Frontend Angular

[![Angular](https://img.shields.io/badge/Angular-21-DD0031)](https://angular.io/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)

Frontend moderno e responsivo para o sistema TechsysLog, desenvolvido com Angular 21, arquitetura escalável e design premium.

## Características

### Arquitetura
- **Standalone Components** - Componentes modernos sem NgModules
- **Signals** - Gerenciamento de estado reativo nativo
- **Lazy Loading** - Carregamento sob demanda de rotas
- **Clean Architecture** - Separação clara de responsabilidades
- **Feature Modules** - Organização por domínio/funcionalidade

### Design
- **Design System Premium** - CSS custom properties para theming
- **Dark Mode** - Tema escuro elegante por padrão
- **Responsive** - Suporte completo a mobile e desktop
- **Animações** - Micro-interações e transições fluidas
- **Acessibilidade** - Suporte a leitores de tela e navegação por teclado

### Funcionalidades
- Autenticação JWT com persistência
- CRUD completo de pedidos
- Gerenciamento de entregas
- Notificações em tempo real via SignalR
- Busca de endereço por CEP
- Dashboard com estatísticas
- Controle de acesso baseado em roles

## Estrutura do Projeto

```
src/
├── app/
│   ├── core/                    # Serviços singleton e configurações
│   │   ├── guards/              # Route guards
│   │   ├── interceptors/        # HTTP interceptors
│   │   ├── models/              # Interfaces e tipos
│   │   ├── services/            # Serviços de domínio
│   │   └── utils/               # Funções utilitárias
│   │
│   ├── features/                # Módulos de funcionalidades
│   │   ├── auth/                # Autenticação
│   │   ├── dashboard/           # Dashboard
│   │   ├── orders/              # Pedidos
│   │   ├── deliveries/          # Entregas
│   │   └── notifications/       # Notificações
│   │
│   ├── shared/                  # Componentes compartilhados
│   │   ├── components/
│   │   │   ├── ui/              # Componentes de UI (Button, Input, Card...)
│   │   │   └── layout/          # Componentes de layout (Header, Sidebar...)
│   │   ├── directives/          # Diretivas customizadas
│   │   └── pipes/               # Pipes customizados
│   │
│   ├── app.component.ts
│   ├── app.config.ts
│   └── app.routes.ts
│
├── assets/
│   ├── icons/
│   └── images/
│
├── environments/
│   ├── environment.ts
│   └── environment.prod.ts
│
├── styles.scss                  # Estilos globais e Design System
├── index.html
└── main.ts
```

## Tecnologias

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| Angular | 21 | Framework principal |
| TypeScript | 5.9 | Linguagem |
| RxJS | 7.8 | Programação reativa |
| SignalR | 8.0 | Comunicação em tempo real |
| SCSS | - | Pré-processador CSS |

## Design System

O projeto utiliza um design system baseado em CSS Custom Properties:

```scss
// Cores principais
--color-primary-500: #6366f1;
--color-success: #10b981;
--color-warning: #f59e0b;
--color-error: #ef4444;

// Tipografia
--font-sans: 'DM Sans', system-ui;
--font-serif: 'Instrument Serif', Georgia;

// Espaçamento
--space-1 a --space-24

// Bordas
--radius-sm a --radius-2xl

// Sombras
--shadow-sm a --shadow-xl
```

## Instalação

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/techsyslog-oms.git

# Entre no diretório do frontend
cd techsyslog-oms/src/TechsysLog.Web

# Instale as dependências
npm install

# Inicie o servidor de desenvolvimento
npm start
```

## Scripts Disponíveis

```bash
npm start          # Inicia servidor de desenvolvimento
npm run build      # Build de produção
npm run build:prod # Build otimizado para produção
npm run lint       # Executa linting
npm run format     # Formata código com Prettier
```

## Variáveis de Ambiente

Edite `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5150/api',
  signalRUrl: 'http://localhost:5150/hubs/notifications',
  tokenKey: 'techsyslog_token',
  userKey: 'techsyslog_user',
};
```

## Responsividade

O layout é totalmente responsivo com breakpoints:

| Breakpoint | Tamanho | Comportamento |
|------------|---------|---------------|
| Mobile | < 768px | Sidebar oculta, layout empilhado |
| Tablet | 768px - 1024px | Sidebar colapsada |
| Desktop | > 1024px | Layout completo |

## Autenticação e Autorização

### Roles disponíveis:
- **Admin** - Acesso total
- **Operator** - Gerencia pedidos e entregas
- **Customer** - Visualiza próprios pedidos

### Guards implementados:
- `authGuard` - Requer autenticação
- `guestGuard` - Apenas não autenticados
- `adminGuard` - Requer role Admin
- `operatorGuard` - Requer Admin ou Operator

## Real-time com SignalR

O sistema utiliza SignalR para notificações em tempo real:

```typescript
// Eventos suportados
- ReceiveNotification
- OrderStatusChanged
- UnreadCountUpdated
```

## Componentes UI

### Disponíveis:
- `app-button` - Botões com variantes
- `app-input` - Campos de formulário
- `app-card` - Cards containers
- `app-badge` - Badges de status
- `app-icon` - Ícones SVG inline
- `app-loader` - Indicadores de loading
- `app-toast-container` - Notificações toast
- `app-empty-state` - Estados vazios
- `app-order-status-badge` - Badge específico para status

## Licença

Este projeto está sob a licença MIT.

---

**Desenvolvido por Luiz Pugliele** | Senior Software Engineer
