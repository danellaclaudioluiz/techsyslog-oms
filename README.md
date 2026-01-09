# TechsysLog - Sistema de Controle de Pedidos e Entregas

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17-DD0031)](https://angular.io/)
[![MongoDB](https://img.shields.io/badge/MongoDB-7.0-47A248)](https://www.mongodb.com/)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)

Sistema de gerenciamento de pedidos e entregas com notificações em tempo real, desenvolvido com Clean Architecture, Domain-Driven Design e CQRS.

---

## Sumário

1. [Visão Geral](#visão-geral)
2. [Bounded Contexts](#bounded-contexts)
3. [Decisões Arquiteturais](#decisões-arquiteturais)
4. [Consistência de Dados](#consistência-de-dados)
5. [Segurança](#segurança)
6. [Tecnologias](#tecnologias)
7. [Arquitetura](#arquitetura)
8. [Estrutura do Projeto](#estrutura-do-projeto)
9. [Funcionalidades](#funcionalidades)
10. [Instalação e Execução](#instalação-e-execução)
11. [API Endpoints](#api-endpoints)
12. [Testes](#testes)
13. [Evolução Arquitetural](#evolução-arquitetural)
14. [Decisões Fora do Escopo](#decisões-fora-do-escopo)

---

## Visão Geral

O TechsysLog é uma plataforma para gestão logística que oferece:

- Cadastro e autenticação de usuários com diferentes níveis de acesso
- Gerenciamento completo do ciclo de vida de pedidos
- Registro e acompanhamento de entregas
- Notificações em tempo real via SignalR
- Preenchimento automático de endereço via CEP

---

## Bounded Contexts

O sistema foi modelado seguindo os princípios de Domain-Driven Design, com contextos delimitados bem definidos:

| Contexto | Responsabilidade | Agregados Principais |
|----------|------------------|---------------------|
| Identity | Gestão de usuários, autenticação e autorização | User |
| Orders | Ciclo de vida de pedidos e regras de negócio | Order, Address |
| Deliveries | Registro, validação e rastreamento de entregas | Delivery |
| Notifications | Comunicação em tempo real e histórico de eventos | Notification |

### Comunicação entre Contextos

```
+──────────────+     Domain Events      +──────────────────+
|    Orders    | ─────────────────────> |   Notifications  |
+──────────────+                        +──────────────────+
       |                                         |
       | Domain Events                           | SignalR
       v                                         v
+──────────────+                        +──────────────────+
|  Deliveries  |                        |   Frontend SPA   |
+──────────────+                        +──────────────────+
```

Os contextos se comunicam exclusivamente via Domain Events, garantindo baixo acoplamento e permitindo evolução independente.

---

## Decisões Arquiteturais

> Algumas informações foram omitidas propositalmente no escopo original. Abaixo estão documentadas as decisões tomadas para cada ponto.

### 1. Status do Pedido

**Problema**: Não foram especificados os status possíveis para um pedido.

**Decisão**: Implementar um fluxo de estados completo e rastreável:

```
+─────────+    +───────────+    +───────────+    +───────────+
| Pending |───>| Confirmed |───>| InTransit |───>| Delivered |
+─────────+    +───────────+    +───────────+    +───────────+
     |                                                  
     v                                                  
+───────────+                                           
| Cancelled |                                           
+───────────+                                           
```

**Justificativa**: Um fluxo bem definido permite rastreabilidade completa, regras de transição validadas no domínio e facilita a implementação de métricas e dashboards.

---

### 2. Modelo de Usuários e Permissões

**Problema**: Não foi definido se o usuário é cliente, operador ou administrador.

**Decisão**: Implementar sistema de roles com três níveis:

| Role | Permissões |
|------|------------|
| Admin | CRUD completo de usuários, pedidos e configurações |
| Operator | Gerenciar pedidos, registrar entregas, visualizar relatórios |
| Customer | Visualizar próprios pedidos, receber notificações |

**Justificativa**: Flexibilidade para diferentes cenários de uso, princípio do menor privilégio (RBAC), preparação para expansão futura.

---

### 3. Regras de Negócio para Entrega

**Problema**: Não foram especificadas validações para registro de entrega.

**Decisão**: Implementar as seguintes regras:

- Pedido deve estar com status `InTransit` para ser entregue
- Data/hora de entrega é registrada automaticamente pelo servidor (UTC)
- Apenas operadores e administradores podem registrar entregas
- Entrega gera evento de domínio para notificação

**Justificativa**: Garante integridade do fluxo, evita manipulação de timestamps pelo cliente, auditoria completa.

---

### 4. Sistema de Notificações vs. Eventos em Tempo Real

**Problema**: Não foram definidos tipos de notificação nem estratégia de persistência.

**Decisão**: O sistema diferencia claramente Notificações Persistentes de Eventos em Tempo Real:

| Aspecto | Notificações Persistentes | Eventos em Tempo Real |
|---------|---------------------------|----------------------|
| Propósito | Auditoria e histórico | UX e feedback imediato |
| Tecnologia | MongoDB | SignalR |
| Durabilidade | Permanente | Volátil |
| Retry | Não aplicável | Reconexão automática |

**Tipos de Notificação**:

- `OrderCreated` - Novo pedido cadastrado
- `OrderStatusChanged` - Mudança de status do pedido
- `OrderDelivered` - Pedido entregue com sucesso

**Persistência**:

- Collection separada no MongoDB
- Campos: `userId`, `type`, `message`, `data`, `read`, `createdAt`
- Índice composto em `(userId, read, createdAt)` para queries otimizadas

**Justificativa**: SignalR é utilizado exclusivamente para comunicação em tempo real (UX), enquanto o histórico e estado das notificações é persistido no banco para consistência e auditoria. Falha na entrega via SignalR não impacta a persistência.

---

### 5. Integração com API de CEP (Anti-Corruption Layer)

**Problema**: Não foi especificada qual API externa utilizar.

**Decisão**: Utilizar ViaCEP (https://viacep.com.br) com Anti-Corruption Layer.

```
+─────────────────+      +─────────────────+      +─────────────────+
|  Domain Layer   |      |      ACL        |      |    ViaCEP       |
|                 |      |                 |      |                 |
|  Address (VO)   |<─────|  CepService     |<─────|  External API   |
|                 |      |  (Translation)  |      |                 |
+─────────────────+      +─────────────────+      +─────────────────+
```

A integração é encapsulada em um serviço de infraestrutura que atua como Anti-Corruption Layer, protegendo o domínio de:

- Mudanças no contrato externo da ViaCEP
- Formato de dados diferente do domínio
- Indisponibilidade do serviço externo

**Resiliência implementada com Polly**:

- Retry: 3 tentativas com backoff exponencial
- Circuit Breaker: abre após 5 falhas consecutivas
- Timeout: 5 segundos por requisição
- Fallback: permite entrada manual quando indisponível

**Justificativa**:

- Gratuita e sem limite de requisições
- Não requer autenticação
- Alta disponibilidade
- Resposta rápida (< 100ms)

---

### 6. Por que MongoDB?

**Problema**: Escolha de banco de dados não especificada explicitamente.

**Decisão**: MongoDB como banco principal.

| Critério | Justificativa |
|----------|---------------|
| Modelo de Documento | Order + Address embarcado como subdocumento, evitando JOINs |
| Escrita Intensiva | Logs e notificações com alto volume de escrita |
| Schema Flexível | Facilita evolução do modelo sem migrations complexas |
| Escalabilidade | Horizontal nativa via sharding |
| Índices Compostos | (userId, read, createdAt) para queries otimizadas |
| TTL Index | Auto-expiração de notificações antigas (opcional) |

**Consideração importante**:

Apesar do domínio ter características relacionais, o modelo foi desenhado para funcionar bem tanto em MongoDB quanto em bancos relacionais, mantendo o domínio completamente desacoplado da infraestrutura via Repository Pattern. O domínio não conhece MongoDB - apenas interfaces de repositório.

---

### 7. Validações de Campos

**Problema**: Critérios de validação não especificados.

**Decisão**:

| Campo | Validação |
|-------|-----------|
| Email | Formato válido (RFC 5322), único no sistema |
| Senha | Mínimo 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 especial |
| CEP | Exatamente 8 dígitos numéricos |
| Número do Pedido | Gerado automaticamente (formato: ORD-YYYYMMDD-XXXXX) |
| Valor | Decimal positivo, máximo 2 casas decimais |

---

### 8. Estratégia de Deleção

**Problema**: Não foi definido se usar soft delete ou hard delete.

**Decisão**: Soft Delete para todas as entidades principais.

```csharp
public abstract class BaseEntity
{
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
    
    // Todos os repositórios filtram automaticamente registros deletados
}
```

**Justificativa**: Auditoria completa, possibilidade de restauração, compliance com LGPD (direito ao esquecimento via anonimização posterior).

---

### 9. Paginação

**Problema**: Estratégia de paginação não definida.

**Decisão**: Cursor-based pagination para listagens principais.

```json
{
  "data": [],
  "pagination": {
    "cursor": "eyJpZCI6IjY1YTEyMzQ1In0=",
    "hasMore": true,
    "limit": 20
  }
}
```

**Justificativa**:

- Performance O(1) vs O(n) do offset-based
- Consistência em datasets que mudam frequentemente
- Ideal para scroll infinito no frontend
- Não pula registros quando dados são inseridos/removidos

---

## Consistência de Dados

O sistema adota Eventual Consistency entre operações de negócio e efeitos colaterais:

```
+─────────────────────────────────────────────────────────────────+
|                      TRANSAÇÃO PRINCIPAL                        |
|  +─────────────+      +─────────────+      +─────────────────+  |
|  |   Validar   |─────>|   Salvar    |─────>| Publicar Evento |  |
|  |   Pedido    |      |   MongoDB   |      |   (In-Memory)   |  |
|  +─────────────+      +─────────────+      +─────────────────+  |
+─────────────────────────────────────────────────────────────────+
                                |
                                v Async (não bloqueia)
+─────────────────────────────────────────────────────────────────+
|                    EFEITOS COLATERAIS                           |
|  +─────────────────+           +─────────────────────────────+  |
|  | Persistir       |           | Enviar via SignalR          |  |
|  | Notificação     |           | (melhor esforço)            |  |
|  +─────────────────+           +─────────────────────────────+  |
+─────────────────────────────────────────────────────────────────+
```

### Garantias

| Operação | Garantia | Comportamento em Falha |
|----------|----------|----------------------|
| Criar/Atualizar Pedido | Strong Consistency | Rollback completo |
| Persistir Notificação | Eventual Consistency | Retry interno, log de falha |
| Enviar SignalR | Best Effort | Cliente reconecta e sincroniza |

### Princípio Fundamental

Falha na notificação (persistência ou tempo real) nunca invalida a transação principal de negócio.

---

## Segurança

### Autenticação JWT

Estrutura do Token:

```json
{
  "sub": "user-id-uuid",
  "email": "user@example.com",
  "role": "Operator",
  "permissions": ["orders:read", "orders:write", "deliveries:write"],
  "iat": 1704067200,
  "exp": 1704153600,
  "iss": "TechsysLog",
  "aud": "TechsysLogUsers"
}
```

| Claim | Propósito |
|-------|-----------|
| sub | Identificador único do usuário (evita consulta ao banco) |
| role | Papel principal para autorização rápida |
| permissions | Permissões granulares (opcional, para ABAC futuro) |

**Decisão**: Autorizações são feitas preferencialmente via claims, evitando consultas repetidas ao banco em cada request.

### Proteção de Senhas

- **Algoritmo**: BCrypt com work factor 12
- **Validação**: Mínimo 8 caracteres, complexidade obrigatória
- **Armazenamento**: Apenas hash, nunca texto plano

### Rate Limiting

Endpoints sensíveis estão preparados para Rate Limiting (ASP.NET RateLimiter Middleware):

| Endpoint | Limite | Janela |
|----------|--------|--------|
| POST /auth/login | 5 tentativas | 15 minutos |
| POST /auth/register | 3 cadastros | 1 hora |
| POST /orders | 100 pedidos | 1 hora |

**Justificativa**: Mitigação de brute force, abuso de API e proteção de recursos.

### Outras Proteções

- HTTPS obrigatório em produção
- CORS configurado por ambiente
- Headers de segurança (HSTS, X-Content-Type-Options, X-Frame-Options)
- Sanitização de inputs
- Proteção contra Mass Assignment via DTOs explícitos

---

## Tecnologias

### Backend

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 8.0 | Framework principal |
| ASP.NET Core | 8.0 | Web API REST |
| SignalR | 8.0 | Comunicação real-time |
| MongoDB.Driver | 2.23 | Acesso ao banco de dados |
| MediatR | 12.2 | Mediator pattern (CQRS) |
| FluentValidation | 11.9 | Validações |
| AutoMapper | 13.0 | Mapeamento de objetos |
| JWT Bearer | 8.0 | Autenticação |
| Serilog | 3.1 | Logging estruturado |
| Polly | 8.2 | Resiliência (retry, circuit breaker) |

### Frontend

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| Angular | 17 | Framework SPA |
| Angular Material | 17 | Componentes UI |
| NgRx | 17 | State management |
| RxJS | 7.8 | Programação reativa |
| SignalR Client | 8.0 | Conexão real-time |

### Infraestrutura

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| MongoDB | 7.0 | Banco de dados |
| Docker | 24+ | Containerização |
| Docker Compose | 2.23 | Orquestração local |

---

## Arquitetura

### Clean Architecture com DDD

```
+─────────────────────────────────────────────────────────────────────────+
|                           PRESENTATION LAYER                            |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
|  |   Angular SPA    |  |    REST API      |  |    SignalR Hub       |   |
|  |   (Port 4200)    |  |   Controllers    |  |   (Notifications)    |   |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
+─────────────────────────────────────────────────────────────────────────+
|                           APPLICATION LAYER                             |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
|  |    Commands      |  |     Queries      |  |    DTOs / Mappers    |   |
|  |    Handlers      |  |     Handlers     |  |    Validators        |   |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
|                              MediatR                                    |
+─────────────────────────────────────────────────────────────────────────+
|                             DOMAIN LAYER                                |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
|  |    Entities      |  |  Value Objects   |  |   Domain Events      |   |
|  |    Aggregates    |  |  (Email, CEP,    |  |   Domain Services    |   |
|  |    (Order, User) |  |   Address)       |  |   Specifications     |   |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
+─────────────────────────────────────────────────────────────────────────+
|                         INFRASTRUCTURE LAYER                            |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
|  |    MongoDB       |  |   External APIs  |  |    Cross-Cutting     |   |
|  |    Repositories  |  |   (ViaCEP + ACL) |  |    (JWT, Logging)    |   |
|  +──────────────────+  +──────────────────+  +──────────────────────+   |
+─────────────────────────────────────────────────────────────────────────+
```

### Padrões Implementados

| Padrão | Aplicação | Justificativa |
|--------|-----------|---------------|
| CQRS | Commands/Queries separados | Otimização de leitura/escrita independente |
| Repository | Abstração do MongoDB | Domínio desacoplado de infraestrutura |
| Unit of Work | Transações | Consistência em operações múltiplas |
| Specification | Filtros encapsulados | Regras de consulta reutilizáveis |
| Domain Events | Comunicação entre agregados | Baixo acoplamento |
| Result Pattern | Tratamento de erros | Fluxo explícito sem exceptions |
| Value Objects | Email, CEP, Address | Imutabilidade e validação embutida |
| Guard Clauses | Validações no construtor | Fail-fast, entidades sempre válidas |
| Mediator | MediatR | Desacoplamento de handlers |
| ACL | Integração ViaCEP | Proteção do domínio |
| Options Pattern | Configurações | Type-safe settings |

---

## Estrutura do Projeto

```
TechsysLog/
|
+-- src/
|   +-- TechsysLog.Domain/
|   |   +-- Entities/
|   |   |   +-- User.cs
|   |   |   +-- Order.cs
|   |   |   +-- Delivery.cs
|   |   |   +-- Notification.cs
|   |   +-- ValueObjects/
|   |   |   +-- Email.cs
|   |   |   +-- Password.cs
|   |   |   +-- Address.cs
|   |   |   +-- Cep.cs
|   |   |   +-- OrderNumber.cs
|   |   +-- Enums/
|   |   |   +-- OrderStatus.cs
|   |   |   +-- UserRole.cs
|   |   |   +-- NotificationType.cs
|   |   +-- Events/
|   |   |   +-- OrderCreatedEvent.cs
|   |   |   +-- OrderStatusChangedEvent.cs
|   |   |   +-- OrderDeliveredEvent.cs
|   |   +-- Specifications/
|   |   |   +-- OrderSpecifications.cs
|   |   +-- Interfaces/
|   |   |   +-- IUserRepository.cs
|   |   |   +-- IOrderRepository.cs
|   |   |   +-- INotificationRepository.cs
|   |   +-- Common/
|   |       +-- BaseEntity.cs
|   |       +-- AggregateRoot.cs
|   |       +-- Result.cs
|   |
|   +-- TechsysLog.Application/
|   |   +-- Commands/
|   |   |   +-- Users/
|   |   |   |   +-- CreateUserCommand.cs
|   |   |   |   +-- CreateUserHandler.cs
|   |   |   +-- Orders/
|   |   |   |   +-- CreateOrderCommand.cs
|   |   |   |   +-- CreateOrderHandler.cs
|   |   |   |   +-- UpdateOrderStatusCommand.cs
|   |   |   |   +-- UpdateOrderStatusHandler.cs
|   |   |   +-- Deliveries/
|   |   |       +-- RegisterDeliveryCommand.cs
|   |   |       +-- RegisterDeliveryHandler.cs
|   |   +-- Queries/
|   |   |   +-- Orders/
|   |   |   |   +-- GetOrderByIdQuery.cs
|   |   |   |   +-- GetOrdersQuery.cs
|   |   |   |   +-- GetOrdersHandler.cs
|   |   |   +-- Notifications/
|   |   |       +-- GetUserNotificationsQuery.cs
|   |   |       +-- GetUserNotificationsHandler.cs
|   |   +-- DTOs/
|   |   |   +-- UserDto.cs
|   |   |   +-- OrderDto.cs
|   |   |   +-- AddressDto.cs
|   |   |   +-- NotificationDto.cs
|   |   +-- Validators/
|   |   |   +-- CreateUserValidator.cs
|   |   |   +-- CreateOrderValidator.cs
|   |   +-- Mappings/
|   |   |   +-- MappingProfile.cs
|   |   +-- Behaviors/
|   |   |   +-- ValidationBehavior.cs
|   |   |   +-- LoggingBehavior.cs
|   |   +-- Interfaces/
|   |       +-- ICepService.cs
|   |       +-- INotificationService.cs
|   |
|   +-- TechsysLog.Infrastructure/
|   |   +-- Persistence/
|   |   |   +-- MongoDbContext.cs
|   |   |   +-- Repositories/
|   |   |   |   +-- UserRepository.cs
|   |   |   |   +-- OrderRepository.cs
|   |   |   |   +-- NotificationRepository.cs
|   |   |   +-- Configurations/
|   |   |       +-- MongoDbSettings.cs
|   |   +-- ExternalServices/
|   |   |   +-- ViaCepService.cs
|   |   |   +-- ViaCepResponse.cs
|   |   +-- Identity/
|   |   |   +-- JwtService.cs
|   |   |   +-- JwtSettings.cs
|   |   |   +-- PasswordHasher.cs
|   |   +-- DependencyInjection.cs
|   |
|   +-- TechsysLog.API/
|   |   +-- Controllers/
|   |   |   +-- AuthController.cs
|   |   |   +-- UsersController.cs
|   |   |   +-- OrdersController.cs
|   |   |   +-- DeliveriesController.cs
|   |   |   +-- NotificationsController.cs
|   |   +-- Hubs/
|   |   |   +-- NotificationHub.cs
|   |   +-- Middleware/
|   |   |   +-- ExceptionMiddleware.cs
|   |   |   +-- RequestLoggingMiddleware.cs
|   |   +-- Filters/
|   |   |   +-- ValidationFilter.cs
|   |   +-- Extensions/
|   |   |   +-- ServiceCollectionExtensions.cs
|   |   +-- Program.cs
|   |   +-- appsettings.json
|   |
|   +-- TechsysLog.Web/
|       +-- src/
|       |   +-- app/
|       |   |   +-- core/
|       |   |   |   +-- auth/
|       |   |   |   +-- guards/
|       |   |   |   +-- interceptors/
|       |   |   |   +-- services/
|       |   |   +-- features/
|       |   |   |   +-- orders/
|       |   |   |   +-- deliveries/
|       |   |   |   +-- notifications/
|       |   |   +-- shared/
|       |   |   |   +-- components/
|       |   |   |   +-- pipes/
|       |   |   +-- store/
|       |   |       +-- orders/
|       |   |       +-- notifications/
|       |   +-- environments/
|       +-- angular.json
|       +-- package.json
|
+-- tests/
|   +-- TechsysLog.Domain.Tests/
|   |   +-- Entities/
|   |   +-- ValueObjects/
|   +-- TechsysLog.Application.Tests/
|   |   +-- Commands/
|   |   +-- Queries/
|   +-- TechsysLog.Integration.Tests/
|       +-- Controllers/
|
+-- docker/
|   +-- Dockerfile.api
|   +-- Dockerfile.web
|   +-- docker-compose.yml
|
+-- docs/
|   +-- API.md
|   +-- ARCHITECTURE.md
|   +-- DECISIONS.md
|
+-- .github/
|   +-- workflows/
|       +-- ci.yml
|
+-- README.md
+-- LICENSE
+-- .gitignore
```

---

## Funcionalidades

### Autenticação e Autorização

- Cadastro de usuários com validação de email único
- Login com geração de token JWT
- Refresh token para renovação de sessão
- Controle de acesso baseado em roles (RBAC)
- Hash seguro de senhas com BCrypt

### Gestão de Pedidos

- Cadastro de pedidos com número automático
- Busca de endereço por CEP via ViaCEP
- Atualização de status com validação de transições
- Listagem com filtros e paginação cursor-based
- Soft delete com auditoria

### Entregas

- Registro de entrega com timestamp do servidor
- Validação de status do pedido
- Histórico completo de entregas

### Notificações em Tempo Real

- Hub SignalR para push notifications
- Persistência de notificações no MongoDB
- Marcação de leitura individual e em lote
- Contador de notificações não lidas

### Painel de Acompanhamento

- Dashboard com visão geral de pedidos
- Filtros por status, data e cliente
- Atualização em tempo real via SignalR
- Indicadores visuais de status

---

## Instalação e Execução

### Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- Angular CLI 17
- Docker Desktop
- MongoDB 7.0 (ou via Docker)

### Opção 1: Docker Compose (Recomendado)

```bash
# Clone o repositório
git clone https://github.com/danellaclaudioluiz/techsyslog-oms.git
cd techsyslog

# Suba todos os serviços
docker-compose up -d
```

Após a inicialização:

| Serviço | URL |
|---------|-----|
| Frontend | http://localhost:4200 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| MongoDB | localhost:27017 |

### Opção 2: Execução Manual

**Backend:**

```bash
cd src/TechsysLog.API
dotnet restore
dotnet run
```

**Frontend:**

```bash
cd src/TechsysLog.Web
npm install
ng serve
```

### Variáveis de Ambiente

```env
# MongoDB
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
MONGODB_DATABASE_NAME=techsyslog

# JWT
JWT_SECRET=your-super-secret-key-min-32-chars
JWT_ISSUER=TechsysLog
JWT_AUDIENCE=TechsysLogUsers
JWT_EXPIRES_IN_HOURS=24

# ViaCEP
VIACEP_BASE_URL=https://viacep.com.br/ws
VIACEP_TIMEOUT_SECONDS=5

# Rate Limiting
RATE_LIMIT_LOGIN_ATTEMPTS=5
RATE_LIMIT_LOGIN_WINDOW_MINUTES=15
```

---

## API Endpoints

### Autenticação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | /api/auth/register | Cadastrar novo usuário |
| POST | /api/auth/login | Autenticar usuário |
| POST | /api/auth/refresh | Renovar token |

### Usuários

| Método | Endpoint | Descrição | Role |
|--------|----------|-----------|------|
| GET | /api/users | Listar usuários | Admin |
| GET | /api/users/{id} | Buscar usuário | Admin |
| PUT | /api/users/{id} | Atualizar usuário | Admin |
| DELETE | /api/users/{id} | Remover usuário | Admin |

### Pedidos

| Método | Endpoint | Descrição | Role |
|--------|----------|-----------|------|
| POST | /api/orders | Criar pedido | Operator, Admin |
| GET | /api/orders | Listar pedidos | All |
| GET | /api/orders/{id} | Buscar pedido | All |
| PATCH | /api/orders/{id}/status | Atualizar status | Operator, Admin |
| DELETE | /api/orders/{id} | Cancelar pedido | Admin |

### Entregas

| Método | Endpoint | Descrição | Role |
|--------|----------|-----------|------|
| POST | /api/deliveries | Registrar entrega | Operator, Admin |
| GET | /api/deliveries/{orderId} | Buscar entrega | All |

### Endereço

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | /api/address/{cep} | Buscar endereço por CEP |

### Notificações

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | /api/notifications | Listar notificações do usuário |
| GET | /api/notifications/unread-count | Contador de não lidas |
| PATCH | /api/notifications/{id}/read | Marcar como lida |
| PATCH | /api/notifications/read-all | Marcar todas como lidas |

### SignalR Hub

| Hub | Evento | Payload |
|-----|--------|---------|
| /hubs/notifications | ReceiveNotification | { type, message, data, createdAt } |
| /hubs/notifications | OrderStatusChanged | { orderId, oldStatus, newStatus } |
| /hubs/notifications | UnreadCountUpdated | { count } |

---

## Testes

### Execução

```bash
# Executar todos os testes
dotnet test

# Testes com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Testes por categoria
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

### Estratégia de Testes

| Camada | Tipo | Foco |
|--------|------|------|
| Domain | Unitário | Entidades, Value Objects, Regras de negócio |
| Application | Unitário | Handlers, Validators, Mappings |
| Infrastructure | Integração | Repositórios, Serviços externos |
| API | Integração | Controllers, Middleware, E2E |

### Cobertura Mínima

| Camada | Target |
|--------|--------|
| Domain | > 90% |
| Application | > 85% |
| Infrastructure | > 70% |
| API | > 80% |

---

## Evolução Arquitetural

A arquitetura atual foi desenhada como monólito modular, permitindo evolução futura para microserviços sem reescrita.

### Fase Atual: Monólito Modular

```
+─────────────────────────────────────+
|           TechsysLog API            |
|  +─────────+ +─────────+ +───────+  |
|  | Orders  | |Delivery | |Notif. |  |
|  | Module  | | Module  | |Module |  |
|  +─────────+ +─────────+ +───────+  |
|         Shared Domain Events        |
+─────────────────────────────────────+
              |
              v
        +──────────+
        | MongoDB  |
        +──────────+
```

### Fase Futura: Microserviços

```
+───────────────+  +───────────────+  +───────────────+
| Orders Service|  |Delivery Service| |Notif. Service |
+───────┬───────+  +───────┬───────+  +───────┬───────+
        |                  |                  |
        +──────────────────┼──────────────────+
                           |
                    +──────v──────+
                    | Message Bus |
                    | (RabbitMQ)  |
                    +─────────────+
```

### Preparação para Evolução

| Aspecto | Implementação Atual | Facilita Migração |
|---------|---------------------|-------------------|
| Bounded Contexts | Separação por módulos | Cada módulo vira serviço |
| Comunicação | Domain Events in-memory | Troca por message broker |
| Dados | Collections separadas | Cada serviço com seu DB |
| Contratos | DTOs bem definidos | Viram contratos de API |

---

## Decisões Fora do Escopo

As seguintes decisões arquiteturais foram conscientemente não implementadas devido ao escopo do projeto, mas são recomendadas para produção:

| Padrão | Motivo da Exclusão | Quando Implementar |
|--------|---------------------|-------------------|
| Outbox Pattern | Complexidade para garantia de entrega | Quando escala exigir garantia de eventos |
| Message Broker (RabbitMQ/Kafka) | Overhead para volume atual | Quando migrar para microserviços |
| Saga Pattern | Não há transações distribuídas | Quando houver múltiplos serviços |
| CQRS com bancos separados | MongoDB atende leitura/escrita | Quando perfis forem muito diferentes |
| Event Sourcing | Complexidade vs benefício | Quando auditoria completa for requisito |
| Cache distribuído (Redis) | Volume não justifica | Quando houver queries repetitivas pesadas |
| API Gateway | Monólito não requer | Quando migrar para microserviços |
| Service Mesh | Overkill para monólito | Em ambiente Kubernetes |

---

## Autor

**Luiz Pugliele**  
Senior Software Engineer

---

## Licença

Este projeto está sob a licença MIT. Consulte o arquivo [LICENSE](LICENSE) para mais detalhes.
